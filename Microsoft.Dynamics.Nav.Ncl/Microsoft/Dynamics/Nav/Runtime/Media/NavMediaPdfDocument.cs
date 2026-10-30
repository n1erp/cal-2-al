using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaPdfDocument : NavMediaDocument
{
	private const string PdfMimeType = "application/pdf";

	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/pdf" };

	public NavMediaPdfDocument(Stream mediaSourceStream)
		: base(mediaSourceStream, "application/pdf")
	{
		ValidateContentHeader(mediaSourceStream);
	}

	public static bool IsSupportedMimeType(string mimeType)
	{
		return SupportedMimeTypes.Contains(mimeType);
	}

	public sealed override void ValidateContentHeader(Stream content)
	{
		int length = "%PDF-".Length;
		byte[] array = new byte[length];
		string text = Encoding.ASCII.GetString(array, 0, array.Length);
		if (text.Equals("%PDF-"))
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027M", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaPdfContent, MimeType)));
		}
	}
}
