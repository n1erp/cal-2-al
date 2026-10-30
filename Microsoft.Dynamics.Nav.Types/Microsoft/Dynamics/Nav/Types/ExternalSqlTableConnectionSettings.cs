using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ExternalSqlTableConnectionSettings : TableConnectionSettingsBase
{
	[DataMember]
	[TelemetrySystemMetadata]
	public string DatabaseServer { get; private set; }

	[DataMember]
	[TelemetryOii]
	public string DatabaseName { get; private set; }

	[DataMember]
	[TelemetryEuii]
	public string UserName { get; private set; }

	[DataMember]
	public ProtectedDatabasePassword Password { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int ConnectionTimeout { get; private set; }

	public ExternalSqlTableConnectionSettings(string id, string databaseServer, string databaseName, string userName, ProtectedDatabasePassword password, int connectionTimeout)
		: base(TableConnectionType.ExternalSQL, id)
	{
		DatabaseServer = databaseServer;
		DatabaseName = databaseName;
		UserName = userName;
		Password = password;
		ConnectionTimeout = connectionTimeout;
	}

	[OnDeserializing]
	private void OnDeserializing(StreamingContext c)
	{
		ConnectionTimeout = ServerUserSettings.Instance.SqlConnectionTimeout.TotalSeconds;
	}
}
