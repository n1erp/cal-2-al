using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class MountApplicationArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string DatabaseName { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string DatabaseServer { get; set; }

	[DataMember]
	[TelemetryEuii]
	public string DatabaseUserName { get; set; }

	[DataMember]
	public ProtectedDatabasePassword DatabasePassword { get; set; }

	public MountApplicationArguments()
	{
	}

	public MountApplicationArguments(string databaseServer, string databaseName, string databaseUserName = "", ProtectedDatabasePassword databasePassword = null)
	{
		if (string.IsNullOrEmpty(databaseServer))
		{
			databaseServer = ".";
		}
		DatabaseServer = databaseServer;
		DatabaseName = databaseName;
		DatabaseUserName = databaseUserName;
		DatabasePassword = databasePassword;
	}
}
