using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class NavNavigationDesignerRequest : NavDesignerRequest
{
	[DataMember]
	public Guid NodeId { get; set; }

	protected NavNavigationDesignerRequest(Guid nodeId)
	{
		NodeId = nodeId;
	}
}
