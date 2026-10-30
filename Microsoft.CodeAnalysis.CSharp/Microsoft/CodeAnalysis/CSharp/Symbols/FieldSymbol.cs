using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class FieldSymbol : Symbol, IFieldReference, ITypeMemberReference, IReference, INamedEntity, IFieldDefinition, ITypeDefinitionMember, IDefinition, ISpecializedFieldReference, IFieldSymbol, ISymbol, IEquatable<ISymbol>
{
	ISpecializedFieldReference IFieldReference.AsSpecializedFieldReference
	{
		get
		{
			if (!IsDefinition)
			{
				return this;
			}
			return null;
		}
	}

	string INamedEntity.Name => MetadataName;

	bool IFieldReference.IsContextualNamedEntity => false;

	ImmutableArray<byte> IFieldDefinition.MappedData => default;

	bool IFieldDefinition.IsCompileTimeConstant => IsMetadataConstant;

	bool IFieldDefinition.IsNotSerialized => IsNotSerialized;

	bool IFieldDefinition.IsReadOnly
	{
		get
		{
			if (!IsReadOnly)
			{
				if (IsConst)
				{
					return !IsMetadataConstant;
				}
				return false;
			}
			return true;
		}
	}

	bool IFieldDefinition.IsRuntimeSpecial => HasRuntimeSpecialName;

	bool IFieldDefinition.IsSpecialName => HasSpecialName;

	bool IFieldDefinition.IsStatic => IsStatic;

	bool IFieldDefinition.IsMarshalledExplicitly => IsMarshalledExplicitly;

	internal virtual bool IsMarshalledExplicitly => MarshallingInformation != null;

	IMarshallingInformation IFieldDefinition.MarshallingInformation => MarshallingInformation;

	ImmutableArray<byte> IFieldDefinition.MarshallingDescriptor => MarshallingDescriptor;

	internal virtual ImmutableArray<byte> MarshallingDescriptor => default;

	int IFieldDefinition.Offset => TypeLayoutOffset ?? 0;

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

	TypeMemberVisibility ITypeDefinitionMember.Visibility => PEModuleBuilder.MemberVisibility(this);

	IFieldReference ISpecializedFieldReference.UnspecializedVersion => OriginalDefinition;

	public new virtual FieldSymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public TypeSymbol Type => GetFieldType(ConsList<FieldSymbol>.Empty);

	public abstract ImmutableArray<CustomModifier> CustomModifiers { get; }

	public abstract Symbol AssociatedSymbol { get; }

	public abstract bool IsReadOnly { get; }

	public abstract bool IsVolatile { get; }

	public virtual bool IsFixed => false;

	public virtual int FixedSize => 0;

	internal virtual bool IsCapturedFrame => false;

	public abstract bool IsConst { get; }

	public bool IsMetadataConstant
	{
		get
		{
			if (IsConst)
			{
				return Type.SpecialType != SpecialType.System_Decimal;
			}
			return false;
		}
	}

	public virtual bool HasConstantValue
	{
		get
		{
			if (!IsConst)
			{
				return false;
			}
			ConstantValue constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
			if (constantValue != null)
			{
				return !constantValue.IsBad;
			}
			return false;
		}
	}

	public virtual object ConstantValue
	{
		get
		{
			if (!IsConst)
			{
				return null;
			}
			ConstantValue constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
			if (!(constantValue == null))
			{
				return constantValue.Value;
			}
			return null;
		}
	}

	public sealed override SymbolKind Kind => SymbolKind.Field;

	public sealed override bool IsAbstract => false;

	public sealed override bool IsExtern => false;

	public sealed override bool IsOverride => false;

	public sealed override bool IsSealed => false;

	public sealed override bool IsVirtual => false;

	internal abstract bool HasSpecialName { get; }

	internal abstract bool HasRuntimeSpecialName { get; }

	internal abstract bool IsNotSerialized { get; }

	internal virtual bool HasPointerType => Type.IsPointerType();

	internal abstract MarshalPseudoCustomAttributeData MarshallingInformation { get; }

	internal virtual UnmanagedType MarshallingType => MarshallingInformation?.UnmanagedType ?? ((UnmanagedType)0);

	internal abstract int? TypeLayoutOffset { get; }

	protected override int HighestPriorityUseSiteError => 570;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				return useSiteDiagnostic.Code == 570;
			}
			return false;
		}
	}

	public virtual bool IsTupleField => false;

	public virtual bool IsVirtualTupleField => false;

	public virtual bool IsDefaultTupleElement => false;

	public virtual FieldSymbol TupleUnderlyingField => null;

	public virtual FieldSymbol CorrespondingTupleField => null;

	public virtual int TupleElementIndex => -1;

	ISymbol IFieldSymbol.AssociatedSymbol => AssociatedSymbol;

	ITypeSymbol IFieldSymbol.Type => Type;

	ImmutableArray<CustomModifier> IFieldSymbol.CustomModifiers => CustomModifiers;

	IFieldSymbol IFieldSymbol.OriginalDefinition => OriginalDefinition;

	IFieldSymbol IFieldSymbol.CorrespondingTupleField => CorrespondingTupleField;

	ITypeReference IFieldReference.GetType(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		ImmutableArray<CustomModifier> customModifiers = CustomModifiers;
		bool isFixed = IsFixed;
		TypeSymbol symbol = (isFixed ? FixedImplementationType(pEModuleBuilder) : Type);
		ITypeReference typeReference = pEModuleBuilder.Translate(symbol, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		if (isFixed || customModifiers.Length == 0)
		{
			return typeReference;
		}
		return new ModifiedTypeReference(typeReference, customModifiers.As<ICustomModifier>());
	}

	IFieldDefinition IFieldReference.GetResolvedField(EmitContext context)
	{
		return ResolvedFieldImpl((PEModuleBuilder)context.Module);
	}

	private IFieldDefinition ResolvedFieldImpl(PEModuleBuilder moduleBeingBuilt)
	{
		if (IsDefinition && ContainingModule == moduleBeingBuilt.SourceModule)
		{
			return this;
		}
		return null;
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

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		if (!IsDefinition)
		{
			visitor.Visit((IFieldReference)this);
		}
		else if (ContainingModule == ((PEModuleBuilder)visitor.Context.Module).SourceModule)
		{
			visitor.Visit(this);
		}
		else
		{
			visitor.Visit((IFieldReference)this);
		}
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		return ResolvedFieldImpl(moduleBeingBuilt);
	}

	MetadataConstant IFieldDefinition.GetCompileTimeValue(EmitContext context)
	{
		return GetMetadataConstantValue(context);
	}

	internal MetadataConstant GetMetadataConstantValue(EmitContext context)
	{
		if (IsMetadataConstant)
		{
			return ((PEModuleBuilder)context.Module).CreateConstant(Type, ConstantValue, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
		}
		return null;
	}

	internal FieldSymbol()
	{
	}

	internal abstract TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound);

	internal virtual NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		return null;
	}

	internal abstract ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes);

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitField(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitField(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}

	internal FieldSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!(newOwner == ContainingSymbol))
		{
			return new SubstitutedFieldSymbol(newOwner as SubstitutedNamedTypeSymbol, this);
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
		if (DeriveUseSiteDiagnosticFromType(ref result, Type) || DeriveUseSiteDiagnosticFromCustomModifiers(ref result, CustomModifiers))
		{
			return true;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			if (Type.GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes) || Symbol.GetUnificationUseSiteDiagnosticRecursive(ref result, CustomModifiers, this, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitField(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}
}
