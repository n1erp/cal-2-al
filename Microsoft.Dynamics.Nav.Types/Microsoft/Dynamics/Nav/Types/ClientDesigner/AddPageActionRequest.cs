using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[KnownType(typeof(ActionContainerType))]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class AddPageActionRequest : AddPageControlRequestBase
{
	[DataMember]
	public string ActionName { get; set; }

	[DataMember]
	public ActionContainerType ContainerType { get; set; }

	public AddPageActionRequest(int pageId, ActionContainerType containerType, string parentActionName, string relativeActionName, string actionName)
		: base(pageId, parentActionName, relativeActionName)
	{
		ActionName = actionName;
		ContainerType = containerType;
	}
}
