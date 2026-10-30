using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaPowerPointDocument : NavMediaDocument
{
	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/vnd.ms-powerpoint.presentation.macroEnabled.12", "application/vnd.openxmlformats-officedocument.presentationml.slideshow", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12", "application/vnd.openxmlformats-officedocument.presentationml.template", "application/vnd.ms-powerpoint.template.macroEnabled.12", "application/vnd.ms-powerpoint.addin.macroEnabled.12", "application/vnd.openxmlformats-officedocument.presentationml.slide", "application/vnd.ms-powerpoint.slide.macroEnabled.12", "application/vnd.ms-powerpoint" };

	public NavMediaPowerPointDocument(Stream mediaSourceStream, string mimeType)
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
