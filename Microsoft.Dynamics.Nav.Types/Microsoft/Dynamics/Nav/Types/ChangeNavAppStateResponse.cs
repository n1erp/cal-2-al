using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ChangeNavAppStateResponse : ServerResponse
{
	[DataMember]
	private readonly ChangeNavAppStateResult result;

	[DataMember]
	private readonly string error;

	public ChangeNavAppStateResult Result => result;

	public string Error => error;

	public ChangeNavAppStateResponse(ChangeNavAppStateResult result)
		: this(result, string.Empty)
	{
	}

	public ChangeNavAppStateResponse(ChangeNavAppStateResult result, string error)
	{
		this.result = result;
		this.error = error;
	}
}
