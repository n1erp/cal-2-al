using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemoveTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ForceImmediateDeletion { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	public RemoveTenantArguments()
	{
	}

	public RemoveTenantArguments(string tenantId, bool forceImmediateDeletion, bool async = true)
	{
		TenantId = tenantId;
		ForceImmediateDeletion = forceImmediateDeletion;
		Async = async;
	}
}
