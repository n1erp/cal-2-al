using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceClonedParameterSymbol : SourceParameterSymbolBase
{
	private readonly bool _suppressOptional;

	private readonly SourceParameterSymbol _originalParam;

	public override bool IsImplicitlyDeclared => true;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsParams
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.IsParams;
			}
			return false;
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.IsMetadataOptional;
			}
			return _originalParam.HasOptionalAttribute;
		}
	}

	internal override ConstantValue ExplicitDefaultConstantValue
	{
		get
		{
			if (!_suppressOptional)
			{
				return _originalParam.ExplicitDefaultConstantValue;
			}
			return _originalParam.DefaultValueFromAttributes;
		}
	}

	internal override ConstantValue DefaultValueFromAttributes => _originalParam.DefaultValueFromAttributes;

	public override TypeSymbol Type => _originalParam.Type;

	public override RefKind RefKind => _originalParam.RefKind;

	internal override bool IsMetadataIn => _originalParam.IsMetadataIn;

	internal override bool IsMetadataOut => _originalParam.IsMetadataOut;

	public override ImmutableArray<Location> Locations => _originalParam.Locations;

	public sealed override string Name => _originalParam.Name;

	public override ImmutableArray<CustomModifier> CustomModifiers => _originalParam.CustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _originalParam.RefCustomModifiers;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => _originalParam.MarshallingInformation;

	internal override bool IsIDispatchConstant => _originalParam.IsIDispatchConstant;

	internal override bool IsIUnknownConstant => _originalParam.IsIUnknownConstant;

	internal override bool IsCallerFilePath => _originalParam.IsCallerFilePath;

	internal override bool IsCallerLineNumber => _originalParam.IsCallerLineNumber;

	internal override bool IsCallerMemberName => _originalParam.IsCallerMemberName;

	internal SourceClonedParameterSymbol(SourceParameterSymbol originalParam, Symbol newOwner, int newOrdinal, bool suppressOptional)
		: base(newOwner, newOrdinal)
	{
		_suppressOptional = suppressOptional;
		_originalParam = originalParam;
	}

	internal override ParameterSymbol WithCustomModifiersAndParams(TypeSymbol newType, ImmutableArray<CustomModifier> newCustomModifiers, ImmutableArray<CustomModifier> newRefCustomModifiers, bool newIsParams)
	{
		return new SourceClonedParameterSymbol(_originalParam.WithCustomModifiersAndParamsCore(newType, newCustomModifiers, newRefCustomModifiers, newIsParams), ContainingSymbol, Ordinal, _suppressOptional);
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _originalParam.GetAttributes();
	}
}
