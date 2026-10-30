using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DemotePageActionRequest : NavPageDesignerRequest
{
	[DataMember]
	public string ActionName { get; set; }

	public DemotePageActionRequest(int pageId, string actionName)
		: base(pageId)
	{
		ActionName = actionName;
	}
}
