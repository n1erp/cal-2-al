using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class TypeParameterSymbol : TypeSymbol, IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry, IGenericMethodParameterReference, IGenericTypeParameterReference, IGenericParameter, IGenericMethodParameter, IGenericTypeParameter, ITypeParameterSymbol, ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	bool ITypeReference.IsEnum => false;

	bool ITypeReference.IsValueType => false;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	TypeDefinitionHandle ITypeReference.TypeDef => default;

	IGenericMethodParameter IGenericParameter.AsGenericMethodParameter
	{
		get
		{
			if (ContainingSymbol.Kind == SymbolKind.Method)
			{
				return this;
			}
			return null;
		}
	}

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference
	{
		get
		{
			if (ContainingSymbol.Kind == SymbolKind.Method)
			{
				return this;
			}
			return null;
		}
	}

	IGenericTypeInstanceReference ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameter IGenericParameter.AsGenericTypeParameter
	{
		get
		{
			if (ContainingSymbol.Kind == SymbolKind.NamedType)
			{
				return this;
			}
			return null;
		}
	}

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference
	{
		get
		{
			if (ContainingSymbol.Kind == SymbolKind.NamedType)
			{
				return this;
			}
			return null;
		}
	}

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => null;

	INestedTypeReference ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference ITypeReference.AsSpecializedNestedTypeReference => null;

	string INamedEntity.Name => MetadataName;

	ushort IParameterListEntry.Index => (ushort)Ordinal;

	IMethodReference IGenericMethodParameterReference.DefiningMethod => (MethodSymbol)ContainingSymbol;

	ITypeReference IGenericTypeParameterReference.DefiningType => (NamedTypeSymbol)ContainingSymbol;

	bool IGenericParameter.MustBeReferenceType => HasReferenceTypeConstraint;

	bool IGenericParameter.MustBeValueType
	{
		get
		{
			if (!HasValueTypeConstraint)
			{
				return HasUnmanagedTypeConstraint;
			}
			return true;
		}
	}

	bool IGenericParameter.MustHaveDefaultConstructor
	{
		get
		{
			if (!HasConstructorConstraint && !HasValueTypeConstraint)
			{
				return HasUnmanagedTypeConstraint;
			}
			return true;
		}
	}

	TypeParameterVariance IGenericParameter.Variance => Variance switch
	{
		VarianceKind.None => TypeParameterVariance.NonVariant, 
		VarianceKind.In => TypeParameterVariance.Contravariant, 
		VarianceKind.Out => TypeParameterVariance.Covariant, 
		_ => throw ExceptionUtilities.UnexpectedValue(Variance), 
	};

	IMethodDefinition IGenericMethodParameter.DefiningMethod => (MethodSymbol)ContainingSymbol;

	ITypeDefinition IGenericTypeParameter.DefiningType => (NamedTypeSymbol)ContainingSymbol;

	public new virtual TypeParameterSymbol OriginalDefinition => this;

	protected sealed override TypeSymbol OriginalTypeSymbolDefinition => OriginalDefinition;

	public virtual TypeParameterSymbol ReducedFrom => null;

	public abstract int Ordinal { get; }

	internal ImmutableArray<TypeSymbol> ConstraintTypesNoUseSiteDiagnostics
	{
		get
		{
			EnsureAllConstraintsAreResolved();
			return GetConstraintTypes(ConsList<TypeParameterSymbol>.Empty);
		}
	}

	public abstract bool HasConstructorConstraint { get; }

	public abstract TypeParameterKind TypeParameterKind { get; }

	public MethodSymbol DeclaringMethod => ContainingSymbol as MethodSymbol;

	public NamedTypeSymbol DeclaringType => ContainingSymbol as NamedTypeSymbol;

	public sealed override SymbolKind Kind => SymbolKind.TypeParameter;

	public sealed override TypeKind TypeKind => TypeKind.TypeParameter;

	public sealed override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public sealed override bool IsStatic => false;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsSealed => false;

	internal sealed override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => null;

	internal NamedTypeSymbol EffectiveBaseClassNoUseSiteDiagnostics
	{
		get
		{
			EnsureAllConstraintsAreResolved();
			return GetEffectiveBaseClass(ConsList<TypeParameterSymbol>.Empty);
		}
	}

	internal ImmutableArray<NamedTypeSymbol> EffectiveInterfacesNoUseSiteDiagnostics
	{
		get
		{
			EnsureAllConstraintsAreResolved();
			return GetInterfaces(ConsList<TypeParameterSymbol>.Empty);
		}
	}

	internal TypeSymbol DeducedBaseTypeNoUseSiteDiagnostics
	{
		get
		{
			EnsureAllConstraintsAreResolved();
			return GetDeducedBaseType(ConsList<TypeParameterSymbol>.Empty);
		}
	}

	internal ImmutableArray<NamedTypeSymbol> AllEffectiveInterfacesNoUseSiteDiagnostics => base.GetAllInterfaces();

	public sealed override bool IsReferenceType
	{
		get
		{
			if (!HasReferenceTypeConstraint)
			{
				return IsReferenceTypeFromConstraintTypes(ConstraintTypesNoUseSiteDiagnostics);
			}
			return true;
		}
	}

	public sealed override bool IsValueType
	{
		get
		{
			if (!HasValueTypeConstraint)
			{
				return IsValueTypeFromConstraintTypes(ConstraintTypesNoUseSiteDiagnostics);
			}
			return true;
		}
	}

	internal sealed override bool IsManagedType => !HasUnmanagedTypeConstraint;

	internal sealed override bool IsByRefLikeType => false;

	internal sealed override bool IsReadOnly => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public abstract bool HasReferenceTypeConstraint { get; }

	public abstract bool HasValueTypeConstraint { get; }

	public abstract bool HasUnmanagedTypeConstraint { get; }

	public abstract VarianceKind Variance { get; }

	TypeParameterKind ITypeParameterSymbol.TypeParameterKind => TypeParameterKind;

	IMethodSymbol ITypeParameterSymbol.DeclaringMethod => DeclaringMethod;

	INamedTypeSymbol ITypeParameterSymbol.DeclaringType => DeclaringType;

	ImmutableArray<ITypeSymbol> ITypeParameterSymbol.ConstraintTypes => StaticCast<ITypeSymbol>.From(ConstraintTypesNoUseSiteDiagnostics);

	ITypeParameterSymbol ITypeParameterSymbol.OriginalDefinition => OriginalDefinition;

	ITypeParameterSymbol ITypeParameterSymbol.ReducedFrom => ReducedFrom;

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		return null;
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		return null;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}

	IEnumerable<TypeReferenceWithAttributes> IGenericParameter.GetConstraints(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		bool seenValueType = false;
		if (HasUnmanagedTypeConstraint)
		{
			INamedTypeReference specialType = moduleBeingBuilt.GetSpecialType(SpecialType.System_ValueType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			CustomModifier item = CSharpCustomModifier.CreateRequired(moduleBeingBuilt.Compilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_UnmanagedType));
			yield return new TypeReferenceWithAttributes(new ModifiedTypeReference(specialType, ImmutableArray.Create((ICustomModifier)item)));
			seenValueType = true;
		}
		foreach (TypeSymbol constraintTypesNoUseSiteDiagnostic in ConstraintTypesNoUseSiteDiagnostics)
		{
			SpecialType specialType2 = constraintTypesNoUseSiteDiagnostic.SpecialType;
			if (specialType2 != SpecialType.System_Object)
			{
				if (specialType2 == SpecialType.System_ValueType)
				{
					seenValueType = true;
				}
				ITypeReference typeRef = moduleBeingBuilt.Translate(constraintTypesNoUseSiteDiagnostic, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
				yield return constraintTypesNoUseSiteDiagnostic.GetTypeRefWithAttributes(DeclaringCompilation, typeRef);
			}
		}
		if (HasValueTypeConstraint && !seenValueType)
		{
			INamedTypeReference specialType3 = moduleBeingBuilt.GetSpecialType(SpecialType.System_ValueType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			yield return new TypeReferenceWithAttributes(specialType3);
		}
	}

	internal virtual DiagnosticInfo GetConstraintsUseSiteErrorInfo()
	{
		return null;
	}

	internal ImmutableArray<TypeSymbol> ConstraintTypesWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<TypeSymbol> constraintTypesNoUseSiteDiagnostics = ConstraintTypesNoUseSiteDiagnostics;
		AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);
		foreach (TypeSymbol item in constraintTypesNoUseSiteDiagnostics)
		{
			item.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return constraintTypesNoUseSiteDiagnostics;
	}

	private void AppendConstraintsUseSiteErrorInfo(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DiagnosticInfo constraintsUseSiteErrorInfo = GetConstraintsUseSiteErrorInfo();
		if (constraintsUseSiteErrorInfo != null)
		{
			if (useSiteDiagnostics == null)
			{
				useSiteDiagnostics = new HashSet<DiagnosticInfo>();
			}
			useSiteDiagnostics.Add(constraintsUseSiteErrorInfo);
		}
	}

	public sealed override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public sealed override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public sealed override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTypeParameter(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitTypeParameter(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameter(this);
	}

	internal TypeParameterSymbol()
	{
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	protected override ImmutableArray<NamedTypeSymbol> GetAllInterfaces()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal NamedTypeSymbol EffectiveBaseClass(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);
		NamedTypeSymbol effectiveBaseClassNoUseSiteDiagnostics = EffectiveBaseClassNoUseSiteDiagnostics;
		effectiveBaseClassNoUseSiteDiagnostics?.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		return effectiveBaseClassNoUseSiteDiagnostics;
	}

	internal TypeSymbol DeducedBaseType(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		AppendConstraintsUseSiteErrorInfo(ref useSiteDiagnostics);
		TypeSymbol deducedBaseTypeNoUseSiteDiagnostics = DeducedBaseTypeNoUseSiteDiagnostics;
		deducedBaseTypeNoUseSiteDiagnostics?.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		return deducedBaseTypeNoUseSiteDiagnostics;
	}

	internal ImmutableArray<NamedTypeSymbol> AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<NamedTypeSymbol> allEffectiveInterfacesNoUseSiteDiagnostics = AllEffectiveInterfacesNoUseSiteDiagnostics;
		TypeSymbol typeSymbol = DeducedBaseType(ref useSiteDiagnostics);
		while ((object)typeSymbol != null)
		{
			typeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		foreach (NamedTypeSymbol item in allEffectiveInterfacesNoUseSiteDiagnostics)
		{
			item.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return allEffectiveInterfacesNoUseSiteDiagnostics;
	}

	internal abstract void EnsureAllConstraintsAreResolved();

	protected static void EnsureAllConstraintsAreResolved(ImmutableArray<TypeParameterSymbol> typeParameters)
	{
		foreach (TypeParameterSymbol item in typeParameters)
		{
			item.GetConstraintTypes(ConsList<TypeParameterSymbol>.Empty);
		}
	}

	internal abstract ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress);

	internal abstract ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress);

	internal abstract NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress);

	internal abstract TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress);

	private static bool ConstraintImpliesReferenceType(TypeSymbol constraint)
	{
		if (constraint.TypeKind == TypeKind.TypeParameter)
		{
			return IsReferenceTypeFromConstraintTypes(((TypeParameterSymbol)constraint).ConstraintTypesNoUseSiteDiagnostics);
		}
		if (!constraint.IsReferenceType)
		{
			return false;
		}
		switch (constraint.TypeKind)
		{
		case TypeKind.Interface:
			return false;
		case TypeKind.Error:
			return false;
		default:
		{
			SpecialType specialType = constraint.SpecialType;
			if ((uint)(specialType - 1) <= 1u || specialType == SpecialType.System_ValueType)
			{
				return false;
			}
			return true;
		}
		}
	}

	internal static bool IsReferenceTypeFromConstraintTypes(ImmutableArray<TypeSymbol> constraintTypes)
	{
		foreach (TypeSymbol item in constraintTypes)
		{
			if (ConstraintImpliesReferenceType(item))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsValueTypeFromConstraintTypes(ImmutableArray<TypeSymbol> constraintTypes)
	{
		foreach (TypeSymbol item in constraintTypes)
		{
			if (item.IsValueType)
			{
				return true;
			}
		}
		return false;
	}

	internal sealed override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		return false;
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		return Equals(t2 as TypeParameterSymbol, comparison);
	}

	internal bool Equals(TypeParameterSymbol other)
	{
		return Equals(other, TypeCompareKind.ConsiderEverything);
	}

	private bool Equals(TypeParameterSymbol other, TypeCompareKind comparison)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other == null || (object)other.OriginalDefinition != OriginalDefinition)
		{
			return false;
		}
		return other.ContainingSymbol.ContainingType.Equals(ContainingSymbol.ContainingType, comparison);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ContainingSymbol, Ordinal);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTypeParameter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameter(this);
	}
}
