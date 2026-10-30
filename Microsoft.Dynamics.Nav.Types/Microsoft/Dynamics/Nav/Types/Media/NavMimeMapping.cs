using System;
using System.Collections.Generic;
using System.Web;

namespace Microsoft.Dynamics.Nav.Types.Media;

internal static class NavMimeMapping
{
	private static readonly Dictionary<string, string> BuiltInNonImageMappings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
	{
		{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		{ ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
		{ ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
		{ ".dotm", "application/vnd.ms-word.template.macroEnabled.12" },
		{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
		{ ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
		{ ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
		{ ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
		{ ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
		{ ".xlam", "application/vnd.ms-excel.addin.macroEnabled.12" },
		{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
		{ ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
		{ ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
		{ ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
		{ ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
		{ ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
		{ ".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12" },
		{ ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
		{ ".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12" },
		{ ".one", "application/msonenote" },
		{ ".onetoc2", "application/msonenote" },
		{ ".onetmp", "application/msonenote" },
		{ ".onepkg", "application/msonenote" },
		{ ".thmx", "application/vnd.ms-officetheme" },
		{ ".pdf", "application/pdf" },
		{ ".txt", "text/plain" },
		{ ".doc", "application/msword" },
		{ ".xls", "application/vnd.ms-excel" },
		{ ".ppt", "application/vnd.ms-powerpoint" },
		{ ".mp4", "video/mp4" },
		{ ".json", "application/json" },
		{ ".noext.", "application/octet-stream" }
	};

	private static readonly Dictionary<string, string> AdditionalImageMappings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) { { ".emf", "image/x-emf" } };

	private static readonly Dictionary<string, string> BuiltInThumbnailMappings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
	{
		{ ".docx", ".doc" },
		{ ".docm", ".doc" },
		{ ".dotx", ".doc" },
		{ ".dotm", ".doc" },
		{ ".doc", ".doc" },
		{ ".xlsx", ".xls" },
		{ ".xlsm", ".xls" },
		{ ".xltx", ".xls" },
		{ ".xltm", ".xls" },
		{ ".xlsb", ".xls" },
		{ ".xlam", ".xls" },
		{ ".xls", ".xls" },
		{ ".pptx", ".ppt" },
		{ ".pptm", ".ppt" },
		{ ".ppsx", ".ppt" },
		{ ".ppsm", ".ppt" },
		{ ".potx", ".ppt" },
		{ ".potm", ".ppt" },
		{ ".ppam", ".ppt" },
		{ ".sldx", ".ppt" },
		{ ".sldm", ".ppt" },
		{ ".ppt", ".ppt" },
		{ ".one", ".one" },
		{ ".onetoc2", ".one" },
		{ ".onetmp", ".one" },
		{ ".onepkg", ".one" },
		{ ".pdf", ".pdf" },
		{ ".txt", ".txt" }
	};

	internal static string MapExtensionToKnownThumbnailType(string extenstion)
	{
		if (!BuiltInThumbnailMappings.TryGetValue(extenstion, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	internal static string GetExtensionForBuiltinMimeType(string mimeType)
	{
		foreach (KeyValuePair<string, string> builtInNonImageMapping in BuiltInNonImageMappings)
		{
			if (string.Compare(builtInNonImageMapping.Value, mimeType, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return builtInNonImageMapping.Key;
			}
		}
		return string.Empty;
	}

	internal static string GetMimeMapping(string filename)
	{
		if (filename == null)
		{
			return string.Empty;
		}
		if (string.IsNullOrWhiteSpace(filename))
		{
			return MimeMapping.GetMimeMapping(string.Empty);
		}
		string value = null;
		int num = filename.LastIndexOf('.');
		if (0 <= num && num > filename.LastIndexOf('\\'))
		{
			string key = filename.Substring(num);
			if (!AdditionalImageMappings.TryGetValue(key, out value))
			{
				BuiltInNonImageMappings.TryGetValue(key, out value);
			}
		}
		if (string.IsNullOrEmpty(value))
		{
			value = MimeMapping.GetMimeMapping(filename);
		}
		return value;
	}
}
