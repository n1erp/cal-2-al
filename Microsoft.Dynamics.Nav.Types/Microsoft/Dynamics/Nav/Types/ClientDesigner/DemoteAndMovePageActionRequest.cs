using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DemoteAndMovePageActionRequest : MovePageActionRequest
{
	[DataMember]
	public string ActionName { get; set; }

	public DemoteAndMovePageActionRequest(int pageId, string actionName, ActionContainerType containerType, string parentActionName, string relativeActionName)
		: base(pageId, actionName, containerType, parentActionName, relativeActionName)
	{
		ActionName = actionName;
	}
}
