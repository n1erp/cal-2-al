using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppCapabilityResponse : ServerResponse
{
	[DataMember]
	public int CapabilityId { get; set; }
}
