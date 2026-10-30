using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class CommonReferenceManager<TCompilation, TAssemblySymbol> : CommonReferenceManager where TCompilation : Compilation where TAssemblySymbol : class, IAssemblySymbolInternal
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal abstract class AssemblyData
	{
		public abstract AssemblyIdentity Identity { get; }

		public abstract ImmutableArray<AssemblyIdentity> AssemblyReferences { get; }

		public abstract IEnumerable<TAssemblySymbol> AvailableSymbols { get; }

		public abstract bool ContainsNoPiaLocalTypes { get; }

		public abstract bool IsLinked { get; }

		public abstract bool DeclaresTheObjectClass { get; }

		public abstract Compilation SourceCompilation { get; }

		public abstract bool IsMatchingAssembly(TAssemblySymbol assembly);

		public abstract AssemblyReferenceBinding[] BindAssemblyReferences(ImmutableArray<AssemblyData> assemblies, AssemblyIdentityComparer assemblyIdentityComparer);

		private string GetDebuggerDisplay()
		{
			return GetType().Name + ": [" + Identity.GetDisplayName() + "]";
		}
	}

	protected sealed class AssemblyDataForAssemblyBeingBuilt : AssemblyData
	{
		private readonly AssemblyIdentity _assemblyIdentity;

		private readonly ImmutableArray<AssemblyData> _referencedAssemblyData;

		private readonly ImmutableArray<AssemblyIdentity> _referencedAssemblies;

		public override AssemblyIdentity Identity => _assemblyIdentity;

		public override ImmutableArray<AssemblyIdentity> AssemblyReferences => _referencedAssemblies;

		public override IEnumerable<TAssemblySymbol> AvailableSymbols
		{
			get
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		public override bool ContainsNoPiaLocalTypes
		{
			get
			{
				throw ExceptionUtilities.Unreachable;
			}
		}

		public override bool IsLinked => false;

		public override bool DeclaresTheObjectClass => false;

		public override Compilation SourceCompilation => null;

		public AssemblyDataForAssemblyBeingBuilt(AssemblyIdentity identity, ImmutableArray<AssemblyData> referencedAssemblyData, ImmutableArray<PEModule> modules)
		{
			_assemblyIdentity = identity;
			_referencedAssemblyData = referencedAssemblyData;
			ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance(referencedAssemblyData.Length + modules.Length);
			foreach (AssemblyData item in referencedAssemblyData)
			{
				instance.Add(item.Identity);
			}
			for (int i = 1; i <= modules.Length; i++)
			{
				instance.AddRange(modules[i - 1].ReferencedAssemblies);
			}
			_referencedAssemblies = instance.ToImmutableAndFree();
		}

		public override AssemblyReferenceBinding[] BindAssemblyReferences(ImmutableArray<AssemblyData> assemblies, AssemblyIdentityComparer assemblyIdentityComparer)
		{
			AssemblyReferenceBinding[] array = new AssemblyReferenceBinding[_referencedAssemblies.Length];
			for (int i = 0; i < _referencedAssemblyData.Length; i++)
			{
				array[i] = new AssemblyReferenceBinding(assemblies[i + 1].Identity, i + 1);
			}
			for (int j = _referencedAssemblyData.Length; j < _referencedAssemblies.Length; j++)
			{
				array[j] = CommonReferenceManager<TCompilation, TAssemblySymbol>.ResolveReferencedAssembly(_referencedAssemblies[j], assemblies, 1, assemblyIdentityComparer);
			}
			return array;
		}

		public override bool IsMatchingAssembly(TAssemblySymbol assembly)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal struct AssemblyReferenceBinding
	{
		private readonly AssemblyIdentity _referenceIdentity;

		private readonly int _definitionIndex;

		private readonly int _versionDifference;

		internal bool BoundToAssemblyBeingBuilt => _definitionIndex == 0;

		internal bool IsBound => _definitionIndex >= 0;

		internal int VersionDifference => _versionDifference;

		internal int DefinitionIndex => _definitionIndex;

		internal AssemblyIdentity ReferenceIdentity => _referenceIdentity;

		public AssemblyReferenceBinding(AssemblyIdentity referenceIdentity)
		{
			_referenceIdentity = referenceIdentity;
			_definitionIndex = -1;
			_versionDifference = 0;
		}

		public AssemblyReferenceBinding(AssemblyIdentity referenceIdentity, int definitionIndex, int versionDifference = 0)
		{
			_referenceIdentity = referenceIdentity;
			_definitionIndex = definitionIndex;
			_versionDifference = versionDifference;
		}

		private string GetDebuggerDisplay()
		{
			if (!IsBound)
			{
				return "unbound";
			}
			return ReferenceIdentity.GetDisplayName() + " -> #" + DefinitionIndex + ((VersionDifference != 0) ? (" VersionDiff=" + VersionDifference) : "");
		}
	}

	private struct AssemblyReferenceCandidate(int definitionIndex, TAssemblySymbol symbol)
	{
		public readonly int DefinitionIndex = definitionIndex;

		public readonly TAssemblySymbol AssemblySymbol = symbol;
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal struct BoundInputAssembly
	{
		internal TAssemblySymbol AssemblySymbol;

		internal AssemblyReferenceBinding[] ReferenceBinding;

		private string GetDebuggerDisplay()
		{
			if (AssemblySymbol != null)
			{
				return AssemblySymbol.ToString();
			}
			return "?";
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	protected struct ResolvedReference(int index, MetadataImageKind kind)
	{
		private readonly MetadataImageKind _kind = kind;

		private readonly int _index = index + 1;

		private readonly ImmutableArray<string> _aliasesOpt = default;

		private readonly ImmutableArray<string> _recursiveAliasesOpt = default;

		private bool IsUninitialized
		{
			get
			{
				if (_aliasesOpt.IsDefault)
				{
					return _recursiveAliasesOpt.IsDefault;
				}
				return false;
			}
		}

		public ImmutableArray<string> AliasesOpt => _aliasesOpt;

		public ImmutableArray<string> RecursiveAliasesOpt => _recursiveAliasesOpt;

		public bool IsSkipped => _index == 0;

		public MetadataImageKind Kind => _kind;

		public int Index => _index - 1;

		public ResolvedReference(int index, MetadataImageKind kind, ImmutableArray<string> aliasesOpt, ImmutableArray<string> recursiveAliasesOpt)
			: this(index, kind)
		{
			_aliasesOpt = aliasesOpt;
			_recursiveAliasesOpt = recursiveAliasesOpt;
		}

		private string GetDebuggerDisplay()
		{
			if (!IsSkipped)
			{
				return string.Format("{0}[{1}]:{2}{3}", (_kind == MetadataImageKind.Assembly) ? "A" : "M", Index, DisplayAliases(_aliasesOpt, "aliases"), DisplayAliases(_recursiveAliasesOpt, "recursive-aliases"));
			}
			return "<skipped>";
		}

		private static string DisplayAliases(ImmutableArray<string> aliasesOpt, string name)
		{
			if (!aliasesOpt.IsDefault)
			{
				return " " + name + " = '" + string.Join("','", aliasesOpt) + "'";
			}
			return "";
		}
	}

	protected struct ReferencedAssemblyIdentity
	{
		public readonly AssemblyIdentity Identity;

		public readonly MetadataReference Reference;

		public readonly int RelativeAssemblyIndex;

		public int GetAssemblyIndex(int explicitlyReferencedAssemblyCount)
		{
			if (RelativeAssemblyIndex < 0)
			{
				return explicitlyReferencedAssemblyCount + RelativeAssemblyIndex;
			}
			return RelativeAssemblyIndex;
		}

		public ReferencedAssemblyIdentity(AssemblyIdentity identity, MetadataReference reference, int relativeAssemblyIndex)
		{
			Identity = identity;
			Reference = reference;
			RelativeAssemblyIndex = relativeAssemblyIndex;
		}
	}

	internal sealed class MetadataReferenceEqualityComparer : IEqualityComparer<MetadataReference>
	{
		internal static readonly MetadataReferenceEqualityComparer Instance = new MetadataReferenceEqualityComparer();

		public bool Equals(MetadataReference x, MetadataReference y)
		{
			if (x == y)
			{
				return true;
			}
			if (x is CompilationReference compilationReference && y is CompilationReference compilationReference2)
			{
				return compilationReference.Compilation == compilationReference2.Compilation;
			}
			return false;
		}

		public int GetHashCode(MetadataReference reference)
		{
			if (reference is CompilationReference compilationReference)
			{
				return RuntimeHelpers.GetHashCode(compilationReference.Compilation);
			}
			return RuntimeHelpers.GetHashCode(reference);
		}
	}

	internal readonly string SimpleAssemblyName;

	internal readonly AssemblyIdentityComparer IdentityComparer;

	internal readonly Dictionary<MetadataReference, object> ObservedMetadata;

	private int _isBound;

	private ThreeState _lazyHasCircularReference;

	private Dictionary<MetadataReference, int> _lazyReferencedAssembliesMap;

	private Dictionary<MetadataReference, int> _lazyReferencedModuleIndexMap;

	private IDictionary<(string, string), MetadataReference> _lazyReferenceDirectiveMap;

	private ImmutableArray<MetadataReference> _lazyDirectiveReferences;

	private ImmutableArray<MetadataReference> _lazyExplicitReferences;

	private ImmutableArray<MetadataReference> _lazyImplicitReferences;

	private ImmutableArray<Diagnostic> _lazyDiagnostics;

	private TAssemblySymbol _lazyCorLibraryOpt;

	private ImmutableArray<PEModule> _lazyReferencedModules;

	private ImmutableArray<ModuleReferences<TAssemblySymbol>> _lazyReferencedModulesReferences;

	private ImmutableArray<TAssemblySymbol> _lazyReferencedAssemblies;

	private ImmutableArray<ImmutableArray<string>> _lazyAliasesOfReferencedAssemblies;

	private ImmutableArray<UnifiedAssembly<TAssemblySymbol>> _lazyUnifiedAssemblies;

	private static readonly ImmutableArray<string> s_supersededAlias = ImmutableArray.Create("<superseded>");

	protected abstract CommonMessageProvider MessageProvider { get; }

	internal ImmutableArray<Diagnostic> Diagnostics => _lazyDiagnostics;

	internal bool HasCircularReference => _lazyHasCircularReference == ThreeState.True;

	internal Dictionary<MetadataReference, int> ReferencedAssembliesMap => _lazyReferencedAssembliesMap;

	internal Dictionary<MetadataReference, int> ReferencedModuleIndexMap => _lazyReferencedModuleIndexMap;

	internal IDictionary<(string, string), MetadataReference> ReferenceDirectiveMap => _lazyReferenceDirectiveMap;

	internal ImmutableArray<MetadataReference> DirectiveReferences => _lazyDirectiveReferences;

	internal override ImmutableArray<MetadataReference> ImplicitReferences => _lazyImplicitReferences;

	internal override ImmutableArray<MetadataReference> ExplicitReferences => _lazyExplicitReferences;

	internal TAssemblySymbol CorLibraryOpt => _lazyCorLibraryOpt;

	internal ImmutableArray<PEModule> ReferencedModules => _lazyReferencedModules;

	internal ImmutableArray<ModuleReferences<TAssemblySymbol>> ReferencedModulesReferences => _lazyReferencedModulesReferences;

	internal ImmutableArray<TAssemblySymbol> ReferencedAssemblies => _lazyReferencedAssemblies;

	internal ImmutableArray<ImmutableArray<string>> AliasesOfReferencedAssemblies => _lazyAliasesOfReferencedAssemblies;

	internal ImmutableArray<UnifiedAssembly<TAssemblySymbol>> UnifiedAssemblies => _lazyUnifiedAssemblies;

	internal bool IsBound => _isBound != 0;

	internal IEnumerable<string> ExternAliases => AliasesOfReferencedAssemblies.SelectMany((ImmutableArray<string> aliases) => aliases);

	protected BoundInputAssembly[] Bind(TCompilation compilation, ImmutableArray<AssemblyData> explicitAssemblies, ImmutableArray<PEModule> explicitModules, ImmutableArray<MetadataReference> explicitReferences, ImmutableArray<ResolvedReference> explicitReferenceMap, MetadataReferenceResolver resolverOpt, MetadataImportOptions importOptions, bool supersedeLowerVersions, [In][Out] Dictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName, out ImmutableArray<AssemblyData> allAssemblies, out ImmutableArray<MetadataReference> implicitlyResolvedReferences, out ImmutableArray<ResolvedReference> implicitlyResolvedReferenceMap, [In][Out] DiagnosticBag resolutionDiagnostics, out bool hasCircularReference, out int corLibraryIndex)
	{
		ArrayBuilder<AssemblyReferenceBinding[]> instance = ArrayBuilder<AssemblyReferenceBinding[]>.GetInstance();
		try
		{
			for (int i = 0; i < explicitAssemblies.Length; i++)
			{
				instance.Add(explicitAssemblies[i].BindAssemblyReferences(explicitAssemblies, IdentityComparer));
			}
			if (resolverOpt != null && resolverOpt.ResolveMissingAssemblies)
			{
				ResolveAndBindMissingAssemblies(compilation, explicitAssemblies, explicitModules, explicitReferences, explicitReferenceMap, resolverOpt, importOptions, supersedeLowerVersions, instance, assemblyReferencesBySimpleName, out allAssemblies, out implicitlyResolvedReferences, out implicitlyResolvedReferenceMap, resolutionDiagnostics);
			}
			else
			{
				allAssemblies = explicitAssemblies;
				implicitlyResolvedReferences = ImmutableArray<MetadataReference>.Empty;
				implicitlyResolvedReferenceMap = ImmutableArray<ResolvedReference>.Empty;
			}
			hasCircularReference = CheckCircularReference(instance);
			corLibraryIndex = IndexOfCorLibrary(explicitAssemblies, assemblyReferencesBySimpleName, supersedeLowerVersions);
			BoundInputAssembly[] array = new BoundInputAssembly[instance.Count];
			for (int j = 0; j < instance.Count; j++)
			{
				array[j].ReferenceBinding = instance[j];
			}
			TAssemblySymbol[] candidateInputAssemblySymbols = new TAssemblySymbol[allAssemblies.Length];
			if (!hasCircularReference && ReuseAssemblySymbolsWithNoPiaLocalTypes(array, candidateInputAssemblySymbols, allAssemblies, corLibraryIndex))
			{
				return array;
			}
			ReuseAssemblySymbols(array, candidateInputAssemblySymbols, allAssemblies, corLibraryIndex);
			return array;
		}
		finally
		{
			instance.Free();
		}
	}

	private void ResolveAndBindMissingAssemblies(TCompilation compilation, ImmutableArray<AssemblyData> explicitAssemblies, ImmutableArray<PEModule> explicitModules, ImmutableArray<MetadataReference> explicitReferences, ImmutableArray<ResolvedReference> explicitReferenceMap, MetadataReferenceResolver resolver, MetadataImportOptions importOptions, bool supersedeLowerVersions, [In][Out] ArrayBuilder<AssemblyReferenceBinding[]> referenceBindings, [In][Out] Dictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName, out ImmutableArray<AssemblyData> allAssemblies, out ImmutableArray<MetadataReference> metadataReferences, out ImmutableArray<ResolvedReference> resolvedReferences, DiagnosticBag resolutionDiagnostics)
	{
		int totalReferencedAssemblyCount = explicitAssemblies.Length - 1;
		ArrayBuilder<AssemblyData> instance = ArrayBuilder<AssemblyData>.GetInstance();
		PooledHashSet<AssemblyIdentity> instance2 = PooledHashSet<AssemblyIdentity>.GetInstance();
		PooledDictionary<AssemblyIdentity, PortableExecutableReference> pooledDictionary = null;
		Compilation compilation2 = compilation.ScriptCompilationInfo?.PreviousScriptCompilation;
		if (compilation2 != null)
		{
			pooledDictionary = PooledDictionary<AssemblyIdentity, PortableExecutableReference>.GetInstance();
			foreach (KeyValuePair<AssemblyIdentity, PortableExecutableReference> implicitlyResolvedAssemblyReference in compilation2.GetBoundReferenceManager().GetImplicitlyResolvedAssemblyReferences())
			{
				pooledDictionary.Add(implicitlyResolvedAssemblyReference.Key, implicitlyResolvedAssemblyReference.Value);
			}
		}
		ArrayBuilder<MetadataReference> instance3 = ArrayBuilder<MetadataReference>.GetInstance();
		Dictionary<MetadataReference, MergedAliases> lazyAliasMap = null;
		ArrayBuilder<(MetadataReference, ArraySegment<AssemblyReferenceBinding>)> instance4 = ArrayBuilder<(MetadataReference, ArraySegment<AssemblyReferenceBinding>)>.GetInstance();
		GetInitialReferenceBindingsToProcess(explicitModules, explicitReferences, explicitReferenceMap, referenceBindings, totalReferencedAssemblyCount, instance4);
		int length = explicitAssemblies.Length;
		try
		{
			while (instance4.Count > 0)
			{
				var (definition, arraySegment) = instance4.Pop();
				foreach (AssemblyReferenceBinding item in (IEnumerable<AssemblyReferenceBinding>)arraySegment/*cast due to constrained. prefix*/)
				{
					if (item.IsBound || !instance2.Add(item.ReferenceIdentity))
					{
						continue;
					}
					if (pooledDictionary == null || !pooledDictionary.TryGetValue(item.ReferenceIdentity, out var value))
					{
						value = resolver.ResolveMissingAssembly(definition, item.ReferenceIdentity);
						if (value == null)
						{
							continue;
						}
					}
					AssemblyData assemblyData = ResolveMissingAssembly(item.ReferenceIdentity, value, importOptions, resolutionDiagnostics);
					if (assemblyData != null)
					{
						int assemblyIndex = length - 1 + instance3.Count;
						MetadataReference metadataReference = TryAddAssembly(assemblyData.Identity, value, assemblyIndex, resolutionDiagnostics, Location.None, assemblyReferencesBySimpleName, supersedeLowerVersions);
						if (metadataReference != null)
						{
							MergeReferenceProperties(metadataReference, value, resolutionDiagnostics, ref lazyAliasMap);
							continue;
						}
						instance3.Add(value);
						instance.Add(assemblyData);
						AssemblyReferenceBinding[] array = assemblyData.BindAssemblyReferences(explicitAssemblies, IdentityComparer);
						referenceBindings.Add(array);
						instance4.Push((value, new ArraySegment<AssemblyReferenceBinding>(array)));
					}
				}
			}
			if (instance.Count == 0)
			{
				resolvedReferences = ImmutableArray<ResolvedReference>.Empty;
				metadataReferences = ImmutableArray<MetadataReference>.Empty;
				allAssemblies = explicitAssemblies;
				return;
			}
			allAssemblies = explicitAssemblies.AddRange(instance);
			for (int i = 0; i < referenceBindings.Count; i++)
			{
				AssemblyReferenceBinding[] array2 = referenceBindings[i];
				for (int j = 0; j < array2.Length; j++)
				{
					AssemblyReferenceBinding assemblyReferenceBinding = array2[j];
					if (!assemblyReferenceBinding.IsBound)
					{
						array2[j] = ResolveReferencedAssembly(assemblyReferenceBinding.ReferenceIdentity, allAssemblies, length, IdentityComparer);
					}
				}
			}
			UpdateBindingsOfAssemblyBeingBuilt(referenceBindings, length, instance);
			metadataReferences = instance3.ToImmutable();
			resolvedReferences = ToResolvedAssemblyReferences(metadataReferences, lazyAliasMap, length);
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance4.Free();
			instance3.Free();
			pooledDictionary?.Free();
		}
	}

	private void GetInitialReferenceBindingsToProcess(ImmutableArray<PEModule> explicitModules, ImmutableArray<MetadataReference> explicitReferences, ImmutableArray<ResolvedReference> explicitReferenceMap, ArrayBuilder<AssemblyReferenceBinding[]> referenceBindings, int totalReferencedAssemblyCount, [Out] ArrayBuilder<(MetadataReference, ArraySegment<AssemblyReferenceBinding>)> result)
	{
		ImmutableArray<int> immutableArray = CalculateModuleToReferenceMap(explicitModules, explicitReferenceMap);
		AssemblyReferenceBinding[] array = referenceBindings[0];
		int num = totalReferencedAssemblyCount;
		for (int i = 0; i < explicitModules.Length; i++)
		{
			MetadataReference item = explicitReferences[immutableArray[i]];
			int length = explicitModules[i].ReferencedAssemblies.Length;
			result.Add((item, new ArraySegment<AssemblyReferenceBinding>(array, num, length)));
			num += length;
		}
		for (int j = 0; j < explicitReferenceMap.Length; j++)
		{
			ResolvedReference resolvedReference = explicitReferenceMap[j];
			if (!resolvedReference.IsSkipped && resolvedReference.Kind != MetadataImageKind.Module)
			{
				result.Add((explicitReferences[j], new ArraySegment<AssemblyReferenceBinding>(referenceBindings[resolvedReference.Index + 1])));
			}
		}
	}

	private static ImmutableArray<int> CalculateModuleToReferenceMap(ImmutableArray<PEModule> modules, ImmutableArray<ResolvedReference> resolvedReferences)
	{
		if (modules.Length == 0)
		{
			return ImmutableArray<int>.Empty;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(modules.Length);
		instance.ZeroInit(modules.Length);
		for (int i = 0; i < resolvedReferences.Length; i++)
		{
			ResolvedReference resolvedReference = resolvedReferences[i];
			if (!resolvedReference.IsSkipped && resolvedReference.Kind == MetadataImageKind.Module)
			{
				instance[resolvedReference.Index] = i;
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<ResolvedReference> ToResolvedAssemblyReferences(ImmutableArray<MetadataReference> references, Dictionary<MetadataReference, MergedAliases> propertyMapOpt, int explicitAssemblyCount)
	{
		ArrayBuilder<ResolvedReference> instance = ArrayBuilder<ResolvedReference>.GetInstance(references.Length);
		for (int i = 0; i < references.Length; i++)
		{
			instance.Add(GetResolvedReferenceAndFreePropertyMapEntry(references[i], explicitAssemblyCount - 1 + i, MetadataImageKind.Assembly, propertyMapOpt));
		}
		return instance.ToImmutableAndFree();
	}

	private static void UpdateBindingsOfAssemblyBeingBuilt(ArrayBuilder<AssemblyReferenceBinding[]> referenceBindings, int explicitAssemblyCount, ArrayBuilder<AssemblyData> implicitAssemblies)
	{
		AssemblyReferenceBinding[] array = referenceBindings[0];
		ArrayBuilder<AssemblyReferenceBinding> instance = ArrayBuilder<AssemblyReferenceBinding>.GetInstance(array.Length + implicitAssemblies.Count);
		instance.AddRange(array, explicitAssemblyCount - 1);
		for (int i = 0; i < implicitAssemblies.Count; i++)
		{
			instance.Add(new AssemblyReferenceBinding(implicitAssemblies[i].Identity, explicitAssemblyCount + i));
		}
		instance.AddRange(array, explicitAssemblyCount - 1, array.Length - explicitAssemblyCount + 1);
		referenceBindings[0] = instance.ToArrayAndFree();
	}

	internal AssemblyData ResolveMissingAssembly(AssemblyIdentity referenceIdentity, PortableExecutableReference peReference, MetadataImportOptions importOptions, DiagnosticBag diagnostics)
	{
		Metadata metadata = GetMetadata(peReference, MessageProvider, Location.None, diagnostics);
		if (metadata == null)
		{
			return null;
		}
		if (!(metadata is AssemblyMetadata assemblyMetadata) || !assemblyMetadata.IsValidAssembly())
		{
			diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotAssembly, Location.None, peReference.Display));
			return null;
		}
		PEAssembly assembly = assemblyMetadata.GetAssembly();
		if (IdentityComparer.Compare(referenceIdentity, assembly.Identity) == AssemblyIdentityComparer.ComparisonResult.NotEquivalent)
		{
			return null;
		}
		return CreateAssemblyDataForFile(assembly, assemblyMetadata.CachedSymbols, peReference.DocumentationProvider, SimpleAssemblyName, importOptions, peReference.Properties.EmbedInteropTypes);
	}

	private bool ReuseAssemblySymbolsWithNoPiaLocalTypes(BoundInputAssembly[] boundInputs, TAssemblySymbol[] candidateInputAssemblySymbols, ImmutableArray<AssemblyData> assemblies, int corLibraryIndex)
	{
		int length = assemblies.Length;
		for (int i = 1; i < length; i++)
		{
			if (!assemblies[i].ContainsNoPiaLocalTypes)
			{
				continue;
			}
			foreach (TAssemblySymbol availableSymbol in assemblies[i].AvailableSymbols)
			{
				if (IsLinked(availableSymbol) != assemblies[i].IsLinked)
				{
					continue;
				}
				ImmutableArray<TAssemblySymbol> noPiaResolutionAssemblies = GetNoPiaResolutionAssemblies(availableSymbol);
				if (noPiaResolutionAssemblies.IsDefault)
				{
					continue;
				}
				Array.Clear(candidateInputAssemblySymbols, 0, candidateInputAssemblySymbols.Length);
				bool flag = true;
				foreach (TAssemblySymbol item in noPiaResolutionAssemblies)
				{
					flag = false;
					for (int j = 1; j < length; j++)
					{
						if (assemblies[j].IsMatchingAssembly(item) && IsLinked(item) == assemblies[j].IsLinked)
						{
							candidateInputAssemblySymbols[j] = item;
							flag = true;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				for (int k = 1; k < length; k++)
				{
					if (candidateInputAssemblySymbols[k] == null)
					{
						flag = false;
						break;
					}
					if (corLibraryIndex < 0)
					{
						if (GetCorLibrary(candidateInputAssemblySymbols[k]) != null)
						{
							flag = false;
							break;
						}
					}
					else if (candidateInputAssemblySymbols[corLibraryIndex] != GetCorLibrary(candidateInputAssemblySymbols[k]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					for (int l = 1; l < length; l++)
					{
						boundInputs[l].AssemblySymbol = candidateInputAssemblySymbols[l];
					}
					return true;
				}
			}
			Array.Clear(candidateInputAssemblySymbols, 0, candidateInputAssemblySymbols.Length);
			break;
		}
		return false;
	}

	private void ReuseAssemblySymbols(BoundInputAssembly[] boundInputs, TAssemblySymbol[] candidateInputAssemblySymbols, ImmutableArray<AssemblyData> assemblies, int corLibraryIndex)
	{
		Queue<AssemblyReferenceCandidate> queue = new Queue<AssemblyReferenceCandidate>();
		int length = assemblies.Length;
		for (int i = 1; i < length; i++)
		{
			if (boundInputs[i].AssemblySymbol != null || assemblies[i].ContainsNoPiaLocalTypes)
			{
				continue;
			}
			foreach (TAssemblySymbol availableSymbol in assemblies[i].AvailableSymbols)
			{
				bool flag = true;
				Array.Clear(candidateInputAssemblySymbols, 0, candidateInputAssemblySymbols.Length);
				queue.Clear();
				queue.Enqueue(new AssemblyReferenceCandidate(i, availableSymbol));
				while (flag && queue.Count > 0)
				{
					AssemblyReferenceCandidate assemblyReferenceCandidate = queue.Dequeue();
					int definitionIndex = assemblyReferenceCandidate.DefinitionIndex;
					TAssemblySymbol val = boundInputs[definitionIndex].AssemblySymbol;
					if (val == null)
					{
						val = candidateInputAssemblySymbols[definitionIndex];
					}
					if (val != null)
					{
						if (val != assemblyReferenceCandidate.AssemblySymbol)
						{
							flag = false;
							break;
						}
						continue;
					}
					if (IsLinked(assemblyReferenceCandidate.AssemblySymbol) != assemblies[definitionIndex].IsLinked)
					{
						flag = false;
						break;
					}
					candidateInputAssemblySymbols[definitionIndex] = assemblyReferenceCandidate.AssemblySymbol;
					AssemblyReferenceBinding[] referenceBinding = boundInputs[definitionIndex].ReferenceBinding;
					TAssemblySymbol[] actualBoundReferencesUsedBy = GetActualBoundReferencesUsedBy(assemblyReferenceCandidate.AssemblySymbol);
					int num = actualBoundReferencesUsedBy.Length;
					for (int j = 0; j < num; j++)
					{
						if (!referenceBinding[j].IsBound)
						{
							if (actualBoundReferencesUsedBy[j] != null)
							{
								flag = false;
								break;
							}
							continue;
						}
						if (actualBoundReferencesUsedBy[j] == null)
						{
							flag = false;
							break;
						}
						int definitionIndex2 = referenceBinding[j].DefinitionIndex;
						if (definitionIndex2 == 0)
						{
							flag = false;
							break;
						}
						if (!assemblies[definitionIndex2].IsMatchingAssembly(actualBoundReferencesUsedBy[j]))
						{
							flag = false;
							break;
						}
						if (assemblies[definitionIndex2].ContainsNoPiaLocalTypes)
						{
							flag = false;
							break;
						}
						if (IsLinked(actualBoundReferencesUsedBy[j]) != assemblies[definitionIndex2].IsLinked)
						{
							flag = false;
							break;
						}
						queue.Enqueue(new AssemblyReferenceCandidate(definitionIndex2, actualBoundReferencesUsedBy[j]));
					}
					if (!flag)
					{
						continue;
					}
					TAssemblySymbol corLibrary = GetCorLibrary(assemblyReferenceCandidate.AssemblySymbol);
					if (corLibrary == null)
					{
						if (corLibraryIndex >= 0)
						{
							flag = false;
							break;
						}
						continue;
					}
					if (corLibraryIndex < 0)
					{
						flag = false;
						break;
					}
					if (!assemblies[corLibraryIndex].IsMatchingAssembly(corLibrary))
					{
						flag = false;
						break;
					}
					queue.Enqueue(new AssemblyReferenceCandidate(corLibraryIndex, corLibrary));
				}
				if (!flag)
				{
					continue;
				}
				for (int k = 0; k < length; k++)
				{
					if (candidateInputAssemblySymbols[k] != null)
					{
						boundInputs[k].AssemblySymbol = candidateInputAssemblySymbols[k];
					}
				}
				break;
			}
		}
	}

	private static bool CheckCircularReference(IReadOnlyList<AssemblyReferenceBinding[]> referenceBindings)
	{
		for (int i = 1; i < referenceBindings.Count; i++)
		{
			AssemblyReferenceBinding[] array = referenceBindings[i];
			foreach (AssemblyReferenceBinding assemblyReferenceBinding in array)
			{
				if (assemblyReferenceBinding.BoundToAssemblyBeingBuilt)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsSuperseded(AssemblyIdentity identity, IReadOnlyDictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName)
	{
		return assemblyReferencesBySimpleName[identity.Name][0].Identity.Version != identity.Version;
	}

	private static int IndexOfCorLibrary(ImmutableArray<AssemblyData> assemblies, IReadOnlyDictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName, bool supersedeLowerVersions)
	{
		ArrayBuilder<int> arrayBuilder = null;
		for (int i = 1; i < assemblies.Length; i++)
		{
			AssemblyData assemblyData = assemblies[i];
			if (!assemblyData.IsLinked && assemblyData.AssemblyReferences.Length == 0 && !assemblyData.ContainsNoPiaLocalTypes && (!supersedeLowerVersions || !IsSuperseded(assemblyData.Identity, assemblyReferencesBySimpleName)) && assemblyData.DeclaresTheObjectClass)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<int>.GetInstance();
				}
				arrayBuilder.Add(i);
			}
		}
		if (arrayBuilder != null)
		{
			if (arrayBuilder.Count == 1)
			{
				int result = arrayBuilder[0];
				arrayBuilder.Free();
				return result;
			}
			arrayBuilder.Free();
		}
		if (assemblies.Length == 1 && assemblies[0].AssemblyReferences.Length == 0)
		{
			return 0;
		}
		return -1;
	}

	internal static bool InternalsMayBeVisibleToAssemblyBeingCompiled(string compilationName, PEAssembly assembly)
	{
		return !assembly.GetInternalsVisibleToPublicKeys(compilationName).IsEmpty();
	}

	protected abstract TAssemblySymbol[] GetActualBoundReferencesUsedBy(TAssemblySymbol assemblySymbol);

	protected abstract ImmutableArray<TAssemblySymbol> GetNoPiaResolutionAssemblies(TAssemblySymbol candidateAssembly);

	protected abstract bool IsLinked(TAssemblySymbol candidateAssembly);

	protected abstract TAssemblySymbol GetCorLibrary(TAssemblySymbol candidateAssembly);

	protected abstract AssemblyData CreateAssemblyDataForFile(PEAssembly assembly, WeakList<IAssemblySymbol> cachedSymbols, DocumentationProvider documentationProvider, string sourceAssemblySimpleName, MetadataImportOptions importOptions, bool embedInteropTypes);

	protected abstract AssemblyData CreateAssemblyDataForCompilation(CompilationReference compilationReference);

	protected abstract bool CheckPropertiesConsistency(MetadataReference primaryReference, MetadataReference duplicateReference, DiagnosticBag diagnostics);

	protected abstract bool WeakIdentityPropertiesEquivalent(AssemblyIdentity identity1, AssemblyIdentity identity2);

	protected ImmutableArray<ResolvedReference> ResolveMetadataReferences(TCompilation compilation, [Out] Dictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName, out ImmutableArray<MetadataReference> references, out IDictionary<(string, string), MetadataReference> boundReferenceDirectiveMap, out ImmutableArray<MetadataReference> boundReferenceDirectives, out ImmutableArray<AssemblyData> assemblies, out ImmutableArray<PEModule> modules, DiagnosticBag diagnostics)
	{
		GetCompilationReferences(compilation, diagnostics, out references, out boundReferenceDirectiveMap, out var referenceDirectiveLocations);
		int length = references.Length;
		int num = ((referenceDirectiveLocations != null) ? referenceDirectiveLocations.Length : 0);
		ResolvedReference[] array = new ResolvedReference[length];
		Dictionary<MetadataReference, MergedAliases> lazyAliasMap = null;
		Dictionary<MetadataReference, MetadataReference> dictionary = new Dictionary<MetadataReference, MetadataReference>(MetadataReferenceEqualityComparer.Instance);
		ArrayBuilder<MetadataReference> arrayBuilder = ((referenceDirectiveLocations != null) ? ArrayBuilder<MetadataReference>.GetInstance() : null);
		ArrayBuilder<AssemblyData> instance = ArrayBuilder<AssemblyData>.GetInstance();
		ArrayBuilder<PEModule> modules2 = null;
		bool referencesSupersedeLowerVersions = compilation.Options.ReferencesSupersedeLowerVersions;
		for (int num2 = length - 1; num2 >= 0; num2--)
		{
			MetadataReference metadataReference = references[num2];
			if (metadataReference != null)
			{
				if (dictionary.TryGetValue(metadataReference, out var value))
				{
					if (metadataReference != value)
					{
						MergeReferenceProperties(value, metadataReference, diagnostics, ref lazyAliasMap);
					}
				}
				else
				{
					dictionary.Add(metadataReference, metadataReference);
					Location location;
					if (num2 < num)
					{
						location = referenceDirectiveLocations[num2];
						arrayBuilder.Add(metadataReference);
					}
					else
					{
						location = Location.None;
					}
					if (metadataReference is CompilationReference { Properties: var properties } compilationReference)
					{
						if (properties.Kind != MetadataImageKind.Assembly)
						{
							throw ExceptionUtilities.UnexpectedValue(compilationReference.Properties.Kind);
						}
						value = TryAddAssembly(compilationReference.Compilation.Assembly.Identity, metadataReference, -instance.Count - 1, diagnostics, location, assemblyReferencesBySimpleName, referencesSupersedeLowerVersions);
						if (value != null)
						{
							MergeReferenceProperties(value, metadataReference, diagnostics, ref lazyAliasMap);
						}
						else
						{
							AddAssembly(CreateAssemblyDataForCompilation(compilationReference), num2, array, instance);
						}
					}
					else
					{
						PortableExecutableReference portableExecutableReference = (PortableExecutableReference)metadataReference;
						Metadata metadata = GetMetadata(portableExecutableReference, MessageProvider, location, diagnostics);
						if (metadata != null)
						{
							switch (portableExecutableReference.Properties.Kind)
							{
							case MetadataImageKind.Assembly:
							{
								AssemblyMetadata assemblyMetadata = (AssemblyMetadata)metadata;
								WeakList<IAssemblySymbol> cachedSymbols = assemblyMetadata.CachedSymbols;
								if (assemblyMetadata.IsValidAssembly())
								{
									PEAssembly assembly = assemblyMetadata.GetAssembly();
									value = TryAddAssembly(assembly.Identity, portableExecutableReference, -instance.Count - 1, diagnostics, location, assemblyReferencesBySimpleName, referencesSupersedeLowerVersions);
									if (value != null)
									{
										MergeReferenceProperties(value, metadataReference, diagnostics, ref lazyAliasMap);
										break;
									}
									AddAssembly(CreateAssemblyDataForFile(assembly, cachedSymbols, portableExecutableReference.DocumentationProvider, SimpleAssemblyName, compilation.Options.MetadataImportOptions, portableExecutableReference.Properties.EmbedInteropTypes), num2, array, instance);
								}
								else
								{
									diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotAssembly, location, portableExecutableReference.Display));
								}
								GC.KeepAlive(assemblyMetadata);
								break;
							}
							case MetadataImageKind.Module:
							{
								ModuleMetadata moduleMetadata = (ModuleMetadata)metadata;
								if (moduleMetadata.Module.IsLinkedModule)
								{
									if (!moduleMetadata.Module.IsEntireImageAvailable)
									{
										diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_LinkedNetmoduleMetadataMustProvideFullPEImage, location, portableExecutableReference.Display));
									}
									AddModule(moduleMetadata.Module, num2, array, ref modules2);
								}
								else
								{
									diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotModule, location, portableExecutableReference.Display));
								}
								break;
							}
							default:
								throw ExceptionUtilities.UnexpectedValue(portableExecutableReference.Properties.Kind);
							}
						}
					}
				}
			}
		}
		if (arrayBuilder != null)
		{
			arrayBuilder.ReverseContents();
			boundReferenceDirectives = arrayBuilder.ToImmutableAndFree();
		}
		else
		{
			boundReferenceDirectives = ImmutableArray<MetadataReference>.Empty;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsSkipped)
			{
				int index = ((array[i].Kind == MetadataImageKind.Assembly) ? instance.Count : (modules2?.Count ?? 0)) - 1 - array[i].Index;
				array[i] = GetResolvedReferenceAndFreePropertyMapEntry(references[i], index, array[i].Kind, lazyAliasMap);
			}
		}
		instance.ReverseContents();
		assemblies = instance.ToImmutableAndFree();
		if (modules2 == null)
		{
			modules = ImmutableArray<PEModule>.Empty;
		}
		else
		{
			modules2.ReverseContents();
			modules = modules2.ToImmutableAndFree();
		}
		return ImmutableArray.CreateRange(array);
	}

	private static ResolvedReference GetResolvedReferenceAndFreePropertyMapEntry(MetadataReference reference, int index, MetadataImageKind kind, Dictionary<MetadataReference, MergedAliases> propertyMapOpt)
	{
		ImmutableArray<string> aliasesOpt;
		ImmutableArray<string> recursiveAliasesOpt;
		if (propertyMapOpt != null && propertyMapOpt.TryGetValue(reference, out var value))
		{
			aliasesOpt = value.AliasesOpt?.ToImmutableAndFree() ?? default(ImmutableArray<string>);
			recursiveAliasesOpt = value.RecursiveAliasesOpt?.ToImmutableAndFree() ?? default(ImmutableArray<string>);
		}
		else if (reference.Properties.HasRecursiveAliases)
		{
			aliasesOpt = default;
			recursiveAliasesOpt = reference.Properties.Aliases;
		}
		else
		{
			aliasesOpt = reference.Properties.Aliases;
			recursiveAliasesOpt = default;
		}
		return new ResolvedReference(index, kind, aliasesOpt, recursiveAliasesOpt);
	}

	private Metadata GetMetadata(PortableExecutableReference peReference, CommonMessageProvider messageProvider, Location location, DiagnosticBag diagnostics)
	{
		Metadata metadata;
		lock (ObservedMetadata)
		{
			if (TryGetObservedMetadata(peReference, diagnostics, out metadata))
			{
				return metadata;
			}
		}
		Diagnostic diagnostic = null;
		Metadata metadata2;
		try
		{
			metadata2 = peReference.GetMetadataNoCopy();
			if (metadata2 is AssemblyMetadata assemblyMetadata)
			{
				assemblyMetadata.IsValidAssembly();
			}
			else
			{
				_ = ((ModuleMetadata)metadata2).Module.IsLinkedModule;
			}
		}
		catch (Exception ex) when (ex is BadImageFormatException || ex is IOException)
		{
			diagnostic = PortableExecutableReference.ExceptionToDiagnostic(ex, messageProvider, location, peReference.Display, peReference.Properties.Kind);
			metadata2 = null;
		}
		lock (ObservedMetadata)
		{
			if (TryGetObservedMetadata(peReference, diagnostics, out metadata))
			{
				return metadata;
			}
			if (diagnostic != null)
			{
				diagnostics.Add(diagnostic);
			}
			ObservedMetadata.Add(peReference, ((object)metadata2) ?? ((object)diagnostic));
			return metadata2;
		}
	}

	private bool TryGetObservedMetadata(PortableExecutableReference peReference, DiagnosticBag diagnostics, out Metadata metadata)
	{
		if (ObservedMetadata.TryGetValue(peReference, out var value))
		{
			metadata = value as Metadata;
			if (metadata == null)
			{
				diagnostics.Add((Diagnostic)value);
			}
			return true;
		}
		metadata = null;
		return false;
	}

	private void MergeReferenceProperties(MetadataReference primaryReference, MetadataReference newReference, DiagnosticBag diagnostics, ref Dictionary<MetadataReference, MergedAliases> lazyAliasMap)
	{
		if (CheckPropertiesConsistency(newReference, primaryReference, diagnostics))
		{
			if (lazyAliasMap == null)
			{
				lazyAliasMap = new Dictionary<MetadataReference, MergedAliases>();
			}
			if (!lazyAliasMap.TryGetValue(primaryReference, out var value))
			{
				value = new MergedAliases();
				lazyAliasMap.Add(primaryReference, value);
				value.Merge(primaryReference);
			}
			value.Merge(newReference);
		}
	}

	private static void AddAssembly(AssemblyData data, int referenceIndex, ResolvedReference[] referenceMap, ArrayBuilder<AssemblyData> assemblies)
	{
		referenceMap[referenceIndex] = new ResolvedReference(assemblies.Count, MetadataImageKind.Assembly);
		assemblies.Add(data);
	}

	private static void AddModule(PEModule module, int referenceIndex, ResolvedReference[] referenceMap, ref ArrayBuilder<PEModule> modules)
	{
		if (modules == null)
		{
			modules = ArrayBuilder<PEModule>.GetInstance();
		}
		referenceMap[referenceIndex] = new ResolvedReference(modules.Count, MetadataImageKind.Module);
		modules.Add(module);
	}

	private MetadataReference TryAddAssembly(AssemblyIdentity identity, MetadataReference reference, int assemblyIndex, DiagnosticBag diagnostics, Location location, Dictionary<string, List<ReferencedAssemblyIdentity>> referencesBySimpleName, bool supersedeLowerVersions)
	{
		ReferencedAssemblyIdentity referencedAssemblyIdentity = new ReferencedAssemblyIdentity(identity, reference, assemblyIndex);
		if (!referencesBySimpleName.TryGetValue(identity.Name, out var value))
		{
			referencesBySimpleName.Add(identity.Name, new List<ReferencedAssemblyIdentity> { referencedAssemblyIdentity });
			return null;
		}
		if (supersedeLowerVersions)
		{
			foreach (ReferencedAssemblyIdentity item in value)
			{
				if (identity.Version == item.Identity.Version)
				{
					return item.Reference;
				}
			}
			if (value[0].Identity.Version > identity.Version)
			{
				value.Add(referencedAssemblyIdentity);
			}
			else
			{
				value.Add(value[0]);
				value[0] = referencedAssemblyIdentity;
			}
			return null;
		}
		ReferencedAssemblyIdentity referencedAssemblyIdentity2 = default;
		if (identity.IsStrongName)
		{
			foreach (ReferencedAssemblyIdentity item2 in value)
			{
				if (item2.Identity.IsStrongName && IdentityComparer.ReferenceMatchesDefinition(identity, item2.Identity) && IdentityComparer.ReferenceMatchesDefinition(item2.Identity, identity))
				{
					referencedAssemblyIdentity2 = item2;
					break;
				}
			}
		}
		else
		{
			foreach (ReferencedAssemblyIdentity item3 in value)
			{
				if (!item3.Identity.IsStrongName && WeakIdentityPropertiesEquivalent(identity, item3.Identity))
				{
					referencedAssemblyIdentity2 = item3;
					break;
				}
			}
		}
		if (referencedAssemblyIdentity2.Identity == null)
		{
			value.Add(referencedAssemblyIdentity);
			return null;
		}
		if (identity.IsStrongName)
		{
			if (identity != referencedAssemblyIdentity2.Identity)
			{
				MessageProvider.ReportDuplicateMetadataReferenceStrong(diagnostics, location, reference, identity, referencedAssemblyIdentity2.Reference, referencedAssemblyIdentity2.Identity);
			}
		}
		else if (identity != referencedAssemblyIdentity2.Identity)
		{
			MessageProvider.ReportDuplicateMetadataReferenceWeak(diagnostics, location, reference, identity, referencedAssemblyIdentity2.Reference, referencedAssemblyIdentity2.Identity);
		}
		return referencedAssemblyIdentity2.Reference;
	}

	protected void GetCompilationReferences(TCompilation compilation, DiagnosticBag diagnostics, out ImmutableArray<MetadataReference> references, out IDictionary<(string, string), MetadataReference> boundReferenceDirectives, out ImmutableArray<Location> referenceDirectiveLocations)
	{
		boundReferenceDirectives = null;
		ArrayBuilder<MetadataReference> instance = ArrayBuilder<MetadataReference>.GetInstance();
		ArrayBuilder<Location> arrayBuilder = null;
		try
		{
			foreach (ReferenceDirective referenceDirective in compilation.ReferenceDirectives)
			{
				if (compilation.Options.MetadataReferenceResolver == null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataReferencesNotSupported, referenceDirective.Location));
					break;
				}
				if (boundReferenceDirectives != null && boundReferenceDirectives.ContainsKey((referenceDirective.Location.SourceTree.FilePath, referenceDirective.File)))
				{
					continue;
				}
				MetadataReference metadataReference = ResolveReferenceDirective(referenceDirective.File, referenceDirective.Location, compilation);
				if (metadataReference == null)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_MetadataFileNotFound, referenceDirective.Location, referenceDirective.File));
					continue;
				}
				if (boundReferenceDirectives == null)
				{
					boundReferenceDirectives = new Dictionary<(string, string), MetadataReference>();
					arrayBuilder = ArrayBuilder<Location>.GetInstance();
				}
				instance.Add(metadataReference);
				arrayBuilder.Add(referenceDirective.Location);
				boundReferenceDirectives.Add((referenceDirective.Location.SourceTree.FilePath, referenceDirective.File), metadataReference);
			}
			instance.AddRange(compilation.ExternalReferences);
			Compilation compilation2 = compilation.ScriptCompilationInfo?.PreviousScriptCompilation;
			if (compilation2 != null)
			{
				instance.AddRange(compilation2.GetBoundReferenceManager().ExplicitReferences);
			}
			if (boundReferenceDirectives == null)
			{
				boundReferenceDirectives = SpecializedCollections.EmptyDictionary<(string, string), MetadataReference>();
			}
			references = instance.ToImmutable();
			referenceDirectiveLocations = arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<Location>.Empty;
		}
		finally
		{
			instance.Free();
		}
	}

	private static PortableExecutableReference ResolveReferenceDirective(string reference, Location location, TCompilation compilation)
	{
		SyntaxTree sourceTree = location.SourceTree;
		string baseFilePath = ((sourceTree != null && sourceTree.FilePath.Length > 0) ? sourceTree.FilePath : null);
		ImmutableArray<PortableExecutableReference> immutableArray = compilation.Options.MetadataReferenceResolver.ResolveReference(reference, baseFilePath, MetadataReferenceProperties.Assembly.WithRecursiveAliases(value: true));
		if (immutableArray.IsDefaultOrEmpty)
		{
			return null;
		}
		if (immutableArray.Length > 1)
		{
			throw new NotSupportedException();
		}
		return immutableArray[0];
	}

	internal static AssemblyReferenceBinding[] ResolveReferencedAssemblies(ImmutableArray<AssemblyIdentity> references, ImmutableArray<AssemblyData> definitions, int definitionStartIndex, AssemblyIdentityComparer assemblyIdentityComparer)
	{
		AssemblyReferenceBinding[] array = new AssemblyReferenceBinding[references.Length];
		for (int i = 0; i < references.Length; i++)
		{
			array[i] = ResolveReferencedAssembly(references[i], definitions, definitionStartIndex, assemblyIdentityComparer);
		}
		return array;
	}

	internal static AssemblyReferenceBinding ResolveReferencedAssembly(AssemblyIdentity reference, ImmutableArray<AssemblyData> definitions, int definitionStartIndex, AssemblyIdentityComparer assemblyIdentityComparer)
	{
		int num = -1;
		int num2 = -1;
		bool flag = definitionStartIndex == 0;
		definitionStartIndex = Math.Max(definitionStartIndex, 1);
		for (int i = definitionStartIndex; i < definitions.Length; i++)
		{
			AssemblyIdentity identity = definitions[i].Identity;
			switch (assemblyIdentityComparer.Compare(reference, identity))
			{
			case AssemblyIdentityComparer.ComparisonResult.Equivalent:
				return new AssemblyReferenceBinding(reference, i);
			case AssemblyIdentityComparer.ComparisonResult.EquivalentIgnoringVersion:
				if (reference.Version < identity.Version)
				{
					if (num == -1 || identity.Version < definitions[num].Identity.Version)
					{
						num = i;
					}
				}
				else if (num2 == -1 || identity.Version > definitions[num2].Identity.Version)
				{
					num2 = i;
				}
				break;
			default:
				throw ExceptionUtilities.Unreachable;
			case AssemblyIdentityComparer.ComparisonResult.NotEquivalent:
				break;
			}
		}
		if (num != -1)
		{
			return new AssemblyReferenceBinding(reference, num, 1);
		}
		if (num2 != -1)
		{
			return new AssemblyReferenceBinding(reference, num2, -1);
		}
		if (reference.IsWindowsComponent())
		{
			for (int j = definitionStartIndex; j < definitions.Length; j++)
			{
				if (definitions[j].Identity.IsWindowsRuntime())
				{
					return new AssemblyReferenceBinding(reference, j);
				}
			}
		}
		if (reference.ContentType == AssemblyContentType.WindowsRuntime)
		{
			for (int k = definitionStartIndex; k < definitions.Length; k++)
			{
				AssemblyIdentity identity2 = definitions[k].Identity;
				Compilation sourceCompilation = definitions[k].SourceCompilation;
				if (identity2.ContentType == AssemblyContentType.Default && sourceCompilation != null && sourceCompilation.Options.OutputKind == OutputKind.WindowsRuntimeMetadata && AssemblyIdentityComparer.SimpleNameComparer.Equals(reference.Name, identity2.Name) && reference.Version.Equals(identity2.Version) && reference.IsRetargetable == identity2.IsRetargetable && AssemblyIdentityComparer.CultureComparer.Equals(reference.CultureName, identity2.CultureName) && AssemblyIdentity.KeysEqual(reference, identity2))
				{
					return new AssemblyReferenceBinding(reference, k);
				}
			}
		}
		if (flag && AssemblyIdentityComparer.SimpleNameComparer.Equals(reference.Name, definitions[0].Identity.Name))
		{
			return new AssemblyReferenceBinding(reference, 0);
		}
		return new AssemblyReferenceBinding(reference);
	}

	public CommonReferenceManager(string simpleAssemblyName, AssemblyIdentityComparer identityComparer, Dictionary<MetadataReference, object> observedMetadata)
	{
		SimpleAssemblyName = simpleAssemblyName;
		IdentityComparer = identityComparer;
		ObservedMetadata = observedMetadata ?? new Dictionary<MetadataReference, object>();
	}

	[Conditional("DEBUG")]
	internal void AssertUnbound()
	{
	}

	[Conditional("DEBUG")]
	internal void AssertBound()
	{
	}

	[Conditional("DEBUG")]
	internal void AssertCanReuseForCompilation(TCompilation compilation)
	{
	}

	internal void InitializeNoLock(Dictionary<MetadataReference, int> referencedAssembliesMap, Dictionary<MetadataReference, int> referencedModulesMap, IDictionary<(string, string), MetadataReference> boundReferenceDirectiveMap, ImmutableArray<MetadataReference> directiveReferences, ImmutableArray<MetadataReference> explicitReferences, ImmutableArray<MetadataReference> implicitReferences, bool containsCircularReferences, ImmutableArray<Diagnostic> diagnostics, TAssemblySymbol corLibraryOpt, ImmutableArray<PEModule> referencedModules, ImmutableArray<ModuleReferences<TAssemblySymbol>> referencedModulesReferences, ImmutableArray<TAssemblySymbol> referencedAssemblies, ImmutableArray<ImmutableArray<string>> aliasesOfReferencedAssemblies, ImmutableArray<UnifiedAssembly<TAssemblySymbol>> unifiedAssemblies)
	{
		_lazyReferencedAssembliesMap = referencedAssembliesMap;
		_lazyReferencedModuleIndexMap = referencedModulesMap;
		_lazyDiagnostics = diagnostics;
		_lazyReferenceDirectiveMap = boundReferenceDirectiveMap;
		_lazyDirectiveReferences = directiveReferences;
		_lazyExplicitReferences = explicitReferences;
		_lazyImplicitReferences = implicitReferences;
		_lazyCorLibraryOpt = corLibraryOpt;
		_lazyReferencedModules = referencedModules;
		_lazyReferencedModulesReferences = referencedModulesReferences;
		_lazyReferencedAssemblies = referencedAssemblies;
		_lazyAliasesOfReferencedAssemblies = aliasesOfReferencedAssemblies;
		_lazyUnifiedAssemblies = unifiedAssemblies;
		_lazyHasCircularReference = containsCircularReferences.ToThreeState();
		Interlocked.Exchange(ref _isBound, 1);
	}

	protected static void BuildReferencedAssembliesAndModulesMaps(BoundInputAssembly[] bindingResult, ImmutableArray<MetadataReference> references, ImmutableArray<ResolvedReference> referenceMap, int referencedModuleCount, int explicitlyReferencedAssemblyCount, IReadOnlyDictionary<string, List<ReferencedAssemblyIdentity>> assemblyReferencesBySimpleName, bool supersedeLowerVersions, out Dictionary<MetadataReference, int> referencedAssembliesMap, out Dictionary<MetadataReference, int> referencedModulesMap, out ImmutableArray<ImmutableArray<string>> aliasesOfReferencedAssemblies)
	{
		referencedAssembliesMap = new Dictionary<MetadataReference, int>(referenceMap.Length);
		referencedModulesMap = new Dictionary<MetadataReference, int>(referencedModuleCount);
		ArrayBuilder<ImmutableArray<string>> instance = ArrayBuilder<ImmutableArray<string>>.GetInstance(referenceMap.Length - referencedModuleCount);
		bool flag = false;
		for (int i = 0; i < referenceMap.Length; i++)
		{
			if (!referenceMap[i].IsSkipped)
			{
				if (referenceMap[i].Kind == MetadataImageKind.Module)
				{
					int value = 1 + referenceMap[i].Index;
					referencedModulesMap.Add(references[i], value);
					continue;
				}
				int index = referenceMap[i].Index;
				referencedAssembliesMap.Add(references[i], index);
				instance.Add(referenceMap[i].AliasesOpt);
				flag |= !referenceMap[i].RecursiveAliasesOpt.IsDefault;
			}
		}
		if (flag)
		{
			PropagateRecursiveAliases(bindingResult, referenceMap, instance);
		}
		if (supersedeLowerVersions)
		{
			foreach (KeyValuePair<string, List<ReferencedAssemblyIdentity>> item in assemblyReferencesBySimpleName)
			{
				for (int j = 1; j < item.Value.Count; j++)
				{
					int assemblyIndex = item.Value[j].GetAssemblyIndex(explicitlyReferencedAssemblyCount);
					instance[assemblyIndex] = s_supersededAlias;
				}
			}
		}
		aliasesOfReferencedAssemblies = instance.ToImmutableAndFree();
	}

	internal static ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> GetAssemblyReferenceIdentityBaselineMap(ImmutableArray<TAssemblySymbol> symbols, ImmutableArray<AssemblyIdentity> originalIdentities)
	{
		ImmutableDictionary<AssemblyIdentity, AssemblyIdentity>.Builder builder = null;
		for (int i = 0; i < originalIdentities.Length; i++)
		{
			AssemblyIdentity identity = symbols[i].Identity;
			Version assemblyVersionPattern = symbols[i].AssemblyVersionPattern;
			AssemblyIdentity value = originalIdentities[i];
			if ((object)assemblyVersionPattern != null)
			{
				builder = builder ?? ImmutableDictionary.CreateBuilder<AssemblyIdentity, AssemblyIdentity>();
				AssemblyIdentity key = identity.WithVersion(assemblyVersionPattern);
				if (builder.ContainsKey(key))
				{
					throw new NotSupportedException(CodeAnalysisResources.CompilationReferencesAssembliesWithDifferentAutoGeneratedVersion);
				}
				builder.Add(key, value);
			}
		}
		return builder?.ToImmutable() ?? ImmutableDictionary<AssemblyIdentity, AssemblyIdentity>.Empty;
	}

	internal static bool CompareVersionPartsSpecifiedInSource(Version version, Version candidateVersion, TAssemblySymbol candidateSymbol)
	{
		if (version.Major != candidateVersion.Major || version.Minor != candidateVersion.Minor)
		{
			return false;
		}
		Version assemblyVersionPattern = candidateSymbol.AssemblyVersionPattern;
		if (((object)assemblyVersionPattern == null || assemblyVersionPattern.Build < 65535) && version.Build != candidateVersion.Build)
		{
			return false;
		}
		if ((object)assemblyVersionPattern == null && version.Revision != candidateVersion.Revision)
		{
			return false;
		}
		return true;
	}

	private static void PropagateRecursiveAliases(BoundInputAssembly[] bindingResult, ImmutableArray<ResolvedReference> referenceMap, ArrayBuilder<ImmutableArray<string>> aliasesOfReferencedAssembliesBuilder)
	{
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		BitVector bitVector = BitVector.Create(bindingResult.Length);
		foreach (ResolvedReference item in referenceMap)
		{
			if (item.IsSkipped || item.RecursiveAliasesOpt.IsDefault)
			{
				continue;
			}
			ImmutableArray<string> recursiveAliasesOpt = item.RecursiveAliasesOpt;
			bitVector.Clear();
			instance.Add(item.Index);
			while (instance.Count > 0)
			{
				int num = instance.Pop();
				bitVector[num] = true;
				aliasesOfReferencedAssembliesBuilder[num] = MergedAliases.Merge(aliasesOfReferencedAssembliesBuilder[num], recursiveAliasesOpt);
				AssemblyReferenceBinding[] referenceBinding = bindingResult[num + 1].ReferenceBinding;
				for (int i = 0; i < referenceBinding.Length; i++)
				{
					AssemblyReferenceBinding assemblyReferenceBinding = referenceBinding[i];
					if (assemblyReferenceBinding.IsBound)
					{
						int num2 = assemblyReferenceBinding.DefinitionIndex - 1;
						if (!bitVector[num2])
						{
							instance.Add(num2);
						}
					}
				}
			}
		}
		for (int j = 0; j < aliasesOfReferencedAssembliesBuilder.Count; j++)
		{
			if (aliasesOfReferencedAssembliesBuilder[j].IsDefault)
			{
				aliasesOfReferencedAssembliesBuilder[j] = ImmutableArray<string>.Empty;
			}
		}
		instance.Free();
	}

	internal sealed override IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>> GetReferencedAssemblies()
	{
		return ReferencedAssembliesMap.Select((KeyValuePair<MetadataReference, int> ra) => KeyValuePairUtil.Create(ra.Key, (IAssemblySymbol)ReferencedAssemblies[ra.Value]));
	}

	internal TAssemblySymbol GetReferencedAssemblySymbol(MetadataReference reference)
	{
		if (!ReferencedAssembliesMap.TryGetValue(reference, out var value))
		{
			return null;
		}
		return ReferencedAssemblies[value];
	}

	internal int GetReferencedModuleIndex(MetadataReference reference)
	{
		if (!ReferencedModuleIndexMap.TryGetValue(reference, out var value))
		{
			return -1;
		}
		return value;
	}

	internal override MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol)
	{
		foreach (KeyValuePair<MetadataReference, int> item in ReferencedAssembliesMap)
		{
			if (ReferencedAssemblies[item.Value] == assemblySymbol)
			{
				return item.Key;
			}
		}
		return null;
	}

	internal override IEnumerable<(IAssemblySymbol, ImmutableArray<string>)> GetReferencedAssemblyAliases()
	{
		for (int i = 0; i < ReferencedAssemblies.Length; i++)
		{
			yield return (ReferencedAssemblies[i], AliasesOfReferencedAssemblies[i]);
		}
	}

	public bool DeclarationsAccessibleWithoutAlias(int referencedAssemblyIndex)
	{
		ImmutableArray<string> immutableArray = AliasesOfReferencedAssemblies[referencedAssemblyIndex];
		if (immutableArray.Length != 0)
		{
			return immutableArray.IndexOf(MetadataReferenceProperties.GlobalAlias, StringComparer.Ordinal) >= 0;
		}
		return true;
	}

	internal override IEnumerable<KeyValuePair<AssemblyIdentity, PortableExecutableReference>> GetImplicitlyResolvedAssemblyReferences()
	{
		foreach (PortableExecutableReference implicitReference in ImplicitReferences)
		{
			yield return KeyValuePairUtil.Create(ReferencedAssemblies[ReferencedAssembliesMap[implicitReference]].Identity, implicitReference);
		}
	}
}
internal abstract class CommonReferenceManager
{
	internal static object SymbolCacheAndReferenceManagerStateGuard = new object();

	internal abstract ImmutableArray<MetadataReference> ExplicitReferences { get; }

	internal abstract ImmutableArray<MetadataReference> ImplicitReferences { get; }

	internal abstract IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>> GetReferencedAssemblies();

	internal abstract IEnumerable<(IAssemblySymbol, ImmutableArray<string>)> GetReferencedAssemblyAliases();

	internal abstract MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol);

	internal abstract IEnumerable<KeyValuePair<AssemblyIdentity, PortableExecutableReference>> GetImplicitlyResolvedAssemblyReferences();
}
