using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SetNavigationNodePropertyRequest : NavNavigationDesignerRequest
{
	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public object Value { get; set; }

	public SetNavigationNodePropertyRequest(Guid nodeId, string name, object value)
		: base(nodeId)
	{
		Name = name;
		Value = value;
	}
}
