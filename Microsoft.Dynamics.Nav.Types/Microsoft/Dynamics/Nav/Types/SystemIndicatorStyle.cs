using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum SystemIndicatorStyle
{
	[EnumMember]
	Standard,
	[EnumMember]
	Accent1,
	[EnumMember]
	Accent2,
	[EnumMember]
	Accent3,
	[EnumMember]
	Accent4,
	[EnumMember]
	Accent5,
	[EnumMember]
	Accent6,
	[EnumMember]
	Accent7,
	[EnumMember]
	Accent8,
	[EnumMember]
	Accent9
}
