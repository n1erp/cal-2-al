namespace Microsoft.CodeAnalysis;

internal class CommonParameterWellKnownAttributeData : WellKnownAttributeData, IMarshalAsAttributeTarget
{
	private bool _hasOutAttribute;

	private bool _hasInAttribute;

	private MarshalPseudoCustomAttributeData _lazyMarshalAsData;

	private bool _hasIDispatchConstantAttribute;

	private bool _hasIUnknownConstantAttribute;

	public bool HasOutAttribute
	{
		get
		{
			return _hasOutAttribute;
		}
		set
		{
			_hasOutAttribute = value;
		}
	}

	public bool HasInAttribute
	{
		get
		{
			return _hasInAttribute;
		}
		set
		{
			_hasInAttribute = value;
		}
	}

	public MarshalPseudoCustomAttributeData MarshallingInformation => _lazyMarshalAsData;

	public bool HasIDispatchConstantAttribute
	{
		get
		{
			return _hasIDispatchConstantAttribute;
		}
		set
		{
			_hasIDispatchConstantAttribute = value;
		}
	}

	public bool HasIUnknownConstantAttribute
	{
		get
		{
			return _hasIUnknownConstantAttribute;
		}
		set
		{
			_hasIUnknownConstantAttribute = value;
		}
	}

	MarshalPseudoCustomAttributeData IMarshalAsAttributeTarget.GetOrCreateData()
	{
		if (_lazyMarshalAsData == null)
		{
			_lazyMarshalAsData = new MarshalPseudoCustomAttributeData();
		}
		return _lazyMarshalAsData;
	}
}
