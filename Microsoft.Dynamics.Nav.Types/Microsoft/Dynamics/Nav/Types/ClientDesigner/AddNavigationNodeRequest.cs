using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class AddNavigationNodeRequest : NavNavigationDesignerRequest
{
	[DataMember]
	public Guid RelativeNodeId { get; set; }

	public AddNavigationNodeRequest(Guid relativeNodeId, Guid nodeId)
		: base(nodeId)
	{
		RelativeNodeId = relativeNodeId;
	}
}
