using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ReferenceManager : IReferenceManager, IReferenceResolver
{
	private ImmutableArray<Diagnostic> diagnostics;

	private ImmutableArray<ModuleSymbol> loadedModules;

	private readonly IEnumerable<SymbolReferenceSpecification> symbolReferences;

	private readonly ISymbolReferenceLoader referenceLoader;

	private readonly IDotNetResolverFactory dotNetResolverFactory;

	private volatile bool loaded;

	private readonly object gate = new object();

	internal static ReferenceManager Create(IEnumerable<SymbolReferenceSpecification> symbolReferences, ISymbolReferenceLoader referenceLoader, IDotNetResolverFactory dotNetResolverFactory)
	{
		return new ReferenceManager(symbolReferences, referenceLoader, dotNetResolverFactory);
	}

	protected ReferenceManager(IEnumerable<SymbolReferenceSpecification> symbolReferences, ISymbolReferenceLoader referenceLoader, IDotNetResolverFactory dotNetResolverFactory)
	{
		this.symbolReferences = symbolReferences;
		this.referenceLoader = referenceLoader;
		this.dotNetResolverFactory = dotNetResolverFactory;
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken)
	{
		EnsureLoaded(cancellationToken);
		return diagnostics;
	}

	public ImmutableArray<ModuleSymbol> GetLoadedModules()
	{
		EnsureLoaded();
		return loadedModules;
	}

	private void EnsureLoaded(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (loaded)
		{
			return;
		}
		lock (gate)
		{
			if (!loaded)
			{
				LoadReferencedModules(cancellationToken);
				loaded = true;
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private void LoadReferencedModules(CancellationToken cancellationToken)
	{
		if (symbolReferences.IsEmpty() || referenceLoader == null)
		{
			loadedModules = ImmutableArray<ModuleSymbol>.Empty;
			diagnostics = ImmutableArray<Diagnostic>.Empty;
			return;
		}
		PooledList<Diagnostic> instance = PooledList<Diagnostic>.GetInstance();
		PooledHashSet<SymbolReferenceSpecification> instance2 = PooledHashSet<SymbolReferenceSpecification>.GetInstance();
		Dictionary<SymbolReferenceSpecification, ModuleSymbol> dictionary = new Dictionary<SymbolReferenceSpecification, ModuleSymbol>(SymbolReferenceSpecification.VersionLessEqualityComparer);
		try
		{
			List<Diagnostic> loadingDiagnostics = new List<Diagnostic>();
			ImmutableDictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>> dependenciesDictionary = GetDependenciesDictionary(loadingDiagnostics);
			IEnumerable<SymbolReferenceSpecification> topologicallySortedModules = new ModuleDependencyGraph(symbolReferences, dependenciesDictionary).GetTopologicallySortedModules(cancellationToken);
			foreach (SymbolReferenceSpecification symbolReference in symbolReferences)
			{
				instance2.Add(symbolReference);
			}
			foreach (SymbolReferenceSpecification item in topologicallySortedModules)
			{
				cancellationToken.ThrowIfCancellationRequested();
				bool flag = instance2.Contains(item);
				bool flag2 = false;
				string text = null;
				PooledList<Diagnostic> instance3 = PooledList<Diagnostic>.GetInstance();
				try
				{
					ModuleDefinition moduleDefinition = referenceLoader.LoadModule(item, instance3);
					bool flag3 = !instance3.IsEmpty() & flag;
					if (flag3)
					{
						instance.AddRange(instance3);
					}
					flag2 = ((moduleDefinition == null) & flag) && !flag3;
					if (moduleDefinition != null && dependenciesDictionary.TryGetValue(item, out var value))
					{
						ImmutableArray<IModuleSymbol> referenceModules = ImmutableArray<IModuleSymbol>.CastUp(GetReferencedModules(dictionary, value));
						ModuleSymbol value2 = new ReferenceModuleSymbol(this, dotNetResolverFactory.CreateResolver(), moduleDefinition, referenceModules);
						dictionary[item] = value2;
					}
				}
				catch (Exception ex)
				{
					GlobalLogger.LogVerbose("An exception happened while reading the module '{0}'. {1}", item.ToDisplayString(), ex.Message);
					if (flag)
					{
						flag2 = true;
						text = ex.Message;
					}
				}
				finally
				{
					instance3.Free();
				}
				if (flag2)
				{
					instance.Add(Diagnostic.Create(MessageProvider.Instance, 1024, item?.Publisher ?? string.Empty, item?.Name ?? string.Empty, item?.Version?.ToString() ?? string.Empty, text ?? string.Empty));
				}
			}
		}
		finally
		{
			instance2.Free();
			loadedModules = dictionary.Values.ToImmutableArray();
			diagnostics = instance.ToImmutableArrayOrEmpty();
			instance.Free();
		}
	}

	private static ImmutableArray<ModuleSymbol> GetReferencedModules(Dictionary<SymbolReferenceSpecification, ModuleSymbol> loadedModuleSymbols, ImmutableHashSet<SymbolReferenceSpecification> dependencies)
	{
		ArrayBuilder<ModuleSymbol> instance = ArrayBuilder<ModuleSymbol>.GetInstance();
		try
		{
			foreach (SymbolReferenceSpecification dependency in dependencies)
			{
				if (loadedModuleSymbols.TryGetValue(dependency, out var value))
				{
					instance.Add(value);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	private ImmutableDictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>> GetDependenciesDictionary(IList<Diagnostic> loadingDiagnostics)
	{
		Dictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>> dictionary = new Dictionary<SymbolReferenceSpecification, ImmutableHashSet<SymbolReferenceSpecification>>(SymbolReferenceSpecification.VersionLessEqualityComparer);
		foreach (SymbolReferenceSpecification symbolReference in symbolReferences)
		{
			PooledHashSet<SymbolReferenceSpecification> instance = PooledHashSet<SymbolReferenceSpecification>.GetInstance();
			try
			{
				instance.AddAll(referenceLoader.GetDependencies(symbolReference, loadingDiagnostics));
			}
			finally
			{
				dictionary[symbolReference] = instance.ToImmutableHashSet();
				instance.Free();
			}
		}
		return dictionary.ToImmutableDictionary(dictionary.Comparer);
	}

	public virtual IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(ModuleSymbol referencingModule, SymbolKind kind)
	{
		EnsureLoaded();
		foreach (ISymbol objectSymbol in referencingModule.GetObjectSymbols(kind))
		{
			yield return objectSymbol;
		}
		foreach (IModuleSymbol referenceModule in referencingModule.ReferenceModules)
		{
			foreach (ISymbol objectSymbol2 in referenceModule.GetObjectSymbols(kind))
			{
				yield return objectSymbol2;
			}
		}
	}

	public virtual ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(ModuleSymbol referencingModule, SymbolKind kind, int id)
	{
		EnsureLoaded();
		ArrayBuilder<ISymbolWithId> instance = ArrayBuilder<ISymbolWithId>.GetInstance();
		try
		{
			ISymbolWithId objectSymbolById = referencingModule.GetObjectSymbolById(kind, id);
			if (objectSymbolById != null)
			{
				instance.Add(objectSymbolById);
			}
			foreach (IModuleSymbol referenceModule in referencingModule.ReferenceModules)
			{
				objectSymbolById = referenceModule.GetObjectSymbolById(kind, id);
				if (objectSymbolById != null)
				{
					instance.Add(objectSymbolById);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public virtual ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(ModuleSymbol referencingModule, SymbolKind kind, string name)
	{
		EnsureLoaded();
		if (kind == SymbolKind.DotNet)
		{
			return ResolveDotNetObjectSymbolsByName(referencingModule, name);
		}
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			ISymbol objectSymbolByName = referencingModule.GetObjectSymbolByName(kind, name);
			if (objectSymbolByName != null)
			{
				instance.Add(objectSymbolByName);
			}
			foreach (ModuleSymbol referenceModule in referencingModule.ReferenceModules)
			{
				objectSymbolByName = referenceModule.GetObjectSymbolByName(kind, name);
				if (objectSymbolByName != null)
				{
					instance.Add(objectSymbolByName);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	private static ImmutableArray<ISymbol> ResolveDotNetObjectSymbolsByName(ModuleSymbol referencingModule, string name)
	{
		PooledHashSet<ISymbol> instance = PooledHashSet<ISymbol>.GetInstance();
		try
		{
			ISymbol objectSymbolByName = referencingModule.GetObjectSymbolByName(SymbolKind.DotNet, name);
			if (objectSymbolByName != null)
			{
				instance.Add(objectSymbolByName);
			}
			foreach (ModuleSymbol referenceModule in referencingModule.ReferenceModules)
			{
				objectSymbolByName = referenceModule.GetObjectSymbolByName(SymbolKind.DotNet, name);
				if (objectSymbolByName != null)
				{
					instance.Add(objectSymbolByName);
				}
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}

	ImmutableArray<Diagnostic> IReferenceManager.GetDiagnostics(CancellationToken cancellationToken)
	{
		return GetDiagnostics(cancellationToken);
	}

	ImmutableArray<IModuleSymbol> IReferenceManager.GetLoadedModules()
	{
		return ImmutableArray<IModuleSymbol>.CastUp(GetLoadedModules());
	}

	ImmutableArray<ISymbolWithId> IReferenceResolver.GetObjectSymbolsByIdAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, int id)
	{
		return GetObjectSymbolsByIdAcrossModules((ModuleSymbol)referencingModule, kind, id);
	}

	ImmutableArray<ISymbol> IReferenceResolver.GetObjectSymbolsByNameAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, string name)
	{
		return GetObjectSymbolsByNameAcrossModules((ModuleSymbol)referencingModule, kind, name);
	}

	IEnumerable<ISymbol> IReferenceResolver.GetObjectSymbolsByKindAcrossModules(IModuleSymbol referencingModule, SymbolKind kind)
	{
		return GetObjectSymbolsByKindAcrossModules((ModuleSymbol)referencingModule, kind);
	}
}
