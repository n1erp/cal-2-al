using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaExcelDocument : NavMediaDocument
{
	private static readonly HashSet<string> SupportedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel.sheet.macroEnabled.12", "application/vnd.openxmlformats-officedocument.spreadsheetml.template", "application/vnd.ms-excel.template.macroEnabled.12", "application/vnd.ms-excel.sheet.binary.macroEnabled.12", "application/vnd.ms-excel.addin.macroEnabled.12", "application/vnd.ms-excel" };

	public NavMediaExcelDocument(Stream mediaSourceStream, string mimeType)
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
