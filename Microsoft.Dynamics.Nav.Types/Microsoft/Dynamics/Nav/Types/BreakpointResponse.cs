using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
public enum BreakpointResponse
{
	[EnumMember]
	Continue,
	[EnumMember]
	StepOver,
	[EnumMember]
	StepIn,
	[EnumMember]
	StepOut,
	[EnumMember]
	ReleaseConnection,
	[EnumMember]
	AbortActivity
}
