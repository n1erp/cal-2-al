using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaWordDocument : NavMediaDocument
{
	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-word.document.macroEnabled.12", "application/vnd.openxmlformats-officedocument.wordprocessingml.template", "application/vnd.ms-word.template.macroEnabled.12", "application/msword" };

	public NavMediaWordDocument(Stream mediaSourceStream, string mimeType)
		: base(mediaSourceStream, mimeType)
	{
		ValidateContentHeader(mediaSourceStream);
	}

	public static bool IsSupportedMimeType(string mimeType)
	{
		return SupportedMimeTypes.Contains(mimeType);
	}

	public sealed override void ValidateContentHeader(Stream contentStream)
	{
		NavMediaContentValidator.ValidateOfficeContentHeader(contentStream, MimeType);
	}
}
