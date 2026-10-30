using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingTypeParameterSymbol : WrappedTypeParameterSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private ImmutableArray<CSharpAttributeData> _lazyCustomAttributes;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingTypeParameter.ContainingSymbol);

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingTypeParameterSymbol(RetargetingModuleSymbol retargetingModule, TypeParameterSymbol underlyingTypeParameter)
		: base(underlyingTypeParameter)
	{
		_retargetingModule = retargetingModule;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return RetargetingTranslator.GetRetargetedAttributes(_underlyingTypeParameter.GetAttributes(), ref _lazyCustomAttributes);
	}

	internal override ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress)
	{
		return RetargetingTranslator.Retarget(_underlyingTypeParameter.GetConstraintTypes(inProgress));
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress)
	{
		return RetargetingTranslator.Retarget(_underlyingTypeParameter.GetInterfaces(inProgress));
	}

	internal override NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress)
	{
		return RetargetingTranslator.Retarget(_underlyingTypeParameter.GetEffectiveBaseClass(inProgress), RetargetOptions.RetargetPrimitiveTypesByTypeCode);
	}

	internal override TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress)
	{
		return RetargetingTranslator.Retarget(_underlyingTypeParameter.GetDeducedBaseType(inProgress), RetargetOptions.RetargetPrimitiveTypesByTypeCode);
	}
}
