using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class StartNavAppDataUpgradeArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

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
	public bool Force { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	[DataMember]
	public string Language { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public StartNavAppDataUpgradeArguments()
	{
	}

	public StartNavAppDataUpgradeArguments(string name, string publisher, Version version, string tenantId, bool force, string origin, string language = null)
	{
		TenantId = tenantId;
		Publisher = publisher;
		Version = version;
		Name = name;
		Force = force;
		Origin = origin;
		Language = language;
	}
}
