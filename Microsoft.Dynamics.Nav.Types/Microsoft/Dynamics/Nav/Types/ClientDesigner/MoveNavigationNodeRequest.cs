using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MoveNavigationNodeRequest : NavNavigationDesignerRequest
{
	[DataMember]
	public Guid RelativeNodeId { get; set; }

	public MoveNavigationNodeRequest(Guid nodeId, Guid relativeNodeId)
		: base(nodeId)
	{
		RelativeNodeId = relativeNodeId;
	}
}
