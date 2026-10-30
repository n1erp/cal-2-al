using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppDependencyResponse : ServerResponse
{
	[DataMember]
	public Guid Id { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string Publisher { get; set; }

	[DataMember]
	public Version Version { get; set; }

	[DataMember]
	public Version CompatibilityId { get; set; }
}
