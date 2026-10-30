using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class OfficeFilenameManager
{
	internal static readonly List<char> IllegalWindowsFileCharacters = InitializeIllegalWindowsFileCharacters();

	internal static readonly List<char> IllegalSharepointFileCharacters = InitializeIllegalSharepointFileCharacters();

	public bool EnableSharepoint { get; set; }

	public OfficeFilenameManager(bool enableSharepoint)
	{
		EnableSharepoint = enableSharepoint;
	}

	public string GenerateDocumentFileName(string fileName, string extension, string pageCaption)
	{
		int num = (EnableSharepoint ? 128 : 260);
		if (string.IsNullOrWhiteSpace(fileName))
		{
			if (string.IsNullOrEmpty(extension))
			{
				throw new ArgumentException(Lang.OfficeDocumentExtensionRequired, "extension");
			}
			if (string.IsNullOrEmpty(pageCaption))
			{
				throw new ArgumentException(Lang.OfficeDocumentCaptionRequired, "pageCaption");
			}
			string text = string.Format(CultureInfo.InvariantCulture, "{0} {1}.{2}", NavCurrentThread.Session.User.Name, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NavCurrentThread.Session.Tenant.TenantSettings.DefaultTimeZone ?? TimeZoneInfo.Utc).ToString("s", CultureInfo.InvariantCulture), extension);
			fileName = string.Format(CultureInfo.InvariantCulture, "{0} {1}", (text.Length + pageCaption.Length > num - 1) ? pageCaption.Substring(0, Math.Abs(num - text.Length - 1)) : pageCaption, text);
			if (EnableSharepoint && fileName.StartsWith(".", StringComparison.OrdinalIgnoreCase))
			{
				fileName = fileName.TrimStart('.');
			}
		}
		else
		{
			string extension2 = Path.GetExtension(fileName);
			if (string.IsNullOrEmpty(extension2))
			{
				throw new ArgumentException(Lang.OfficeDocumentExtensionRequired, "fileName");
			}
			if (fileName.Length > num)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
				string path = fileNameWithoutExtension.Substring(0, num - extension2.Length - 2);
				fileName = Path.ChangeExtension(path, extension2);
			}
		}
		return NormalizeFileName(fileName, EnableSharepoint);
	}

	private static string NormalizeFileName(string fileName, bool useSharepointStorage)
	{
		List<char> list = (useSharepointStorage ? IllegalSharepointFileCharacters : IllegalWindowsFileCharacters);
		foreach (char item in list)
		{
			if (fileName.IndexOf(item) >= 0)
			{
				fileName = fileName.Replace(item, '_');
			}
		}
		while (useSharepointStorage && fileName.Contains(".."))
		{
			fileName = fileName.Replace("..", ".");
		}
		return fileName;
	}

	private static List<char> InitializeIllegalWindowsFileCharacters()
	{
		List<char> list = new List<char>();
		list.AddRange(Path.GetInvalidFileNameChars());
		list = AddIfNotFound(list, Path.GetInvalidPathChars());
		return AddIfNotFound(list, new List<char>
		{
			Path.PathSeparator,
			Path.AltDirectorySeparatorChar
		});
	}

	private static List<char> InitializeIllegalSharepointFileCharacters()
	{
		List<char> invalidChars = InitializeIllegalWindowsFileCharacters();
		return AddIfNotFound(invalidChars, new List<char>
		{
			'\\', '/', ':', '*', '?', '"', '\'', '<', '>', '|',
			'#', '{', '}', '%', '~', '&'
		});
	}

	private static List<char> AddIfNotFound(List<char> invalidChars, IEnumerable<char> chars)
	{
		foreach (char item in chars.Where((char invalidChar) => !invalidChars.Contains(invalidChar)))
		{
			invalidChars.Add(item);
		}
		return invalidChars;
	}
}
