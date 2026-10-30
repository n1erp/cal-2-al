using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MoveTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	[DataMember]
	[TelemetryOii]
	public string DestinationTenantDatabaseId { get; set; }

	[DataMember]
	[TelemetryOii]
	public bool ImmediateSourceTenantRemoval { get; set; }

	public MoveTenantArguments()
	{
	}

	public MoveTenantArguments(string tenantId, string destinationTenantDatabaseId)
	{
		TenantId = tenantId;
		DestinationTenantDatabaseId = destinationTenantDatabaseId;
	}
}
