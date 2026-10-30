using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

internal class XmlMetadataCleaner
{
	internal struct ErrorsFound(string id, string language)
	{
		public string ID = id;

		public string Language = language;
	}

	internal static string FormatErrors(List<ErrorsFound> errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ErrorsFound error in errors)
		{
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, Lang.FoundMetadataError, error.ID, error.Language));
		}
		return stringBuilder.ToString();
	}

	internal static string GetMetaTableId(XmlDocument document)
	{
		XmlNodeList elementsByTagName = document.GetElementsByTagName("MetaTable");
		if (elementsByTagName != null && elementsByTagName.Item(0) != null && elementsByTagName.Item(0).Attributes != null && elementsByTagName.Item(0).Attributes.GetNamedItem("ID") != null)
		{
			return document.GetElementsByTagName("MetaTable").Item(0).Attributes.GetNamedItem("ID").Value;
		}
		return string.Empty;
	}

	internal static string GetMetaTableName(XmlDocument document)
	{
		XmlNodeList elementsByTagName = document.GetElementsByTagName("MetaTable");
		if (elementsByTagName != null && elementsByTagName.Item(0) != null && elementsByTagName.Item(0).Attributes != null && elementsByTagName.Item(0).Attributes.GetNamedItem("Name") != null)
		{
			return document.GetElementsByTagName("MetaTable").Item(0).Attributes.GetNamedItem("Name").Value;
		}
		return string.Empty;
	}

	internal static XmlDocument RemoveInvalidCharacters(Stream blob, List<ErrorsFound> errors)
	{
		byte[] array = new byte[blob.Length];
		blob.Read(array, 0, Convert.ToInt32(blob.Length));
		return RemoveInvalidCharacters(Encoding.Default.GetString(array), errors);
	}

	internal static XmlDocument RemoveInvalidCharacters(string text, List<ErrorsFound> errors)
	{
		byte[] bytes = Encoding.Default.GetBytes(CleanInvalidChars(text, errors));
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		using MemoryStream documentStream = new MemoryStream(bytes);
		xmlDocument.Load(XmlHelper.SafeXmlReader(documentStream));
		return xmlDocument;
	}

	internal static string CleanInvalidChars(string text, List<ErrorsFound> errors = null)
	{
		if (errors == null)
		{
			errors = new List<ErrorsFound>();
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in text)
		{
			switch (char.GetUnicodeCategory(c))
			{
			case UnicodeCategory.Control:
			{
				if (c == '\t' || c == '\n' || c == '\r' || (c >= ' ' && c <= '\ud7ff') || (c >= '\ue000' && c <= '\ufffd'))
				{
					stringBuilder.Append(c);
					break;
				}
				string text2 = stringBuilder.ToString();
				string text3 = string.Empty;
				if (text2.LastIndexOf('<') != -1 && text2.Length - text2.LastIndexOf('<') > 0)
				{
					text3 = stringBuilder.ToString(text2.LastIndexOf('<'), text2.Length - text2.LastIndexOf('<'));
				}
				if (!string.IsNullOrWhiteSpace(text3))
				{
					string[] array = text3.Split('"');
					if (text2.LastIndexOf('=') - 3 > 0 && array.Length > 1)
					{
						string language = stringBuilder.ToString(text2.LastIndexOf('=') - 3, 3);
						errors.Add(new ErrorsFound(array[1], language));
					}
				}
				break;
			}
			default:
				stringBuilder.Append(c);
				break;
			case UnicodeCategory.Format:
			case UnicodeCategory.PrivateUse:
				break;
			}
		}
		return stringBuilder.ToString();
	}

	internal static void CleanForInvalidCharacters(Stream navObject, Action<EventLogEntryType, string, bool> logger, ref XmlDocument document)
	{
		List<ErrorsFound> errors = new List<ErrorsFound>();
		document = RemoveInvalidCharacters(navObject, errors);
		logger?.Invoke(EventLogEntryType.Warning, string.Format(CultureInfo.CurrentCulture, Lang.ChangedInvalidMetadata, GetMetaTableId(document), GetMetaTableName(document), FormatErrors(errors)), arg3: false);
	}

	internal static void CleanForInvalidCharacters(string navObject, ref XmlDocument document)
	{
		List<ErrorsFound> errors = new List<ErrorsFound>();
		document = RemoveInvalidCharacters(navObject, errors);
	}
}
