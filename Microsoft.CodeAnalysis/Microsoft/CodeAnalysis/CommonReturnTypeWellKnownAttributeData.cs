namespace Microsoft.CodeAnalysis;

internal class CommonReturnTypeWellKnownAttributeData : WellKnownAttributeData, IMarshalAsAttributeTarget
{
	private MarshalPseudoCustomAttributeData _lazyMarshalAsData;

	public MarshalPseudoCustomAttributeData MarshallingInformation => _lazyMarshalAsData;

	MarshalPseudoCustomAttributeData IMarshalAsAttributeTarget.GetOrCreateData()
	{
		if (_lazyMarshalAsData == null)
		{
			_lazyMarshalAsData = new MarshalPseudoCustomAttributeData();
		}
		return _lazyMarshalAsData;
	}
}
