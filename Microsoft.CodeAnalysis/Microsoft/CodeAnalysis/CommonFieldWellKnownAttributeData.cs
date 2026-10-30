namespace Microsoft.CodeAnalysis;

internal class CommonFieldWellKnownAttributeData : WellKnownAttributeData, IMarshalAsAttributeTarget
{
	private int _offset;

	private const int Uninitialized = -1;

	private ConstantValue _constValue = ConstantValue.Unset;

	private bool _hasSpecialNameAttribute;

	private bool _hasNonSerializedAttribute;

	private MarshalPseudoCustomAttributeData _lazyMarshalAsData;

	public int? Offset
	{
		get
		{
			if (_offset == -1)
			{
				return null;
			}
			return _offset;
		}
	}

	public ConstantValue ConstValue
	{
		get
		{
			return _constValue;
		}
		set
		{
			_constValue = value;
		}
	}

	public bool HasSpecialNameAttribute
	{
		get
		{
			return _hasSpecialNameAttribute;
		}
		set
		{
			_hasSpecialNameAttribute = value;
		}
	}

	public bool HasNonSerializedAttribute
	{
		get
		{
			return _hasNonSerializedAttribute;
		}
		set
		{
			_hasNonSerializedAttribute = value;
		}
	}

	public MarshalPseudoCustomAttributeData MarshallingInformation => _lazyMarshalAsData;

	public CommonFieldWellKnownAttributeData()
	{
		_offset = -1;
	}

	public void SetFieldOffset(int offset)
	{
		_offset = offset;
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
