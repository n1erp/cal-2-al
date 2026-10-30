using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class InstallNavAppArguments : IExtensibleDataObject
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
	public bool DoNotLoadData { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Force { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	[DataMember]
	public string Language { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public InstallNavAppArguments()
	{
	}

	public InstallNavAppArguments(string name, string publisher, Version version, string tenantId, bool doNotLoadData, bool force, string origin, string language = null)
	{
		Name = name;
		Publisher = publisher;
		Version = version;
		TenantId = tenantId;
		DoNotLoadData = doNotLoadData;
		Force = force;
		Origin = origin;
		Language = language;
	}
}
