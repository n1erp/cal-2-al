using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaTextDocument : NavMediaDocument
{
	public NavMediaTextDocument(Stream mediaSourceStream, string mimeType)
		: base(mediaSourceStream, mimeType)
	{
		ValidateContentHeader(mediaSourceStream);
	}

	public sealed override void ValidateContentHeader(Stream content)
	{
		NavMediaContentValidator.VerifyStreamData(content, MimeType);
	}

	public static bool IsSupportedMimeType(string mimeType)
	{
		if (!mimeType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
		{
			return mimeType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
