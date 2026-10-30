using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class ParameterSymbol : Symbol, IParameterTypeInformation, IParameterListEntry, IParameterDefinition, IDefinition, IReference, INamedEntity, IParameterSymbol, ISymbol, IEquatable<ISymbol>
{
	internal const string ValueParameterName = "value";

	ImmutableArray<ICustomModifier> IParameterTypeInformation.CustomModifiers => CustomModifiers.As<ICustomModifier>();

	bool IParameterTypeInformation.IsByReference => RefKind != RefKind.None;

	ImmutableArray<ICustomModifier> IParameterTypeInformation.RefCustomModifiers => RefCustomModifiers.As<ICustomModifier>();

	ushort IParameterListEntry.Index => (ushort)Ordinal;

	internal virtual bool HasMetadataConstantValue
	{
		get
		{
			if (ExplicitDefaultConstantValue != null && ExplicitDefaultConstantValue.SpecialType != SpecialType.System_Decimal)
			{
				return ExplicitDefaultConstantValue.SpecialType != SpecialType.System_DateTime;
			}
			return false;
		}
	}

	bool IParameterDefinition.HasDefaultValue => HasMetadataConstantValue;

	bool IParameterDefinition.IsOptional => IsMetadataOptional;

	bool IParameterDefinition.IsIn => IsMetadataIn;

	bool IParameterDefinition.IsMarshalledExplicitly => IsMarshalledExplicitly;

	internal virtual bool IsMarshalledExplicitly => MarshallingInformation != null;

	bool IParameterDefinition.IsOut => IsMetadataOut;

	IMarshallingInformation IParameterDefinition.MarshallingInformation => MarshallingInformation;

	ImmutableArray<byte> IParameterDefinition.MarshallingDescriptor => MarshallingDescriptor;

	internal virtual ImmutableArray<byte> MarshallingDescriptor => default;

	string INamedEntity.Name => MetadataName;

	public new virtual ParameterSymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public abstract TypeSymbol Type { get; }

	public abstract RefKind RefKind { get; }

	public abstract ImmutableArray<CustomModifier> CustomModifiers { get; }

	public abstract ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	internal abstract MarshalPseudoCustomAttributeData MarshallingInformation { get; }

	internal virtual UnmanagedType MarshallingType => MarshallingInformation?.UnmanagedType ?? ((UnmanagedType)0);

	internal bool IsMarshalAsObject
	{
		get
		{
			UnmanagedType marshallingType = MarshallingType;
			if ((uint)(marshallingType - 25) <= 1u || marshallingType == UnmanagedType.Interface)
			{
				return true;
			}
			return false;
		}
	}

	public abstract int Ordinal { get; }

	public abstract bool IsParams { get; }

	public bool IsOptional
	{
		get
		{
			if (!IsParams && IsMetadataOptional)
			{
				RefKind refKind;
				if ((refKind = RefKind) != RefKind.None)
				{
					switch (refKind)
					{
					case RefKind.Ref:
						return ContainingSymbol.ContainingType.IsComImport;
					default:
						return false;
					case RefKind.In:
						break;
					}
				}
				return true;
			}
			return false;
		}
	}

	internal abstract bool IsMetadataOptional { get; }

	internal abstract bool IsMetadataIn { get; }

	internal abstract bool IsMetadataOut { get; }

	public bool HasExplicitDefaultValue
	{
		get
		{
			if (IsOptional)
			{
				return ExplicitDefaultConstantValue != null;
			}
			return false;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public object ExplicitDefaultValue
	{
		get
		{
			if (HasExplicitDefaultValue)
			{
				return ExplicitDefaultConstantValue.Value;
			}
			throw new InvalidOperationException();
		}
	}

	internal abstract ConstantValue ExplicitDefaultConstantValue { get; }

	public sealed override SymbolKind Kind => SymbolKind.Parameter;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsStatic => false;

	public override bool IsExtern => false;

	public virtual bool IsThis => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal abstract bool IsIDispatchConstant { get; }

	internal abstract bool IsIUnknownConstant { get; }

	internal abstract bool IsCallerFilePath { get; }

	internal abstract bool IsCallerLineNumber { get; }

	internal abstract bool IsCallerMemberName { get; }

	protected sealed override int HighestPriorityUseSiteError => 648;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo result = null;
			DeriveUseSiteDiagnosticFromParameter(ref result, this);
			if (result != null)
			{
				return result.Code == 648;
			}
			return false;
		}
	}

	ITypeSymbol IParameterSymbol.Type => Type;

	ImmutableArray<CustomModifier> IParameterSymbol.CustomModifiers => CustomModifiers;

	ImmutableArray<CustomModifier> IParameterSymbol.RefCustomModifiers => RefCustomModifiers;

	IParameterSymbol IParameterSymbol.OriginalDefinition => OriginalDefinition;

	ITypeReference IParameterTypeInformation.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(Type, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	MetadataConstant IParameterDefinition.GetDefaultValue(EmitContext context)
	{
		return GetMetadataConstantValue(context);
	}

	internal MetadataConstant GetMetadataConstantValue(EmitContext context)
	{
		if (!HasMetadataConstantValue)
		{
			return null;
		}
		ConstantValue explicitDefaultConstantValue = ExplicitDefaultConstantValue;
		TypeSymbol type = ((explicitDefaultConstantValue.SpecialType == SpecialType.None) ? Type : ContainingAssembly.GetSpecialType(explicitDefaultConstantValue.SpecialType));
		return ((PEModuleBuilder)context.Module).CreateConstant(type, explicitDefaultConstantValue.Value, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		PEModuleBuilder pEModuleBuilder = (PEModuleBuilder)context.Module;
		if (IsDefinition && ContainingModule == pEModuleBuilder.SourceModule)
		{
			return this;
		}
		return null;
	}

	internal ParameterSymbol()
	{
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParameter(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}
}
