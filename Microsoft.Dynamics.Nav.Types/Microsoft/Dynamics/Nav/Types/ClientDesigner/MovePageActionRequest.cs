using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[KnownType(typeof(ActionContainerType))]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MovePageActionRequest : MovePageControlRequestBase
{
	[DataMember]
	public ActionContainerType ContainerType { get; set; }

	public MovePageActionRequest(int pageId, string actionName, ActionContainerType containerType, string parentActionName, string relativeActionName)
		: base(pageId, actionName, parentActionName, relativeActionName)
	{
		ContainerType = containerType;
	}
}
