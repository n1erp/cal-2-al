using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetNavAppTenantArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetrySystemMetadata]
	public string AppId { get; set; }

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
	public bool IncludeFailed { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public GetNavAppTenantArguments()
	{
	}

	public GetNavAppTenantArguments(string appId, string name, string publisher, Version version, bool includeFailed)
	{
		AppId = appId;
		Name = name;
		Publisher = publisher;
		Version = version;
		IncludeFailed = includeFailed;
	}
}
