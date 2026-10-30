using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum TableType
{
	[EnumMember]
	Normal,
	[EnumMember]
	CRM,
	[EnumMember]
	ExternalSQL,
	[EnumMember]
	Exchange,
	[EnumMember]
	MicrosoftGraph,
	[EnumMember]
	Query
}
