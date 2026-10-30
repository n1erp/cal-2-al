using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConvertTenantDatabaseToSharedSchemaArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool PerTable { get; set; }

	public ConvertTenantDatabaseToSharedSchemaArguments()
	{
	}

	public ConvertTenantDatabaseToSharedSchemaArguments(string tenantId, bool async = false, bool perTable = false)
	{
		TenantId = tenantId;
		Async = async;
		PerTable = perTable;
	}
}
