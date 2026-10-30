using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class DatabaseInfo
{
	[DataMember]
	[TelemetryOii]
	public string Name { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Server { get; set; }

	[DataMember]
	[TelemetryEuii]
	public string UserName { get; set; }

	[DataMember]
	public ProtectedDatabasePassword Password { get; set; }
}
