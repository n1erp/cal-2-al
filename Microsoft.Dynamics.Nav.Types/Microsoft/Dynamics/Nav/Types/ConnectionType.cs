using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum ConnectionType
{
	[EnumMember]
	[SessionEventClientType(ClientType.Unknown)]
	UnknownClient = 0,
	[EnumMember]
	[SessionEventClientType(ClientType.Rtc)]
	RoleTailoredClient = 109,
	[EnumMember]
	[SessionEventClientType(ClientType.WebServiceClient)]
	WebServiceClient = 110,
	[EnumMember]
	[SessionEventClientType(ClientType.Nas)]
	Nas = 107,
	[EnumMember]
	[SessionEventClientType(ClientType.Management)]
	ManagementClient = 112,
	[EnumMember]
	[SessionEventClientType(ClientType.SharePoint)]
	SharePoint = 113,
	[EnumMember]
	[SessionEventClientType(ClientType.Background)]
	Background = 114,
	[EnumMember]
	[SessionEventClientType(ClientType.ClientService)]
	ClientService = 115,
	[EnumMember]
	[SessionEventClientType(ClientType.WebClient)]
	WebClient = 116,
	[EnumMember]
	[SessionEventClientType(ClientType.Tablet)]
	Tablet = 117,
	[EnumMember]
	[SessionEventClientType(ClientType.Phone)]
	Phone = 118,
	[EnumMember]
	[SessionEventClientType(ClientType.Desktop)]
	Desktop = 119
}
