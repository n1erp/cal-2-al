namespace Microsoft.CodeAnalysis;

internal class CommonPropertyWellKnownAttributeData : WellKnownAttributeData
{
	private bool _hasSpecialNameAttribute;

	private bool _hasExcludeFromCodeCoverageAttribute;

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

	public bool HasExcludeFromCodeCoverageAttribute
	{
		get
		{
			return _hasExcludeFromCodeCoverageAttribute;
		}
		set
		{
			_hasExcludeFromCodeCoverageAttribute = value;
		}
	}
}
