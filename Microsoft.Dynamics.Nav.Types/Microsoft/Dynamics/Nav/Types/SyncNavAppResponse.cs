using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SyncNavAppResponse : ServerResponse
{
	[DataMember]
	private readonly SyncNavAppStatus result;

	[DataMember]
	private readonly string error;

	public SyncNavAppStatus Result => result;

	public string Error => error;

	public SyncNavAppResponse(SyncNavAppStatus result)
		: this(result, string.Empty)
	{
	}

	public SyncNavAppResponse(SyncNavAppStatus result, string error)
	{
		this.result = result;
		this.error = error;
	}
}
