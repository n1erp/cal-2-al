using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UninstallNavAppArguments : IExtensibleDataObject
{
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
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool DoNotSaveData { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Force { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public UninstallNavAppArguments()
	{
	}

	public UninstallNavAppArguments(string name, string publisher, Version version, string tenantId, bool doNotSaveData, bool force, string origin)
	{
		Name = name;
		Publisher = publisher;
		Version = version;
		TenantId = tenantId;
		DoNotSaveData = doNotSaveData;
		Force = force;
		Origin = origin;
	}
}
