using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class BufferTenantArguments : SetTenantArguments
{
	[DataMember]
	[TelemetryOii]
	public string TenantDatabaseId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int TimeoutSeconds { get; set; }
}
