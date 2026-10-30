using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class UploadTempFileResponse
{
	[MessageHeader(MustUnderstand = true)]
	private string fileName;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}
}
