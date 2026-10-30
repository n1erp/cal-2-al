using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class PropertySymbol : Symbol, IPropertyDefinition, ISignature, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IPropertySymbol, ISymbol, IEquatable<ISymbol>
{
	private ParameterSignature _lazyParameterSignature;

	MetadataConstant IPropertyDefinition.DefaultValue => null;

	IMethodReference IPropertyDefinition.Getter
	{
		get
		{
			MethodSymbol getMethod = GetMethod;
			if ((object)getMethod != null || !IsSealed)
			{
				return getMethod;
			}
			return GetSynthesizedSealedAccessor(MethodKind.PropertyGet);
		}
	}

	bool IPropertyDefinition.HasDefaultValue => false;

	bool IPropertyDefinition.IsRuntimeSpecial => HasRuntimeSpecialName;

	internal virtual bool HasRuntimeSpecialName => false;

	bool IPropertyDefinition.IsSpecialName => HasSpecialName;

	ImmutableArray<IParameterDefinition> IPropertyDefinition.Parameters => StaticCast<IParameterDefinition>.From(Parameters);

	IMethodReference IPropertyDefinition.Setter
	{
		get
		{
			MethodSymbol setMethod = SetMethod;
			if ((object)setMethod != null || !IsSealed)
			{
				return setMethod;
			}
			return GetSynthesizedSealedAccessor(MethodKind.PropertySet);
		}
	}

	CallingConvention ISignature.CallingConvention => CallingConvention;

	ushort ISignature.ParameterCount => (ushort)ParameterCount;

	ImmutableArray<ICustomModifier> ISignature.ReturnValueCustomModifiers => TypeCustomModifiers.As<ICustomModifier>();

	ImmutableArray<ICustomModifier> ISignature.RefCustomModifiers => RefCustomModifiers.As<ICustomModifier>();

	bool ISignature.ReturnValueIsByRef => RefKind.IsManagedReference();

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

	TypeMemberVisibility ITypeDefinitionMember.Visibility => PEModuleBuilder.MemberVisibility(this);

	string INamedEntity.Name => MetadataName;

	public new virtual PropertySymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public bool ReturnsByRef => RefKind == RefKind.Ref;

	public bool ReturnsByRefReadonly => RefKind == RefKind.In;

	public abstract RefKind RefKind { get; }

	public abstract TypeSymbol Type { get; }

	public abstract ImmutableArray<CustomModifier> TypeCustomModifiers { get; }

	public abstract ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

	internal int ParameterCount => Parameters.Length;

	internal ImmutableArray<TypeSymbol> ParameterTypes
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterTypes;
		}
	}

	internal ImmutableArray<RefKind> ParameterRefKinds
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref _lazyParameterSignature);
			return _lazyParameterSignature.parameterRefKinds;
		}
	}

	public abstract bool IsIndexer { get; }

	public virtual bool IsIndexedProperty => false;

	public bool IsReadOnly => (object)((PropertySymbol)this.GetLeastOverriddenMember(ContainingType)).SetMethod == null;

	public bool IsWriteOnly => (object)((PropertySymbol)this.GetLeastOverriddenMember(ContainingType)).GetMethod == null;

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	internal abstract bool HasSpecialName { get; }

	public abstract MethodSymbol GetMethod { get; }

	public abstract MethodSymbol SetMethod { get; }

	internal abstract CallingConvention CallingConvention { get; }

	internal abstract bool MustCallMethodsDirectly { get; }

	public PropertySymbol OverriddenProperty
	{
		get
		{
			if (IsOverride)
			{
				if (IsDefinition)
				{
					return (PropertySymbol)OverriddenOrHiddenMembers.GetOverriddenMember();
				}
				return (PropertySymbol)OverriddenOrHiddenMembersResult.GetOverriddenMember(this, OriginalDefinition.OverriddenProperty);
			}
			return null;
		}
	}

	internal virtual OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers => this.MakeOverriddenOrHiddenMembers();

	internal bool HidesBasePropertiesByName => (GetMethod ?? SetMethod)?.HidesBaseMethodsByName ?? false;

	internal virtual bool IsExplicitInterfaceImplementation => ExplicitInterfaceImplementations.Any();

	public abstract ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations { get; }

	public sealed override SymbolKind Kind => SymbolKind.Property;

	protected override int HighestPriorityUseSiteError => 570;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				if (useSiteDiagnostic.Code != 570)
				{
					return useSiteDiagnostic.Code == 7085;
				}
				return true;
			}
			return false;
		}
	}

	public virtual bool IsTupleProperty => false;

	public virtual PropertySymbol TupleUnderlyingProperty => null;

	bool IPropertySymbol.IsIndexer => IsIndexer;

	ITypeSymbol IPropertySymbol.Type => Type;

	ImmutableArray<IParameterSymbol> IPropertySymbol.Parameters => StaticCast<IParameterSymbol>.From(Parameters);

	IMethodSymbol IPropertySymbol.GetMethod => GetMethod;

	IMethodSymbol IPropertySymbol.SetMethod => SetMethod;

	IPropertySymbol IPropertySymbol.OriginalDefinition => OriginalDefinition;

	IPropertySymbol IPropertySymbol.OverriddenProperty => OverriddenProperty;

	ImmutableArray<IPropertySymbol> IPropertySymbol.ExplicitInterfaceImplementations => ExplicitInterfaceImplementations.Cast<PropertySymbol, IPropertySymbol>();

	bool IPropertySymbol.IsReadOnly => IsReadOnly;

	bool IPropertySymbol.IsWriteOnly => IsWriteOnly;

	bool IPropertySymbol.IsWithEvents => false;

	ImmutableArray<CustomModifier> IPropertySymbol.TypeCustomModifiers => TypeCustomModifiers;

	ImmutableArray<CustomModifier> IPropertySymbol.RefCustomModifiers => RefCustomModifiers;

	IEnumerable<IMethodReference> IPropertyDefinition.GetAccessors(EmitContext context)
	{
		MethodSymbol getMethod = GetMethod;
		if (getMethod != null && getMethod.ShouldInclude(context))
		{
			yield return getMethod;
		}
		MethodSymbol setMethod = SetMethod;
		if (setMethod != null && setMethod.ShouldInclude(context))
		{
			yield return setMethod;
		}
		if (this is SourcePropertySymbol sourcePropertySymbol && sourcePropertySymbol.ShouldInclude(context))
		{
			SynthesizedSealedPropertyAccessor synthesizedSealedAccessorOpt = sourcePropertySymbol.SynthesizedSealedAccessorOpt;
			if ((object)synthesizedSealedAccessorOpt != null)
			{
				yield return synthesizedSealedAccessorOpt;
			}
		}
	}

	[Conditional("DEBUG")]
	private void CheckDefinitionInvariantAllowEmbedded()
	{
	}

	ImmutableArray<IParameterTypeInformation> ISignature.GetParameters(EmitContext context)
	{
		return StaticCast<IParameterTypeInformation>.From(Parameters);
	}

	ITypeReference ISignature.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(Type, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ContainingType;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	private IMethodReference GetSynthesizedSealedAccessor(MethodKind targetMethodKind)
	{
		if (this is SourcePropertySymbol { SynthesizedSealedAccessorOpt: var synthesizedSealedAccessorOpt })
		{
			if ((object)synthesizedSealedAccessorOpt == null || synthesizedSealedAccessorOpt.MethodKind != targetMethodKind)
			{
				return null;
			}
			return synthesizedSealedAccessorOpt;
		}
		return null;
	}

	internal PropertySymbol()
	{
	}

	internal PropertySymbol GetLeastOverriddenProperty(NamedTypeSymbol accessingTypeOpt)
	{
		NamedTypeSymbol originalDefinition = (((object)accessingTypeOpt == null) ? ContainingType : accessingTypeOpt).OriginalDefinition;
		PropertySymbol propertySymbol = this;
		while (propertySymbol.IsOverride && !propertySymbol.HidesBasePropertiesByName)
		{
			PropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if ((object)overriddenProperty == null || !AccessCheck.IsSymbolAccessible(overriddenProperty, originalDefinition, ref useSiteDiagnostics))
			{
				break;
			}
			propertySymbol = overriddenProperty;
		}
		return propertySymbol;
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitProperty(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitProperty(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitProperty(this);
	}

	internal virtual PropertySymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!(newOwner == ContainingSymbol))
		{
			return new SubstitutedPropertySymbol(newOwner as SubstitutedNamedTypeSymbol, this);
		}
		return this;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (IsDefinition)
		{
			return base.GetUseSiteDiagnostic();
		}
		return OriginalDefinition.GetUseSiteDiagnostic();
	}

	internal bool CalculateUseSiteDiagnostic(ref DiagnosticInfo result)
	{
		if (DeriveUseSiteDiagnosticFromType(ref result, Type) || DeriveUseSiteDiagnosticFromCustomModifiers(ref result, RefCustomModifiers) || DeriveUseSiteDiagnosticFromCustomModifiers(ref result, TypeCustomModifiers) || DeriveUseSiteDiagnosticFromParameters(ref result, Parameters))
		{
			return true;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			if (Type.GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, RefCustomModifiers, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, TypeCustomModifiers, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, Parameters, this, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitProperty(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitProperty(this);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertySymbol propertySymbol))
		{
			return false;
		}
		if ((object)this == propertySymbol)
		{
			return true;
		}
		if (ContainingType == propertySymbol.ContainingType)
		{
			return (object)OriginalDefinition == propertySymbol.OriginalDefinition;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int currentKey = 1;
		currentKey = Hash.Combine(ContainingType, currentKey);
		currentKey = Hash.Combine(Name, currentKey);
		return Hash.Combine(currentKey, ParameterCount);
	}
}
