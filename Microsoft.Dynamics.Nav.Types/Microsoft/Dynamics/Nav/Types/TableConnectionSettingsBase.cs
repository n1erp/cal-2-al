using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(ExternalSqlTableConnectionSettings))]
[KnownType(typeof(CrmTableConnectionSettings))]
[KnownType(typeof(ExchangeTableConnectionSettings))]
public abstract class TableConnectionSettingsBase
{
	[DataMember]
	[TelemetrySystemMetadata]
	public TableConnectionType TableConnectionType { get; private set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Id { get; private set; }

	protected TableConnectionSettingsBase(TableConnectionType tableConnectionType, string id)
	{
		TableConnectionType = tableConnectionType;
		Id = id;
	}
}
