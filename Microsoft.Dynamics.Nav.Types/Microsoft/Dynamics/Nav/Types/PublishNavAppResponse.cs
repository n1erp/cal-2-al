using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PublishNavAppResponse
{
	[MessageHeader]
	private string error;

	public string Error
	{
		get
		{
			return error;
		}
		set
		{
			error = value;
		}
	}

	public bool DidErrorOccur => !string.IsNullOrWhiteSpace(Error);

	public static PublishNavAppResponse CreateForError(string error)
	{
		return new PublishNavAppResponse
		{
			Error = error
		};
	}

	public static PublishNavAppResponse CreateForSuccess()
	{
		return new PublishNavAppResponse();
	}
}
