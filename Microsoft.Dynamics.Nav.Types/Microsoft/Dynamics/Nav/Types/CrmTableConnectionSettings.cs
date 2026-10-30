using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class CrmTableConnectionSettings : TableConnectionSettingsBase
{
	[DataMember]
	[TelemetryCustomerContent]
	public string ConnectionString { get; private set; }

	public CrmTableConnectionSettings(string id, string connectionString)
		: base(TableConnectionType.CRM, id)
	{
		ConnectionString = connectionString;
	}
}
