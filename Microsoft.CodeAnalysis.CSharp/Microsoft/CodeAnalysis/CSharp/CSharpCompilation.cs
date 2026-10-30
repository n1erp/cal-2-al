using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpCompilation : Compilation
{
	internal class EntryPoint
	{
		public readonly MethodSymbol MethodSymbol;

		public readonly ImmutableArray<Diagnostic> Diagnostics;

		public EntryPoint(MethodSymbol methodSymbol, ImmutableArray<Diagnostic> diagnostics)
		{
			MethodSymbol = methodSymbol;
			Diagnostics = diagnostics;
		}
	}

	private struct ImportInfo(SyntaxTree tree, SyntaxKind kind, TextSpan span) : IEquatable<ImportInfo>
	{
		public readonly SyntaxTree Tree = tree;

		public readonly SyntaxKind Kind = kind;

		public readonly TextSpan Span = span;

		public override bool Equals(object obj)
		{
			if (obj is ImportInfo)
			{
				return Equals((ImportInfo)obj);
			}
			return false;
		}

		public bool Equals(ImportInfo other)
		{
			if (other.Kind == Kind && other.Tree == Tree)
			{
				return other.Span == Span;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(Tree, Span.Start);
		}
	}

	private abstract class AbstractSymbolSearcher
	{
		private readonly PooledDictionary<Declaration, NamespaceOrTypeSymbol> _cache;

		private readonly CSharpCompilation _compilation;

		private readonly bool _includeNamespace;

		private readonly bool _includeType;

		private readonly bool _includeMember;

		private readonly CancellationToken _cancellationToken;

		protected AbstractSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, CancellationToken cancellationToken)
		{
			_cache = PooledDictionary<Declaration, NamespaceOrTypeSymbol>.GetInstance();
			_compilation = compilation;
			_includeNamespace = (filter & SymbolFilter.Namespace) == SymbolFilter.Namespace;
			_includeType = (filter & SymbolFilter.Type) == SymbolFilter.Type;
			_includeMember = (filter & SymbolFilter.Member) == SymbolFilter.Member;
			_cancellationToken = cancellationToken;
		}

		protected abstract bool Matches(string name);

		protected abstract bool ShouldCheckTypeForMembers(MergedTypeDeclaration current);

		public IEnumerable<ISymbol> GetSymbolsWithName()
		{
			HashSet<ISymbol> hashSet = new HashSet<ISymbol>();
			ArrayBuilder<MergedNamespaceOrTypeDeclaration> instance = ArrayBuilder<MergedNamespaceOrTypeDeclaration>.GetInstance();
			AppendSymbolsWithName(instance, _compilation.MergedRootDeclaration, hashSet);
			instance.Free();
			_cache.Free();
			return hashSet;
		}

		private void AppendSymbolsWithName(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine, MergedNamespaceOrTypeDeclaration current, HashSet<ISymbol> set)
		{
			if (current.Kind == DeclarationKind.Namespace)
			{
				if (_includeNamespace && Matches(current.Name))
				{
					NamespaceOrTypeSymbol spineSymbol = GetSpineSymbol(spine);
					NamespaceOrTypeSymbol symbol = GetSymbol(spineSymbol, current);
					if (symbol != null)
					{
						set.Add(symbol);
					}
				}
			}
			else
			{
				if (_includeType && Matches(current.Name))
				{
					NamespaceOrTypeSymbol spineSymbol2 = GetSpineSymbol(spine);
					NamespaceOrTypeSymbol symbol2 = GetSymbol(spineSymbol2, current);
					if (symbol2 != null)
					{
						set.Add(symbol2);
					}
				}
				if (_includeMember)
				{
					MergedTypeDeclaration current2 = (MergedTypeDeclaration)current;
					if (ShouldCheckTypeForMembers(current2))
					{
						AppendMemberSymbolsWithName(spine, current2, set);
					}
				}
			}
			spine.Add(current);
			foreach (Declaration child in current.Children)
			{
				if (child is MergedNamespaceOrTypeDeclaration current4 && (_includeMember || _includeType || child.Kind == DeclarationKind.Namespace))
				{
					AppendSymbolsWithName(spine, current4, set);
				}
			}
			spine.RemoveAt(spine.Count - 1);
		}

		private void AppendMemberSymbolsWithName(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine, MergedTypeDeclaration current, HashSet<ISymbol> set)
		{
			CancellationToken cancellationToken = _cancellationToken;
			cancellationToken.ThrowIfCancellationRequested();
			spine.Add(current);
			NamespaceOrTypeSymbol spineSymbol = GetSpineSymbol(spine);
			if (spineSymbol != null)
			{
				foreach (Symbol member in spineSymbol.GetMembers())
				{
					if (!member.IsTypeOrTypeAlias() && (member.CanBeReferencedByName || member.IsExplicitInterfaceImplementation() || member.IsIndexer()) && Matches(member.Name))
					{
						set.Add(member);
					}
				}
			}
			spine.RemoveAt(spine.Count - 1);
		}

		protected NamespaceOrTypeSymbol GetSpineSymbol(ArrayBuilder<MergedNamespaceOrTypeDeclaration> spine)
		{
			if (spine.Count == 0)
			{
				return null;
			}
			NamespaceOrTypeSymbol cachedSymbol = GetCachedSymbol(spine[spine.Count - 1]);
			if (cachedSymbol != null)
			{
				return cachedSymbol;
			}
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = _compilation.GlobalNamespace;
			for (int i = 1; i < spine.Count; i++)
			{
				namespaceOrTypeSymbol = GetSymbol(namespaceOrTypeSymbol, spine[i]);
			}
			return namespaceOrTypeSymbol;
		}

		private NamespaceOrTypeSymbol GetCachedSymbol(MergedNamespaceOrTypeDeclaration declaration)
		{
			if (!_cache.TryGetValue(declaration, out var value))
			{
				return null;
			}
			return value;
		}

		private NamespaceOrTypeSymbol GetSymbol(NamespaceOrTypeSymbol container, MergedNamespaceOrTypeDeclaration declaration)
		{
			if (container == null)
			{
				return _compilation.GlobalNamespace;
			}
			if (declaration.Kind == DeclarationKind.Namespace)
			{
				AddCache(container.GetMembers(declaration.Name).OfType<NamespaceOrTypeSymbol>());
			}
			else
			{
				AddCache(container.GetTypeMembers(declaration.Name));
			}
			return GetCachedSymbol(declaration);
		}

		private void AddCache(IEnumerable<NamespaceOrTypeSymbol> symbols)
		{
			foreach (NamespaceOrTypeSymbol symbol in symbols)
			{
				MergedNamespaceSymbol mergedNamespaceSymbol = symbol as MergedNamespaceSymbol;
				if (mergedNamespaceSymbol != null)
				{
					_cache[mergedNamespaceSymbol.ConstituentNamespaces.OfType<SourceNamespaceSymbol>().First().MergedDeclaration] = symbol;
					continue;
				}
				SourceNamespaceSymbol sourceNamespaceSymbol = symbol as SourceNamespaceSymbol;
				if (sourceNamespaceSymbol != null)
				{
					_cache[sourceNamespaceSymbol.MergedDeclaration] = sourceNamespaceSymbol;
					continue;
				}
				SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol = symbol as SourceMemberContainerTypeSymbol;
				if (sourceMemberContainerTypeSymbol != null)
				{
					_cache[sourceMemberContainerTypeSymbol.MergedDeclaration] = sourceMemberContainerTypeSymbol;
				}
			}
		}
	}

	private class PredicateSymbolSearcher : AbstractSymbolSearcher
	{
		private readonly Func<string, bool> _predicate;

		public PredicateSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, Func<string, bool> predicate, CancellationToken cancellationToken)
			: base(compilation, filter, cancellationToken)
		{
			_predicate = predicate;
		}

		protected override bool ShouldCheckTypeForMembers(MergedTypeDeclaration current)
		{
			return true;
		}

		protected override bool Matches(string name)
		{
			return _predicate(name);
		}
	}

	private class NameSymbolSearcher : AbstractSymbolSearcher
	{
		private readonly string _name;

		public NameSymbolSearcher(CSharpCompilation compilation, SymbolFilter filter, string name, CancellationToken cancellationToken)
			: base(compilation, filter, cancellationToken)
		{
			_name = name;
		}

		protected override bool ShouldCheckTypeForMembers(MergedTypeDeclaration current)
		{
			foreach (SingleTypeDeclaration declaration in current.Declarations)
			{
				if (declaration.MemberNames.Contains(_name))
				{
					return true;
				}
			}
			return false;
		}

		protected override bool Matches(string name)
		{
			return _name == name;
		}
	}

	internal static class TupleNamesEncoder
	{
		public static ImmutableArray<string> Encode(TypeSymbol type)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			if (!TryGetNames(type, instance))
			{
				instance.Free();
				return default;
			}
			return instance.ToImmutableAndFree();
		}

		public static ImmutableArray<TypedConstant> Encode(TypeSymbol type, TypeSymbol stringType)
		{
			ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
			if (!TryGetNames(type, instance))
			{
				instance.Free();
				return default;
			}
			ImmutableArray<TypedConstant> result = instance.SelectAsArray((string name, TypeSymbol constantType) => new TypedConstant(constantType, TypedConstantKind.Primitive, name), stringType);
			instance.Free();
			return result;
		}

		internal static bool TryGetNames(TypeSymbol type, ArrayBuilder<string> namesBuilder)
		{
			type.VisitType((TypeSymbol t, ArrayBuilder<string> builder, bool _ignore) => AddNames(t, builder), namesBuilder);
			return namesBuilder.Any((string name) => name != null);
		}

		private static bool AddNames(TypeSymbol type, ArrayBuilder<string> namesBuilder)
		{
			if (type.IsTupleType)
			{
				if (type.TupleElementNames.IsDefaultOrEmpty)
				{
					namesBuilder.AddMany(null, type.TupleElementTypes.Length);
				}
				else
				{
					namesBuilder.AddRange(type.TupleElementNames);
				}
			}
			return false;
		}
	}

	internal static class DynamicTransformsEncoder
	{
		internal static ImmutableArray<TypedConstant> Encode(TypeSymbol type, RefKind refKind, int customModifiersCount, TypeSymbol booleanType)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, customModifiersCount, refKind, instance, addCustomModifierFlags: true);
			ImmutableArray<TypedConstant> result = instance.SelectAsArray((bool flag, TypeSymbol constantType) => new TypedConstant(constantType, TypedConstantKind.Primitive, flag), booleanType);
			instance.Free();
			return result;
		}

		internal static ImmutableArray<bool> Encode(TypeSymbol type, RefKind refKind, int customModifiersCount)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, customModifiersCount, refKind, instance, addCustomModifierFlags: true);
			return instance.ToImmutableAndFree();
		}

		internal static ImmutableArray<bool> EncodeWithoutCustomModifierFlags(TypeSymbol type, RefKind refKind)
		{
			ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance();
			Encode(type, -1, refKind, instance, addCustomModifierFlags: false);
			return instance.ToImmutableAndFree();
		}

		internal static void Encode(TypeSymbol type, int customModifiersCount, RefKind refKind, ArrayBuilder<bool> transformFlagsBuilder, bool addCustomModifierFlags)
		{
			if (refKind != RefKind.None)
			{
				transformFlagsBuilder.Add(item: false);
			}
			if (addCustomModifierFlags)
			{
				HandleCustomModifiers(customModifiersCount, transformFlagsBuilder);
				type.VisitType((TypeSymbol typeSymbol, ArrayBuilder<bool> builder, bool isNested) => AddFlags(typeSymbol, builder, isNested, addCustomModifierFlags: true), transformFlagsBuilder);
			}
			else
			{
				type.VisitType((TypeSymbol typeSymbol, ArrayBuilder<bool> builder, bool isNested) => AddFlags(typeSymbol, builder, isNested, addCustomModifierFlags: false), transformFlagsBuilder);
			}
		}

		private static bool AddFlags(TypeSymbol type, ArrayBuilder<bool> transformFlagsBuilder, bool isNestedNamedType, bool addCustomModifierFlags)
		{
			switch (type.TypeKind)
			{
			case TypeKind.Dynamic:
				transformFlagsBuilder.Add(item: true);
				break;
			case TypeKind.Array:
				if (addCustomModifierFlags)
				{
					HandleCustomModifiers(((ArrayTypeSymbol)type).CustomModifiers.Length, transformFlagsBuilder);
				}
				transformFlagsBuilder.Add(item: false);
				break;
			case TypeKind.Pointer:
				if (addCustomModifierFlags)
				{
					HandleCustomModifiers(((PointerTypeSymbol)type).CustomModifiers.Length, transformFlagsBuilder);
				}
				transformFlagsBuilder.Add(item: false);
				break;
			default:
				if (!isNestedNamedType)
				{
					transformFlagsBuilder.Add(item: false);
				}
				break;
			}
			return false;
		}

		private static void HandleCustomModifiers(int customModifiersCount, ArrayBuilder<bool> transformFlagsBuilder)
		{
			for (int i = 0; i < customModifiersCount; i++)
			{
				transformFlagsBuilder.Add(item: false);
			}
		}
	}

	internal class SpecialMembersSignatureComparer : SignatureComparer<MethodSymbol, FieldSymbol, PropertySymbol, TypeSymbol, ParameterSymbol>
	{
		public static readonly SpecialMembersSignatureComparer Instance = new SpecialMembersSignatureComparer();

		protected SpecialMembersSignatureComparer()
		{
		}

		protected override TypeSymbol GetMDArrayElementType(TypeSymbol type)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return null;
			}
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			if (arrayTypeSymbol.IsSZArray)
			{
				return null;
			}
			return arrayTypeSymbol.ElementType;
		}

		protected override TypeSymbol GetFieldType(FieldSymbol field)
		{
			return field.Type;
		}

		protected override TypeSymbol GetPropertyType(PropertySymbol property)
		{
			return property.Type;
		}

		protected override TypeSymbol GetGenericTypeArgument(TypeSymbol type, int argumentIndex)
		{
			if (type.Kind != SymbolKind.NamedType)
			{
				return null;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			if (namedTypeSymbol.Arity <= argumentIndex)
			{
				return null;
			}
			if ((object)namedTypeSymbol.ContainingType != null)
			{
				return null;
			}
			return namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics[argumentIndex];
		}

		protected override TypeSymbol GetGenericTypeDefinition(TypeSymbol type)
		{
			if (type.Kind != SymbolKind.NamedType)
			{
				return null;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			if ((object)namedTypeSymbol.ContainingType != null)
			{
				return null;
			}
			if (namedTypeSymbol.Arity == 0)
			{
				return null;
			}
			return namedTypeSymbol.OriginalDefinition;
		}

		protected override ImmutableArray<ParameterSymbol> GetParameters(MethodSymbol method)
		{
			return method.Parameters;
		}

		protected override ImmutableArray<ParameterSymbol> GetParameters(PropertySymbol property)
		{
			return property.Parameters;
		}

		protected override TypeSymbol GetParamType(ParameterSymbol parameter)
		{
			return parameter.Type;
		}

		protected override TypeSymbol GetPointedToType(TypeSymbol type)
		{
			if (type.Kind != SymbolKind.PointerType)
			{
				return null;
			}
			return ((PointerTypeSymbol)type).PointedAtType;
		}

		protected override TypeSymbol GetReturnType(MethodSymbol method)
		{
			return method.ReturnType;
		}

		protected override TypeSymbol GetSZArrayElementType(TypeSymbol type)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return null;
			}
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			if (!arrayTypeSymbol.IsSZArray)
			{
				return null;
			}
			return arrayTypeSymbol.ElementType;
		}

		protected override bool IsByRefParam(ParameterSymbol parameter)
		{
			return parameter.RefKind != RefKind.None;
		}

		protected override bool IsByRefMethod(MethodSymbol method)
		{
			return method.RefKind != RefKind.None;
		}

		protected override bool IsGenericMethodTypeParam(TypeSymbol type, int paramPosition)
		{
			if (type.Kind != SymbolKind.TypeParameter)
			{
				return false;
			}
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)type;
			if (typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.Method)
			{
				return false;
			}
			return typeParameterSymbol.Ordinal == paramPosition;
		}

		protected override bool IsGenericTypeParam(TypeSymbol type, int paramPosition)
		{
			if (type.Kind != SymbolKind.TypeParameter)
			{
				return false;
			}
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)type;
			if (typeParameterSymbol.ContainingSymbol.Kind != SymbolKind.NamedType)
			{
				return false;
			}
			return typeParameterSymbol.Ordinal == paramPosition;
		}

		protected override bool MatchArrayRank(TypeSymbol type, int countOfDimensions)
		{
			if (type.Kind != SymbolKind.ArrayType)
			{
				return false;
			}
			return ((ArrayTypeSymbol)type).Rank == countOfDimensions;
		}

		protected override bool MatchTypeToTypeId(TypeSymbol type, int typeId)
		{
			return (int)type.SpecialType == typeId;
		}
	}

	internal sealed class WellKnownMembersSignatureComparer : SpecialMembersSignatureComparer
	{
		private readonly CSharpCompilation _compilation;

		public WellKnownMembersSignatureComparer(CSharpCompilation compilation)
		{
			_compilation = compilation;
		}

		protected override bool MatchTypeToTypeId(TypeSymbol type, int typeId)
		{
			if (((WellKnownType)typeId).IsWellKnownType())
			{
				return type == _compilation.GetWellKnownType((WellKnownType)typeId);
			}
			return base.MatchTypeToTypeId(type, typeId);
		}
	}

	internal sealed class ReferenceManager : CommonReferenceManager<CSharpCompilation, AssemblySymbol>
	{
		private abstract class AssemblyDataForMetadataOrCompilation : AssemblyData
		{
			private List<AssemblySymbol> _assemblies;

			private readonly AssemblyIdentity _identity;

			private readonly ImmutableArray<AssemblyIdentity> _referencedAssemblies;

			private readonly bool _embedInteropTypes;

			public override AssemblyIdentity Identity => _identity;

			public override IEnumerable<AssemblySymbol> AvailableSymbols
			{
				get
				{
					if (_assemblies == null)
					{
						_assemblies = new List<AssemblySymbol>();
						AddAvailableSymbols(_assemblies);
					}
					return _assemblies;
				}
			}

			public override ImmutableArray<AssemblyIdentity> AssemblyReferences => _referencedAssemblies;

			public sealed override bool IsLinked => _embedInteropTypes;

			protected AssemblyDataForMetadataOrCompilation(AssemblyIdentity identity, ImmutableArray<AssemblyIdentity> referencedAssemblies, bool embedInteropTypes)
			{
				_embedInteropTypes = embedInteropTypes;
				_identity = identity;
				_referencedAssemblies = referencedAssemblies;
			}

			internal abstract AssemblySymbol CreateAssemblySymbol();

			protected abstract void AddAvailableSymbols(List<AssemblySymbol> assemblies);

			public override AssemblyReferenceBinding[] BindAssemblyReferences(ImmutableArray<AssemblyData> assemblies, AssemblyIdentityComparer assemblyIdentityComparer)
			{
				return CommonReferenceManager<CSharpCompilation, AssemblySymbol>.ResolveReferencedAssemblies(_referencedAssemblies, assemblies, 0, assemblyIdentityComparer);
			}
		}

		private sealed class AssemblyDataForFile : AssemblyDataForMetadataOrCompilation
		{
			public readonly PEAssembly Assembly;

			public readonly WeakList<IAssemblySymbol> CachedSymbols;

			public readonly DocumentationProvider DocumentationProvider;

			private readonly MetadataImportOptions _compilationImportOptions;

			private readonly string _sourceAssemblySimpleName;

			private bool _internalsVisibleComputed;

			private bool _internalsPotentiallyVisibleToCompilation;

			internal bool InternalsMayBeVisibleToCompilation
			{
				get
				{
					if (!_internalsVisibleComputed)
					{
						_internalsPotentiallyVisibleToCompilation = CommonReferenceManager<CSharpCompilation, AssemblySymbol>.InternalsMayBeVisibleToAssemblyBeingCompiled(_sourceAssemblySimpleName, Assembly);
						_internalsVisibleComputed = true;
					}
					return _internalsPotentiallyVisibleToCompilation;
				}
			}

			internal MetadataImportOptions EffectiveImportOptions
			{
				get
				{
					if (InternalsMayBeVisibleToCompilation && _compilationImportOptions == MetadataImportOptions.Public)
					{
						return MetadataImportOptions.Internal;
					}
					return _compilationImportOptions;
				}
			}

			public override bool ContainsNoPiaLocalTypes => Assembly.ContainsNoPiaLocalTypes();

			public override bool DeclaresTheObjectClass => Assembly.DeclaresTheObjectClass;

			public override Compilation SourceCompilation => null;

			public AssemblyDataForFile(PEAssembly assembly, WeakList<IAssemblySymbol> cachedSymbols, bool embedInteropTypes, DocumentationProvider documentationProvider, string sourceAssemblySimpleName, MetadataImportOptions compilationImportOptions)
				: base(assembly.Identity, assembly.AssemblyReferences, embedInteropTypes)
			{
				CachedSymbols = cachedSymbols;
				Assembly = assembly;
				DocumentationProvider = documentationProvider;
				_compilationImportOptions = compilationImportOptions;
				_sourceAssemblySimpleName = sourceAssemblySimpleName;
			}

			internal override AssemblySymbol CreateAssemblySymbol()
			{
				return new PEAssemblySymbol(Assembly, DocumentationProvider, IsLinked, EffectiveImportOptions);
			}

			protected override void AddAvailableSymbols(List<AssemblySymbol> assemblies)
			{
				lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
				{
					foreach (IAssemblySymbol cachedSymbol in CachedSymbols)
					{
						PEAssemblySymbol pEAssemblySymbol = cachedSymbol as PEAssemblySymbol;
						if (IsMatchingAssembly(pEAssemblySymbol))
						{
							assemblies.Add(pEAssemblySymbol);
						}
					}
				}
			}

			public override bool IsMatchingAssembly(AssemblySymbol candidateAssembly)
			{
				return IsMatchingAssembly(candidateAssembly as PEAssemblySymbol);
			}

			private bool IsMatchingAssembly(PEAssemblySymbol peAssembly)
			{
				if ((object)peAssembly == null)
				{
					return false;
				}
				if (peAssembly.Assembly != Assembly)
				{
					return false;
				}
				if (EffectiveImportOptions != peAssembly.PrimaryModule.ImportOptions)
				{
					return false;
				}
				if (!peAssembly.DocumentationProvider.Equals(DocumentationProvider))
				{
					return false;
				}
				return true;
			}
		}

		private sealed class AssemblyDataForCompilation : AssemblyDataForMetadataOrCompilation
		{
			public readonly CSharpCompilation Compilation;

			public override bool ContainsNoPiaLocalTypes => Compilation.MightContainNoPiaLocalTypes();

			public override bool DeclaresTheObjectClass => Compilation.DeclaresTheObjectClass;

			public override Compilation SourceCompilation => Compilation;

			public AssemblyDataForCompilation(CSharpCompilation compilation, bool embedInteropTypes)
				: base(compilation.Assembly.Identity, GetReferencedAssemblies(compilation), embedInteropTypes)
			{
				Compilation = compilation;
			}

			private static ImmutableArray<AssemblyIdentity> GetReferencedAssemblies(CSharpCompilation compilation)
			{
				ArrayBuilder<AssemblyIdentity> instance = ArrayBuilder<AssemblyIdentity>.GetInstance();
				ImmutableArray<ModuleSymbol> modules = compilation.Assembly.Modules;
				ImmutableArray<AssemblyIdentity> referencedAssemblies = modules[0].GetReferencedAssemblies();
				ImmutableArray<AssemblySymbol> referencedAssemblySymbols = modules[0].GetReferencedAssemblySymbols();
				for (int i = 0; i < referencedAssemblies.Length; i++)
				{
					if (!referencedAssemblySymbols[i].IsLinked)
					{
						instance.Add(referencedAssemblies[i]);
					}
				}
				for (int j = 1; j < modules.Length; j++)
				{
					instance.AddRange(modules[j].GetReferencedAssemblies());
				}
				return instance.ToImmutableAndFree();
			}

			internal override AssemblySymbol CreateAssemblySymbol()
			{
				return new RetargetingAssemblySymbol(Compilation.SourceAssembly, IsLinked);
			}

			protected override void AddAvailableSymbols(List<AssemblySymbol> assemblies)
			{
				assemblies.Add(Compilation.Assembly);
				lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
				{
					Compilation.AddRetargetingAssemblySymbolsNoLock(assemblies);
				}
			}

			public override bool IsMatchingAssembly(AssemblySymbol candidateAssembly)
			{
				AssemblySymbol assemblySymbol = ((!(candidateAssembly is RetargetingAssemblySymbol retargetingAssemblySymbol)) ? (candidateAssembly as SourceAssemblySymbol) : retargetingAssemblySymbol.UnderlyingAssembly);
				return (object)assemblySymbol == Compilation.Assembly;
			}
		}

		protected override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

		public ReferenceManager(string simpleAssemblyName, AssemblyIdentityComparer identityComparer, Dictionary<MetadataReference, object> observedMetadata)
			: base(simpleAssemblyName, identityComparer, observedMetadata)
		{
		}

		protected override AssemblyData CreateAssemblyDataForFile(PEAssembly assembly, WeakList<IAssemblySymbol> cachedSymbols, DocumentationProvider documentationProvider, string sourceAssemblySimpleName, MetadataImportOptions importOptions, bool embedInteropTypes)
		{
			return new AssemblyDataForFile(assembly, cachedSymbols, embedInteropTypes, documentationProvider, sourceAssemblySimpleName, importOptions);
		}

		protected override AssemblyData CreateAssemblyDataForCompilation(CompilationReference compilationReference)
		{
			if (!(compilationReference is CSharpCompilationReference { Compilation: var compilation, Properties: var properties }))
			{
				throw new NotSupportedException(string.Format(CSharpResources.CantReferenceCompilationOf, compilationReference.GetType(), "C#"));
			}
			return new AssemblyDataForCompilation(compilation, properties.EmbedInteropTypes);
		}

		protected override bool CheckPropertiesConsistency(MetadataReference primaryReference, MetadataReference duplicateReference, DiagnosticBag diagnostics)
		{
			if (primaryReference.Properties.EmbedInteropTypes != duplicateReference.Properties.EmbedInteropTypes)
			{
				diagnostics.Add(ErrorCode.ERR_AssemblySpecifiedForLinkAndRef, NoLocation.Singleton, duplicateReference.Display, primaryReference.Display);
				return false;
			}
			return true;
		}

		protected override bool WeakIdentityPropertiesEquivalent(AssemblyIdentity identity1, AssemblyIdentity identity2)
		{
			return AssemblyIdentityComparer.CultureComparer.Equals(identity1.CultureName, identity2.CultureName);
		}

		protected override AssemblySymbol[] GetActualBoundReferencesUsedBy(AssemblySymbol assemblySymbol)
		{
			List<AssemblySymbol> list = new List<AssemblySymbol>();
			foreach (ModuleSymbol module in assemblySymbol.Modules)
			{
				list.AddRange(module.GetReferencedAssemblySymbols());
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsMissing)
				{
					list[i] = null;
				}
			}
			return list.ToArray();
		}

		protected override ImmutableArray<AssemblySymbol> GetNoPiaResolutionAssemblies(AssemblySymbol candidateAssembly)
		{
			if (candidateAssembly is SourceAssemblySymbol)
			{
				return ImmutableArray<AssemblySymbol>.Empty;
			}
			return candidateAssembly.GetNoPiaResolutionAssemblies();
		}

		protected override bool IsLinked(AssemblySymbol candidateAssembly)
		{
			return candidateAssembly.IsLinked;
		}

		protected override AssemblySymbol GetCorLibrary(AssemblySymbol candidateAssembly)
		{
			AssemblySymbol corLibrary = candidateAssembly.CorLibrary;
			if (!corLibrary.IsMissing)
			{
				return corLibrary;
			}
			return null;
		}

		public void CreateSourceAssemblyForCompilation(CSharpCompilation compilation)
		{
			if (IsBound || !CreateAndSetSourceAssemblyFullBind(compilation))
			{
				if (!HasCircularReference)
				{
					CreateAndSetSourceAssemblyReuseData(compilation);
				}
				else
				{
					new ReferenceManager(SimpleAssemblyName, IdentityComparer, ObservedMetadata).CreateAndSetSourceAssemblyFullBind(compilation);
				}
			}
		}

		public PEAssemblySymbol CreatePEAssemblyForAssemblyMetadata(AssemblyMetadata metadata, MetadataImportOptions importOptions, out ImmutableDictionary<AssemblyIdentity, AssemblyIdentity> assemblyReferenceIdentityMap)
		{
			AssemblyIdentityMap<AssemblySymbol> referencedAssembliesByIdentity = new AssemblyIdentityMap<AssemblySymbol>();
			foreach (AssemblySymbol referencedAssembly in ReferencedAssemblies)
			{
				referencedAssembliesByIdentity.Add(referencedAssembly.Identity, referencedAssembly);
			}
			PEAssembly assembly = metadata.GetAssembly();
			ImmutableArray<AssemblySymbol> immutableArray = assembly.AssemblyReferences.SelectAsArray(MapAssemblyIdentityToResolvedSymbol, referencedAssembliesByIdentity);
			assemblyReferenceIdentityMap = CommonReferenceManager<CSharpCompilation, AssemblySymbol>.GetAssemblyReferenceIdentityBaselineMap(immutableArray, assembly.AssemblyReferences);
			PEAssemblySymbol pEAssemblySymbol = new PEAssemblySymbol(assembly, DocumentationProvider.Default, isLinked: false, importOptions);
			ImmutableArray<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies = UnifiedAssemblies.WhereAsArray((UnifiedAssembly<AssemblySymbol> unified) => referencedAssembliesByIdentity.Contains(unified.OriginalReference, allowHigherVersion: false));
			InitializeAssemblyReuseData(pEAssemblySymbol, immutableArray, unifiedAssemblies);
			if (assembly.ContainsNoPiaLocalTypes())
			{
				pEAssemblySymbol.SetNoPiaResolutionAssemblies(ReferencedAssemblies);
			}
			return pEAssemblySymbol;
		}

		private static AssemblySymbol MapAssemblyIdentityToResolvedSymbol(AssemblyIdentity identity, AssemblyIdentityMap<AssemblySymbol> map)
		{
			if (map.TryGetValue(identity, out var value, CommonReferenceManager<CSharpCompilation, AssemblySymbol>.CompareVersionPartsSpecifiedInSource))
			{
				return value;
			}
			if (map.TryGetValue(identity, out value, (Version v1, Version v2, AssemblySymbol s) => true))
			{
				throw new NotSupportedException($"Changing the version of an assembly reference is not allowed during debugging: '{identity}' changed version to {value.Identity.Version}");
			}
			return new MissingAssemblySymbol(identity);
		}

		private void CreateAndSetSourceAssemblyReuseData(CSharpCompilation compilation)
		{
			string moduleName = compilation.MakeSourceModuleName();
			SourceAssemblySymbol sourceAssemblySymbol = new SourceAssemblySymbol(compilation, SimpleAssemblyName, moduleName, ReferencedModules);
			InitializeAssemblyReuseData(sourceAssemblySymbol, ReferencedAssemblies, UnifiedAssemblies);
			if ((object)compilation._lazyAssemblySymbol != null)
			{
				return;
			}
			lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
			{
				if ((object)compilation._lazyAssemblySymbol == null)
				{
					compilation._lazyAssemblySymbol = sourceAssemblySymbol;
				}
			}
		}

		private void InitializeAssemblyReuseData(AssemblySymbol assemblySymbol, ImmutableArray<AssemblySymbol> referencedAssemblies, ImmutableArray<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies)
		{
			assemblySymbol.SetCorLibrary(CorLibraryOpt ?? assemblySymbol);
			ModuleReferences<AssemblySymbol> moduleReferences = new ModuleReferences<AssemblySymbol>(referencedAssemblies.SelectAsArray((AssemblySymbol a) => a.Identity), referencedAssemblies, unifiedAssemblies);
			assemblySymbol.Modules[0].SetReferences(moduleReferences);
			ImmutableArray<ModuleSymbol> modules = assemblySymbol.Modules;
			ImmutableArray<ModuleReferences<AssemblySymbol>> referencedModulesReferences = ReferencedModulesReferences;
			for (int num = 1; num < modules.Length; num++)
			{
				modules[num].SetReferences(referencedModulesReferences[num - 1]);
			}
		}

		private bool CreateAndSetSourceAssemblyFullBind(CSharpCompilation compilation)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			PooledDictionary<string, List<ReferencedAssemblyIdentity>> instance2 = PooledDictionary<string, List<ReferencedAssemblyIdentity>>.GetInstance();
			bool referencesSupersedeLowerVersions = compilation.Options.ReferencesSupersedeLowerVersions;
			try
			{
				ImmutableArray<ResolvedReference> explicitReferenceMap = ResolveMetadataReferences(compilation, instance2, out var references, out var boundReferenceDirectiveMap, out var boundReferenceDirectives, out var assemblies, out var modules, instance);
				AssemblyDataForAssemblyBeingBuilt item = new AssemblyDataForAssemblyBeingBuilt(new AssemblyIdentity(noThrow: true, SimpleAssemblyName), assemblies, modules);
				ImmutableArray<AssemblyData> explicitAssemblies = assemblies.Insert(0, item);
				BoundInputAssembly[] array = Bind(compilation, explicitAssemblies, modules, references, explicitReferenceMap, compilation.Options.MetadataReferenceResolver, compilation.Options.MetadataImportOptions, referencesSupersedeLowerVersions, instance2, out var allAssemblies, out var implicitlyResolvedReferences, out var implicitlyResolvedReferenceMap, instance, out var hasCircularReference, out var corLibraryIndex);
				ImmutableArray<MetadataReference> references2 = references.AddRange(implicitlyResolvedReferences);
				explicitReferenceMap = explicitReferenceMap.AddRange(implicitlyResolvedReferenceMap);
				CommonReferenceManager<CSharpCompilation, AssemblySymbol>.BuildReferencedAssembliesAndModulesMaps(array, references2, explicitReferenceMap, modules.Length, assemblies.Length, instance2, referencesSupersedeLowerVersions, out var referencedAssembliesMap, out var referencedModulesMap, out var aliasesOfReferencedAssemblies);
				List<int> list = new List<int>();
				for (int i = 1; i < array.Length; i++)
				{
					if ((object)array[i].AssemblySymbol == null)
					{
						array[i].AssemblySymbol = ((AssemblyDataForMetadataOrCompilation)allAssemblies[i]).CreateAssemblySymbol();
						list.Add(i);
					}
				}
				SourceAssemblySymbol sourceAssemblySymbol = new SourceAssemblySymbol(compilation, SimpleAssemblyName, compilation.MakeSourceModuleName(), modules);
				AssemblySymbol assemblySymbol;
				if (corLibraryIndex != 0)
				{
					assemblySymbol = ((corLibraryIndex <= 0) ? MissingCorLibrarySymbol.Instance : array[corLibraryIndex].AssemblySymbol);
				}
				else
				{
					assemblySymbol = sourceAssemblySymbol;
				}
				sourceAssemblySymbol.SetCorLibrary(assemblySymbol);
				Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies = null;
				int totalReferencedAssemblyCount = allAssemblies.Length - 1;
				SetupReferencesForSourceAssembly(sourceAssemblySymbol, modules, totalReferencedAssemblyCount, array, ref missingAssemblies, out var moduleReferences);
				if (list.Count > 0)
				{
					if (hasCircularReference)
					{
						array[0].AssemblySymbol = sourceAssemblySymbol;
					}
					InitializeNewSymbols(list, sourceAssemblySymbol, allAssemblies, array, missingAssemblies);
				}
				if ((object)compilation._lazyAssemblySymbol == null)
				{
					lock (CommonReferenceManager.SymbolCacheAndReferenceManagerStateGuard)
					{
						if ((object)compilation._lazyAssemblySymbol == null)
						{
							if (IsBound)
							{
								return false;
							}
							UpdateSymbolCacheNoLock(list, allAssemblies, array);
							InitializeNoLock(referencedAssembliesMap, referencedModulesMap, boundReferenceDirectiveMap, boundReferenceDirectives, references, implicitlyResolvedReferences, hasCircularReference, instance.ToReadOnly(), ((object)assemblySymbol == sourceAssemblySymbol) ? null : assemblySymbol, modules, moduleReferences, sourceAssemblySymbol.SourceModule.GetReferencedAssemblySymbols(), aliasesOfReferencedAssemblies, sourceAssemblySymbol.SourceModule.GetUnifiedAssemblies());
							compilation._referenceManager = this;
							compilation._lazyAssemblySymbol = sourceAssemblySymbol;
						}
					}
				}
				return true;
			}
			finally
			{
				instance.Free();
				instance2.Free();
			}
		}

		private static void InitializeNewSymbols(List<int> newSymbols, SourceAssemblySymbol sourceAssembly, ImmutableArray<AssemblyData> assemblies, BoundInputAssembly[] bindingResult, Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies)
		{
			AssemblySymbol corLibrary = sourceAssembly.CorLibrary;
			foreach (int newSymbol in newSymbols)
			{
				if (assemblies[newSymbol] is AssemblyDataForCompilation)
				{
					SetupReferencesForRetargetingAssembly(bindingResult, newSymbol, ref missingAssemblies, sourceAssembly);
				}
				else
				{
					SetupReferencesForFileAssembly((AssemblyDataForFile)assemblies[newSymbol], bindingResult, newSymbol, ref missingAssemblies, sourceAssembly);
				}
			}
			ArrayBuilder<AssemblySymbol> instance = ArrayBuilder<AssemblySymbol>.GetInstance();
			ImmutableArray<AssemblySymbol> referencedAssemblySymbols = sourceAssembly.Modules[0].GetReferencedAssemblySymbols();
			foreach (int newSymbol2 in newSymbols)
			{
				if (assemblies[newSymbol2].ContainsNoPiaLocalTypes)
				{
					bindingResult[newSymbol2].AssemblySymbol.SetNoPiaResolutionAssemblies(referencedAssemblySymbols);
				}
				instance.Clear();
				if (assemblies[newSymbol2].IsLinked)
				{
					instance.Add(bindingResult[newSymbol2].AssemblySymbol);
				}
				AssemblyReferenceBinding[] referenceBinding = bindingResult[newSymbol2].ReferenceBinding;
				for (int i = 0; i < referenceBinding.Length; i++)
				{
					AssemblyReferenceBinding assemblyReferenceBinding = referenceBinding[i];
					if (assemblyReferenceBinding.IsBound && assemblies[assemblyReferenceBinding.DefinitionIndex].IsLinked)
					{
						instance.Add(bindingResult[assemblyReferenceBinding.DefinitionIndex].AssemblySymbol);
					}
				}
				if (instance.Count > 0)
				{
					instance.RemoveDuplicates();
					bindingResult[newSymbol2].AssemblySymbol.SetLinkedReferencedAssemblies(instance.ToImmutable());
				}
				bindingResult[newSymbol2].AssemblySymbol.SetCorLibrary(corLibrary);
			}
			instance.Free();
			if (missingAssemblies == null)
			{
				return;
			}
			foreach (MissingAssemblySymbol value in missingAssemblies.Values)
			{
				value.SetCorLibrary(corLibrary);
			}
		}

		private static void UpdateSymbolCacheNoLock(List<int> newSymbols, ImmutableArray<AssemblyData> assemblies, BoundInputAssembly[] bindingResult)
		{
			foreach (int newSymbol in newSymbols)
			{
				if (assemblies[newSymbol] is AssemblyDataForCompilation assemblyDataForCompilation)
				{
					assemblyDataForCompilation.Compilation.CacheRetargetingAssemblySymbolNoLock(bindingResult[newSymbol].AssemblySymbol);
				}
				else
				{
					((AssemblyDataForFile)assemblies[newSymbol]).CachedSymbols.Add((PEAssemblySymbol)bindingResult[newSymbol].AssemblySymbol);
				}
			}
		}

		private static void SetupReferencesForRetargetingAssembly(BoundInputAssembly[] bindingResult, int bindingIndex, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies, SourceAssemblySymbol sourceAssemblyDebugOnly)
		{
			RetargetingAssemblySymbol retargetingAssemblySymbol = (RetargetingAssemblySymbol)bindingResult[bindingIndex].AssemblySymbol;
			ImmutableArray<ModuleSymbol> modules = retargetingAssemblySymbol.Modules;
			int length = modules.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				ImmutableArray<AssemblyIdentity> identities = retargetingAssemblySymbol.UnderlyingAssembly.Modules[i].GetReferencedAssemblies();
				if (i == 0)
				{
					ImmutableArray<AssemblySymbol> referencedAssemblySymbols = retargetingAssemblySymbol.UnderlyingAssembly.Modules[0].GetReferencedAssemblySymbols();
					int num2 = 0;
					foreach (AssemblySymbol item in referencedAssemblySymbols)
					{
						if (item.IsLinked)
						{
							num2++;
						}
					}
					if (num2 > 0)
					{
						AssemblyIdentity[] array = new AssemblyIdentity[identities.Length - num2];
						int num3 = 0;
						for (int j = 0; j < referencedAssemblySymbols.Length; j++)
						{
							if (!referencedAssemblySymbols[j].IsLinked)
							{
								array[num3] = identities[j];
								num3++;
							}
						}
						identities = array.AsImmutableOrNull();
					}
				}
				int length2 = identities.Length;
				AssemblySymbol[] array2 = new AssemblySymbol[length2];
				ArrayBuilder<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies = null;
				for (int k = 0; k < length2; k++)
				{
					AssemblyReferenceBinding referenceBinding = bindingResult[bindingIndex].ReferenceBinding[num + k];
					if (referenceBinding.IsBound)
					{
						array2[k] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[k] = GetOrAddMissingAssemblySymbol(identities[k], ref missingAssemblies);
					}
				}
				ModuleReferences<AssemblySymbol> moduleReferences = new ModuleReferences<AssemblySymbol>(identities, array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				modules[i].SetReferences(moduleReferences, sourceAssemblyDebugOnly);
				num += length2;
			}
		}

		private static void SetupReferencesForFileAssembly(AssemblyDataForFile fileData, BoundInputAssembly[] bindingResult, int bindingIndex, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies, SourceAssemblySymbol sourceAssemblyDebugOnly)
		{
			ImmutableArray<ModuleSymbol> modules = ((PEAssemblySymbol)bindingResult[bindingIndex].AssemblySymbol).Modules;
			int length = modules.Length;
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				int num2 = fileData.Assembly.ModuleReferenceCounts[i];
				AssemblyIdentity[] array = new AssemblyIdentity[num2];
				AssemblySymbol[] array2 = new AssemblySymbol[num2];
				fileData.AssemblyReferences.CopyTo(num, array, 0, num2);
				ArrayBuilder<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies = null;
				for (int j = 0; j < num2; j++)
				{
					AssemblyReferenceBinding referenceBinding = bindingResult[bindingIndex].ReferenceBinding[num + j];
					if (referenceBinding.IsBound)
					{
						array2[j] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[j] = GetOrAddMissingAssemblySymbol(array[j], ref missingAssemblies);
					}
				}
				ModuleReferences<AssemblySymbol> moduleReferences = new ModuleReferences<AssemblySymbol>(array.AsImmutableOrNull(), array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				modules[i].SetReferences(moduleReferences, sourceAssemblyDebugOnly);
				num += num2;
			}
		}

		private static void SetupReferencesForSourceAssembly(SourceAssemblySymbol sourceAssembly, ImmutableArray<PEModule> modules, int totalReferencedAssemblyCount, BoundInputAssembly[] bindingResult, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies, out ImmutableArray<ModuleReferences<AssemblySymbol>> moduleReferences)
		{
			ImmutableArray<ModuleSymbol> modules2 = sourceAssembly.Modules;
			ArrayBuilder<ModuleReferences<AssemblySymbol>> arrayBuilder = ((modules2.Length > 1) ? ArrayBuilder<ModuleReferences<AssemblySymbol>>.GetInstance() : null);
			int num = 0;
			for (int i = 0; i < modules2.Length; i++)
			{
				int num2 = ((i == 0) ? totalReferencedAssemblyCount : modules[i - 1].ReferencedAssemblies.Length);
				AssemblyIdentity[] array = new AssemblyIdentity[num2];
				AssemblySymbol[] array2 = new AssemblySymbol[num2];
				ArrayBuilder<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies = null;
				for (int j = 0; j < num2; j++)
				{
					AssemblyReferenceBinding referenceBinding = bindingResult[0].ReferenceBinding[num + j];
					if (referenceBinding.IsBound)
					{
						array2[j] = GetAssemblyDefinitionSymbol(bindingResult, referenceBinding, ref unifiedAssemblies);
					}
					else
					{
						array2[j] = GetOrAddMissingAssemblySymbol(referenceBinding.ReferenceIdentity, ref missingAssemblies);
					}
					array[j] = referenceBinding.ReferenceIdentity;
				}
				ModuleReferences<AssemblySymbol> moduleReferences2 = new ModuleReferences<AssemblySymbol>(array.AsImmutableOrNull(), array2.AsImmutableOrNull(), unifiedAssemblies.AsImmutableOrEmpty());
				if (i > 0)
				{
					arrayBuilder.Add(moduleReferences2);
				}
				modules2[i].SetReferences(moduleReferences2, sourceAssembly);
				num += num2;
			}
			moduleReferences = arrayBuilder.ToImmutableOrEmptyAndFree();
		}

		private static AssemblySymbol GetAssemblyDefinitionSymbol(BoundInputAssembly[] bindingResult, AssemblyReferenceBinding referenceBinding, ref ArrayBuilder<UnifiedAssembly<AssemblySymbol>> unifiedAssemblies)
		{
			AssemblySymbol assemblySymbol = bindingResult[referenceBinding.DefinitionIndex].AssemblySymbol;
			if (referenceBinding.VersionDifference != 0)
			{
				if (unifiedAssemblies == null)
				{
					unifiedAssemblies = new ArrayBuilder<UnifiedAssembly<AssemblySymbol>>();
				}
				unifiedAssemblies.Add(new UnifiedAssembly<AssemblySymbol>(assemblySymbol, referenceBinding.ReferenceIdentity));
			}
			return assemblySymbol;
		}

		private static MissingAssemblySymbol GetOrAddMissingAssemblySymbol(AssemblyIdentity assemblyIdentity, ref Dictionary<AssemblyIdentity, MissingAssemblySymbol> missingAssemblies)
		{
			MissingAssemblySymbol value;
			if (missingAssemblies == null)
			{
				missingAssemblies = new Dictionary<AssemblyIdentity, MissingAssemblySymbol>();
			}
			else if (missingAssemblies.TryGetValue(assemblyIdentity, out value))
			{
				return value;
			}
			value = new MissingAssemblySymbol(assemblyIdentity);
			missingAssemblies.Add(assemblyIdentity, value);
			return value;
		}

		internal static bool IsSourceAssemblySymbolCreated(CSharpCompilation compilation)
		{
			return (object)compilation._lazyAssemblySymbol != null;
		}

		internal static bool IsReferenceManagerInitialized(CSharpCompilation compilation)
		{
			return compilation._referenceManager.IsBound;
		}
	}

	internal static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

	private readonly CSharpCompilationOptions _options;

	private readonly Lazy<Imports> _globalImports;

	private readonly Lazy<Imports> _previousSubmissionImports;

	private readonly Lazy<AliasSymbol> _globalNamespaceAlias;

	private readonly Lazy<ImplicitNamedTypeSymbol> _scriptClass;

	private ConcurrentSet<ImportInfo> _lazyImportInfos;

	private ImmutableArray<Diagnostic> _lazyClsComplianceDiagnostics;

	private Conversions _conversions;

	private readonly AnonymousTypeManager _anonymousTypeManager;

	private NamespaceSymbol _lazyGlobalNamespace;

	internal readonly BuiltInOperators builtInOperators;

	private SourceAssemblySymbol _lazyAssemblySymbol;

	private ReferenceManager _referenceManager;

	private readonly SyntaxAndDeclarationManager _syntaxAndDeclarations;

	private EntryPoint _lazyEntryPoint;

	private HashSet<SyntaxTree> _lazyCompilationUnitCompletedTrees;

	private static readonly CSharpCompilationOptions s_defaultOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

	private static readonly CSharpCompilationOptions s_defaultSubmissionOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithReferencesSupersedeLowerVersions(value: true);

	private ConcurrentDictionary<string, NamespaceSymbol> _externAliasTargets;

	private TypeSymbol _lazyHostObjectTypeSymbol;

	private WeakReference<BinderFactory>[] _binderFactories;

	private DiagnosticBag _lazyDeclarationDiagnostics;

	private bool _declarationDiagnosticsFrozen;

	private readonly DiagnosticBag _additionalCodegenWarnings = new DiagnosticBag();

	internal readonly WellKnownMembersSignatureComparer WellKnownMemberSignatureComparer;

	private NamedTypeSymbol[] _lazyWellKnownTypes;

	private Symbol[] _lazyWellKnownTypeMembers;

	private bool _needsGeneratedIsReadOnlyAttribute_Value;

	private bool _needsGeneratedIsByRefLikeAttribute_Value;

	private bool _needsGeneratedIsUnmanagedAttribute_Value;

	private bool _needsGeneratedAttributes_IsFrozen;

	internal Conversions Conversions
	{
		get
		{
			if (_conversions == null)
			{
				Interlocked.CompareExchange(ref _conversions, new BuckStopsHereBinder(this).Conversions, null);
			}
			return _conversions;
		}
	}

	public override string Language => "C#";

	public override bool IsCaseSensitive => true;

	public new CSharpCompilationOptions Options => _options;

	internal AnonymousTypeManager AnonymousTypeManager => _anonymousTypeManager;

	internal override CommonAnonymousTypeManager CommonAnonymousTypeManager => AnonymousTypeManager;

	internal bool FeatureStrictEnabled => Feature("strict") != null;

	internal bool FeaturePEVerifyCompatEnabled => Feature("peverify-compat") != null;

	public LanguageVersion LanguageVersion { get; }

	public new CSharpScriptCompilationInfo ScriptCompilationInfo { get; }

	internal override ScriptCompilationInfo CommonScriptCompilationInfo => ScriptCompilationInfo;

	internal CSharpCompilation PreviousSubmission => ScriptCompilationInfo?.PreviousScriptCompilation;

	public new ImmutableArray<SyntaxTree> SyntaxTrees => _syntaxAndDeclarations.GetLazyState().SyntaxTrees;

	public override ImmutableArray<MetadataReference> DirectiveReferences => GetBoundReferenceManager().DirectiveReferences;

	internal override IDictionary<(string path, string content), MetadataReference> ReferenceDirectiveMap => GetBoundReferenceManager().ReferenceDirectiveMap;

	internal IEnumerable<string> ExternAliases => GetBoundReferenceManager().ExternAliases;

	public override IEnumerable<AssemblyIdentity> ReferencedAssemblyNames => Assembly.Modules.SelectMany((ModuleSymbol module) => module.GetReferencedAssemblies());

	internal override IEnumerable<ReferenceDirective> ReferenceDirectives => Declarations.ReferenceDirectives;

	internal SourceAssemblySymbol SourceAssembly
	{
		get
		{
			GetBoundReferenceManager();
			return _lazyAssemblySymbol;
		}
	}

	internal new AssemblySymbol Assembly => SourceAssembly;

	internal new ModuleSymbol SourceModule => Assembly.Modules[0];

	internal new NamespaceSymbol GlobalNamespace
	{
		get
		{
			if ((object)_lazyGlobalNamespace == null)
			{
				ArrayBuilder<ModuleSymbol> instance = ArrayBuilder<ModuleSymbol>.GetInstance();
				GetAllUnaliasedModules(instance);
				NamespaceSymbol value = MergedNamespaceSymbol.Create(new NamespaceExtent(this), null, instance.SelectDistinct((ModuleSymbol m) => m.GlobalNamespace));
				instance.Free();
				Interlocked.CompareExchange(ref _lazyGlobalNamespace, value, null);
			}
			return _lazyGlobalNamespace;
		}
	}

	internal new NamedTypeSymbol ScriptClass => _scriptClass.Value;

	internal Imports GlobalImports => _globalImports.Value;

	internal AliasSymbol GlobalNamespaceAlias => _globalNamespaceAlias.Value;

	internal new TypeSymbol DynamicType => AssemblySymbol.DynamicType;

	internal new NamedTypeSymbol ObjectType => Assembly.ObjectType;

	internal bool DeclaresTheObjectClass => SourceAssembly.DeclaresTheObjectClass;

	internal override CommonMessageProvider MessageProvider => _syntaxAndDeclarations.MessageProvider;

	internal DiagnosticBag DeclarationDiagnostics
	{
		get
		{
			if (_lazyDeclarationDiagnostics == null)
			{
				DiagnosticBag value = new DiagnosticBag();
				Interlocked.CompareExchange(ref _lazyDeclarationDiagnostics, value, null);
			}
			return _lazyDeclarationDiagnostics;
		}
	}

	internal DiagnosticBag AdditionalCodegenWarnings => _additionalCodegenWarnings;

	internal DeclarationTable Declarations => _syntaxAndDeclarations.GetLazyState().DeclarationTable;

	internal MergedNamespaceDeclaration MergedRootDeclaration => Declarations.GetMergedRoot(this);

	internal override byte LinkerMajorVersion => 48;

	internal override bool IsDelaySigned => SourceAssembly.IsDelaySigned;

	internal override StrongNameKeys StrongNameKeys => SourceAssembly.StrongNameKeys;

	internal override Guid DebugSourceDocumentLanguageId => DebugSourceDocument.CorSymLanguageTypeCSharp;

	protected override IAssemblySymbol CommonAssembly => Assembly;

	protected override INamespaceSymbol CommonGlobalNamespace => GlobalNamespace;

	protected override CompilationOptions CommonOptions => _options;

	protected override IEnumerable<SyntaxTree> CommonSyntaxTrees => SyntaxTrees;

	protected override IModuleSymbol CommonSourceModule => SourceModule;

	protected override INamedTypeSymbol CommonScriptClass => ScriptClass;

	protected override ITypeSymbol CommonDynamicType => DynamicType;

	protected override INamedTypeSymbol CommonObjectType => ObjectType;

	internal bool HasTupleNamesAttributes => (object)GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_TupleElementNamesAttribute__ctorTransformNames) != null;

	internal bool EnableEnumArrayBlockInitialization
	{
		get
		{
			Symbol wellKnownTypeMember = GetWellKnownTypeMember(WellKnownMember.System_Runtime_GCLatencyMode__SustainedLowLatency);
			if (wellKnownTypeMember != null)
			{
				return wellKnownTypeMember.ContainingAssembly == Assembly.CorLibrary;
			}
			return false;
		}
	}

	internal bool NeedsGeneratedIsReadOnlyAttribute
	{
		get
		{
			_needsGeneratedAttributes_IsFrozen = true;
			return _needsGeneratedIsReadOnlyAttribute_Value;
		}
	}

	internal bool NeedsGeneratedIsByRefLikeAttribute
	{
		get
		{
			_needsGeneratedAttributes_IsFrozen = true;
			return _needsGeneratedIsByRefLikeAttribute_Value;
		}
	}

	internal bool NeedsGeneratedIsUnmanagedAttribute
	{
		get
		{
			_needsGeneratedAttributes_IsFrozen = true;
			return _needsGeneratedIsUnmanagedAttribute_Value;
		}
	}

	protected override INamedTypeSymbol CommonCreateErrorTypeSymbol(INamespaceOrTypeSymbol container, string name, int arity)
	{
		return new ExtendedErrorTypeSymbol(container.EnsureCSharpSymbolOrNull<INamespaceOrTypeSymbol, NamespaceOrTypeSymbol>("container"), name, arity, null);
	}

	protected override INamespaceSymbol CommonCreateErrorNamespaceSymbol(INamespaceSymbol container, string name)
	{
		return new MissingNamespaceSymbol(container.EnsureCSharpSymbolOrNull<INamespaceSymbol, NamespaceSymbol>("container"), name);
	}

	public static CSharpCompilation Create(string assemblyName, IEnumerable<SyntaxTree> syntaxTrees = null, IEnumerable<MetadataReference> references = null, CSharpCompilationOptions options = null)
	{
		return Create(assemblyName, options ?? s_defaultOptions, syntaxTrees, references, null, null, null, isSubmission: false);
	}

	public static CSharpCompilation CreateScriptCompilation(string assemblyName, SyntaxTree syntaxTree = null, IEnumerable<MetadataReference> references = null, CSharpCompilationOptions options = null, CSharpCompilation previousScriptCompilation = null, Type returnType = null, Type globalsType = null)
	{
		Compilation.CheckSubmissionOptions(options);
		Compilation.ValidateScriptCompilationParameters(previousScriptCompilation, returnType, ref globalsType);
		CSharpCompilationOptions options2 = options?.WithReferencesSupersedeLowerVersions(value: true) ?? s_defaultSubmissionOptions;
		IEnumerable<SyntaxTree> syntaxTrees;
		if (syntaxTree == null)
		{
			syntaxTrees = SpecializedCollections.EmptyEnumerable<SyntaxTree>();
		}
		else
		{
			IEnumerable<SyntaxTree> enumerable = new SyntaxTree[1] { syntaxTree };
			syntaxTrees = enumerable;
		}
		return Create(assemblyName, options2, syntaxTrees, references, previousScriptCompilation, returnType, globalsType, isSubmission: true);
	}

	private static CSharpCompilation Create(string assemblyName, CSharpCompilationOptions options, IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference> references, CSharpCompilation previousSubmission, Type returnType, Type hostObjectType, bool isSubmission)
	{
		ImmutableArray<MetadataReference> references2 = Compilation.ValidateReferences<CSharpCompilationReference>(references);
		CSharpCompilation cSharpCompilation = new CSharpCompilation(assemblyName, options, references2, previousSubmission, returnType, hostObjectType, isSubmission, null, reuseReferenceManager: false, new SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree>.Empty, options.ScriptClassName, options.SourceReferenceResolver, Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, isSubmission, null));
		if (syntaxTrees != null)
		{
			cSharpCompilation = cSharpCompilation.AddSyntaxTrees(syntaxTrees);
		}
		return cSharpCompilation;
	}

	private CSharpCompilation(string assemblyName, CSharpCompilationOptions options, ImmutableArray<MetadataReference> references, CSharpCompilation previousSubmission, Type submissionReturnType, Type hostObjectType, bool isSubmission, ReferenceManager referenceManager, bool reuseReferenceManager, SyntaxAndDeclarationManager syntaxAndDeclarations, AsyncQueue<CompilationEvent> eventQueue = null)
		: base(assemblyName, references, Compilation.SyntaxTreeCommonFeatures(syntaxAndDeclarations.ExternalSyntaxTrees), isSubmission, eventQueue)
	{
		WellKnownMemberSignatureComparer = new WellKnownMembersSignatureComparer(this);
		_options = options;
		builtInOperators = new BuiltInOperators(this);
		_scriptClass = new Lazy<ImplicitNamedTypeSymbol>(BindScriptClass);
		_globalImports = new Lazy<Imports>(BindGlobalImports);
		_previousSubmissionImports = new Lazy<Imports>(ExpandPreviousSubmissionImports);
		_globalNamespaceAlias = new Lazy<AliasSymbol>(CreateGlobalNamespaceAlias);
		_anonymousTypeManager = new AnonymousTypeManager(this);
		LanguageVersion = CommonLanguageVersion(syntaxAndDeclarations.ExternalSyntaxTrees);
		if (isSubmission)
		{
			ScriptCompilationInfo = new CSharpScriptCompilationInfo(previousSubmission, submissionReturnType, hostObjectType);
		}
		if (reuseReferenceManager)
		{
			_referenceManager = referenceManager;
		}
		else
		{
			_referenceManager = new ReferenceManager(MakeSourceAssemblySimpleName(), Options.AssemblyIdentityComparer, referenceManager?.ObservedMetadata);
		}
		_syntaxAndDeclarations = syntaxAndDeclarations;
		if (EventQueue != null)
		{
			EventQueue.TryEnqueue(new CompilationStartedEvent(this));
		}
	}

	internal override void ValidateDebugEntryPoint(IMethodSymbol debugEntryPoint, DiagnosticBag diagnostics)
	{
		MethodSymbol methodSymbol = debugEntryPoint as MethodSymbol;
		if (methodSymbol?.DeclaringCompilation != this || !methodSymbol.IsDefinition)
		{
			diagnostics.Add(ErrorCode.ERR_DebugEntryPointNotSourceMethodDefinition, Location.None);
		}
	}

	private static LanguageVersion CommonLanguageVersion(ImmutableArray<SyntaxTree> syntaxTrees)
	{
		LanguageVersion? languageVersion = null;
		foreach (SyntaxTree item in syntaxTrees)
		{
			LanguageVersion languageVersion2 = ((CSharpParseOptions)item.Options).LanguageVersion;
			if (!languageVersion.HasValue)
			{
				languageVersion = languageVersion2;
			}
			else if (languageVersion != languageVersion2)
			{
				throw new ArgumentException(CodeAnalysisResources.InconsistentLanguageVersions, "syntaxTrees");
			}
		}
		return languageVersion ?? LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
	}

	public new CSharpCompilation Clone()
	{
		return new CSharpCompilation(AssemblyName, _options, ExternalReferences, PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations);
	}

	private CSharpCompilation Update(ReferenceManager referenceManager, bool reuseReferenceManager, SyntaxAndDeclarationManager syntaxAndDeclarations)
	{
		return new CSharpCompilation(AssemblyName, _options, ExternalReferences, PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, referenceManager, reuseReferenceManager, syntaxAndDeclarations);
	}

	public new CSharpCompilation WithAssemblyName(string assemblyName)
	{
		return new CSharpCompilation(assemblyName, _options, ExternalReferences, PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, _referenceManager, assemblyName == AssemblyName, _syntaxAndDeclarations);
	}

	public new CSharpCompilation WithReferences(IEnumerable<MetadataReference> references)
	{
		return new CSharpCompilation(AssemblyName, _options, Compilation.ValidateReferences<CSharpCompilationReference>(references), PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, null, reuseReferenceManager: false, _syntaxAndDeclarations);
	}

	public new CSharpCompilation WithReferences(params MetadataReference[] references)
	{
		return WithReferences((IEnumerable<MetadataReference>)references);
	}

	public CSharpCompilation WithOptions(CSharpCompilationOptions options)
	{
		CSharpCompilationOptions options2 = Options;
		bool reuseReferenceManager = options2.CanReuseCompilationReferenceManager(options);
		bool flag = options2.ScriptClassName == options.ScriptClassName && options2.SourceReferenceResolver == options.SourceReferenceResolver;
		return new CSharpCompilation(AssemblyName, options, ExternalReferences, PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, _referenceManager, reuseReferenceManager, flag ? _syntaxAndDeclarations : new SyntaxAndDeclarationManager(_syntaxAndDeclarations.ExternalSyntaxTrees, options.ScriptClassName, options.SourceReferenceResolver, _syntaxAndDeclarations.MessageProvider, _syntaxAndDeclarations.IsSubmission, null));
	}

	public CSharpCompilation WithScriptCompilationInfo(CSharpScriptCompilationInfo info)
	{
		if (info == ScriptCompilationInfo)
		{
			return this;
		}
		return new CSharpCompilation(AssemblyName, _options, ExternalReferences, info?.PreviousScriptCompilation, info?.ReturnTypeOpt, info?.GlobalsType, info != null, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations);
	}

	internal override Compilation WithEventQueue(AsyncQueue<CompilationEvent> eventQueue)
	{
		return new CSharpCompilation(AssemblyName, _options, ExternalReferences, PreviousSubmission, SubmissionReturnType, HostObjectType, IsSubmission, _referenceManager, reuseReferenceManager: true, _syntaxAndDeclarations, eventQueue);
	}

	internal override bool HasSubmissionResult()
	{
		SyntaxTree syntaxTree = _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		if (syntaxTree == null)
		{
			return false;
		}
		CompilationUnitSyntax compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();
		if (compilationUnitRoot.HasErrors)
		{
			return false;
		}
		if (compilationUnitRoot.DescendantNodes((SyntaxNode n) => n is GlobalStatementSyntax || n is StatementSyntax || n is CompilationUnitSyntax).Any((SyntaxNode n) => n.IsKind(SyntaxKind.ReturnStatement)))
		{
			return true;
		}
		GlobalStatementSyntax globalStatementSyntax = (GlobalStatementSyntax)compilationUnitRoot.Members.LastOrDefault((MemberDeclarationSyntax m) => m.IsKind(SyntaxKind.GlobalStatement));
		if (globalStatementSyntax != null)
		{
			StatementSyntax statement = globalStatementSyntax.Statement;
			if (statement.IsKind(SyntaxKind.ExpressionStatement))
			{
				ExpressionStatementSyntax expressionStatementSyntax = (ExpressionStatementSyntax)statement;
				if (expressionStatementSyntax.SemicolonToken.IsMissing)
				{
					SemanticModel semanticModel = GetSemanticModel(syntaxTree);
					ExpressionSyntax expression = expressionStatementSyntax.Expression;
					ITypeSymbol convertedType = semanticModel.GetTypeInfo(expression).ConvertedType;
					if (convertedType == null)
					{
						return true;
					}
					return convertedType.SpecialType != SpecialType.System_Void;
				}
			}
		}
		return false;
	}

	public new bool ContainsSyntaxTree(SyntaxTree syntaxTree)
	{
		if (syntaxTree != null)
		{
			return _syntaxAndDeclarations.GetLazyState().RootNamespaces.ContainsKey(syntaxTree);
		}
		return false;
	}

	public new CSharpCompilation AddSyntaxTrees(params SyntaxTree[] trees)
	{
		return AddSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public new CSharpCompilation AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		if (trees.IsEmpty())
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		instance.AddAll(syntaxAndDeclarations.ExternalSyntaxTrees);
		bool flag = true;
		int num = 0;
		foreach (CSharpSyntaxTree item in trees.Cast<CSharpSyntaxTree>())
		{
			if (item == null)
			{
				throw new ArgumentNullException(string.Format("{0}[{1}]", "trees", num));
			}
			if (!item.HasCompilationUnitRoot)
			{
				throw new ArgumentException(CSharpResources.TreeMustHaveARootNodeWith, string.Format("{0}[{1}]", "trees", num));
			}
			if (instance.Contains(item))
			{
				throw new ArgumentException(CSharpResources.SyntaxTreeAlreadyPresent, string.Format("{0}[{1}]", "trees", num));
			}
			if (IsSubmission && item.Options.Kind == SourceCodeKind.Regular)
			{
				throw new ArgumentException(CSharpResources.SubmissionCanOnlyInclude, string.Format("{0}[{1}]", "trees", num));
			}
			instance.Add(item);
			flag &= !item.HasReferenceOrLoadDirectives;
			num++;
		}
		instance.Free();
		if (IsSubmission && num > 1)
		{
			throw new ArgumentException(CSharpResources.SubmissionCanHaveAtMostOne, "trees");
		}
		syntaxAndDeclarations = syntaxAndDeclarations.AddSyntaxTrees(trees);
		return Update(_referenceManager, flag, syntaxAndDeclarations);
	}

	public new CSharpCompilation RemoveSyntaxTrees(params SyntaxTree[] trees)
	{
		return RemoveSyntaxTrees((IEnumerable<SyntaxTree>)trees);
	}

	public new CSharpCompilation RemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		if (trees == null)
		{
			throw new ArgumentNullException("trees");
		}
		if (trees.IsEmpty())
		{
			return this;
		}
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		PooledHashSet<SyntaxTree> instance2 = PooledHashSet<SyntaxTree>.GetInstance();
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		instance2.AddAll(syntaxAndDeclarations.ExternalSyntaxTrees);
		bool flag = true;
		int num = 0;
		foreach (CSharpSyntaxTree item in trees.Cast<CSharpSyntaxTree>())
		{
			if (!instance2.Contains(item))
			{
				ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap = syntaxAndDeclarations.GetLazyState().LoadedSyntaxTreeMap;
				if (SyntaxAndDeclarationManager.IsLoadedSyntaxTree(item, loadedSyntaxTreeMap))
				{
					throw new ArgumentException(CSharpResources.SyntaxTreeFromLoadNoRemoveReplace, string.Format("{0}[{1}]", "trees", num));
				}
				throw new ArgumentException(CSharpResources.SyntaxTreeNotFoundToRemove, string.Format("{0}[{1}]", "trees", num));
			}
			instance.Add(item);
			flag &= !item.HasReferenceOrLoadDirectives;
			num++;
		}
		instance2.Free();
		syntaxAndDeclarations = syntaxAndDeclarations.RemoveSyntaxTrees(instance);
		instance.Free();
		return Update(_referenceManager, flag, syntaxAndDeclarations);
	}

	public new CSharpCompilation RemoveAllSyntaxTrees()
	{
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		return Update(_referenceManager, !syntaxAndDeclarations.MayHaveReferenceDirectives(), syntaxAndDeclarations.WithExternalSyntaxTrees(ImmutableArray<SyntaxTree>.Empty));
	}

	public new CSharpCompilation ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		oldTree = (CSharpSyntaxTree)oldTree;
		newTree = (CSharpSyntaxTree)newTree;
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		if (newTree == null)
		{
			return RemoveSyntaxTrees(oldTree);
		}
		if (newTree == oldTree)
		{
			return this;
		}
		if (!newTree.HasCompilationUnitRoot)
		{
			throw new ArgumentException(CSharpResources.TreeMustHaveARootNodeWith, "newTree");
		}
		SyntaxAndDeclarationManager syntaxAndDeclarations = _syntaxAndDeclarations;
		ImmutableArray<SyntaxTree> externalSyntaxTrees = syntaxAndDeclarations.ExternalSyntaxTrees;
		if (!externalSyntaxTrees.Contains(oldTree))
		{
			ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap = syntaxAndDeclarations.GetLazyState().LoadedSyntaxTreeMap;
			if (SyntaxAndDeclarationManager.IsLoadedSyntaxTree(oldTree, loadedSyntaxTreeMap))
			{
				throw new ArgumentException(CSharpResources.SyntaxTreeFromLoadNoRemoveReplace, "oldTree");
			}
			throw new ArgumentException(CSharpResources.SyntaxTreeNotFoundToRemove, "oldTree");
		}
		if (externalSyntaxTrees.Contains(newTree))
		{
			throw new ArgumentException(CSharpResources.SyntaxTreeAlreadyPresent, "newTree");
		}
		bool reuseReferenceManager = !oldTree.HasReferenceOrLoadDirectives() && !newTree.HasReferenceOrLoadDirectives();
		syntaxAndDeclarations = syntaxAndDeclarations.ReplaceSyntaxTree(oldTree, newTree);
		return Update(_referenceManager, reuseReferenceManager, syntaxAndDeclarations);
	}

	internal override int GetSyntaxTreeOrdinal(SyntaxTree tree)
	{
		return _syntaxAndDeclarations.GetLazyState().OrdinalMap[tree];
	}

	internal override CommonReferenceManager CommonGetBoundReferenceManager()
	{
		return GetBoundReferenceManager();
	}

	internal new ReferenceManager GetBoundReferenceManager()
	{
		if ((object)_lazyAssemblySymbol == null)
		{
			_referenceManager.CreateSourceAssemblyForCompilation(this);
		}
		return _referenceManager;
	}

	internal bool ReferenceManagerEquals(CSharpCompilation other)
	{
		return _referenceManager == other._referenceManager;
	}

	internal new Symbol GetAssemblyOrModuleSymbol(MetadataReference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (reference.Properties.Kind == MetadataImageKind.Assembly)
		{
			return GetBoundReferenceManager().GetReferencedAssemblySymbol(reference);
		}
		int referencedModuleIndex = GetBoundReferenceManager().GetReferencedModuleIndex(reference);
		if (referencedModuleIndex >= 0)
		{
			return Assembly.Modules[referencedModuleIndex];
		}
		return null;
	}

	public MetadataReference GetDirectiveReference(ReferenceDirectiveTriviaSyntax directive)
	{
		if (!ReferenceDirectiveMap.TryGetValue((directive.SyntaxTree.FilePath, directive.File.ValueText), out var value))
		{
			return null;
		}
		return value;
	}

	public new CSharpCompilation AddReferences(params MetadataReference[] references)
	{
		return (CSharpCompilation)base.AddReferences(references);
	}

	public new CSharpCompilation AddReferences(IEnumerable<MetadataReference> references)
	{
		return (CSharpCompilation)base.AddReferences(references);
	}

	public new CSharpCompilation RemoveReferences(params MetadataReference[] references)
	{
		return (CSharpCompilation)base.RemoveReferences(references);
	}

	public new CSharpCompilation RemoveReferences(IEnumerable<MetadataReference> references)
	{
		return (CSharpCompilation)base.RemoveReferences(references);
	}

	public new CSharpCompilation RemoveAllReferences()
	{
		return (CSharpCompilation)base.RemoveAllReferences();
	}

	public new CSharpCompilation ReplaceReference(MetadataReference oldReference, MetadataReference newReference)
	{
		return (CSharpCompilation)base.ReplaceReference(oldReference, newReference);
	}

	public override CompilationReference ToMetadataReference(ImmutableArray<string> aliases = default(ImmutableArray<string>), bool embedInteropTypes = false)
	{
		return new CSharpCompilationReference(this, aliases, embedInteropTypes);
	}

	private void GetAllUnaliasedModules(ArrayBuilder<ModuleSymbol> modules)
	{
		modules.AddRange(Assembly.Modules);
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
		{
			if (boundReferenceManager.DeclarationsAccessibleWithoutAlias(i))
			{
				modules.AddRange(boundReferenceManager.ReferencedAssemblies[i].Modules);
			}
		}
	}

	internal void GetUnaliasedReferencedAssemblies(ArrayBuilder<AssemblySymbol> assemblies)
	{
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
		{
			if (boundReferenceManager.DeclarationsAccessibleWithoutAlias(i))
			{
				assemblies.Add(boundReferenceManager.ReferencedAssemblies[i]);
			}
		}
	}

	public new MetadataReference GetMetadataReference(IAssemblySymbol assemblySymbol)
	{
		return base.GetMetadataReference(assemblySymbol);
	}

	internal new NamespaceSymbol GetCompilationNamespace(INamespaceSymbol namespaceSymbol)
	{
		if (namespaceSymbol is NamespaceSymbol && namespaceSymbol.NamespaceKind == NamespaceKind.Compilation && namespaceSymbol.ContainingCompilation == this)
		{
			return (NamespaceSymbol)namespaceSymbol;
		}
		INamespaceSymbol containingNamespace = namespaceSymbol.ContainingNamespace;
		if (containingNamespace == null)
		{
			return GlobalNamespace;
		}
		return GetCompilationNamespace(containingNamespace)?.GetNestedNamespace(namespaceSymbol.Name);
	}

	internal bool GetExternAliasTarget(string aliasName, out NamespaceSymbol @namespace)
	{
		if (_externAliasTargets == null)
		{
			Interlocked.CompareExchange(ref _externAliasTargets, new ConcurrentDictionary<string, NamespaceSymbol>(), null);
		}
		else if (_externAliasTargets.TryGetValue(aliasName, out @namespace))
		{
			return !(@namespace is MissingNamespaceSymbol);
		}
		ArrayBuilder<NamespaceSymbol> arrayBuilder = null;
		ReferenceManager boundReferenceManager = GetBoundReferenceManager();
		for (int i = 0; i < boundReferenceManager.ReferencedAssemblies.Length; i++)
		{
			if (boundReferenceManager.AliasesOfReferencedAssemblies[i].Contains(aliasName))
			{
				arrayBuilder = arrayBuilder ?? ArrayBuilder<NamespaceSymbol>.GetInstance();
				arrayBuilder.Add(boundReferenceManager.ReferencedAssemblies[i].GlobalNamespace);
			}
		}
		bool flag = arrayBuilder != null;
		@namespace = (flag ? MergedNamespaceSymbol.Create(new NamespaceExtent(this), null, arrayBuilder.ToImmutableAndFree()) : new MissingNamespaceSymbol(new MissingModuleSymbol(new MissingAssemblySymbol(new AssemblyIdentity(Guid.NewGuid().ToString())), -1)));
		@namespace = _externAliasTargets.GetOrAdd(aliasName, @namespace);
		return flag;
	}

	private ImplicitNamedTypeSymbol BindScriptClass()
	{
		return (ImplicitNamedTypeSymbol)CommonBindScriptClass();
	}

	internal bool IsSubmissionSyntaxTree(SyntaxTree tree)
	{
		if (IsSubmission)
		{
			return tree == _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		}
		return false;
	}

	private Imports BindGlobalImports()
	{
		return Imports.FromGlobalUsings(this);
	}

	internal Imports GetSubmissionImports()
	{
		SyntaxTree syntaxTree = _syntaxAndDeclarations.ExternalSyntaxTrees.SingleOrDefault();
		if (syntaxTree == null)
		{
			return Imports.Empty;
		}
		return GetBinderFactory(syntaxTree).GetImportsBinder((CSharpSyntaxNode)syntaxTree.GetRoot()).GetImports(null);
	}

	internal Imports GetPreviousSubmissionImports()
	{
		return _previousSubmissionImports.Value;
	}

	private Imports ExpandPreviousSubmissionImports()
	{
		CSharpCompilation previousSubmission = PreviousSubmission;
		if (previousSubmission == null)
		{
			return Imports.Empty;
		}
		return Imports.ExpandPreviousSubmissionImports(previousSubmission.GetPreviousSubmissionImports(), this).Concat(Imports.ExpandPreviousSubmissionImports(previousSubmission.GetSubmissionImports(), this));
	}

	internal new NamedTypeSymbol GetSpecialType(SpecialType specialType)
	{
		if (specialType <= SpecialType.None || specialType > SpecialType.System_AsyncCallback)
		{
			throw new ArgumentOutOfRangeException("specialType", $"Unexpected SpecialType: '{(int)specialType}'.");
		}
		return Assembly.GetSpecialType(specialType);
	}

	internal Symbol GetSpecialTypeMember(SpecialMember specialMember)
	{
		return Assembly.GetSpecialTypeMember(specialMember);
	}

	internal override ISymbol CommonGetSpecialTypeMember(SpecialMember specialMember)
	{
		return GetSpecialTypeMember(specialMember);
	}

	internal TypeSymbol GetTypeByReflectionType(Type type, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = Assembly.GetTypeByReflectionType(type, includeReferences: true);
		if ((object)typeSymbol == null)
		{
			ExtendedErrorTypeSymbol extendedErrorTypeSymbol = new ExtendedErrorTypeSymbol(this, type.Name, 0, CreateReflectionTypeNotFoundError(type));
			diagnostics.Add(extendedErrorTypeSymbol.ErrorInfo, NoLocation.Singleton);
			typeSymbol = extendedErrorTypeSymbol;
		}
		return typeSymbol;
	}

	private static CSDiagnosticInfo CreateReflectionTypeNotFoundError(Type type)
	{
		return new CSDiagnosticInfo(ErrorCode.ERR_GlobalSingleTypeNameNotFound, new object[1] { type.AssemblyQualifiedName }, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty);
	}

	internal TypeSymbol GetHostObjectTypeSymbol()
	{
		if ((object)HostObjectType != null && (object)_lazyHostObjectTypeSymbol == null)
		{
			TypeSymbol typeSymbol = Assembly.GetTypeByReflectionType(HostObjectType, includeReferences: true);
			if ((object)typeSymbol == null)
			{
				MetadataTypeName emittedName = MetadataTypeName.FromNamespaceAndTypeName(HostObjectType.Namespace ?? string.Empty, HostObjectType.Name, useCLSCompliantNameArityEncoding: true);
				typeSymbol = new MissingMetadataTypeSymbol.TopLevelWithCustomErrorInfo(new MissingAssemblySymbol(AssemblyIdentity.FromAssemblyDefinition(HostObjectType.GetTypeInfo().Assembly)).Modules[0], ref emittedName, CreateReflectionTypeNotFoundError(HostObjectType), SpecialType.None);
			}
			Interlocked.CompareExchange(ref _lazyHostObjectTypeSymbol, typeSymbol, null);
		}
		return _lazyHostObjectTypeSymbol;
	}

	internal SynthesizedInteractiveInitializerMethod GetSubmissionInitializer()
	{
		if (!IsSubmission || (object)ScriptClass == null)
		{
			return null;
		}
		return ScriptClass.GetScriptInitializer();
	}

	internal new NamedTypeSymbol GetTypeByMetadataName(string fullyQualifiedMetadataName)
	{
		(AssemblySymbol, AssemblySymbol) conflicts;
		return Assembly.GetTypeByMetadataName(fullyQualifiedMetadataName, includeReferences: true, isWellKnownType: false, out conflicts);
	}

	internal new MethodSymbol GetEntryPoint(CancellationToken cancellationToken)
	{
		return GetEntryPointAndDiagnostics(cancellationToken)?.MethodSymbol;
	}

	internal EntryPoint GetEntryPointAndDiagnostics(CancellationToken cancellationToken)
	{
		if (!Options.OutputKind.IsApplication() && (object)ScriptClass == null)
		{
			return null;
		}
		if (Options.MainTypeName != null && !Options.MainTypeName.IsValidClrTypeName())
		{
			return new EntryPoint(null, ImmutableArray<Diagnostic>.Empty);
		}
		if (_lazyEntryPoint == null)
		{
			MethodSymbol methodSymbol = FindEntryPoint(cancellationToken, out var sealedDiagnostics);
			Interlocked.CompareExchange(ref _lazyEntryPoint, new EntryPoint(methodSymbol, sealedDiagnostics), null);
		}
		return _lazyEntryPoint;
	}

	private MethodSymbol FindEntryPoint(CancellationToken cancellationToken, out ImmutableArray<Diagnostic> sealedDiagnostics)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
		try
		{
			string mainTypeName = Options.MainTypeName;
			NamespaceSymbol globalNamespace = SourceModule.GlobalNamespace;
			NamedTypeSymbol namedTypeSymbol;
			if (mainTypeName != null)
			{
				NamedTypeSymbol scriptClass = ScriptClass;
				if (scriptClass != null)
				{
					instance.Add(ErrorCode.WRN_MainIgnored, NoLocation.Singleton, mainTypeName);
					return scriptClass.GetScriptEntryPoint();
				}
				NamespaceOrTypeSymbol namespaceOrTypeSymbol = globalNamespace.GetNamespaceOrTypeByQualifiedName(mainTypeName.Split(new char[1] { '.' })).OfMinimalArity();
				if ((object)namespaceOrTypeSymbol == null)
				{
					instance.Add(ErrorCode.ERR_MainClassNotFound, NoLocation.Singleton, mainTypeName);
					return null;
				}
				namedTypeSymbol = namespaceOrTypeSymbol as NamedTypeSymbol;
				if ((object)namedTypeSymbol == null || namedTypeSymbol.IsGenericType || (namedTypeSymbol.TypeKind != TypeKind.Class && namedTypeSymbol.TypeKind != TypeKind.Struct))
				{
					instance.Add(ErrorCode.ERR_MainClassNotClass, namespaceOrTypeSymbol.Locations.First(), namespaceOrTypeSymbol);
					return null;
				}
				AddEntryPointCandidates(instance2, namedTypeSymbol.GetMembersUnordered());
			}
			else
			{
				namedTypeSymbol = null;
				AddEntryPointCandidates(instance2, GetSymbolsWithName("Main", SymbolFilter.Member, cancellationToken));
				NamedTypeSymbol scriptClass2 = ScriptClass;
				if (scriptClass2 != null)
				{
					foreach (MethodSymbol item in instance2)
					{
						instance.Add(ErrorCode.WRN_MainIgnored, item.Locations.First(), item);
					}
					return scriptClass2.GetScriptEntryPoint();
				}
			}
			ArrayBuilder<(bool, MethodSymbol, DiagnosticBag)> instance3 = ArrayBuilder<(bool, MethodSymbol, DiagnosticBag)>.GetInstance();
			DiagnosticBag noMainFoundDiagnostics = DiagnosticBag.GetInstance();
			ArrayBuilder<MethodSymbol> instance4 = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (MethodSymbol item2 in instance2)
			{
				DiagnosticBag instance5 = DiagnosticBag.GetInstance();
				(bool IsCandidate, bool IsTaskLike) tuple = HasEntryPointSignature(item2, instance5);
				var (flag, _) = tuple;
				if (tuple.IsTaskLike)
				{
					instance3.Add((flag, item2, instance5));
					continue;
				}
				if (CheckValid(item2, flag, instance5))
				{
					if (item2.IsAsync)
					{
						instance.Add(ErrorCode.ERR_NonTaskMainCantBeAsync, item2.Locations.First(), item2);
					}
					else
					{
						instance.AddRange(instance5);
						instance4.Add(item2);
					}
				}
				instance5.Free();
			}
			if (instance4.Count == 0)
			{
				foreach (var (isCandidate, methodSymbol, diagnosticBag) in instance3)
				{
					if (CheckValid(methodSymbol, isCandidate, diagnosticBag) && Binder.CheckFeatureAvailability(methodSymbol.ExtractReturnTypeSyntax(), MessageID.IDS_FeatureAsyncMain, instance))
					{
						instance.AddRange(diagnosticBag);
						instance4.Add(methodSymbol);
					}
				}
			}
			foreach (var item3 in instance3)
			{
				item3.Item3.Free();
			}
			if (instance4.Count == 0)
			{
				instance.AddRange(noMainFoundDiagnostics);
			}
			else if ((object)namedTypeSymbol == null)
			{
				foreach (Diagnostic item4 in noMainFoundDiagnostics.AsEnumerable())
				{
					if (item4.Code == 28 || item4.Code == 402)
					{
						instance.Add(item4);
					}
				}
			}
			MethodSymbol result = null;
			if (instance4.Count == 0)
			{
				if ((object)namedTypeSymbol == null)
				{
					instance.Add(ErrorCode.ERR_NoEntryPoint, NoLocation.Singleton);
				}
				else
				{
					instance.Add(ErrorCode.ERR_NoMainInClass, namedTypeSymbol.Locations.First(), namedTypeSymbol);
				}
			}
			else if (instance4.Count > 1)
			{
				instance4.Sort(LexicalOrderSymbolComparer.Instance);
				CSDiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_MultipleEntryPoints, Array.Empty<object>(), instance4.OfType<Symbol>().AsImmutable(), instance4.Select((MethodSymbol m) => m.Locations.First()).OfType<Location>().AsImmutable());
				instance.Add(new CSDiagnostic(info, instance4.First().Locations.First()));
			}
			else
			{
				result = instance4[0];
			}
			instance3.Free();
			instance4.Free();
			noMainFoundDiagnostics.Free();
			return result;
			bool CheckValid(MethodSymbol candidate, bool flag2, DiagnosticBag specificDiagnostics)
			{
				if (!flag2)
				{
					noMainFoundDiagnostics.Add(ErrorCode.WRN_InvalidMainSig, candidate.Locations.First(), candidate);
					noMainFoundDiagnostics.AddRange(specificDiagnostics);
					return false;
				}
				if (candidate.IsGenericMethod || candidate.ContainingType.IsGenericType)
				{
					noMainFoundDiagnostics.Add(ErrorCode.WRN_MainCantBeGeneric, candidate.Locations.First(), candidate);
					return false;
				}
				return true;
			}
		}
		finally
		{
			instance2.Free();
			sealedDiagnostics = instance.ToReadOnlyAndFree();
		}
	}

	private static void AddEntryPointCandidates(ArrayBuilder<MethodSymbol> entryPointCandidates, IEnumerable<ISymbol> members)
	{
		foreach (ISymbol member in members)
		{
			if (member is MethodSymbol { IsEntryPointCandidate: not false } methodSymbol)
			{
				entryPointCandidates.Add(methodSymbol);
			}
		}
	}

	internal bool ReturnsAwaitableToVoidOrInt(MethodSymbol method, DiagnosticBag diagnostics)
	{
		if (method.ReturnType.SpecialType == SpecialType.System_Void || method.ReturnType.SpecialType == SpecialType.System_Int32)
		{
			return false;
		}
		if (!(method.ReturnType is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (!(namedTypeSymbol.ConstructedFrom == GetWellKnownType(WellKnownType.System_Threading_Tasks_Task)) && !(namedTypeSymbol.ConstructedFrom == GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T)))
		{
			return false;
		}
		CSharpSyntaxNode cSharpSyntaxNode = method.ExtractReturnTypeSyntax();
		BoundLiteral expression = new BoundLiteral(cSharpSyntaxNode, ConstantValue.Null, method.ReturnType);
		if (GetBinder(cSharpSyntaxNode).GetAwaitableExpressionInfo(expression, out var _, out var _, out var _, out var getAwaiterGetResultCall, cSharpSyntaxNode, diagnostics))
		{
			if (getAwaiterGetResultCall.Type.SpecialType != SpecialType.System_Void)
			{
				return getAwaiterGetResultCall.Type.SpecialType == SpecialType.System_Int32;
			}
			return true;
		}
		return false;
	}

	private (bool IsCandidate, bool IsTaskLike) HasEntryPointSignature(MethodSymbol method, DiagnosticBag bag)
	{
		if (method.IsVararg)
		{
			return (IsCandidate: false, IsTaskLike: false);
		}
		TypeSymbol returnType = method.ReturnType;
		bool flag = false;
		if (returnType.SpecialType != SpecialType.System_Int32 && returnType.SpecialType != SpecialType.System_Void)
		{
			flag = ReturnsAwaitableToVoidOrInt(method, bag);
			if (!flag)
			{
				return (IsCandidate: false, IsTaskLike: false);
			}
		}
		if (method.RefKind != RefKind.None)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		if (method.Parameters.Length == 0)
		{
			return (IsCandidate: true, IsTaskLike: flag);
		}
		if (method.Parameters.Length > 1)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		if (!method.ParameterRefKinds.IsDefault)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		TypeSymbol type = method.Parameters[0].Type;
		if (type.TypeKind != TypeKind.Array)
		{
			return (IsCandidate: false, IsTaskLike: flag);
		}
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
		return (IsCandidate: arrayTypeSymbol.IsSZArray && arrayTypeSymbol.ElementType.SpecialType == SpecialType.System_String, IsTaskLike: flag);
	}

	internal override bool IsUnreferencedAssemblyIdentityDiagnosticCode(int code)
	{
		return code == 12;
	}

	internal bool MightContainNoPiaLocalTypes()
	{
		return SourceAssembly.MightContainNoPiaLocalTypes();
	}

	public Conversion ClassifyConversion(ITypeSymbol source, ITypeSymbol destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		TypeSymbol source2 = source.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("source");
		TypeSymbol destination2 = destination.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("destination");
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return Conversions.ClassifyConversionFromType(source2, destination2, ref useSiteDiagnostics);
	}

	public override CommonConversion ClassifyCommonConversion(ITypeSymbol source, ITypeSymbol destination)
	{
		return ClassifyConversion(source, destination).ToCommonConversion();
	}

	internal override IConvertibleConversion ClassifyConvertibleConversion(IOperation source, ITypeSymbol destination, out Optional<object> constantValue)
	{
		constantValue = default;
		if (destination == null)
		{
			return Conversion.NoConversion;
		}
		ITypeSymbol type = source.Type;
		if (type == null)
		{
			if (source.ConstantValue.HasValue && source.ConstantValue.Value == null && destination.IsReferenceType)
			{
				constantValue = source.ConstantValue;
				return Conversion.DefaultOrNullLiteral;
			}
			return Conversion.NoConversion;
		}
		Conversion conversion = ClassifyConversion(type, destination);
		if (conversion.IsReference && source.ConstantValue.HasValue && source.ConstantValue.Value == null)
		{
			constantValue = source.ConstantValue;
		}
		return conversion;
	}

	internal ArrayTypeSymbol CreateArrayTypeSymbol(TypeSymbol elementType, int rank = 1)
	{
		if ((object)elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		return ArrayTypeSymbol.CreateCSharpArray(Assembly, elementType, ImmutableArray<CustomModifier>.Empty, rank);
	}

	internal PointerTypeSymbol CreatePointerTypeSymbol(TypeSymbol elementType)
	{
		if ((object)elementType == null)
		{
			throw new ArgumentNullException("elementType");
		}
		return new PointerTypeSymbol(elementType);
	}

	public new SemanticModel GetSemanticModel(SyntaxTree syntaxTree, bool ignoreAccessibility)
	{
		if (syntaxTree == null)
		{
			throw new ArgumentNullException("syntaxTree");
		}
		if (!_syntaxAndDeclarations.GetLazyState().RootNamespaces.ContainsKey(syntaxTree))
		{
			throw new ArgumentException(CSharpResources.SyntaxTreeNotFound, "syntaxTree");
		}
		return new SyntaxTreeSemanticModel(this, syntaxTree, ignoreAccessibility);
	}

	internal BinderFactory GetBinderFactory(SyntaxTree syntaxTree)
	{
		int syntaxTreeOrdinal = GetSyntaxTreeOrdinal(syntaxTree);
		WeakReference<BinderFactory>[] array = _binderFactories;
		if (array == null)
		{
			array = new WeakReference<BinderFactory>[SyntaxTrees.Length];
			array = Interlocked.CompareExchange(ref _binderFactories, array, null) ?? array;
		}
		WeakReference<BinderFactory> weakReference = array[syntaxTreeOrdinal];
		if (weakReference != null && weakReference.TryGetTarget(out var target))
		{
			return target;
		}
		return AddNewFactory(syntaxTree, ref array[syntaxTreeOrdinal]);
	}

	private BinderFactory AddNewFactory(SyntaxTree syntaxTree, ref WeakReference<BinderFactory> slot)
	{
		BinderFactory binderFactory = new BinderFactory(this, syntaxTree);
		WeakReference<BinderFactory> value = new WeakReference<BinderFactory>(binderFactory);
		WeakReference<BinderFactory> weakReference;
		do
		{
			weakReference = slot;
			if (weakReference != null && weakReference.TryGetTarget(out var target))
			{
				return target;
			}
		}
		while (Interlocked.CompareExchange(ref slot, value, weakReference) != weakReference);
		return binderFactory;
	}

	internal Binder GetBinder(CSharpSyntaxNode syntax)
	{
		return GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
	}

	internal Imports GetImports(SingleNamespaceDeclaration declaration)
	{
		return GetBinderFactory(declaration.SyntaxReference.SyntaxTree).GetImportsBinder((CSharpSyntaxNode)declaration.SyntaxReference.GetSyntax()).GetImports(null);
	}

	private AliasSymbol CreateGlobalNamespaceAlias()
	{
		return AliasSymbol.CreateGlobalNamespaceAlias(GlobalNamespace, new InContainerBinder(GlobalNamespace, new BuckStopsHereBinder(this)));
	}

	private void CompleteTree(SyntaxTree tree)
	{
		if (_lazyCompilationUnitCompletedTrees == null)
		{
			Interlocked.CompareExchange(ref _lazyCompilationUnitCompletedTrees, new HashSet<SyntaxTree>(), null);
		}
		lock (_lazyCompilationUnitCompletedTrees)
		{
			if (_lazyCompilationUnitCompletedTrees.Add(tree))
			{
				EventQueue.TryEnqueue(new CompilationUnitCompletedEvent(this, tree));
				if (_lazyCompilationUnitCompletedTrees.Count == SyntaxTrees.Length)
				{
					CompleteCompilationEventQueue_NoLock();
				}
			}
		}
	}

	internal override void ReportUnusedImports(SyntaxTree filterTree, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		if (_lazyImportInfos != null && (filterTree == null || filterTree.Options.DocumentationMode != DocumentationMode.None))
		{
			foreach (ImportInfo lazyImportInfo in _lazyImportInfos)
			{
				cancellationToken.ThrowIfCancellationRequested();
				SyntaxTree tree = lazyImportInfo.Tree;
				if ((filterTree == null || filterTree == tree) && tree.Options.DocumentationMode != DocumentationMode.None)
				{
					TextSpan span = lazyImportInfo.Span;
					if (!IsImportDirectiveUsed(tree, span.Start))
					{
						ErrorCode code = ((lazyImportInfo.Kind == SyntaxKind.ExternAliasDirective) ? ErrorCode.HDN_UnusedExternAlias : ErrorCode.HDN_UnusedUsingDirective);
						diagnostics.Add(code, tree.GetLocation(span));
					}
				}
			}
		}
		CompleteTrees(filterTree);
	}

	internal override void CompleteTrees(SyntaxTree filterTree)
	{
		if (EventQueue == null)
		{
			return;
		}
		if (filterTree != null)
		{
			CompleteTree(filterTree);
			return;
		}
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			CompleteTree(syntaxTree);
		}
	}

	internal void RecordImport(UsingDirectiveSyntax syntax)
	{
		RecordImportInternal(syntax);
	}

	internal void RecordImport(ExternAliasDirectiveSyntax syntax)
	{
		RecordImportInternal(syntax);
	}

	private void RecordImportInternal(CSharpSyntaxNode syntax)
	{
		LazyInitializer.EnsureInitialized(ref _lazyImportInfos).Add(new ImportInfo(syntax.SyntaxTree, syntax.Kind(), syntax.Span));
	}

	public override ImmutableArray<Diagnostic> GetParseDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Parse, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Declare, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Compile, includeEarlierStages: false, cancellationToken);
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(CompilationStage.Compile, includeEarlierStages: true, cancellationToken);
	}

	internal ImmutableArray<Diagnostic> GetDiagnostics(CompilationStage stage, bool includeEarlierStages, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		GetDiagnostics(stage, includeEarlierStages, instance, cancellationToken);
		return instance.ToReadOnlyAndFree();
	}

	internal override void GetDiagnostics(CompilationStage stage, bool includeEarlierStages, DiagnosticBag diagnostics, CancellationToken cancellationToken = default(CancellationToken))
	{
		DiagnosticBag builder = DiagnosticBag.GetInstance();
		if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
		{
			ImmutableArray<SyntaxTree> syntaxTrees = SyntaxTrees;
			if (Options.ConcurrentBuild)
			{
				ParallelOptions parallelOptions = (cancellationToken.CanBeCanceled ? new ParallelOptions
				{
					CancellationToken = cancellationToken
				} : DefaultParallelOptions);
				Parallel.For(0, syntaxTrees.Length, parallelOptions, UICultureUtilities.WithCurrentUICulture((int i) =>
				{
					SyntaxTree syntaxTree = syntaxTrees[i];
					AppendLoadDirectiveDiagnostics(builder, _syntaxAndDeclarations, syntaxTree);
					builder.AddRange(syntaxTree.GetDiagnostics(cancellationToken));
				}));
			}
			else
			{
				foreach (SyntaxTree item in syntaxTrees)
				{
					cancellationToken.ThrowIfCancellationRequested();
					AppendLoadDirectiveDiagnostics(builder, _syntaxAndDeclarations, item);
					cancellationToken.ThrowIfCancellationRequested();
					builder.AddRange(item.GetDiagnostics(cancellationToken));
				}
			}
			HashSet<ParseOptions> hashSet = new HashSet<ParseOptions>();
			foreach (SyntaxTree item2 in syntaxTrees)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!item2.Options.Errors.IsDefaultOrEmpty && hashSet.Add(item2.Options))
				{
					Location location = item2.GetLocation(TextSpan.FromBounds(0, 0));
					foreach (Diagnostic error in item2.Options.Errors)
					{
						builder.Add(error.WithLocation(location));
					}
				}
			}
		}
		if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
		{
			CheckAssemblyName(builder);
			builder.AddRange(Options.Errors);
			cancellationToken.ThrowIfCancellationRequested();
			builder.AddRange(GetBoundReferenceManager().Diagnostics);
			cancellationToken.ThrowIfCancellationRequested();
			builder.AddRange(GetSourceDeclarationDiagnostics(null, null, null, cancellationToken));
			if (EventQueue != null && SyntaxTrees.Length == 0)
			{
				EnsureCompilationEventQueueCompleted();
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			GetDiagnosticsForAllMethodBodies(instance, cancellationToken);
			builder.AddRangeAndFree(instance);
		}
		FilterAndAppendAndFreeDiagnostics(diagnostics, ref builder);
	}

	private static void AppendLoadDirectiveDiagnostics(DiagnosticBag builder, SyntaxAndDeclarationManager syntaxAndDeclarations, SyntaxTree syntaxTree, Func<IEnumerable<Diagnostic>, IEnumerable<Diagnostic>> locationFilterOpt = null)
	{
		if (!syntaxAndDeclarations.GetLazyState().LoadDirectiveMap.TryGetValue(syntaxTree, out var value))
		{
			return;
		}
		foreach (LoadDirective item in value)
		{
			IEnumerable<Diagnostic> enumerable = item.Diagnostics;
			if (locationFilterOpt != null)
			{
				enumerable = locationFilterOpt(enumerable);
			}
			builder.AddRange(enumerable);
		}
	}

	private void GetDiagnosticsForAllMethodBodies(DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		MethodCompiler.CompileMethodBodies(this, null, emittingPdb: false, emitTestCoverageData: false, hasDeclarationErrors: false, diagnostics, null, cancellationToken);
		DocumentationCommentCompiler.WriteDocumentationCommentXml(this, null, null, diagnostics, cancellationToken);
		ReportUnusedImports(null, diagnostics, cancellationToken);
	}

	private static bool IsDefinedOrImplementedInSourceTree(Symbol symbol, SyntaxTree tree, TextSpan? span)
	{
		if (symbol.IsDefinedInSourceTree(tree, span))
		{
			return true;
		}
		if (symbol.Kind == SymbolKind.Method && symbol.IsImplicitlyDeclared && ((MethodSymbol)symbol).MethodKind == MethodKind.Constructor)
		{
			return IsDefinedOrImplementedInSourceTree(symbol.ContainingType, tree, span);
		}
		return false;
	}

	private ImmutableArray<Diagnostic> GetDiagnosticsForMethodBodiesInTree(SyntaxTree tree, TextSpan? span, CancellationToken cancellationToken)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		MethodCompiler.CompileMethodBodies(this, null, emittingPdb: false, emitTestCoverageData: false, hasDeclarationErrors: false, instance, (Symbol s) => IsDefinedOrImplementedInSourceTree(s, tree, span), cancellationToken);
		DocumentationCommentCompiler.WriteDocumentationCommentXml(this, null, null, instance, cancellationToken, tree, span);
		if (!span.HasValue || span.Value == tree.GetRoot(cancellationToken).FullSpan)
		{
			ReportUnusedImports(tree, instance, cancellationToken);
		}
		return instance.ToReadOnlyAndFree();
	}

	private ImmutableArray<Diagnostic> GetSourceDeclarationDiagnostics(SyntaxTree syntaxTree = null, TextSpan? filterSpanWithinTree = null, Func<IEnumerable<Diagnostic>, SyntaxTree, TextSpan?, IEnumerable<Diagnostic>> locationFilterOpt = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		GlobalImports.Complete(cancellationToken);
		SourceLocation locationOpt = null;
		if (syntaxTree != null)
		{
			SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
			locationOpt = (filterSpanWithinTree.HasValue ? new SourceLocation(syntaxTree, filterSpanWithinTree.Value) : new SourceLocation(root));
		}
		Assembly.ForceComplete(locationOpt, cancellationToken);
		if (syntaxTree == null)
		{
			_declarationDiagnosticsFrozen = true;
			_needsGeneratedAttributes_IsFrozen = true;
		}
		IEnumerable<Diagnostic> enumerable = _lazyDeclarationDiagnostics?.AsEnumerable() ?? Enumerable.Empty<Diagnostic>();
		if (locationFilterOpt != null)
		{
			enumerable = locationFilterOpt(enumerable, syntaxTree, filterSpanWithinTree);
		}
		ImmutableArray<Diagnostic> clsComplianceDiagnostics = GetClsComplianceDiagnostics(syntaxTree, filterSpanWithinTree, cancellationToken);
		return enumerable.AsImmutable().Concat(clsComplianceDiagnostics);
	}

	private ImmutableArray<Diagnostic> GetClsComplianceDiagnostics(SyntaxTree syntaxTree, TextSpan? filterSpanWithinTree, CancellationToken cancellationToken)
	{
		if (syntaxTree != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			ClsComplianceChecker.CheckCompliance(this, instance, cancellationToken, syntaxTree, filterSpanWithinTree);
			return instance.ToReadOnlyAndFree();
		}
		if (_lazyClsComplianceDiagnostics.IsDefault)
		{
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			ClsComplianceChecker.CheckCompliance(this, instance2, cancellationToken);
			ImmutableInterlocked.InterlockedInitialize(ref _lazyClsComplianceDiagnostics, instance2.ToReadOnlyAndFree());
		}
		return _lazyClsComplianceDiagnostics;
	}

	private static IEnumerable<Diagnostic> FilterDiagnosticsByLocation(IEnumerable<Diagnostic> diagnostics, SyntaxTree tree, TextSpan? filterSpanWithinTree)
	{
		foreach (Diagnostic diagnostic in diagnostics)
		{
			if (diagnostic.HasIntersectingLocation(tree, filterSpanWithinTree))
			{
				yield return diagnostic;
			}
		}
	}

	internal ImmutableArray<Diagnostic> GetDiagnosticsForSyntaxTree(CompilationStage stage, SyntaxTree syntaxTree, TextSpan? filterSpanWithinTree, bool includeEarlierStages, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		if (stage == CompilationStage.Parse || ((stage > CompilationStage.Parse) & includeEarlierStages))
		{
			AppendLoadDirectiveDiagnostics(incoming, _syntaxAndDeclarations, syntaxTree, (IEnumerable<Diagnostic> diagnostics3) => FilterDiagnosticsByLocation(diagnostics3, syntaxTree, filterSpanWithinTree));
			IEnumerable<Diagnostic> diagnostics = syntaxTree.GetDiagnostics();
			diagnostics = FilterDiagnosticsByLocation(diagnostics, syntaxTree, filterSpanWithinTree);
			incoming.AddRange(diagnostics);
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Declare || ((stage > CompilationStage.Declare) & includeEarlierStages))
		{
			ImmutableArray<Diagnostic> sourceDeclarationDiagnostics = GetSourceDeclarationDiagnostics(syntaxTree, filterSpanWithinTree, FilterDiagnosticsByLocation, cancellationToken);
			incoming.AddRange(sourceDeclarationDiagnostics);
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (stage == CompilationStage.Compile || ((stage > CompilationStage.Compile) & includeEarlierStages))
		{
			IEnumerable<Diagnostic> diagnostics2 = GetDiagnosticsForMethodBodiesInTree(syntaxTree, filterSpanWithinTree, cancellationToken);
			diagnostics2 = FilterDiagnosticsByLocation(diagnostics2, syntaxTree, filterSpanWithinTree);
			incoming.AddRange(diagnostics2);
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		FilterAndAppendAndFreeDiagnostics(instance, ref incoming);
		return instance.ToReadOnlyAndFree<Diagnostic>();
	}

	protected override void AppendDefaultVersionResource(Stream resourceStream)
	{
		SourceAssemblySymbol sourceAssembly = SourceAssembly;
		string text = sourceAssembly.FileVersion ?? sourceAssembly.Identity.Version.ToString();
		Win32ResourceConversions.AppendVersionToResourceStream(resourceStream, !Options.OutputKind.IsApplication(), text, SourceModule.Name, SourceModule.Name, sourceAssembly.InformationalVersion ?? text, fileDescription: sourceAssembly.Title ?? " ", assemblyVersion: sourceAssembly.Identity.Version, legalCopyright: sourceAssembly.Copyright ?? " ", legalTrademarks: sourceAssembly.Trademark, productName: sourceAssembly.Product, comments: sourceAssembly.Description, companyName: sourceAssembly.Company);
	}

	internal override CommonPEModuleBuilder CreateModuleBuilder(EmitOptions emitOptions, IMethodSymbol debugEntryPoint, Stream sourceLinkStream, IEnumerable<EmbeddedText> embeddedTexts, IEnumerable<ResourceDescription> manifestResources, CompilationTestData testData, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		string runtimeMetadataVersion = GetRuntimeMetadataVersion(emitOptions, diagnostics);
		if (runtimeMetadataVersion == null)
		{
			return null;
		}
		ModulePropertiesForSerialization serializationProperties = ConstructModuleSerializationProperties(emitOptions, runtimeMetadataVersion);
		if (manifestResources == null)
		{
			manifestResources = SpecializedCollections.EmptyEnumerable<ResourceDescription>();
		}
		PEModuleBuilder pEModuleBuilder;
		if (_options.OutputKind.IsNetModule())
		{
			pEModuleBuilder = new PENetModuleBuilder((SourceModuleSymbol)SourceModule, emitOptions, serializationProperties, manifestResources);
		}
		else
		{
			OutputKind outputKind = (_options.OutputKind.IsValid() ? _options.OutputKind : OutputKind.DynamicallyLinkedLibrary);
			pEModuleBuilder = new PEAssemblyBuilder(SourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources);
		}
		if (debugEntryPoint != null)
		{
			pEModuleBuilder.SetDebugEntryPoint((MethodSymbol)debugEntryPoint, diagnostics);
		}
		pEModuleBuilder.SourceLinkStreamOpt = sourceLinkStream;
		if (embeddedTexts != null)
		{
			pEModuleBuilder.EmbeddedTexts = embeddedTexts;
		}
		if (testData != null)
		{
			pEModuleBuilder.SetMethodTestData(testData.Methods);
			testData.Module = pEModuleBuilder;
		}
		return pEModuleBuilder;
	}

	internal override bool CompileMethods(CommonPEModuleBuilder moduleBuilder, bool emittingPdb, bool emitMetadataOnly, bool emitTestCoverageData, DiagnosticBag diagnostics, Predicate<ISymbol> filterOpt, CancellationToken cancellationToken)
	{
		PooledHashSet<int> pooledHashSet = null;
		if (emitMetadataOnly)
		{
			pooledHashSet = PooledHashSet<int>.GetInstance();
			pooledHashSet.Add(501);
		}
		bool flag = !FilterAndAppendDiagnostics(diagnostics, GetDiagnostics(CompilationStage.Declare, includeEarlierStages: true, cancellationToken), pooledHashSet);
		pooledHashSet?.Free();
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)moduleBuilder;
		if (emitMetadataOnly)
		{
			if (flag)
			{
				return false;
			}
			if (pEModuleBuilder.SourceModule.HasBadAttributes)
			{
				diagnostics.Add(ErrorCode.ERR_ModuleEmitFailure, NoLocation.Singleton, ((INamedEntity)pEModuleBuilder).Name);
				return false;
			}
			SynthesizedMetadataCompiler.ProcessSynthesizedMembers(this, pEModuleBuilder, cancellationToken);
		}
		else
		{
			if ((emittingPdb | emitTestCoverageData) && !CreateDebugDocuments(pEModuleBuilder.DebugDocumentsBuilder, pEModuleBuilder.EmbeddedTexts, diagnostics))
			{
				return false;
			}
			DiagnosticBag incoming = DiagnosticBag.GetInstance();
			MethodCompiler.CompileMethodBodies(this, pEModuleBuilder, emittingPdb, emitTestCoverageData, flag, incoming, filterOpt, cancellationToken);
			bool flag2 = !FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming);
			if (flag | flag2)
			{
				return false;
			}
		}
		return true;
	}

	internal override bool GenerateResourcesAndDocumentationComments(CommonPEModuleBuilder moduleBuilder, Stream xmlDocStream, Stream win32Resources, string outputNameOverride, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		SetupWin32Resources(moduleBuilder, win32Resources, incoming);
		ReportManifestResourceDuplicates(moduleBuilder.ManifestResources, from m in SourceAssembly.Modules.Skip(1)
			select m.Name, AddedModulesResourceNames(incoming), incoming);
		if (!FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming))
		{
			return false;
		}
		cancellationToken.ThrowIfCancellationRequested();
		DiagnosticBag incoming2 = DiagnosticBag.GetInstance();
		string assemblyName = FileNameUtilities.ChangeExtension(outputNameOverride, null);
		DocumentationCommentCompiler.WriteDocumentationCommentXml(this, assemblyName, xmlDocStream, incoming2, cancellationToken);
		return FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming2);
	}

	private IEnumerable<string> AddedModulesResourceNames(DiagnosticBag diagnostics)
	{
		ImmutableArray<ModuleSymbol> modules = SourceAssembly.Modules;
		for (int i = 1; i < modules.Length; i++)
		{
			PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)modules[i];
			ImmutableArray<EmbeddedResource> embeddedResourcesOrThrow;
			try
			{
				embeddedResourcesOrThrow = pEModuleSymbol.Module.GetEmbeddedResourcesOrThrow();
			}
			catch (BadImageFormatException)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, pEModuleSymbol), NoLocation.Singleton);
				continue;
			}
			foreach (EmbeddedResource item in embeddedResourcesOrThrow)
			{
				yield return item.Name;
			}
		}
	}

	internal override EmitDifferenceResult EmitDifference(EmitBaseline baseline, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol, Stream metadataStream, Stream ilStream, Stream pdbStream, ICollection<MethodDefinitionHandle> updatedMethods, CompilationTestData testData, CancellationToken cancellationToken)
	{
		return EmitHelpers.EmitDifference(this, baseline, edits, isAddedSymbol, metadataStream, ilStream, pdbStream, updatedMethods, testData, cancellationToken);
	}

	internal string GetRuntimeMetadataVersion(EmitOptions emitOptions, DiagnosticBag diagnostics)
	{
		string runtimeMetadataVersion = GetRuntimeMetadataVersion(emitOptions);
		if (runtimeMetadataVersion != null)
		{
			return runtimeMetadataVersion;
		}
		DiagnosticBag incoming = DiagnosticBag.GetInstance();
		incoming.Add(ErrorCode.WRN_NoRuntimeMetadataVersion, NoLocation.Singleton);
		if (!FilterAndAppendAndFreeDiagnostics(diagnostics, ref incoming))
		{
			return null;
		}
		return string.Empty;
	}

	private string GetRuntimeMetadataVersion(EmitOptions emitOptions)
	{
		if (Assembly.CorLibrary is PEAssemblySymbol pEAssemblySymbol)
		{
			return pEAssemblySymbol.Assembly.ManifestModule.MetadataVersion;
		}
		return emitOptions.RuntimeMetadataVersion;
	}

	internal override void AddDebugSourceDocumentsForChecksumDirectives(DebugDocumentsBuilder documentsBuilder, SyntaxTree tree, DiagnosticBag diagnostics)
	{
		foreach (PragmaChecksumDirectiveTriviaSyntax directive in tree.GetRoot().GetDirectives((DirectiveTriviaSyntax d) => d.Kind() == SyntaxKind.PragmaChecksumDirectiveTrivia && !d.ContainsDiagnostics))
		{
			string valueText = directive.File.ValueText;
			string valueText2 = directive.Bytes.ValueText;
			string text = documentsBuilder.NormalizeDebugDocumentPath(valueText, tree.FilePath);
			DebugSourceDocument debugSourceDocument = documentsBuilder.TryGetDebugDocumentForNormalizedPath(text);
			if (debugSourceDocument != null)
			{
				if (!debugSourceDocument.IsComputedChecksum)
				{
					DebugSourceInfo sourceInfo = debugSourceDocument.GetSourceInfo();
					if (!ChecksumMatches(valueText2, sourceInfo.Checksum) || !(Guid.Parse(directive.Guid.ValueText) == sourceInfo.ChecksumAlgorithmId))
					{
						diagnostics.Add(ErrorCode.WRN_ConflictingChecksum, new SourceLocation(directive), valueText);
					}
				}
			}
			else
			{
				DebugSourceDocument document = new DebugSourceDocument(text, DebugSourceDocument.CorSymLanguageTypeCSharp, MakeChecksumBytes(directive.Bytes.ValueText), Guid.Parse(directive.Guid.ValueText));
				documentsBuilder.AddDebugDocument(document);
			}
		}
	}

	private static bool ChecksumMatches(string bytesText, ImmutableArray<byte> bytes)
	{
		if (bytesText.Length != bytes.Length * 2)
		{
			return false;
		}
		int i = 0;
		for (int num = bytesText.Length / 2; i < num; i++)
		{
			if (SyntaxFacts.HexValue(bytesText[i * 2]) * 16 + SyntaxFacts.HexValue(bytesText[i * 2 + 1]) != bytes[i])
			{
				return false;
			}
		}
		return true;
	}

	private static ImmutableArray<byte> MakeChecksumBytes(string bytesText)
	{
		int num = bytesText.Length / 2;
		ArrayBuilder<byte> instance = ArrayBuilder<byte>.GetInstance(num);
		for (int i = 0; i < num; i++)
		{
			int num2 = SyntaxFacts.HexValue(bytesText[i * 2]) * 16 + SyntaxFacts.HexValue(bytesText[i * 2 + 1]);
			instance.Add((byte)num2);
		}
		return instance.ToImmutableAndFree();
	}

	internal override bool HasCodeToEmit()
	{
		foreach (SyntaxTree syntaxTree in SyntaxTrees)
		{
			if (syntaxTree.GetCompilationUnitRoot().Members.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	protected override Compilation CommonWithReferences(IEnumerable<MetadataReference> newReferences)
	{
		return WithReferences(newReferences);
	}

	protected override Compilation CommonWithAssemblyName(string assemblyName)
	{
		return WithAssemblyName(assemblyName);
	}

	protected override SemanticModel CommonGetSemanticModel(SyntaxTree syntaxTree, bool ignoreAccessibility)
	{
		return GetSemanticModel(syntaxTree, ignoreAccessibility);
	}

	protected override Compilation CommonAddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return AddSyntaxTrees(trees);
	}

	protected override Compilation CommonRemoveSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		return RemoveSyntaxTrees(trees);
	}

	protected override Compilation CommonRemoveAllSyntaxTrees()
	{
		return RemoveAllSyntaxTrees();
	}

	protected override Compilation CommonReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		return ReplaceSyntaxTree(oldTree, newTree);
	}

	protected override Compilation CommonWithOptions(CompilationOptions options)
	{
		return WithOptions((CSharpCompilationOptions)options);
	}

	protected override Compilation CommonWithScriptCompilationInfo(ScriptCompilationInfo info)
	{
		return WithScriptCompilationInfo((CSharpScriptCompilationInfo)info);
	}

	protected override bool CommonContainsSyntaxTree(SyntaxTree syntaxTree)
	{
		return ContainsSyntaxTree(syntaxTree);
	}

	protected override ISymbol CommonGetAssemblyOrModuleSymbol(MetadataReference reference)
	{
		return GetAssemblyOrModuleSymbol(reference);
	}

	protected override Compilation CommonClone()
	{
		return Clone();
	}

	protected override INamedTypeSymbol CommonGetSpecialType(SpecialType specialType)
	{
		return GetSpecialType(specialType);
	}

	protected override INamespaceSymbol CommonGetCompilationNamespace(INamespaceSymbol namespaceSymbol)
	{
		return GetCompilationNamespace(namespaceSymbol);
	}

	protected override INamedTypeSymbol CommonGetTypeByMetadataName(string metadataName)
	{
		return GetTypeByMetadataName(metadataName);
	}

	protected override IArrayTypeSymbol CommonCreateArrayTypeSymbol(ITypeSymbol elementType, int rank)
	{
		return CreateArrayTypeSymbol(elementType.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("elementType"), rank);
	}

	protected override IPointerTypeSymbol CommonCreatePointerTypeSymbol(ITypeSymbol elementType)
	{
		return CreatePointerTypeSymbol(elementType.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("elementType"));
	}

	protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(ImmutableArray<ITypeSymbol> elementTypes, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(elementTypes.Length);
		for (int i = 0; i < elementTypes.Length; i++)
		{
			instance.Add(elementTypes[i].EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>(string.Format("{0}[{1}]", "elementTypes", i)));
		}
		return TupleTypeSymbol.Create(null, instance.ToImmutableAndFree(), elementLocations, elementNames, this, shouldCheckConstraints: false, default);
	}

	protected override INamedTypeSymbol CommonCreateTupleTypeSymbol(INamedTypeSymbol underlyingType, ImmutableArray<string> elementNames, ImmutableArray<Location> elementLocations)
	{
		NamedTypeSymbol namedTypeSymbol = underlyingType.EnsureCSharpSymbolOrNull<INamedTypeSymbol, NamedTypeSymbol>("underlyingType");
		if (!namedTypeSymbol.IsTupleCompatible(out var tupleCardinality))
		{
			throw new ArgumentException(CodeAnalysisResources.TupleUnderlyingTypeMustBeTupleCompatible, "underlyingType");
		}
		elementNames = Compilation.CheckTupleElementNames(tupleCardinality, elementNames);
		Compilation.CheckTupleElementLocations(tupleCardinality, elementLocations);
		return TupleTypeSymbol.Create(namedTypeSymbol, elementNames, default, null, elementLocations);
	}

	protected override INamedTypeSymbol CommonCreateAnonymousTypeSymbol(ImmutableArray<ITypeSymbol> memberTypes, ImmutableArray<string> memberNames, ImmutableArray<Location> memberLocations, ImmutableArray<bool> memberIsReadOnly)
	{
		int i = 0;
		for (int length = memberTypes.Length; i < length; i++)
		{
			memberTypes[i].EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>(string.Format("{0}[{1}]", "memberTypes", i));
		}
		if (!memberIsReadOnly.IsDefault && memberIsReadOnly.Any((bool v) => !v))
		{
			throw new ArgumentException("Non-ReadOnly members are not supported in C# anonymous types.");
		}
		ArrayBuilder<AnonymousTypeField> instance = ArrayBuilder<AnonymousTypeField>.GetInstance();
		int num = 0;
		for (int length2 = memberTypes.Length; num < length2; num++)
		{
			ITypeSymbol typeSymbol = memberTypes[num];
			string name = memberNames[num];
			Location location = (memberLocations.IsDefault ? Location.None : memberLocations[num]);
			instance.Add(new AnonymousTypeField(name, location, (TypeSymbol)typeSymbol));
		}
		AnonymousTypeDescriptor typeDescr = new AnonymousTypeDescriptor(instance.ToImmutableAndFree(), Location.None);
		return AnonymousTypeManager.ConstructAnonymousTypeSymbol(typeDescr);
	}

	protected override IMethodSymbol CommonGetEntryPoint(CancellationToken cancellationToken)
	{
		return GetEntryPoint(cancellationToken);
	}

	internal override int CompareSourceLocations(Location loc1, Location loc2)
	{
		int num = CompareSyntaxTreeOrdering(loc1.SourceTree, loc2.SourceTree);
		if (num != 0)
		{
			return num;
		}
		return loc1.SourceSpan.Start - loc2.SourceSpan.Start;
	}

	internal override int CompareSourceLocations(SyntaxReference loc1, SyntaxReference loc2)
	{
		int num = CompareSyntaxTreeOrdering(loc1.SyntaxTree, loc2.SyntaxTree);
		if (num != 0)
		{
			return num;
		}
		return loc1.Span.Start - loc2.Span.Start;
	}

	public override bool ContainsSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return DeclarationTable.ContainsName(MergedRootDeclaration, predicate, filter, cancellationToken);
	}

	public override IEnumerable<ISymbol> GetSymbolsWithName(Func<string, bool> predicate, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return new PredicateSymbolSearcher(this, filter, predicate, cancellationToken).GetSymbolsWithName();
	}

	public override bool ContainsSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return DeclarationTable.ContainsName(MergedRootDeclaration, name, filter, cancellationToken);
	}

	public override IEnumerable<ISymbol> GetSymbolsWithName(string name, SymbolFilter filter = SymbolFilter.TypeAndMember, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (filter == SymbolFilter.None)
		{
			throw new ArgumentException(CSharpResources.NoNoneSearchCriteria, "filter");
		}
		return new NameSymbolSearcher(this, filter, name, cancellationToken).GetSymbolsWithName();
	}

	internal bool HasDynamicEmitAttributes()
	{
		if ((object)GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctor) != null)
		{
			return (object)GetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctorTransformFlags) != null;
		}
		return false;
	}

	internal bool CanEmitBoolean()
	{
		return CanEmitSpecialType(SpecialType.System_Boolean);
	}

	internal bool CanEmitSpecialType(SpecialType type)
	{
		DiagnosticInfo useSiteDiagnostic = GetSpecialType(type).GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			return useSiteDiagnostic.Severity != DiagnosticSeverity.Error;
		}
		return true;
	}

	internal override AnalyzerDriver AnalyzerForLanguage(ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerManager analyzerManager)
	{
		Func<SyntaxNode, SyntaxKind> getKind = (SyntaxNode node) => node.Kind();
		Func<SyntaxTrivia, bool> isComment = (SyntaxTrivia trivia) => trivia.Kind() == SyntaxKind.SingleLineCommentTrivia || trivia.Kind() == SyntaxKind.MultiLineCommentTrivia;
		return new AnalyzerDriver<SyntaxKind>(analyzers, getKind, analyzerManager, isComment);
	}

	internal void SymbolDeclaredEvent(Symbol symbol)
	{
		EventQueue?.TryEnqueue(new SymbolDeclaredCompilationEvent(this, symbol));
	}

	internal Symbol GetWellKnownTypeMember(WellKnownMember member)
	{
		if (IsMemberMissing(member))
		{
			return null;
		}
		if (_lazyWellKnownTypeMembers == null || (object)_lazyWellKnownTypeMembers[(int)member] == ErrorTypeSymbol.UnknownResultType)
		{
			if (_lazyWellKnownTypeMembers == null)
			{
				Symbol[] array = new Symbol[362];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = ErrorTypeSymbol.UnknownResultType;
				}
				Interlocked.CompareExchange(ref _lazyWellKnownTypeMembers, array, null);
			}
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(member);
			NamedTypeSymbol namedTypeSymbol = ((descriptor.DeclaringTypeId <= 43) ? GetSpecialType((SpecialType)descriptor.DeclaringTypeId) : GetWellKnownType((WellKnownType)descriptor.DeclaringTypeId));
			Symbol value = null;
			if (!namedTypeSymbol.IsErrorType())
			{
				value = GetRuntimeMember(namedTypeSymbol, ref descriptor, WellKnownMemberSignatureComparer, Assembly);
			}
			Interlocked.CompareExchange(ref _lazyWellKnownTypeMembers[(int)member], value, ErrorTypeSymbol.UnknownResultType);
		}
		return _lazyWellKnownTypeMembers[(int)member];
	}

	internal NamedTypeSymbol GetWellKnownType(WellKnownType type)
	{
		bool ignoreCorLibraryDuplicatedTypes = Options.TopLevelBinderFlags.Includes(BinderFlags.IgnoreCorLibraryDuplicatedTypes);
		int num = (int)(type - 44);
		if (_lazyWellKnownTypes == null || (object)_lazyWellKnownTypes[num] == null)
		{
			if (_lazyWellKnownTypes == null)
			{
				Interlocked.CompareExchange(ref _lazyWellKnownTypes, new NamedTypeSymbol[226], null);
			}
			string metadataName = type.GetMetadataName();
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			(AssemblySymbol, AssemblySymbol) conflicts = default;
			NamedTypeSymbol namedTypeSymbol;
			if (IsTypeMissing(type))
			{
				namedTypeSymbol = null;
			}
			else
			{
				DiagnosticBag warnings = ((type <= WellKnownType.System_IFormatProvider) ? instance : null);
				namedTypeSymbol = Assembly.GetTypeByMetadataName(metadataName, includeReferences: true, isWellKnownType: true, out conflicts, useCLSCompliantNameArityEncoding: true, warnings, ignoreCorLibraryDuplicatedTypes);
			}
			if ((object)namedTypeSymbol == null)
			{
				MetadataTypeName fullName = MetadataTypeName.FromFullName(metadataName, useCLSCompliantNameArityEncoding: true);
				namedTypeSymbol = ((!type.IsValueTupleType()) ? new MissingMetadataTypeSymbol.TopLevel(Assembly.Modules[0], ref fullName, type) : new MissingMetadataTypeSymbol.TopLevelWithCustomErrorInfo(errorInfo: ((object)conflicts.Item1 != null) ? new CSDiagnosticInfo(ErrorCode.ERR_PredefinedValueTupleTypeAmbiguous3, fullName.FullName, conflicts.Item1, conflicts.Item2) : new CSDiagnosticInfo(ErrorCode.ERR_PredefinedValueTupleTypeNotFound, fullName.FullName), module: Assembly.Modules[0], emittedName: ref fullName, typeId: type));
			}
			if ((object)Interlocked.CompareExchange(ref _lazyWellKnownTypes[num], namedTypeSymbol, null) == null)
			{
				AdditionalCodegenWarnings.AddRange(instance);
			}
			instance.Free();
		}
		return _lazyWellKnownTypes[num];
	}

	internal bool IsAttributeType(TypeSymbol type)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return IsEqualOrDerivedFromWellKnownClass(type, WellKnownType.System_Attribute, ref useSiteDiagnostics);
	}

	internal override bool IsAttributeType(ITypeSymbol type)
	{
		return IsAttributeType((TypeSymbol)type);
	}

	internal bool IsExceptionType(TypeSymbol type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return IsEqualOrDerivedFromWellKnownClass(type, WellKnownType.System_Exception, ref useSiteDiagnostics);
	}

	internal bool IsReadOnlySpanType(TypeSymbol type)
	{
		return type.OriginalDefinition == GetWellKnownType(WellKnownType.System_ReadOnlySpan_T);
	}

	internal bool IsEqualOrDerivedFromWellKnownClass(TypeSymbol type, WellKnownType wellKnownType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (type.Kind != SymbolKind.NamedType || type.TypeKind != TypeKind.Class)
		{
			return false;
		}
		NamedTypeSymbol wellKnownType2 = GetWellKnownType(wellKnownType);
		if (!type.Equals(wellKnownType2))
		{
			return type.IsDerivedFrom(wellKnownType2, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics);
		}
		return true;
	}

	internal override bool IsSystemTypeReference(ITypeSymbol type)
	{
		return (TypeSymbol)type == GetWellKnownType(WellKnownType.System_Type);
	}

	internal override ISymbol CommonGetWellKnownTypeMember(WellKnownMember member)
	{
		return GetWellKnownTypeMember(member);
	}

	internal static Symbol GetRuntimeMember(NamedTypeSymbol declaringType, ref MemberDescriptor descriptor, SignatureComparer<MethodSymbol, FieldSymbol, PropertySymbol, TypeSymbol, ParameterSymbol> comparer, AssemblySymbol accessWithinOpt)
	{
		Symbol symbol = null;
		MethodKind methodKind = MethodKind.Ordinary;
		bool flag = (descriptor.Flags & MemberFlags.Static) != 0;
		SymbolKind symbolKind;
		switch (descriptor.Flags & MemberFlags.KindMask)
		{
		case MemberFlags.Constructor:
			symbolKind = SymbolKind.Method;
			methodKind = MethodKind.Constructor;
			break;
		case MemberFlags.Method:
			symbolKind = SymbolKind.Method;
			break;
		case MemberFlags.PropertyGet:
			symbolKind = SymbolKind.Method;
			methodKind = MethodKind.PropertyGet;
			break;
		case MemberFlags.Field:
			symbolKind = SymbolKind.Field;
			break;
		case MemberFlags.Property:
			symbolKind = SymbolKind.Property;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(descriptor.Flags);
		}
		foreach (Symbol member in declaringType.GetMembers(descriptor.Name))
		{
			if (member.Kind != symbolKind || member.IsStatic != flag || (member.DeclaredAccessibility != Accessibility.Public && ((object)accessWithinOpt == null || !Symbol.IsSymbolAccessible(member, accessWithinOpt))))
			{
				continue;
			}
			switch (symbolKind)
			{
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)member;
				MethodKind methodKind2 = methodSymbol.MethodKind;
				if (methodKind2 == MethodKind.Conversion || methodKind2 == MethodKind.UserDefinedOperator)
				{
					methodKind2 = MethodKind.Ordinary;
				}
				if (methodSymbol.Arity != descriptor.Arity || methodKind2 != methodKind || (descriptor.Flags & MemberFlags.Virtual) != 0 != (methodSymbol.IsVirtual || methodSymbol.IsOverride || methodSymbol.IsAbstract) || !comparer.MatchMethodSignature(methodSymbol, descriptor.Signature))
				{
					continue;
				}
				break;
			}
			case SymbolKind.Property:
			{
				PropertySymbol propertySymbol = (PropertySymbol)member;
				if ((descriptor.Flags & MemberFlags.Virtual) != 0 != (propertySymbol.IsVirtual || propertySymbol.IsOverride || propertySymbol.IsAbstract) || !comparer.MatchPropertySignature(propertySymbol, descriptor.Signature))
				{
					continue;
				}
				break;
			}
			case SymbolKind.Field:
				if (!comparer.MatchFieldSignature((FieldSymbol)member, descriptor.Signature))
				{
					continue;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbolKind);
			}
			if ((object)symbol != null)
			{
				symbol = null;
				break;
			}
			symbol = member;
		}
		return symbol;
	}

	internal SynthesizedAttributeData TrySynthesizeAttribute(WellKnownMember constructor, ImmutableArray<TypedConstant> arguments = default(ImmutableArray<TypedConstant>), ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>> namedArguments = default(ImmutableArray<KeyValuePair<WellKnownMember, TypedConstant>>), bool isOptionalUse = false)
	{
		MethodSymbol methodSymbol = (MethodSymbol)Binder.GetWellKnownTypeMember(this, constructor, out var diagnosticInfo, isOptional: true);
		if ((object)methodSymbol == null)
		{
			return null;
		}
		if (arguments.IsDefault)
		{
			arguments = ImmutableArray<TypedConstant>.Empty;
		}
		ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments2;
		if (namedArguments.IsDefault)
		{
			namedArguments2 = ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
		}
		else
		{
			ArrayBuilder<KeyValuePair<string, TypedConstant>> arrayBuilder = new ArrayBuilder<KeyValuePair<string, TypedConstant>>(namedArguments.Length);
			foreach (KeyValuePair<WellKnownMember, TypedConstant> item in namedArguments)
			{
				Symbol wellKnownTypeMember = Binder.GetWellKnownTypeMember(this, item.Key, out diagnosticInfo, isOptional: true);
				if (wellKnownTypeMember == null || wellKnownTypeMember is ErrorTypeSymbol)
				{
					return null;
				}
				arrayBuilder.Add(new KeyValuePair<string, TypedConstant>(wellKnownTypeMember.Name, item.Value));
			}
			namedArguments2 = arrayBuilder.ToImmutableAndFree();
		}
		return new SynthesizedAttributeData(methodSymbol, arguments, namedArguments2);
	}

	internal SynthesizedAttributeData SynthesizeDecimalConstantAttribute(decimal value)
	{
		value.GetBits(out var isNegative, out var scale, out var low, out var mid, out var high);
		NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Byte);
		NamedTypeSymbol specialType2 = GetSpecialType(SpecialType.System_UInt32);
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor, ImmutableArray.Create<TypedConstant>(new TypedConstant(specialType, TypedConstantKind.Primitive, scale), new TypedConstant(specialType, TypedConstantKind.Primitive, (byte)(isNegative ? 128u : 0u)), new TypedConstant(specialType2, TypedConstantKind.Primitive, high), new TypedConstant(specialType2, TypedConstantKind.Primitive, mid), new TypedConstant(specialType2, TypedConstantKind.Primitive, low)));
	}

	internal SynthesizedAttributeData SynthesizeDebuggerBrowsableNeverAttribute()
	{
		if (Options.OptimizationLevel != OptimizationLevel.Debug)
		{
			return null;
		}
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerBrowsableAttribute__ctor, ImmutableArray.Create(new TypedConstant(GetWellKnownType(WellKnownType.System_Diagnostics_DebuggerBrowsableState), TypedConstantKind.Enum, DebuggerBrowsableState.Never)));
	}

	internal SynthesizedAttributeData SynthesizeDebuggerStepThroughAttribute()
	{
		if (Options.OptimizationLevel != OptimizationLevel.Debug)
		{
			return null;
		}
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerStepThroughAttribute__ctor);
	}

	internal void EnsureIsReadOnlyAttributeExists(DiagnosticBag diagnostics, Location location, bool modifyCompilationForRefReadOnly)
	{
		if (CheckIfIsReadOnlyAttributeShouldBeEmbedded(diagnostics, location) & modifyCompilationForRefReadOnly)
		{
			_needsGeneratedIsReadOnlyAttribute_Value = true;
		}
	}

	internal void EnsureIsByRefLikeAttributeExists(DiagnosticBag diagnostics, Location location, bool modifyCompilationForIsByRefLike)
	{
		if (CheckIfIsByRefLikeAttributeShouldBeEmbedded(diagnostics, location) & modifyCompilationForIsByRefLike)
		{
			_needsGeneratedIsByRefLikeAttribute_Value = true;
		}
	}

	internal void EnsureIsUnmanagedAttributeExists(DiagnosticBag diagnostics, Location location, bool modifyCompilationForIsUnmanaged)
	{
		if (CheckIfIsUnmanagedAttributeShouldBeEmbedded(diagnostics, location) & modifyCompilationForIsUnmanaged)
		{
			_needsGeneratedIsUnmanagedAttribute_Value = true;
		}
	}

	internal bool CheckIfIsReadOnlyAttributeShouldBeEmbedded(DiagnosticBag diagnosticsOpt, Location locationOpt)
	{
		return CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsReadOnlyAttribute, WellKnownMember.System_Runtime_CompilerServices_IsReadOnlyAttribute__ctor);
	}

	internal bool CheckIfIsByRefLikeAttributeShouldBeEmbedded(DiagnosticBag diagnosticsOpt, Location locationOpt)
	{
		return CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsByRefLikeAttribute, WellKnownMember.System_Runtime_CompilerServices_IsByRefLikeAttribute__ctor);
	}

	internal bool CheckIfIsUnmanagedAttributeShouldBeEmbedded(DiagnosticBag diagnosticsOpt, Location locationOpt)
	{
		return CheckIfAttributeShouldBeEmbedded(diagnosticsOpt, locationOpt, WellKnownType.System_Runtime_CompilerServices_IsUnmanagedAttribute, WellKnownMember.System_Runtime_CompilerServices_IsUnmanagedAttribute__ctor);
	}

	private bool CheckIfAttributeShouldBeEmbedded(DiagnosticBag diagnosticsOpt, Location locationOpt, WellKnownType attributeType, WellKnownMember attributeCtor)
	{
		NamedTypeSymbol wellKnownType = GetWellKnownType(attributeType);
		if (wellKnownType is MissingMetadataTypeSymbol)
		{
			if (Options.OutputKind != OutputKind.NetModule)
			{
				return true;
			}
			if (diagnosticsOpt != null)
			{
				Binder.ReportUseSiteDiagnostics(wellKnownType, diagnosticsOpt, locationOpt);
			}
		}
		else if (diagnosticsOpt != null)
		{
			Binder.GetWellKnownTypeMember(this, attributeCtor, diagnosticsOpt, locationOpt);
		}
		return false;
	}

	internal SynthesizedAttributeData SynthesizeDebuggableAttribute()
	{
		if (GetWellKnownType(WellKnownType.System_Diagnostics_DebuggableAttribute) is MissingMetadataTypeSymbol)
		{
			return null;
		}
		TypeSymbol wellKnownType = GetWellKnownType(WellKnownType.System_Diagnostics_DebuggableAttribute__DebuggingModes);
		if (wellKnownType is MissingMetadataTypeSymbol)
		{
			return null;
		}
		FieldSymbol fieldSymbol = (FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__IgnoreSymbolStoreSequencePoints);
		if ((object)fieldSymbol == null || !fieldSymbol.HasConstantValue)
		{
			return null;
		}
		int num = fieldSymbol.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		if (_options.OptimizationLevel == OptimizationLevel.Debug)
		{
			FieldSymbol fieldSymbol2 = (FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__Default);
			if ((object)fieldSymbol2 == null || !fieldSymbol2.HasConstantValue)
			{
				return null;
			}
			FieldSymbol fieldSymbol3 = (FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__DisableOptimizations);
			if ((object)fieldSymbol3 == null || !fieldSymbol3.HasConstantValue)
			{
				return null;
			}
			num |= fieldSymbol2.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
			num |= fieldSymbol3.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		}
		if (_options.EnableEditAndContinue)
		{
			FieldSymbol fieldSymbol4 = (FieldSymbol)GetWellKnownTypeMember(WellKnownMember.System_Diagnostics_DebuggableAttribute_DebuggingModes__EnableEditAndContinue);
			if ((object)fieldSymbol4 == null || !fieldSymbol4.HasConstantValue)
			{
				return null;
			}
			num |= fieldSymbol4.GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false).Int32Value;
		}
		TypedConstant item = new TypedConstant(wellKnownType, TypedConstantKind.Enum, num);
		return TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggableAttribute__ctorDebuggingModes, ImmutableArray.Create(item));
	}

	internal SynthesizedAttributeData SynthesizeDynamicAttribute(TypeSymbol type, int customModifiersCount, RefKind refKindOpt = RefKind.None)
	{
		if (type.IsDynamic() && refKindOpt == RefKind.None && customModifiersCount == 0)
		{
			return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctor);
		}
		NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean);
		ImmutableArray<TypedConstant> array = DynamicTransformsEncoder.Encode(type, refKindOpt, customModifiersCount, specialType);
		ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, specialType, ImmutableArray<CustomModifier>.Empty), array));
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_DynamicAttribute__ctorTransformFlags, arguments);
	}

	internal SynthesizedAttributeData SynthesizeTupleNamesAttribute(TypeSymbol type)
	{
		NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_String);
		ImmutableArray<TypedConstant> array = TupleNamesEncoder.Encode(type, specialType);
		ImmutableArray<TypedConstant> arguments = ImmutableArray.Create(new TypedConstant(ArrayTypeSymbol.CreateSZArray(specialType.ContainingAssembly, specialType, ImmutableArray<CustomModifier>.Empty), array));
		return TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_TupleElementNamesAttribute__ctorTransformNames, arguments);
	}
}
