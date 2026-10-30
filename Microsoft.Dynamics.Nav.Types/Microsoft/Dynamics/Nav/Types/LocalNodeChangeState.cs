using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
public enum LocalNodeChangeState
{
	[EnumMember]
	Unchanged,
	[EnumMember]
	New,
	[EnumMember]
	ValueChanged,
	[EnumMember]
	DescendantChanged
}
