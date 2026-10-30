using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RepairNavAppResponse : ServerResponse
{
	[DataMember]
	private readonly string error;

	public string Error => error;

	public bool DidErrorOccur => !string.IsNullOrWhiteSpace(Error);

	private RepairNavAppResponse(string error)
	{
		this.error = error;
	}

	public static RepairNavAppResponse CreateForError(string error)
	{
		return new RepairNavAppResponse(error);
	}

	public static RepairNavAppResponse CreateForSuccess()
	{
		return new RepairNavAppResponse(null);
	}
}
