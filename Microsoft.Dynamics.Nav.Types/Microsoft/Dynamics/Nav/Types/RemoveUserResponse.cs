using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RemoveUserResponse : ServerResponse
{
	[DataMember]
	private readonly RemoveUserResult result;

	public RemoveUserResult Result => result;

	public RemoveUserResponse(RemoveUserResult result)
	{
		this.result = result;
	}
}
