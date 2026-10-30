using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaBinaryFile : NavMediaDocument
{
	public NavMediaBinaryFile(Stream mediaSourceStream, string mimeType)
		: base(mediaSourceStream, mimeType)
	{
		ValidateContentHeader(mediaSourceStream);
	}

	public sealed override void ValidateContentHeader(Stream contentStream)
	{
		NavMediaContentValidator.VerifyStreamData(contentStream, MimeType);
	}
}
