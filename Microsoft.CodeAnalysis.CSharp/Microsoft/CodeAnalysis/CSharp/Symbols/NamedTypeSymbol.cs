using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class NamedTypeSymbol : TypeSymbol, ITypeReference, IReference, ITypeDefinition, IDefinition, INamedTypeReference, INamedEntity, INamedTypeDefinition, INamespaceTypeReference, INamespaceTypeDefinition, INestedTypeReference, ITypeMemberReference, INestedTypeDefinition, ITypeDefinitionMember, IGenericTypeInstanceReference, ISpecializedNestedTypeReference, INamedTypeSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	private bool _hasNoBaseCycles;

	protected static Func<Symbol, bool> IsInstanceFieldOrEvent = (Symbol symbol) =>
	{
		if (!symbol.IsStatic)
		{
			SymbolKind kind = symbol.Kind;
			if ((uint)(kind - 5) <= 1u)
			{
				return true;
			}
		}
		return false;
	};

	internal static readonly Func<TypeWithModifiers, bool> TypeSymbolIsNullFunction = (TypeWithModifiers type) => (object)type.Type == null;

	internal static readonly Func<TypeWithModifiers, bool> TypeSymbolIsErrorType = (TypeWithModifiers type) => (object)type.Type != null && type.Type.IsErrorType();

	bool ITypeReference.IsEnum => TypeKind == TypeKind.Enum;

	bool ITypeReference.IsValueType => IsValueType;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode
	{
		get
		{
			if (IsDefinition)
			{
				return PrimitiveTypeCode;
			}
			return Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;
		}
	}

	TypeDefinitionHandle ITypeReference.TypeDef
	{
		get
		{
			if (this is PENamedTypeSymbol pENamedTypeSymbol)
			{
				return pENamedTypeSymbol.Handle;
			}
			return default;
		}
	}

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference
	{
		get
		{
			if (!IsDefinition && Arity > 0)
			{
				return this;
			}
			return null;
		}
	}

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference
	{
		get
		{
			if (IsDefinition && (object)ContainingType == null)
			{
				return this;
			}
			return null;
		}
	}

	INestedTypeReference ITypeReference.AsNestedTypeReference
	{
		get
		{
			if ((object)ContainingType != null)
			{
				return this;
			}
			return null;
		}
	}

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference
	{
		get
		{
			if (!IsDefinition && (Arity == 0 || PEModuleBuilder.IsGenericType(ContainingType)))
			{
				return this;
			}
			return null;
		}
	}

	IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters
	{
		get
		{
			foreach (TypeParameterSymbol typeParameter in TypeParameters)
			{
				yield return typeParameter;
			}
		}
	}

	ushort ITypeDefinition.GenericParameterCount => GenericParameterCountImpl;

	private ushort GenericParameterCountImpl => (ushort)Arity;

	bool ITypeDefinition.IsAbstract => IsMetadataAbstract;

	internal virtual bool IsMetadataAbstract
	{
		get
		{
			if (!IsAbstract)
			{
				return IsStatic;
			}
			return true;
		}
	}

	bool ITypeDefinition.IsBeforeFieldInit
	{
		get
		{
			switch (TypeKind)
			{
			case TypeKind.Delegate:
			case TypeKind.Enum:
			case TypeKind.Interface:
				return false;
			default:
				foreach (Symbol member in GetMembers(".cctor"))
				{
					if (!member.IsImplicitlyDeclared)
					{
						return false;
					}
				}
				return true;
			}
		}
	}

	bool ITypeDefinition.IsComObject => IsComImport;

	bool ITypeDefinition.IsGeneric => Arity != 0;

	bool ITypeDefinition.IsInterface => IsInterface;

	bool ITypeDefinition.IsRuntimeSpecial => false;

	bool ITypeDefinition.IsSerializable => IsSerializable;

	bool ITypeDefinition.IsSpecialName => HasSpecialName;

	bool ITypeDefinition.IsWindowsRuntimeImport => IsWindowsRuntimeImport;

	bool ITypeDefinition.IsSealed => IsMetadataSealed;

	internal virtual bool IsMetadataSealed
	{
		get
		{
			if (!IsSealed)
			{
				return IsStatic;
			}
			return true;
		}
	}

	bool ITypeDefinition.HasDeclarativeSecurity => HasDeclarativeSecurity;

	IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes => GetSecurityInformation() ?? SpecializedCollections.EmptyEnumerable<SecurityAttribute>();

	ushort ITypeDefinition.Alignment => (ushort)Layout.Alignment;

	LayoutKind ITypeDefinition.Layout => Layout.Kind;

	uint ITypeDefinition.SizeOf => (uint)Layout.Size;

	CharSet ITypeDefinition.StringFormat => MarshallingCharSet;

	ushort INamedTypeReference.GenericParameterCount => GenericParameterCountImpl;

	bool INamedTypeReference.MangleName => MangleName;

	string INamedEntity.Name => Name;

	string INamespaceTypeReference.NamespaceName => ContainingNamespace.QualifiedName;

	bool INamespaceTypeDefinition.IsPublic => PEModuleBuilder.MemberVisibility(this) == TypeMemberVisibility.Public;

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

	TypeMemberVisibility ITypeDefinitionMember.Visibility => PEModuleBuilder.MemberVisibility(this);

	public abstract int Arity { get; }

	public abstract ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	internal abstract bool HasTypeArgumentsCustomModifiers { get; }

	internal abstract ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics { get; }

	public abstract NamedTypeSymbol ConstructedFrom { get; }

	public virtual NamedTypeSymbol EnumUnderlyingType => null;

	public override NamedTypeSymbol ContainingType => ContainingSymbol as NamedTypeSymbol;

	internal virtual bool KnownCircularStruct => false;

	internal bool KnownToHaveNoDeclaredBaseCycles => _hasNoBaseCycles;

	internal virtual bool IsExplicitDefinitionOfNoPiaLocalType => false;

	public MethodSymbol DelegateInvokeMethod
	{
		get
		{
			if (TypeKind != TypeKind.Delegate)
			{
				return null;
			}
			ImmutableArray<Symbol> members = GetMembers("Invoke");
			if (members.Length != 1)
			{
				return null;
			}
			return members[0] as MethodSymbol;
		}
	}

	public ImmutableArray<MethodSymbol> InstanceConstructors => GetConstructors<MethodSymbol>(includeInstance: true, includeStatic: false);

	public ImmutableArray<MethodSymbol> StaticConstructors => GetConstructors<MethodSymbol>(includeInstance: false, includeStatic: true);

	public ImmutableArray<MethodSymbol> Constructors => GetConstructors<MethodSymbol>(includeInstance: true, includeStatic: true);

	public ImmutableArray<PropertySymbol> Indexers
	{
		get
		{
			ImmutableArray<Symbol> simpleNonTypeMembers = GetSimpleNonTypeMembers("this[]");
			if (simpleNonTypeMembers.IsEmpty)
			{
				return ImmutableArray<PropertySymbol>.Empty;
			}
			ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
			foreach (Symbol item in simpleNonTypeMembers)
			{
				if (item.Kind == SymbolKind.Property)
				{
					instance.Add((PropertySymbol)item);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public abstract bool MightContainExtensionMethods { get; }

	public override bool IsReferenceType
	{
		get
		{
			TypeKind typeKind = TypeKind;
			if (typeKind != TypeKind.Enum && typeKind != TypeKind.Struct)
			{
				return typeKind != TypeKind.Error;
			}
			return false;
		}
	}

	public override bool IsValueType
	{
		get
		{
			TypeKind typeKind = TypeKind;
			if (typeKind != TypeKind.Struct)
			{
				return typeKind == TypeKind.Enum;
			}
			return true;
		}
	}

	internal override bool IsManagedType => BaseTypeAnalysis.IsManagedType(this);

	public virtual bool IsScriptClass => false;

	internal bool IsSubmissionClass => TypeKind == TypeKind.Submission;

	public virtual bool IsImplicitClass => false;

	public abstract override string Name { get; }

	public override string MetadataName
	{
		get
		{
			if (!MangleName)
			{
				return Name;
			}
			return MetadataHelpers.ComposeAritySuffixedMetadataName(Name, Arity);
		}
	}

	internal abstract bool MangleName { get; }

	public abstract IEnumerable<string> MemberNames { get; }

	public abstract override Accessibility DeclaredAccessibility { get; }

	public override SymbolKind Kind => SymbolKind.NamedType;

	internal abstract bool HasCodeAnalysisEmbeddedAttribute { get; }

	public bool IsGenericType
	{
		get
		{
			NamedTypeSymbol namedTypeSymbol = this;
			while ((object)namedTypeSymbol != null)
			{
				if (namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics.Length != 0)
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			return false;
		}
	}

	public virtual bool IsUnboundGenericType => false;

	public new virtual NamedTypeSymbol OriginalDefinition => this;

	protected sealed override TypeSymbol OriginalTypeSymbolDefinition => OriginalDefinition;

	internal virtual TypeMap TypeSubstitution => null;

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	internal abstract bool HasSpecialName { get; }

	internal abstract bool IsComImport { get; }

	internal abstract bool IsWindowsRuntimeImport { get; }

	internal abstract bool ShouldAddWinRTMembers { get; }

	internal bool IsConditional
	{
		get
		{
			if (GetAppliedConditionalSymbols().Any())
			{
				return true;
			}
			return BaseTypeNoUseSiteDiagnostics?.IsConditional ?? false;
		}
	}

	public abstract bool IsSerializable { get; }

	internal abstract TypeLayout Layout { get; }

	protected CharSet DefaultMarshallingCharSet => GetEffectiveDefaultMarshallingCharSet() ?? CharSet.Ansi;

	internal abstract CharSet MarshallingCharSet { get; }

	internal abstract bool HasDeclarativeSecurity { get; }

	internal virtual NamedTypeSymbol ComImportCoClass => null;

	internal virtual FieldSymbol FixedElementField => null;

	internal abstract bool IsInterface { get; }

	int INamedTypeSymbol.Arity => Arity;

	ImmutableArray<IMethodSymbol> INamedTypeSymbol.InstanceConstructors => GetConstructors<IMethodSymbol>(includeInstance: true, includeStatic: false);

	ImmutableArray<IMethodSymbol> INamedTypeSymbol.StaticConstructors => GetConstructors<IMethodSymbol>(includeInstance: false, includeStatic: true);

	ImmutableArray<IMethodSymbol> INamedTypeSymbol.Constructors => GetConstructors<IMethodSymbol>(includeInstance: true, includeStatic: true);

	IEnumerable<string> INamedTypeSymbol.MemberNames => MemberNames;

	ImmutableArray<ITypeParameterSymbol> INamedTypeSymbol.TypeParameters => StaticCast<ITypeParameterSymbol>.From(TypeParameters);

	ImmutableArray<ITypeSymbol> INamedTypeSymbol.TypeArguments => StaticCast<ITypeSymbol>.From(TypeArgumentsNoUseSiteDiagnostics);

	INamedTypeSymbol INamedTypeSymbol.OriginalDefinition => OriginalDefinition;

	IMethodSymbol INamedTypeSymbol.DelegateInvokeMethod => DelegateInvokeMethod;

	INamedTypeSymbol INamedTypeSymbol.EnumUnderlyingType => EnumUnderlyingType;

	INamedTypeSymbol INamedTypeSymbol.ConstructedFrom => ConstructedFrom;

	ISymbol INamedTypeSymbol.AssociatedSymbol => null;

	ImmutableArray<IFieldSymbol> INamedTypeSymbol.TupleElements => StaticCast<IFieldSymbol>.From(TupleElements);

	INamedTypeSymbol INamedTypeSymbol.TupleUnderlyingType => TupleUnderlyingType;

	bool INamedTypeSymbol.IsComImport => IsComImport;

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if ((object)ContainingType == null && IsDefinition && ContainingModule == pEModuleBuilder.SourceModule)
		{
			return this;
		}
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsNestedTypeDefinitionImpl(moduleBeingBuilt);
	}

	private INestedTypeDefinition AsNestedTypeDefinitionImpl(PEModuleBuilder moduleBeingBuilt)
	{
		if ((object)ContainingType != null && IsDefinition && ContainingModule == moduleBeingBuilt.SourceModule)
		{
			return this;
		}
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	private ITypeDefinition AsTypeDefinitionImpl(PEModuleBuilder moduleBeingBuilt)
	{
		if (IsDefinition && ContainingModule == moduleBeingBuilt.SourceModule)
		{
			return this;
		}
		return null;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return AsTypeDefinitionImpl(moduleBeingBuilt);
	}

	ITypeReference ITypeDefinition.GetBaseClass(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		NamedTypeSymbol namedTypeSymbol = BaseTypeNoUseSiteDiagnostics;
		if (IsScriptClass)
		{
			namedTypeSymbol = ContainingAssembly.GetSpecialType(SpecialType.System_Object);
		}
		if ((object)namedTypeSymbol == null)
		{
			return null;
		}
		return pEModuleBuilder.Translate(namedTypeSymbol, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	IEnumerable<IEventDefinition> ITypeDefinition.GetEvents(EmitContext context)
	{
		foreach (EventSymbol item in GetEventsToEmit())
		{
			if (item.ShouldInclude(context) || !((IEventDefinition)item).GetAccessors(context).IsEmpty())
			{
				yield return item;
			}
		}
	}

	internal virtual IEnumerable<EventSymbol> GetEventsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Event)
			{
				yield return (EventSymbol)member;
			}
		}
	}

	IEnumerable<Microsoft.Cci.MethodImplementation> ITypeDefinition.GetExplicitImplementationOverrides(EmitContext context)
	{
		if (IsInterface)
		{
			yield break;
		}
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind != SymbolKind.Method)
			{
				continue;
			}
			MethodSymbol method = (MethodSymbol)member;
			if (method.ExplicitInterfaceImplementations.Length != 0)
			{
				foreach (MethodSymbol explicitInterfaceImplementation in method.ExplicitInterfaceImplementations)
				{
					yield return new Microsoft.Cci.MethodImplementation(method, moduleBeingBuilt.TranslateOverriddenMethodReference(explicitInterfaceImplementation, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics));
				}
			}
			if (method.RequiresExplicitOverride())
			{
				yield return new Microsoft.Cci.MethodImplementation(method, moduleBeingBuilt.TranslateOverriddenMethodReference(method.OverriddenMethod, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics));
			}
			else
			{
				if (method.MethodKind != MethodKind.Destructor || SpecialType == SpecialType.System_Object)
				{
					continue;
				}
				TypeSymbol specialType = DeclaringCompilation.GetSpecialType(SpecialType.System_Object);
				foreach (Symbol member2 in specialType.GetMembers("Finalize"))
				{
					if (member2 is MethodSymbol { MethodKind: MethodKind.Destructor } methodSymbol)
					{
						yield return new Microsoft.Cci.MethodImplementation(method, moduleBeingBuilt.TranslateOverriddenMethodReference(methodSymbol, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics));
					}
				}
			}
		}
		IEnumerable<IMethodDefinition> synthesizedMethods = moduleBeingBuilt.GetSynthesizedMethods(this);
		if (synthesizedMethods == null)
		{
			yield break;
		}
		foreach (IMethodDefinition item in synthesizedMethods)
		{
			if (item is MethodSymbol { ExplicitInterfaceImplementations: var explicitInterfaceImplementations } method2)
			{
				foreach (MethodSymbol item2 in explicitInterfaceImplementations)
				{
					yield return new Microsoft.Cci.MethodImplementation(method2, moduleBeingBuilt.TranslateOverriddenMethodReference(item2, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics));
				}
			}
		}
	}

	IEnumerable<IFieldDefinition> ITypeDefinition.GetFields(EmitContext context)
	{
		bool isStruct = this.IsStructType();
		foreach (FieldSymbol item in GetFieldsToEmit())
		{
			if (isStruct || item.ShouldInclude(context))
			{
				yield return item;
			}
		}
		IEnumerable<IFieldDefinition> synthesizedFields = ((PEModuleBuilder)context.Module).GetSynthesizedFields(this);
		if (synthesizedFields == null)
		{
			yield break;
		}
		foreach (IFieldDefinition item2 in synthesizedFields)
		{
			if (isStruct || item2.ShouldInclude(context))
			{
				yield return item2;
			}
		}
	}

	internal abstract IEnumerable<FieldSymbol> GetFieldsToEmit();

	IEnumerable<TypeReferenceWithAttributes> ITypeDefinition.Interfaces(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (NamedTypeSymbol item in GetInterfacesToEmit())
		{
			INamedTypeReference typeRef = moduleBeingBuilt.Translate(item, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics, fromImplements: true);
			yield return item.GetTypeRefWithAttributes(DeclaringCompilation, typeRef);
		}
	}

	internal abstract ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit();

	protected ImmutableArray<NamedTypeSymbol> CalculateInterfacesToEmit()
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		HashSet<NamedTypeSymbol> seen = null;
		InterfacesVisit(this, instance, ref seen);
		return instance.ToImmutableAndFree();
	}

	private static void InterfacesVisit(NamedTypeSymbol namedType, ArrayBuilder<NamedTypeSymbol> builder, ref HashSet<NamedTypeSymbol> seen)
	{
		foreach (NamedTypeSymbol item in namedType.InterfacesNoUseSiteDiagnostics())
		{
			if (seen == null)
			{
				seen = new HashSet<NamedTypeSymbol>();
			}
			if (seen.Add(item))
			{
				builder.Add(item);
				InterfacesVisit(item, builder, ref seen);
			}
		}
	}

	IEnumerable<IMethodDefinition> ITypeDefinition.GetMethods(EmitContext context)
	{
		foreach (MethodSymbol item in GetMethodsToEmit())
		{
			if (item.ShouldInclude(context))
			{
				yield return item;
			}
		}
		IEnumerable<IMethodDefinition> synthesizedMethods = ((PEModuleBuilder)context.Module).GetSynthesizedMethods(this);
		if (synthesizedMethods == null)
		{
			yield break;
		}
		foreach (IMethodDefinition item2 in synthesizedMethods)
		{
			if (item2.ShouldInclude(context))
			{
				yield return item2;
			}
		}
	}

	internal virtual IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind != SymbolKind.Method)
			{
				continue;
			}
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (methodSymbol.IsPartialDefinition())
			{
				if ((object)methodSymbol.PartialImplementationPart == null)
				{
					continue;
				}
			}
			else if (methodSymbol.IsDefaultValueTypeConstructor())
			{
				continue;
			}
			yield return methodSymbol;
		}
	}

	IEnumerable<INestedTypeDefinition> ITypeDefinition.GetNestedTypes(EmitContext context)
	{
		foreach (NamedTypeSymbol typeMember in GetTypeMembers())
		{
			yield return typeMember;
		}
		IEnumerable<INestedTypeDefinition> synthesizedTypes = ((PEModuleBuilder)context.Module).GetSynthesizedTypes(this);
		if (synthesizedTypes == null)
		{
			yield break;
		}
		foreach (INestedTypeDefinition item in synthesizedTypes)
		{
			yield return item;
		}
	}

	IEnumerable<IPropertyDefinition> ITypeDefinition.GetProperties(EmitContext context)
	{
		foreach (PropertySymbol item in GetPropertiesToEmit())
		{
			if (item.ShouldInclude(context) || !((IPropertyDefinition)item).GetAccessors(context).IsEmpty())
			{
				yield return item;
			}
		}
		IEnumerable<IPropertyDefinition> synthesizedProperties = ((PEModuleBuilder)context.Module).GetSynthesizedProperties(this);
		if (synthesizedProperties == null)
		{
			yield break;
		}
		foreach (IPropertyDefinition item2 in synthesizedProperties)
		{
			if (item2.ShouldInclude(context) || !item2.GetAccessors(context).IsEmpty())
			{
				yield return item2;
			}
		}
	}

	internal virtual IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Property)
			{
				yield return (PropertySymbol)member;
			}
		}
	}

	IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(ContainingModule, context.Diagnostics);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (!IsDefinition)
		{
			return pEModuleBuilder.Translate(ContainingType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}
		return ContainingType;
	}

	ImmutableArray<ITypeReference> IGenericTypeInstanceReference.GetGenericArguments(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		ArrayBuilder<ITypeReference> instance = ArrayBuilder<ITypeReference>.GetInstance();
		bool hasTypeArgumentsCustomModifiers = HasTypeArgumentsCustomModifiers;
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = TypeArgumentsNoUseSiteDiagnostics;
		for (int i = 0; i < typeArgumentsNoUseSiteDiagnostics.Length; i++)
		{
			ITypeReference typeReference = pEModuleBuilder.Translate(typeArgumentsNoUseSiteDiagnostics[i], (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			if (hasTypeArgumentsCustomModifiers)
			{
				ImmutableArray<CustomModifier> typeArgumentCustomModifiers = GetTypeArgumentCustomModifiers(i);
				if (!typeArgumentCustomModifiers.IsDefaultOrEmpty)
				{
					typeReference = new ModifiedTypeReference(typeReference, typeArgumentCustomModifiers.As<ICustomModifier>());
				}
			}
			instance.Add(typeReference);
		}
		return instance.ToImmutableAndFree();
	}

	INamedTypeReference IGenericTypeInstanceReference.GetGenericType(EmitContext context)
	{
		return GenericTypeImpl(context);
	}

	private INamedTypeReference GenericTypeImpl(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics, fromImplements: false, needDeclaration: true);
	}

	INestedTypeReference ISpecializedNestedTypeReference.GetUnspecializedVersion(EmitContext context)
	{
		return GenericTypeImpl(context).AsNestedTypeReference;
	}

	internal NamedTypeSymbol()
	{
	}

	public abstract ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal);

	internal ImmutableArray<CustomModifier> GetEmptyTypeArgumentCustomModifiers(int ordinal)
	{
		if (ordinal < 0 || ordinal >= Arity)
		{
			throw new IndexOutOfRangeException();
		}
		return ImmutableArray<CustomModifier>.Empty;
	}

	internal ImmutableArray<TypeSymbol> TypeArgumentsWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = TypeArgumentsNoUseSiteDiagnostics;
		foreach (TypeSymbol item in typeArgumentsNoUseSiteDiagnostics)
		{
			item.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return typeArgumentsNoUseSiteDiagnostics;
	}

	internal TypeSymbol TypeArgumentWithDefinitionUseSiteDiagnostics(int index, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol typeSymbol = TypeArgumentsNoUseSiteDiagnostics[index];
		typeSymbol.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		return typeSymbol;
	}

	internal void SetKnownToHaveNoDeclaredBaseCycles()
	{
		_hasNoBaseCycles = true;
	}

	internal virtual bool GetGuidString(out string guidString)
	{
		return GetGuidStringDefaultImplementation(out guidString);
	}

	internal ImmutableArray<MethodSymbol> GetOperators(string name)
	{
		ImmutableArray<Symbol> simpleNonTypeMembers = GetSimpleNonTypeMembers(name);
		if (simpleNonTypeMembers.IsEmpty)
		{
			return ImmutableArray<MethodSymbol>.Empty;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (MethodSymbol item in simpleNonTypeMembers.OfType<MethodSymbol>())
		{
			if (item.MethodKind == MethodKind.UserDefinedOperator || item.MethodKind == MethodKind.Conversion)
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<TMethodSymbol> GetConstructors<TMethodSymbol>(bool includeInstance, bool includeStatic) where TMethodSymbol : class, IMethodSymbol
	{
		ImmutableArray<Symbol> immutableArray = (includeInstance ? GetMembers(".ctor") : ImmutableArray<Symbol>.Empty);
		ImmutableArray<Symbol> immutableArray2 = (includeStatic ? GetMembers(".cctor") : ImmutableArray<Symbol>.Empty);
		if (immutableArray.IsEmpty && immutableArray2.IsEmpty)
		{
			return ImmutableArray<TMethodSymbol>.Empty;
		}
		ArrayBuilder<TMethodSymbol> instance = ArrayBuilder<TMethodSymbol>.GetInstance();
		foreach (Symbol item3 in immutableArray)
		{
			if (item3.Kind == SymbolKind.Method)
			{
				TMethodSymbol item = item3 as TMethodSymbol;
				instance.Add(item);
			}
		}
		foreach (Symbol item4 in immutableArray2)
		{
			if (item4.Kind == SymbolKind.Method)
			{
				TMethodSymbol item2 = item4 as TMethodSymbol;
				instance.Add(item2);
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void GetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		if (MightContainExtensionMethods)
		{
			DoGetExtensionMethods(methods, nameOpt, arity, options);
		}
	}

	internal void DoGetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		foreach (Symbol item in (nameOpt == null) ? GetMembersUnordered() : GetSimpleNonTypeMembers(nameOpt))
		{
			if (item.Kind != SymbolKind.Method)
			{
				continue;
			}
			MethodSymbol methodSymbol = (MethodSymbol)item;
			if (methodSymbol.IsExtensionMethod && ((options & LookupOptions.AllMethodsOnArityZero) != 0 || arity == methodSymbol.Arity))
			{
				ParameterSymbol parameterSymbol = methodSymbol.Parameters.First();
				if ((parameterSymbol.RefKind != RefKind.Ref || parameterSymbol.Type.IsValueType) && (parameterSymbol.RefKind != RefKind.In || parameterSymbol.Type.TypeKind == TypeKind.Struct))
				{
					methods.Add(methodSymbol);
				}
			}
		}
	}

	internal abstract AttributeUsageInfo GetAttributeUsageInfo();

	internal SynthesizedInstanceConstructor GetScriptConstructor()
	{
		return (SynthesizedInstanceConstructor)InstanceConstructors.Single();
	}

	internal SynthesizedInteractiveInitializerMethod GetScriptInitializer()
	{
		return (SynthesizedInteractiveInitializerMethod)GetMembers("<Initialize>").Single();
	}

	internal SynthesizedEntryPointSymbol GetScriptEntryPoint()
	{
		string name = ((TypeKind == TypeKind.Submission) ? "<Factory>" : "<Main>");
		return (SynthesizedEntryPointSymbol)GetMembers(name).Single();
	}

	public abstract override ImmutableArray<Symbol> GetMembers();

	public abstract override ImmutableArray<Symbol> GetMembers(string name);

	internal virtual ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		return GetMembers(name);
	}

	public abstract override ImmutableArray<NamedTypeSymbol> GetTypeMembers();

	public abstract override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name);

	public abstract override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity);

	internal virtual IEnumerable<Symbol> GetInstanceFieldsAndEvents()
	{
		return GetMembersUnordered().Where(IsInstanceFieldOrEvent);
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNamedType(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitNamedType(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitNamedType(this);
	}

	internal abstract ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers();

	internal abstract ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name);

	internal abstract NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved);

	internal abstract ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved);

	public override int GetHashCode()
	{
		if (SpecialType == SpecialType.System_Object)
		{
			return 1;
		}
		return RuntimeHelpers.GetHashCode(OriginalDefinition);
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((object)t2 == this)
		{
			return true;
		}
		if ((object)t2 == null)
		{
			return false;
		}
		if ((comparison & TypeCompareKind.IgnoreDynamic) != 0 && t2.TypeKind == TypeKind.Dynamic && SpecialType == SpecialType.System_Object)
		{
			return true;
		}
		if ((comparison & TypeCompareKind.IgnoreTupleNames) != 0 && t2.IsTupleType)
		{
			t2 = t2.TupleUnderlyingType;
			if (Equals(t2, comparison))
			{
				return true;
			}
		}
		if (!(t2 is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		NamedTypeSymbol originalDefinition = OriginalDefinition;
		NamedTypeSymbol originalDefinition2 = namedTypeSymbol.OriginalDefinition;
		if (((object)this == originalDefinition || (object)namedTypeSymbol == originalDefinition2) && ((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0 || (!HasTypeArgumentsCustomModifiers && !namedTypeSymbol.HasTypeArgumentsCustomModifiers)))
		{
			return false;
		}
		if (originalDefinition != originalDefinition2)
		{
			return false;
		}
		return EqualsComplicatedCases(namedTypeSymbol, comparison);
	}

	private bool EqualsComplicatedCases(NamedTypeSymbol other, TypeCompareKind comparison)
	{
		if ((object)ContainingType != null && !ContainingType.Equals(other.ContainingType, comparison))
		{
			return false;
		}
		bool flag = (object)ConstructedFrom == this;
		bool flag2 = (object)other.ConstructedFrom == other;
		if (flag & flag2)
		{
			return true;
		}
		if (((flag | flag2) && ((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0 || (!HasTypeArgumentsCustomModifiers && !other.HasTypeArgumentsCustomModifiers))) || IsUnboundGenericType != other.IsUnboundGenericType)
		{
			return false;
		}
		bool hasTypeArgumentsCustomModifiers = HasTypeArgumentsCustomModifiers;
		if ((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0 && hasTypeArgumentsCustomModifiers != other.HasTypeArgumentsCustomModifiers)
		{
			return false;
		}
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = TypeArgumentsNoUseSiteDiagnostics;
		ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics2 = other.TypeArgumentsNoUseSiteDiagnostics;
		int length = typeArgumentsNoUseSiteDiagnostics.Length;
		for (int i = 0; i < length; i++)
		{
			if (!typeArgumentsNoUseSiteDiagnostics[i].Equals(typeArgumentsNoUseSiteDiagnostics2[i], comparison))
			{
				return false;
			}
		}
		if (((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0) & hasTypeArgumentsCustomModifiers)
		{
			for (int j = 0; j < length; j++)
			{
				if (!GetTypeArgumentCustomModifiers(j).SequenceEqual(other.GetTypeArgumentCustomModifiers(j)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public NamedTypeSymbol Construct(params TypeSymbol[] typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments.AsImmutableOrNull(), unbound: false);
	}

	public NamedTypeSymbol Construct(ImmutableArray<TypeSymbol> typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments, unbound: false);
	}

	public NamedTypeSymbol Construct(IEnumerable<TypeSymbol> typeArguments)
	{
		return ConstructWithoutModifiers(typeArguments.AsImmutableOrNull(), unbound: false);
	}

	public NamedTypeSymbol ConstructUnboundGenericType()
	{
		return OriginalDefinition.AsUnboundGenericType();
	}

	internal NamedTypeSymbol GetUnboundGenericTypeOrSelf()
	{
		if (!IsGenericType)
		{
			return this;
		}
		return ConstructUnboundGenericType();
	}

	internal NamedTypeSymbol ConstructWithoutModifiers(ImmutableArray<TypeSymbol> arguments, bool unbound)
	{
		ImmutableArray<TypeWithModifiers> arguments2;
		if (arguments.IsDefault)
		{
			arguments2 = default;
		}
		else if (arguments.IsEmpty)
		{
			arguments2 = ImmutableArray<TypeWithModifiers>.Empty;
		}
		else
		{
			ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance(arguments.Length);
			foreach (TypeSymbol item in arguments)
			{
				instance.Add(((object)item == null) ? default(TypeWithModifiers) : new TypeWithModifiers(item));
			}
			arguments2 = instance.ToImmutableAndFree();
		}
		return Construct(arguments2, unbound);
	}

	internal NamedTypeSymbol Construct(ImmutableArray<TypeWithModifiers> arguments, bool unbound)
	{
		if ((object)this != ConstructedFrom || Arity == 0)
		{
			throw new InvalidOperationException();
		}
		if (arguments.IsDefault)
		{
			throw new ArgumentNullException("arguments");
		}
		if (arguments.Any(TypeSymbolIsNullFunction))
		{
			throw new ArgumentException(CSharpResources.TypeArgumentCannotBeNull, "typeArguments");
		}
		if (arguments.Length != Arity)
		{
			throw new ArgumentException(CSharpResources.WrongNumberOfTypeArguments, "typeArguments");
		}
		if (ConstructedNamedTypeSymbol.TypeParametersMatchTypeArguments(TypeParameters, arguments))
		{
			return this;
		}
		return ConstructCore(arguments, unbound);
	}

	protected virtual NamedTypeSymbol ConstructCore(ImmutableArray<TypeWithModifiers> typeArguments, bool unbound)
	{
		return new ConstructedNamedTypeSymbol(this, typeArguments, unbound);
	}

	internal void GetAllTypeArguments(ArrayBuilder<TypeSymbol> builder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ContainingType?.GetAllTypeArguments(builder, ref useSiteDiagnostics);
		builder.AddRange(TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics));
	}

	internal ImmutableArray<TypeWithModifiers> GetAllTypeArguments(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance();
		GetAllTypeArguments(instance, ref useSiteDiagnostics);
		return instance.ToImmutableAndFree();
	}

	internal void GetAllTypeArguments(ArrayBuilder<TypeWithModifiers> builder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ContainingType?.GetAllTypeArguments(builder, ref useSiteDiagnostics);
		ImmutableArray<TypeSymbol> immutableArray = TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		if (!HasTypeArgumentsCustomModifiers)
		{
			for (int i = 0; i < immutableArray.Length; i++)
			{
				builder.Add(new TypeWithModifiers(immutableArray[i]));
			}
		}
		else
		{
			for (int j = 0; j < immutableArray.Length; j++)
			{
				builder.Add(new TypeWithModifiers(immutableArray[j], GetTypeArgumentCustomModifiers(j)));
			}
		}
	}

	internal int AllTypeArgumentCount()
	{
		int num = TypeArgumentsNoUseSiteDiagnostics.Length;
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null)
		{
			num += containingType.AllTypeArgumentCount();
		}
		return num;
	}

	internal virtual NamedTypeSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!newOwner.IsDefinition)
		{
			return new SubstitutedNestedTypeSymbol((SubstitutedNamedTypeSymbol)newOwner, this);
		}
		return this;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (IsDefinition)
		{
			return base.GetUseSiteDiagnostic();
		}
		DiagnosticInfo result = null;
		if (!DeriveUseSiteDiagnosticFromType(ref result, OriginalDefinition))
		{
			DeriveUseSiteDiagnosticFromTypeArguments(ref result);
		}
		return result;
	}

	private bool DeriveUseSiteDiagnosticFromTypeArguments(ref DiagnosticInfo result)
	{
		foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in TypeArgumentsNoUseSiteDiagnostics)
		{
			if (DeriveUseSiteDiagnosticFromType(ref result, typeArgumentsNoUseSiteDiagnostic))
			{
				return true;
			}
		}
		if (HasTypeArgumentsCustomModifiers)
		{
			for (int i = 0; i < Arity; i++)
			{
				if (DeriveUseSiteDiagnosticFromCustomModifiers(ref result, GetTypeArgumentCustomModifiers(i)))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal DiagnosticInfo CalculateUseSiteDiagnostic()
	{
		DiagnosticInfo result = null;
		if (MergeUseSiteDiagnostics(ref result, DeriveUseSiteDiagnosticFromBase()))
		{
			return result;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes);
			return result;
		}
		return result;
	}

	private DiagnosticInfo DeriveUseSiteDiagnosticFromBase()
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			if (baseTypeNoUseSiteDiagnostics.IsErrorType() && baseTypeNoUseSiteDiagnostics is NoPiaIllegalGenericInstantiationSymbol)
			{
				return baseTypeNoUseSiteDiagnostics.GetUseSiteDiagnostic();
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return null;
	}

	internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (!this.MarkCheckedIfNecessary(ref checkedTypes))
		{
			return false;
		}
		if (owner.ContainingModule.GetUnificationUseSiteDiagnostic(ref result, this))
		{
			return true;
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null && baseTypeNoUseSiteDiagnostics.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
		{
			return true;
		}
		if (!Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, InterfacesNoUseSiteDiagnostics(), owner, ref checkedTypes))
		{
			return Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, TypeParameters, owner, ref checkedTypes);
		}
		return true;
	}

	internal abstract IEnumerable<SecurityAttribute> GetSecurityInformation();

	internal abstract ImmutableArray<string> GetAppliedConditionalSymbols();

	public sealed override bool IsTupleCompatible(out int tupleCardinality)
	{
		if (IsTupleType)
		{
			tupleCardinality = 0;
			return false;
		}
		if (!IsUnboundGenericType)
		{
			Symbol containingSymbol = ContainingSymbol;
			if ((object)containingSymbol != null && containingSymbol.Kind == SymbolKind.Namespace)
			{
				NamespaceSymbol containingNamespace = ContainingNamespace.ContainingNamespace;
				if ((object)containingNamespace != null && containingNamespace.IsGlobalNamespace && Name == "ValueTuple" && ContainingNamespace.Name == "System")
				{
					int arity = Arity;
					if (arity > 0 && arity < 8)
					{
						tupleCardinality = arity;
						return true;
					}
					if (arity == 8 && !IsDefinition)
					{
						TypeSymbol typeSymbol = this;
						int num = 0;
						do
						{
							num++;
							typeSymbol = ((NamedTypeSymbol)typeSymbol).TypeArgumentsNoUseSiteDiagnostics[7];
						}
						while (typeSymbol.OriginalDefinition == OriginalDefinition && !typeSymbol.IsDefinition);
						if (typeSymbol.IsTupleType)
						{
							NamedTypeSymbol tupleUnderlyingType = typeSymbol.TupleUnderlyingType;
							if (tupleUnderlyingType.Arity == 8 && tupleUnderlyingType.OriginalDefinition != OriginalDefinition)
							{
								tupleCardinality = 0;
								return false;
							}
							tupleCardinality = 7 * num + typeSymbol.TupleElementTypes.Length;
							return true;
						}
						arity = (typeSymbol as NamedTypeSymbol)?.Arity ?? 0;
						if (arity > 0 && arity < 8 && typeSymbol.IsTupleCompatible(out tupleCardinality))
						{
							tupleCardinality += 7 * num;
							return true;
						}
					}
				}
			}
		}
		tupleCardinality = 0;
		return false;
	}

	INamedTypeSymbol INamedTypeSymbol.Construct(params ITypeSymbol[] arguments)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			arguments[i].EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("typeArguments");
		}
		return Construct(arguments.Cast<TypeSymbol>().ToArray());
	}

	INamedTypeSymbol INamedTypeSymbol.ConstructUnboundGenericType()
	{
		return ConstructUnboundGenericType();
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitNamedType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitNamedType(this);
	}
}
