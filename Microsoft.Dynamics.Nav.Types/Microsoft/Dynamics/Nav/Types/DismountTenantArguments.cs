using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class DismountTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember(IsRequired = false)]
	[TelemetrySystemMetadata]
	public string ExclusiveAccessTicket { get; set; }

	public DismountTenantArguments()
	{
	}

	public DismountTenantArguments(string tenantId)
	{
		TenantId = tenantId;
	}
}
