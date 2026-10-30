using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppObjectPrerequisiteResponse : ServerResponse
{
	[DataMember]
	public ObjectType Type { get; set; }

	[DataMember]
	public int Id { get; set; }
}
