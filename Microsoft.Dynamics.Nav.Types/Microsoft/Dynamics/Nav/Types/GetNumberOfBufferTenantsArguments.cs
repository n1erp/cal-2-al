using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetNumberOfBufferTenantsArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public DatabaseInfo DatabaseInfo { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool IncludeObsoleteBufferTenants { get; set; }

	[DataMember]
	public string DatabaseId { get; set; }

	public GetNumberOfBufferTenantsArguments()
	{
	}

	public GetNumberOfBufferTenantsArguments(DatabaseInfo databaseInfo = null)
	{
		DatabaseInfo = databaseInfo;
	}

	public GetNumberOfBufferTenantsArguments(string databaseId)
	{
		DatabaseId = databaseId;
	}
}
