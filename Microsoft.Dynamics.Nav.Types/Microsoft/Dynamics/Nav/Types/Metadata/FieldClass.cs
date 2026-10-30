using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum FieldClass
{
	[EnumMember]
	Normal,
	[EnumMember]
	FlowField,
	[EnumMember]
	FlowFilter
}
