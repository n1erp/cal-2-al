using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class AddPageFieldRequest : AddPageControlRequestBase
{
	[DataMember]
	public int FieldId { get; set; }

	[DataMember]
	public bool IsTableField { get; set; }

	public AddPageFieldRequest(int pageId, string parentControlName, string relativeControlName, int fieldId, bool isTableField)
		: base(pageId, parentControlName, relativeControlName)
	{
		FieldId = fieldId;
		IsTableField = isTableField;
	}
}
