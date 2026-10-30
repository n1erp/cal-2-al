using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RepairNavAppArguments : IExtensibleDataObject
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
	[TelemetrySystemMetadata]
	public bool SkipSigningVerification { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }
}
