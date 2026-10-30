namespace Microsoft.CodeAnalysis;

internal class CommonEventEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ObsoleteAttributeData _obsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	public ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_obsoleteAttributeData.IsUninitialized)
			{
				return _obsoleteAttributeData;
			}
			return null;
		}
		set
		{
			_obsoleteAttributeData = value;
		}
	}
}
