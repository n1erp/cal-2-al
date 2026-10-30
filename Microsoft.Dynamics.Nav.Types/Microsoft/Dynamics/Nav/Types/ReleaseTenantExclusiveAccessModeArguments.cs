using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ReleaseTenantExclusiveAccessModeArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string ExclusiveAccessTicket { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Force { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Justification { get; set; }
}
