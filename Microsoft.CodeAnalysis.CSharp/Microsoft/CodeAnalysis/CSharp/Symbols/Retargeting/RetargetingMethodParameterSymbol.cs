namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingMethodParameterSymbol : RetargetingParameterSymbol
{
	private readonly RetargetingMethodSymbol _retargetingMethod;

	protected override RetargetingModuleSymbol RetargetingModule => _retargetingMethod.RetargetingModule;

	public RetargetingMethodParameterSymbol(RetargetingMethodSymbol retargetingMethod, ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
		_retargetingMethod = retargetingMethod;
	}
}
