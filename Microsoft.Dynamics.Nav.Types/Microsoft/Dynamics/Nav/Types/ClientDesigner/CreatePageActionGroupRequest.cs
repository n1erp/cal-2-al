using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CreatePageActionGroupRequest : AddPageActionRequest
{
	public CreatePageActionGroupRequest(int pageId, ActionContainerType containerType, string parentActionName, string relativeActionName, string groupName)
		: base(pageId, containerType, parentActionName, relativeActionName, groupName)
	{
	}
}
