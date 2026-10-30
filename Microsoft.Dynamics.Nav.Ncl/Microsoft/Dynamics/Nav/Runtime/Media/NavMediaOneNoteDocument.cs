using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaOneNoteDocument : NavMediaDocument
{
	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/msonenote" };

	public NavMediaOneNoteDocument(Stream mediaSourceStream, string mimeType)
		: base(mediaSourceStream, mimeType)
	{
		ValidateContentHeader(mediaSourceStream);
	}

	public static bool IsSupportedMimeType(string mimeType)
	{
		return SupportedMimeTypes.Contains(mimeType);
	}

	public sealed override void ValidateContentHeader(Stream content)
	{
		Guid g = new Guid("{7B5C52E4-D88C-4DA7-AEB1-5378D02996D3} ");
		Guid g2 = new Guid("{43FF2FA1-EFD9-4C76-9EE2-10EA5722765F} ");
		byte[] array = new byte[16];
		long position = content.Position;
		if (position != 0L && content.CanSeek)
		{
			content.Seek(0L, SeekOrigin.Begin);
		}
		int num = content.Read(array, 0, 16);
		content.Seek(position, SeekOrigin.Begin);
		if (num >= 16)
		{
			Guid guid;
			try
			{
				guid = new Guid(array);
			}
			catch (ArgumentException)
			{
				guid = Guid.Empty;
			}
			if (!guid.Equals(g) && !guid.Equals(g2))
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000027M", Category.Runtime, new NavMediaUnsupportedFormatException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidMediaOfficeDocContent, MimeType)));
			}
		}
	}
}
