using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MultiLanguage
{
	private const string SpanishTraditional = "ESP";

	private const string SpanishTraditionalLegacy = "ESN";

	private const string CommentTag = "@@@";

	private const string CaptionMLSeparator = ";";

	private const string OptionMLSeparator = ",";

	private const string CaptionMLFormat = "{0}={1};";

	private const string CaptionMLFormatNoSeparator = "{0}={1}";

	public static string GetLanguageValue(string language, string primaryLanguage, string languageValues)
	{
		if (string.IsNullOrEmpty(language))
		{
			throw new ArgumentException("The argument is null or empty.", "language");
		}
		string value = "";
		if (string.IsNullOrEmpty(languageValues))
		{
			return value;
		}
		Dictionary<string, string> dictionary = SplitMultiLanguageString(languageValues);
		if (dictionary.TryGetValue(language, out value))
		{
			return value;
		}
		if (primaryLanguage == null || !dictionary.TryGetValue(primaryLanguage, out value))
		{
			dictionary.TryGetValue("ENU", out value);
		}
		return value;
	}

	public static string SetLanguageValue(string language, string newValue, string languageValues)
	{
		if (string.IsNullOrEmpty(language))
		{
			throw new ArgumentException("The argument is null or empty.", "language");
		}
		bool flag = string.IsNullOrEmpty(languageValues);
		Dictionary<string, string> dictionary = (flag ? new Dictionary<string, string>() : SplitMultiLanguageString(languageValues));
		if (!flag)
		{
			if (string.IsNullOrEmpty(newValue))
			{
				dictionary.Remove(language);
			}
			if (dictionary.ContainsKey(language))
			{
				dictionary[language] = newValue;
			}
			else
			{
				dictionary.Add(language, newValue);
			}
		}
		else
		{
			if (string.Compare(language, "ENU", StringComparison.OrdinalIgnoreCase) != 0)
			{
				dictionary.Add("ENU", newValue);
			}
			dictionary.Add(language, newValue);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", item.Key, item.Value);
		}
		return stringBuilder.ToString().TrimEnd(";".ToCharArray());
	}

	public static string OverrideOrMergeMultiLanguageStrings(string originalValue, string newValue)
	{
		if (string.IsNullOrEmpty(newValue))
		{
			return originalValue;
		}
		if (string.IsNullOrEmpty(originalValue))
		{
			return newValue;
		}
		Dictionary<string, string> dictionary = SplitMultiLanguageString(originalValue, forceRaw: true, includeCommentTag: true);
		foreach (KeyValuePair<string, string> item in SplitMultiLanguageString(newValue, forceRaw: true, includeCommentTag: true))
		{
			dictionary[item.Key] = item.Value;
		}
		return JoinMultiLanguageString(dictionary);
	}

	public static Dictionary<string, string> SplitMultiLanguageString(string textMultiLanguage, bool forceRaw = false, bool includeCommentTag = false)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		int i = 0;
		string languageId = null;
		for (; i < textMultiLanguage.Length && i > -1; i++)
		{
			string value = ParseLanguage(textMultiLanguage, ref i, ref languageId, forceRaw);
			if ((languageId != "@@@") | includeCommentTag)
			{
				if (dictionary.ContainsKey(languageId))
				{
					EventLogWriter.Write(string.Format(CultureInfo.CurrentCulture, Lang.MultiLanguageStringError, textMultiLanguage, languageId), EventLogEntryType.Error, null, 0, null, 0);
				}
				else
				{
					dictionary.Add(languageId, value);
				}
			}
		}
		return dictionary;
	}

	public static string JoinMultiLanguageString(string language, string value)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}={1}", language, value);
	}

	public static string JoinMultiLanguageString(IDictionary<string, string> languageEntries)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in languageEntries.OrderBy((KeyValuePair<string, string> le) => le.Key))
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", item.Key, item.Value);
		}
		return stringBuilder.ToString().TrimEnd(";".ToCharArray());
	}

	public static string[] SplitMultiLanguageOptions(string textMultiLanguage)
	{
		if (string.IsNullOrEmpty(textMultiLanguage))
		{
			return Array.Empty<string>();
		}
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		int num = -1;
		foreach (KeyValuePair<string, string> item in SplitMultiLanguageString(textMultiLanguage))
		{
			string[] array = item.Value.Split(",".ToCharArray(), StringSplitOptions.None);
			if (num < 0)
			{
				num = array.Length;
			}
			else if (num != array.Length)
			{
				return Array.Empty<string>();
			}
			dictionary[item.Key] = array;
		}
		string[] array2 = new string[num];
		for (int i = 0; i < num; i++)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string[]> item2 in dictionary)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1};", item2.Key, item2.Value[i]);
			}
			array2[i] = stringBuilder.ToString().TrimEnd(";".ToCharArray());
		}
		return array2;
	}

	private static string ParseLanguage(string textMultiLanguage, ref int offset, ref string languageId, bool forceRaw)
	{
		if (offset == textMultiLanguage.Length)
		{
			return string.Empty;
		}
		while (offset < textMultiLanguage.Length && char.IsWhiteSpace(textMultiLanguage[offset]))
		{
			offset++;
		}
		languageId = textMultiLanguage.Substring(offset, 3);
		offset += 3;
		if (offset < textMultiLanguage.Length && textMultiLanguage[offset] == '=')
		{
			offset++;
		}
		else
		{
			while (offset < textMultiLanguage.Length && textMultiLanguage[offset] != '=')
			{
				offset++;
			}
		}
		if (offset >= textMultiLanguage.Length)
		{
			return string.Empty;
		}
		if (textMultiLanguage[offset] != '"')
		{
			return ParseMultiLanguageText(textMultiLanguage, ref offset);
		}
		return ParseVerbose(textMultiLanguage, ref offset, forceRaw);
	}

	private static string ParseMultiLanguageText(string textMultiLanguage, ref int offset)
	{
		int num = offset;
		offset = textMultiLanguage.IndexOf(';', offset);
		if (offset < 0)
		{
			offset = textMultiLanguage.Length;
		}
		return textMultiLanguage.Substring(num, offset - num);
	}

	private static string ParseVerbose(string textMultiLanguage, ref int offset, bool forceRaw)
	{
		int num = offset;
		offset++;
		int num2 = offset;
		while ((offset < textMultiLanguage.Length && textMultiLanguage[offset] != '"') || (offset + 1 < textMultiLanguage.Length && textMultiLanguage[offset + 1] == '"'))
		{
			if (textMultiLanguage[offset] == '"')
			{
				offset += 2;
			}
			else
			{
				offset++;
			}
		}
		if (offset > textMultiLanguage.Length)
		{
			offset = textMultiLanguage.Length;
		}
		if (forceRaw)
		{
			offset++;
			return textMultiLanguage.Substring(num, offset - num);
		}
		string result = textMultiLanguage.Substring(num2, offset - num2).Replace("\"\"", "\"");
		offset++;
		return result;
	}
}
