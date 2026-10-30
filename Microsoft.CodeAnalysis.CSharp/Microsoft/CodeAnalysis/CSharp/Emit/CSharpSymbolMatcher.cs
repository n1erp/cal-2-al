using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class CSharpSymbolMatcher : SymbolMatcher
{
	private abstract class MatchDefs
	{
		private readonly EmitContext _sourceContext;

		private readonly ConcurrentDictionary<IDefinition, IDefinition> _matches;

		private IReadOnlyDictionary<string, INamespaceTypeDefinition> _lazyTopLevelTypes;

		public MatchDefs(EmitContext sourceContext)
		{
			_sourceContext = sourceContext;
			_matches = new ConcurrentDictionary<IDefinition, IDefinition>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		}

		public IDefinition VisitDef(IDefinition def)
		{
			return _matches.GetOrAdd(def, VisitDefInternal);
		}

		private IDefinition VisitDefInternal(IDefinition def)
		{
			if (def is ITypeDefinition typeDefinition)
			{
				INamespaceTypeDefinition namespaceTypeDefinition = typeDefinition.AsNamespaceTypeDefinition(_sourceContext);
				if (namespaceTypeDefinition != null)
				{
					return VisitNamespaceType(namespaceTypeDefinition);
				}
				INestedTypeDefinition nestedTypeDefinition = typeDefinition.AsNestedTypeDefinition(_sourceContext);
				ITypeDefinition typeDefinition2 = (ITypeDefinition)VisitDef(nestedTypeDefinition.ContainingTypeDefinition);
				if (typeDefinition2 == null)
				{
					return null;
				}
				return VisitTypeMembers(typeDefinition2, nestedTypeDefinition, GetNestedTypes, (INestedTypeDefinition a, INestedTypeDefinition b) => StringOrdinalComparer.Equals(a.Name, b.Name));
			}
			if (def is ITypeDefinitionMember typeDefinitionMember)
			{
				ITypeDefinition typeDefinition3 = (ITypeDefinition)VisitDef(typeDefinitionMember.ContainingTypeDefinition);
				if (typeDefinition3 == null)
				{
					return null;
				}
				if (def is IFieldDefinition member)
				{
					return VisitTypeMembers(typeDefinition3, member, GetFields, (IFieldDefinition a, IFieldDefinition b) => StringOrdinalComparer.Equals(a.Name, b.Name));
				}
			}
			throw ExceptionUtilities.UnexpectedValue(def);
		}

		protected abstract IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes();

		protected abstract IEnumerable<INestedTypeDefinition> GetNestedTypes(ITypeDefinition def);

		protected abstract IEnumerable<IFieldDefinition> GetFields(ITypeDefinition def);

		private INamespaceTypeDefinition VisitNamespaceType(INamespaceTypeDefinition def)
		{
			if (!string.IsNullOrEmpty(def.NamespaceName))
			{
				return null;
			}
			GetTopLevelTypesByName().TryGetValue(def.Name, out var value);
			return value;
		}

		private IReadOnlyDictionary<string, INamespaceTypeDefinition> GetTopLevelTypesByName()
		{
			if (_lazyTopLevelTypes == null)
			{
				Dictionary<string, INamespaceTypeDefinition> dictionary = new Dictionary<string, INamespaceTypeDefinition>(StringOrdinalComparer.Instance);
				foreach (INamespaceTypeDefinition topLevelType in GetTopLevelTypes())
				{
					if (string.IsNullOrEmpty(topLevelType.NamespaceName))
					{
						dictionary.Add(topLevelType.Name, topLevelType);
					}
				}
				Interlocked.CompareExchange(ref _lazyTopLevelTypes, dictionary, null);
			}
			return _lazyTopLevelTypes;
		}

		private static T VisitTypeMembers<T>(ITypeDefinition otherContainer, T member, Func<ITypeDefinition, IEnumerable<T>> getMembers, Func<T, T, bool> predicate) where T : class, ITypeDefinitionMember
		{
			return getMembers(otherContainer).FirstOrDefault((T otherMember) => predicate(member, otherMember));
		}
	}

	private sealed class MatchDefsToMetadata : MatchDefs
	{
		private readonly PEAssemblySymbol _otherAssembly;

		public MatchDefsToMetadata(EmitContext sourceContext, PEAssemblySymbol otherAssembly)
			: base(sourceContext)
		{
			_otherAssembly = otherAssembly;
		}

		protected override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes()
		{
			ArrayBuilder<INamespaceTypeDefinition> instance = ArrayBuilder<INamespaceTypeDefinition>.GetInstance();
			GetTopLevelTypes(instance, _otherAssembly.GlobalNamespace);
			return instance.ToArrayAndFree();
		}

		protected override IEnumerable<INestedTypeDefinition> GetNestedTypes(ITypeDefinition def)
		{
			return ((PENamedTypeSymbol)def).GetTypeMembers().Cast<INestedTypeDefinition>();
		}

		protected override IEnumerable<IFieldDefinition> GetFields(ITypeDefinition def)
		{
			return ((PENamedTypeSymbol)def).GetFieldsToEmit().Cast<IFieldDefinition>();
		}

		private static void GetTopLevelTypes(ArrayBuilder<INamespaceTypeDefinition> builder, NamespaceSymbol @namespace)
		{
			foreach (Symbol member in @namespace.GetMembers())
			{
				if (member.Kind == SymbolKind.Namespace)
				{
					GetTopLevelTypes(builder, (NamespaceSymbol)member);
				}
				else
				{
					builder.Add((INamespaceTypeDefinition)member);
				}
			}
		}
	}

	private sealed class MatchDefsToSource : MatchDefs
	{
		private readonly EmitContext _otherContext;

		public MatchDefsToSource(EmitContext sourceContext, EmitContext otherContext)
			: base(sourceContext)
		{
			_otherContext = otherContext;
		}

		protected override IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes()
		{
			return _otherContext.Module.GetTopLevelTypes(_otherContext);
		}

		protected override IEnumerable<INestedTypeDefinition> GetNestedTypes(ITypeDefinition def)
		{
			return def.GetNestedTypes(_otherContext);
		}

		protected override IEnumerable<IFieldDefinition> GetFields(ITypeDefinition def)
		{
			return def.GetFields(_otherContext);
		}
	}

	private sealed class MatchSymbols : CSharpSymbolVisitor<Symbol>
	{
		private sealed class SymbolComparer
		{
			private readonly MatchSymbols _matcher;

			private readonly DeepTranslator _deepTranslatorOpt;

			public SymbolComparer(MatchSymbols matcher, DeepTranslator deepTranslatorOpt)
			{
				_matcher = matcher;
				_deepTranslatorOpt = deepTranslatorOpt;
			}

			public bool Equals(TypeSymbol source, TypeSymbol other)
			{
				TypeSymbol typeSymbol = (TypeSymbol)_matcher.Visit(source);
				TypeSymbol t = ((_deepTranslatorOpt != null) ? ((TypeSymbol)_deepTranslatorOpt.Visit(other)) : other);
				return typeSymbol?.Equals(t, TypeCompareKind.IgnoreDynamicAndTupleNames) ?? false;
			}
		}

		private readonly IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> _anonymousTypeMap;

		private readonly SourceAssemblySymbol _sourceAssembly;

		private readonly AssemblySymbol _otherAssembly;

		private readonly ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> _otherSynthesizedMembersOpt;

		private readonly SymbolComparer _comparer;

		private readonly ConcurrentDictionary<Symbol, Symbol> _matches;

		private readonly ConcurrentDictionary<NamedTypeSymbol, IReadOnlyDictionary<string, ImmutableArray<ITypeDefinitionMember>>> _otherTypeMembers;

		public MatchSymbols(IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap, SourceAssemblySymbol sourceAssembly, AssemblySymbol otherAssembly, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> otherSynthesizedMembersOpt, DeepTranslator deepTranslatorOpt)
		{
			_anonymousTypeMap = anonymousTypeMap;
			_sourceAssembly = sourceAssembly;
			_otherAssembly = otherAssembly;
			_otherSynthesizedMembersOpt = otherSynthesizedMembersOpt;
			_comparer = new SymbolComparer(this, deepTranslatorOpt);
			_matches = new ConcurrentDictionary<Symbol, Symbol>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
			_otherTypeMembers = new ConcurrentDictionary<NamedTypeSymbol, IReadOnlyDictionary<string, ImmutableArray<ITypeDefinitionMember>>>();
		}

		internal bool TryGetAnonymousTypeName(NamedTypeSymbol type, out string name, out int index)
		{
			if (TryFindAnonymousType(type, out var otherType))
			{
				name = otherType.Name;
				index = otherType.UniqueIndex;
				return true;
			}
			name = null;
			index = -1;
			return false;
		}

		public override Symbol DefaultVisit(Symbol symbol)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override Symbol Visit(Symbol symbol)
		{
			return _matches.GetOrAdd(symbol, base.Visit);
		}

		public override Symbol VisitArrayType(ArrayTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.ElementType);
			if ((object)typeSymbol == null)
			{
				return null;
			}
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.CustomModifiers);
			if (symbol.IsSZArray)
			{
				return ArrayTypeSymbol.CreateSZArray(_otherAssembly, typeSymbol, customModifiers);
			}
			return ArrayTypeSymbol.CreateMDArray(_otherAssembly, typeSymbol, symbol.Rank, symbol.Sizes, symbol.LowerBounds, customModifiers);
		}

		public override Symbol VisitEvent(EventSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreEventsEqual);
		}

		public override Symbol VisitField(FieldSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreFieldsEqual);
		}

		public override Symbol VisitMethod(MethodSymbol symbol)
		{
			return VisitNamedTypeMember(symbol, AreMethodsEqual);
		}

		public override Symbol VisitModule(ModuleSymbol module)
		{
			AssemblySymbol assemblySymbol = (AssemblySymbol)Visit(module.ContainingAssembly);
			if ((object)assemblySymbol == null)
			{
				return null;
			}
			if (module.Ordinal == 0)
			{
				return assemblySymbol.Modules[0];
			}
			for (int i = 1; i < assemblySymbol.Modules.Length; i++)
			{
				ModuleSymbol moduleSymbol = assemblySymbol.Modules[i];
				if (StringComparer.Ordinal.Equals(moduleSymbol.Name, module.Name))
				{
					return moduleSymbol;
				}
			}
			return null;
		}

		public override Symbol VisitAssembly(AssemblySymbol assembly)
		{
			if (assembly.IsLinked)
			{
				return assembly;
			}
			if (IdentityEqualIgnoringVersionWildcard(assembly, _sourceAssembly))
			{
				return _otherAssembly;
			}
			foreach (AssemblySymbol referencedAssemblySymbol in _otherAssembly.Modules[0].ReferencedAssemblySymbols)
			{
				if (IdentityEqualIgnoringVersionWildcard(assembly, referencedAssemblySymbol))
				{
					return referencedAssemblySymbol;
				}
			}
			return null;
		}

		private static bool IdentityEqualIgnoringVersionWildcard(AssemblySymbol left, AssemblySymbol right)
		{
			AssemblyIdentity identity = left.Identity;
			AssemblyIdentity identity2 = right.Identity;
			if (AssemblyIdentityComparer.SimpleNameComparer.Equals(identity.Name, identity2.Name) && (left.AssemblyVersionPattern ?? identity.Version).Equals(right.AssemblyVersionPattern ?? identity2.Version))
			{
				return AssemblyIdentity.EqualIgnoringNameAndVersion(identity, identity2);
			}
			return false;
		}

		public override Symbol VisitNamespace(NamespaceSymbol @namespace)
		{
			Symbol symbol = Visit(@namespace.ContainingSymbol);
			return symbol.Kind switch
			{
				SymbolKind.NetModule => (Symbol)((ModuleSymbol)symbol).GlobalNamespace, 
				SymbolKind.Namespace => FindMatchingNamespaceMember((NamespaceSymbol)symbol, @namespace, (NamespaceSymbol s, NamespaceSymbol o) => true), 
				_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
			};
		}

		public override Symbol VisitDynamicType(DynamicTypeSymbol symbol)
		{
			return _otherAssembly.GetSpecialType(SpecialType.System_Object);
		}

		public override Symbol VisitNamedType(NamedTypeSymbol sourceType)
		{
			NamedTypeSymbol originalDefinition = sourceType.OriginalDefinition;
			if ((object)originalDefinition != sourceType)
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				ImmutableArray<TypeWithModifiers> allTypeArguments = sourceType.GetAllTypeArguments(ref useSiteDiagnostics);
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(originalDefinition);
				if ((object)namedTypeSymbol == null)
				{
					return null;
				}
				ImmutableArray<TypeParameterSymbol> allTypeParameters = namedTypeSymbol.GetAllTypeParameters();
				bool translationFailed = false;
				ImmutableArray<TypeWithModifiers> to = allTypeArguments.SelectAsArray((TypeWithModifiers t, MatchSymbols v) =>
				{
					TypeSymbol typeSymbol = (TypeSymbol)v.Visit(t.Type);
					if ((object)typeSymbol == null)
					{
						translationFailed = true;
						typeSymbol = t.Type;
					}
					return new TypeWithModifiers(typeSymbol, v.VisitCustomModifiers(t.CustomModifiers));
				}, this);
				if (translationFailed)
				{
					return null;
				}
				return new TypeMap(allTypeParameters, to, allowAlpha: true).SubstituteNamedType(namedTypeSymbol);
			}
			if (sourceType.IsTupleType)
			{
				NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)Visit(sourceType.TupleUnderlyingType);
				if ((object)namedTypeSymbol2 == null || !namedTypeSymbol2.IsTupleOrCompatibleWithTupleOfCardinality(sourceType.TupleElementTypes.Length))
				{
					return null;
				}
				return namedTypeSymbol2;
			}
			Symbol symbol = Visit(sourceType.ContainingSymbol);
			if ((object)symbol == null)
			{
				return null;
			}
			switch (symbol.Kind)
			{
			case SymbolKind.Namespace:
				if (AnonymousTypeManager.IsAnonymousTypeTemplate(sourceType))
				{
					TryFindAnonymousType(sourceType, out var otherType);
					return (NamedTypeSymbol)otherType.Type;
				}
				if (sourceType.IsAnonymousType)
				{
					return Visit(AnonymousTypeManager.TranslateAnonymousTypeSymbol(sourceType));
				}
				return FindMatchingNamespaceMember((NamespaceSymbol)symbol, sourceType, AreNamedTypesEqual);
			case SymbolKind.NamedType:
				return FindMatchingNamedTypeMember((NamedTypeSymbol)symbol, sourceType, AreNamedTypesEqual);
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}

		public override Symbol VisitParameter(ParameterSymbol parameter)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override Symbol VisitPointerType(PointerTypeSymbol symbol)
		{
			TypeSymbol typeSymbol = (TypeSymbol)Visit(symbol.PointedAtType);
			if ((object)typeSymbol == null)
			{
				return null;
			}
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.CustomModifiers);
			return new PointerTypeSymbol(typeSymbol, customModifiers);
		}

		public override Symbol VisitProperty(PropertySymbol symbol)
		{
			return VisitNamedTypeMember(symbol, ArePropertiesEqual);
		}

		public override Symbol VisitTypeParameter(TypeParameterSymbol symbol)
		{
			if (symbol is IndexedTypeParameterSymbol result)
			{
				return result;
			}
			Symbol symbol2 = Visit(symbol.ContainingSymbol);
			ImmutableArray<TypeParameterSymbol> typeParameters;
			switch (symbol2.Kind)
			{
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
				typeParameters = ((NamedTypeSymbol)symbol2).TypeParameters;
				break;
			case SymbolKind.Method:
				typeParameters = ((MethodSymbol)symbol2).TypeParameters;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol2.Kind);
			}
			return typeParameters[symbol.Ordinal];
		}

		private ImmutableArray<CustomModifier> VisitCustomModifiers(ImmutableArray<CustomModifier> modifiers)
		{
			return modifiers.SelectAsArray(VisitCustomModifier);
		}

		private CustomModifier VisitCustomModifier(CustomModifier modifier)
		{
			NamedTypeSymbol modifier2 = (NamedTypeSymbol)Visit((Symbol)modifier.Modifier);
			if (!modifier.IsOptional)
			{
				return CSharpCustomModifier.CreateRequired(modifier2);
			}
			return CSharpCustomModifier.CreateOptional(modifier2);
		}

		internal bool TryFindAnonymousType(NamedTypeSymbol type, out AnonymousTypeValue otherType)
		{
			AnonymousTypeKey anonymousTypeKey = AnonymousTypeManager.GetAnonymousTypeKey(type);
			return _anonymousTypeMap.TryGetValue(anonymousTypeKey, out otherType);
		}

		private static T FindMatchingNamespaceMember<T>(NamespaceSymbol otherNamespace, T sourceMember, Func<T, T, bool> predicate) where T : Symbol
		{
			foreach (Symbol member in otherNamespace.GetMembers(sourceMember.Name))
			{
				if (sourceMember.Kind == member.Kind)
				{
					T val = (T)member;
					if (predicate(sourceMember, val))
					{
						return val;
					}
				}
			}
			return null;
		}

		private Symbol VisitNamedTypeMember<T>(T member, Func<T, T, bool> predicate) where T : Symbol
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(member.ContainingType);
			if ((object)namedTypeSymbol == null)
			{
				return null;
			}
			return FindMatchingNamedTypeMember(namedTypeSymbol, member, predicate);
		}

		private T FindMatchingNamedTypeMember<T>(NamedTypeSymbol otherType, T sourceMember, Func<T, T, bool> predicate) where T : Symbol
		{
			if (_otherTypeMembers.GetOrAdd(otherType, GetOtherTypeMembers).TryGetValue(sourceMember.MetadataName, out var value))
			{
				foreach (ITypeDefinitionMember item in value)
				{
					T val = item as T;
					if (val != null && predicate(sourceMember, val))
					{
						return val;
					}
				}
			}
			return null;
		}

		private bool AreArrayTypesEqual(ArrayTypeSymbol type, ArrayTypeSymbol other)
		{
			if (type.HasSameShapeAs(other))
			{
				return AreTypesEqual(type.ElementType, other.ElementType);
			}
			return false;
		}

		private bool AreEventsEqual(EventSymbol @event, EventSymbol other)
		{
			return _comparer.Equals(@event.Type, other.Type);
		}

		private bool AreFieldsEqual(FieldSymbol field, FieldSymbol other)
		{
			return _comparer.Equals(field.Type, other.Type);
		}

		private bool AreMethodsEqual(MethodSymbol method, MethodSymbol other)
		{
			method = SubstituteTypeParameters(method);
			other = SubstituteTypeParameters(other);
			if (_comparer.Equals(method.ReturnType, other.ReturnType) && method.RefKind.Equals(other.RefKind) && method.Parameters.SequenceEqual(other.Parameters, AreParametersEqual))
			{
				return method.TypeParameters.SequenceEqual(other.TypeParameters, AreTypesEqual);
			}
			return false;
		}

		private static MethodSymbol SubstituteTypeParameters(MethodSymbol method)
		{
			int length = method.TypeParameters.Length;
			if (length == 0)
			{
				return method;
			}
			return method.Construct(IndexedTypeParameterSymbol.Take(length).Cast<TypeParameterSymbol, TypeSymbol>());
		}

		private bool AreNamedTypesEqual(NamedTypeSymbol type, NamedTypeSymbol other)
		{
			return type.TypeArgumentsNoUseSiteDiagnostics.SequenceEqual(other.TypeArgumentsNoUseSiteDiagnostics, AreTypesEqual);
		}

		private bool AreParametersEqual(ParameterSymbol parameter, ParameterSymbol other)
		{
			if (StringOrdinalComparer.Equals(parameter.MetadataName, other.MetadataName) && parameter.RefKind == other.RefKind)
			{
				return _comparer.Equals(parameter.Type, other.Type);
			}
			return false;
		}

		private bool ArePointerTypesEqual(PointerTypeSymbol type, PointerTypeSymbol other)
		{
			return AreTypesEqual(type.PointedAtType, other.PointedAtType);
		}

		private bool ArePropertiesEqual(PropertySymbol property, PropertySymbol other)
		{
			if (_comparer.Equals(property.Type, other.Type) && property.RefKind.Equals(other.RefKind))
			{
				return property.Parameters.SequenceEqual(other.Parameters, AreParametersEqual);
			}
			return false;
		}

		private static bool AreTypeParametersEqual(TypeParameterSymbol type, TypeParameterSymbol other)
		{
			return true;
		}

		private bool AreTypesEqual(TypeSymbol type, TypeSymbol other)
		{
			if (type.Kind != other.Kind)
			{
				return false;
			}
			switch (type.Kind)
			{
			case SymbolKind.ArrayType:
				return AreArrayTypesEqual((ArrayTypeSymbol)type, (ArrayTypeSymbol)other);
			case SymbolKind.PointerType:
				return ArePointerTypesEqual((PointerTypeSymbol)type, (PointerTypeSymbol)other);
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
				return AreNamedTypesEqual((NamedTypeSymbol)type, (NamedTypeSymbol)other);
			case SymbolKind.TypeParameter:
				return AreTypeParametersEqual((TypeParameterSymbol)type, (TypeParameterSymbol)other);
			default:
				throw ExceptionUtilities.UnexpectedValue(type.Kind);
			}
		}

		private IReadOnlyDictionary<string, ImmutableArray<ITypeDefinitionMember>> GetOtherTypeMembers(NamedTypeSymbol otherType)
		{
			ArrayBuilder<ITypeDefinitionMember> instance = ArrayBuilder<ITypeDefinitionMember>.GetInstance();
			instance.AddRange(otherType.GetEventsToEmit());
			instance.AddRange(otherType.GetFieldsToEmit());
			instance.AddRange(otherType.GetMethodsToEmit());
			instance.AddRange(otherType.GetTypeMembers());
			instance.AddRange(otherType.GetPropertiesToEmit());
			if (_otherSynthesizedMembersOpt != null && _otherSynthesizedMembersOpt.TryGetValue(otherType, out var value))
			{
				instance.AddRange(value);
			}
			Dictionary<string, ImmutableArray<ITypeDefinitionMember>> result = instance.ToDictionary((ITypeDefinitionMember s) => ((Symbol)s).MetadataName, StringOrdinalComparer.Instance);
			instance.Free();
			return result;
		}
	}

	internal sealed class DeepTranslator : CSharpSymbolVisitor<Symbol>
	{
		private readonly ConcurrentDictionary<Symbol, Symbol> _matches;

		private readonly NamedTypeSymbol _systemObject;

		public DeepTranslator(NamedTypeSymbol systemObject)
		{
			_matches = new ConcurrentDictionary<Symbol, Symbol>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
			_systemObject = systemObject;
		}

		public override Symbol DefaultVisit(Symbol symbol)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override Symbol Visit(Symbol symbol)
		{
			return _matches.GetOrAdd(symbol, base.Visit(symbol));
		}

		public override Symbol VisitArrayType(ArrayTypeSymbol symbol)
		{
			TypeSymbol elementType = (TypeSymbol)Visit(symbol.ElementType);
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.CustomModifiers);
			if (symbol.IsSZArray)
			{
				return ArrayTypeSymbol.CreateSZArray(symbol.BaseTypeNoUseSiteDiagnostics.ContainingAssembly, elementType, customModifiers);
			}
			return ArrayTypeSymbol.CreateMDArray(symbol.BaseTypeNoUseSiteDiagnostics.ContainingAssembly, elementType, symbol.Rank, symbol.Sizes, symbol.LowerBounds, customModifiers);
		}

		public override Symbol VisitDynamicType(DynamicTypeSymbol symbol)
		{
			return _systemObject;
		}

		public override Symbol VisitNamedType(NamedTypeSymbol type)
		{
			if (type.IsTupleType)
			{
				type = type.TupleUnderlyingType;
			}
			NamedTypeSymbol originalDefinition = type.OriginalDefinition;
			if ((object)originalDefinition != type)
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				ImmutableArray<TypeWithModifiers> to = type.GetAllTypeArguments(ref useSiteDiagnostics).SelectAsArray((TypeWithModifiers t, DeepTranslator v) => new TypeWithModifiers((TypeSymbol)v.Visit(t.Type), v.VisitCustomModifiers(t.CustomModifiers)), this);
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)Visit(originalDefinition);
				return new TypeMap(namedTypeSymbol.GetAllTypeParameters(), to, allowAlpha: true).SubstituteNamedType(namedTypeSymbol);
			}
			if (type.IsAnonymousType)
			{
				return Visit(AnonymousTypeManager.TranslateAnonymousTypeSymbol(type));
			}
			return type;
		}

		public override Symbol VisitPointerType(PointerTypeSymbol symbol)
		{
			TypeSymbol pointedAtType = (TypeSymbol)Visit(symbol.PointedAtType);
			ImmutableArray<CustomModifier> customModifiers = VisitCustomModifiers(symbol.CustomModifiers);
			return new PointerTypeSymbol(pointedAtType, customModifiers);
		}

		public override Symbol VisitTypeParameter(TypeParameterSymbol symbol)
		{
			return symbol;
		}

		private ImmutableArray<CustomModifier> VisitCustomModifiers(ImmutableArray<CustomModifier> modifiers)
		{
			return modifiers.SelectAsArray(VisitCustomModifier);
		}

		private CustomModifier VisitCustomModifier(CustomModifier modifier)
		{
			NamedTypeSymbol modifier2 = (NamedTypeSymbol)Visit((Symbol)modifier.Modifier);
			if (!modifier.IsOptional)
			{
				return CSharpCustomModifier.CreateRequired(modifier2);
			}
			return CSharpCustomModifier.CreateOptional(modifier2);
		}
	}

	private readonly MatchDefs _defs;

	private readonly MatchSymbols _symbols;

	public CSharpSymbolMatcher(IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap, SourceAssemblySymbol sourceAssembly, EmitContext sourceContext, SourceAssemblySymbol otherAssembly, EmitContext otherContext, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> otherSynthesizedMembersOpt)
	{
		_defs = new MatchDefsToSource(sourceContext, otherContext);
		_symbols = new MatchSymbols(anonymousTypeMap, sourceAssembly, otherAssembly, otherSynthesizedMembersOpt, new DeepTranslator(otherAssembly.GetSpecialType(SpecialType.System_Object)));
	}

	public CSharpSymbolMatcher(IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap, SourceAssemblySymbol sourceAssembly, EmitContext sourceContext, PEAssemblySymbol otherAssembly)
	{
		_defs = new MatchDefsToMetadata(sourceContext, otherAssembly);
		_symbols = new MatchSymbols(anonymousTypeMap, sourceAssembly, otherAssembly, null, null);
	}

	public override IDefinition MapDefinition(IDefinition definition)
	{
		if (definition is Symbol symbol)
		{
			return (IDefinition)_symbols.Visit(symbol);
		}
		return _defs.VisitDef(definition);
	}

	public override ITypeReference MapReference(ITypeReference reference)
	{
		if (reference is Symbol symbol)
		{
			return (ITypeReference)_symbols.Visit(symbol);
		}
		return null;
	}

	internal bool TryGetAnonymousTypeName(NamedTypeSymbol template, out string name, out int index)
	{
		return _symbols.TryGetAnonymousTypeName(template, out name, out index);
	}
}
