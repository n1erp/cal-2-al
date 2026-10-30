using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetTableSynchSetupForDataUpgradeArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetrySystemMetadata]
	public string DatabaseServerName { get; set; }

	[DataMember]
	[TelemetryOii]
	public string DatabaseName { get; set; }

	[DataMember]
	[TelemetryEuii]
	public string UserName { get; set; }

	[DataMember]
	public ProtectedDatabasePassword Password { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public GetTableSynchSetupForDataUpgradeArguments()
	{
	}

	public GetTableSynchSetupForDataUpgradeArguments(string databaseServer, string databaseName, string userName, ProtectedDatabasePassword password)
	{
		DatabaseServerName = databaseServer;
		DatabaseName = databaseName;
		UserName = userName;
		Password = password;
	}
}
