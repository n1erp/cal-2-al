using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RegisterTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string RegisteredTenantId { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantDatabaseId { get; set; }

	[DataMember]
	[TelemetryOii]
	public string BufferTenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Force { get; set; }

	public RegisterTenantArguments()
	{
	}

	public RegisterTenantArguments(string registeredTenantId, string tenantDatabaseId = null, string bufferTenantId = null, bool force = false)
	{
		if (registeredTenantId == null)
		{
			throw new ArgumentNullException("registeredTenantId");
		}
		RegisteredTenantId = registeredTenantId;
		TenantDatabaseId = tenantDatabaseId;
		BufferTenantId = bufferTenantId;
		Force = force;
	}
}
