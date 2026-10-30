namespace Microsoft.CodeAnalysis;

internal abstract class CommonParameterEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ConstantValue _defaultParameterValue = ConstantValue.Unset;

	private bool _hasCallerLineNumberAttribute;

	private bool _hasCallerFilePathAttribute;

	private bool _hasCallerMemberNameAttribute;

	public ConstantValue DefaultParameterValue
	{
		get
		{
			return _defaultParameterValue;
		}
		set
		{
			_defaultParameterValue = value;
		}
	}

	public bool HasCallerLineNumberAttribute
	{
		get
		{
			return _hasCallerLineNumberAttribute;
		}
		set
		{
			_hasCallerLineNumberAttribute = value;
		}
	}

	public bool HasCallerFilePathAttribute
	{
		get
		{
			return _hasCallerFilePathAttribute;
		}
		set
		{
			_hasCallerFilePathAttribute = value;
		}
	}

	public bool HasCallerMemberNameAttribute
	{
		get
		{
			return _hasCallerMemberNameAttribute;
		}
		set
		{
			_hasCallerMemberNameAttribute = value;
		}
	}
}
