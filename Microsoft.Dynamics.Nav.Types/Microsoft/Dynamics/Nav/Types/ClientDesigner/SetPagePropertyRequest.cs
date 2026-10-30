using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetPagePropertyRequest : NavPageDesignerRequest
{
	[DataMember]
	public string PropertyName { get; set; }

	[DataMember]
	public object PropertyValue { get; set; }

	public SetPagePropertyRequest(int pageId, string propertyName, object propertyValue)
		: base(pageId)
	{
		PropertyName = propertyName;
		PropertyValue = propertyValue;
	}
}
