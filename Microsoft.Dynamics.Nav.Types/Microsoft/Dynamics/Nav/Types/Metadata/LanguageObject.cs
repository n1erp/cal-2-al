namespace Microsoft.Dynamics.Nav.Types.Metadata;

internal class LanguageObject
{
	public int LanguageCode { get; private set; }

	public int PropertyType { get; private set; }

	public ControlType ControlType { get; private set; }

	public int ControlId { get; private set; }

	public string ItemValue { get; private set; }

	public bool RequestPage { get; private set; }

	public int CodeId { get; private set; }

	public int PropertyId { get; private set; }

	protected internal int DataIndex { get; set; }

	public LanguageObject(bool isRequestPage, int language, int propertyType, ControlType type, int controlId, int codeId, int propertyId, string itemValue)
	{
		LanguageCode = language;
		PropertyType = propertyType;
		ControlId = controlId;
		ControlType = type;
		ItemValue = itemValue;
		RequestPage = isRequestPage;
		CodeId = codeId;
		PropertyId = propertyId;
	}
}
