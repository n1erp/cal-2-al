using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UnpublishNavAppArguments : IExtensibleDataObject
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
	public Guid? PackageId { get; set; }

	[DataMember]
	public string TenantId { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }
}
