using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class RadReferenceManager : IReferenceManager, IReferenceResolver
{
	private readonly HashSet<ObjectChangeElementWrapper> radAddedModifiedChanges = new HashSet<ObjectChangeElementWrapper>();

	private readonly ModuleSymbol packagedModuleSymbol;

	private readonly IReferenceManager radCompiledObjectsReferenceManager;

	public RadReferenceManager(IReferenceManager radCompiledObjectsReferenceManager, ISymbolReferenceLoader referenceLoader, IDotNetResolverFactory dotNetResolverFactory, ObjectChangeModelDefinition radChanges, ImmutableArray<IModuleSymbol> sourceReferenceModules, ModuleDefinition packagedModuleDefinition)
	{
		if (radChanges == null)
		{
			throw new ArgumentNullException("radChanges");
		}
		if (radCompiledObjectsReferenceManager == null)
		{
			throw new ArgumentNullException("radCompiledObjectsReferenceManager");
		}
		this.radCompiledObjectsReferenceManager = radCompiledObjectsReferenceManager;
		UpdateHash(radChanges.Added);
		UpdateHash(radChanges.Modified);
		if (packagedModuleDefinition != null)
		{
			ReferenceManager resolver = ReferenceManager.Create(Enumerable.Empty<SymbolReferenceSpecification>(), referenceLoader, dotNetResolverFactory);
			packagedModuleSymbol = new ReferenceModuleSymbol(resolver, dotNetResolverFactory.CreateResolver(), packagedModuleDefinition, sourceReferenceModules);
		}
	}

	public ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, int id)
	{
		ImmutableArray<ISymbolWithId> result = ImmutableArray<ISymbolWithId>.Empty;
		ObjectChangeElementWrapper key = new ObjectChangeElementWrapper(new ObjectChangeElement
		{
			Id = id,
			Kind = kind,
			Name = string.Empty
		});
		if (packagedModuleSymbol != null)
		{
			result = RemoveRadChangeSymbols(key, packagedModuleSymbol.GetObjectSymbolsByIdAcrossModules(kind, id));
		}
		if (IsEmptyOrWithMissingReferences(result))
		{
			result = radCompiledObjectsReferenceManager.GetObjectSymbolsByIdAcrossModules(referencingModule as ModuleSymbol, kind, id);
		}
		return result;
	}

	public ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, string name)
	{
		ImmutableArray<ISymbol> result = ImmutableArray<ISymbol>.Empty;
		ObjectChangeElementWrapper key = new ObjectChangeElementWrapper(new ObjectChangeElement
		{
			Id = null,
			Kind = kind,
			Name = name
		});
		if (packagedModuleSymbol != null)
		{
			result = RemoveRadChangeSymbols(key, packagedModuleSymbol.GetObjectSymbolsByNameAcrossModules(kind, name));
		}
		if (IsEmptyOrWithMissingReferences(result))
		{
			result = radCompiledObjectsReferenceManager.GetObjectSymbolsByNameAcrossModules(referencingModule as ModuleSymbol, kind, name);
		}
		return result;
	}

	public IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(IModuleSymbol referencingModule, SymbolKind kind)
	{
		IEnumerable<ISymbol> objectSymbolsByKindAcrossModules = radCompiledObjectsReferenceManager.GetObjectSymbolsByKindAcrossModules(referencingModule as ModuleSymbol, kind);
		if (packagedModuleSymbol != null)
		{
			IEnumerable<ISymbol> objectSymbolsByKindAcrossModules2 = packagedModuleSymbol.GetObjectSymbolsByKindAcrossModules(kind);
			return GetCommonObjectTypeSymbolsFromFirstTheRemainderFromSecond(objectSymbolsByKindAcrossModules, objectSymbolsByKindAcrossModules2);
		}
		return objectSymbolsByKindAcrossModules;
	}

	public ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken)
	{
		return radCompiledObjectsReferenceManager.GetDiagnostics(cancellationToken);
	}

	public ImmutableArray<IModuleSymbol> GetLoadedModules()
	{
		return ImmutableArray<IModuleSymbol>.CastUp(radCompiledObjectsReferenceManager.GetLoadedModules());
	}

	private ImmutableArray<SymbolType> RemoveRadChangeSymbols<SymbolType>(ObjectChangeElementWrapper key, IEnumerable<SymbolType> symbols) where SymbolType : ISymbol
	{
		ArrayBuilder<SymbolType> instance = ArrayBuilder<SymbolType>.GetInstance();
		try
		{
			foreach (SymbolType symbol in symbols)
			{
				if (!radAddedModifiedChanges.Contains(key))
				{
					instance.Add(symbol);
				}
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private static bool IsEmptyOrWithMissingReferences<T>(ImmutableArray<T> result) where T : ISymbol
	{
		if (result.Length != 0)
		{
			return result.Any((T r) => (object)r == MissingTypeSymbol.Instance);
		}
		return true;
	}

	private static IEnumerable<ISymbol> GetCommonObjectTypeSymbolsFromFirstTheRemainderFromSecond(IEnumerable<ISymbol> first, IEnumerable<ISymbol> second)
	{
		PooledObjectTypeSymbolHashset hash = PooledObjectTypeSymbolHashset.GetInstance();
		try
		{
			foreach (IObjectTypeSymbol item in second)
			{
				DebugAssertHelper.Assert(item != null);
				hash.Add(item);
			}
			foreach (IObjectTypeSymbol item2 in first)
			{
				DebugAssertHelper.Assert(item2 != null);
				if (hash.Count == 0 || hash.Remove(item2))
				{
					yield return item2;
				}
			}
			foreach (IObjectTypeSymbol item3 in hash)
			{
				yield return item3;
			}
		}
		finally
		{
			hash.Free();
		}
	}

	private void UpdateHash(ObjectChangeElement[] elements)
	{
		if (elements != null && elements.Length != 0)
		{
			foreach (ObjectChangeElement objectChangeElement in elements)
			{
				radAddedModifiedChanges.Add(new ObjectChangeElementWrapper(objectChangeElement));
			}
		}
	}
}
