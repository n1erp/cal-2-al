using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemoveNavigationNodeRequest : NavNavigationDesignerRequest
{
	public RemoveNavigationNodeRequest(Guid nodeId)
		: base(nodeId)
	{
	}
}
