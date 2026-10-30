using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CopyTenantDataArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string SourceTenantId { get; set; }

	[DataMember]
	[TelemetryOii]
	public string DestinationTenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	public CopyTenantDataArguments()
	{
	}

	public CopyTenantDataArguments(string sourceTenantId, string destinationTenantId, bool async)
	{
		SourceTenantId = sourceTenantId;
		DestinationTenantId = destinationTenantId;
		Async = async;
	}
}
