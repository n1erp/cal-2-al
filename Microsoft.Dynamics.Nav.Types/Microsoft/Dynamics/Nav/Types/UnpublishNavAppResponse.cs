using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class UnpublishNavAppResponse : ServerResponse
{
	[DataMember]
	private readonly string error;

	public string Error => error;

	public bool DidErrorOccur => !string.IsNullOrWhiteSpace(Error);

	private UnpublishNavAppResponse(string error)
	{
		this.error = error;
	}

	public static UnpublishNavAppResponse CreateForError(string error)
	{
		return new UnpublishNavAppResponse(error);
	}

	public static UnpublishNavAppResponse CreateForSuccess()
	{
		return new UnpublishNavAppResponse(null);
	}
}
