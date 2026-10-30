using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SyncNavAppArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantDatabaseId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Name { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Publisher { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public Version Version { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public NavAppSyncMode Mode { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public SyncNavAppArguments()
	{
	}

	public SyncNavAppArguments(string tenantId, string tenantDatabaseId, string name, string publisher, Version version, NavAppSyncMode mode, string origin)
	{
		TenantId = tenantId;
		TenantDatabaseId = tenantDatabaseId;
		Publisher = publisher;
		Version = version;
		Name = name;
		Mode = mode;
		Origin = origin;
	}
}
