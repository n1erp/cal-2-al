using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public static class ModelTextHelper
{
	private static readonly char[] DoubleQuoteNeededChars = new char[2] { '=', ';' };

	public const string CommentThreeLetterId = "@@@";

	public const string CommentString = "Comment";

	public const int CommentLcid = 1;

	public static string QuoteIdentifierIfNeeded(string value)
	{
		if (!string.IsNullOrEmpty(value) && value[0] != '"')
		{
			for (int num = value.Length - 1; num >= 0; num--)
			{
				if ((value[num] < 'a' || value[num] > 'z') && (value[num] < 'A' || value[num] > 'Z') && !char.IsDigit(value[num]))
				{
					return "\"" + value.Replace("\"", "\"\"") + "\"";
				}
			}
		}
		return value;
	}

	public static bool IsNavLetter(char c)
	{
		return char.IsLetter(c);
	}

	public static IEnumerable<KeyValuePair<CultureInfo, string>> MLTextSplitter(string mlt)
	{
		if (mlt == null)
		{
			yield break;
		}
		int pos = 0;
		while (pos < mlt.Length)
		{
			if (pos + 3 >= mlt.Length || mlt[pos + 3] != '=')
			{
				throw new InvalidOperationException($"Unable to parse {mlt} as a MLText.  = expected at position {pos}");
			}
			string text = mlt.Substring(pos, 3);
			CultureInfo cultureInfo;
			if (text == "@@@")
			{
				cultureInfo = null;
			}
			else
			{
				cultureInfo = WindowsLanguageHelper.FindCulture(text);
				if (cultureInfo == null)
				{
					throw new InvalidOperationException($"Unable to parse {mlt} as a MLText.  {text} is not a valid language specifier");
				}
			}
			pos += 4;
			string text2;
			if (pos == mlt.Length)
			{
				text2 = string.Empty;
			}
			else if (pos < mlt.Length && mlt[pos] != '"')
			{
				int num = mlt.IndexOf(';', pos);
				text2 = ((num < 0) ? mlt.Substring(pos) : mlt.Substring(pos, num - pos));
				pos = ((num < 0) ? mlt.Length : (num + 1));
			}
			else
			{
				int num2 = mlt.IndexOf('"', pos + 1);
				bool flag = false;
				while (true)
				{
					if (num2 < 0)
					{
						throw new InvalidOperationException(string.Format("Unable to parse {0} as MLText. Open \" found after language identifier {1}", mlt, (cultureInfo != null) ? cultureInfo.ThreeLetterWindowsLanguageName : "@@@"));
					}
					if (num2 + 1 >= mlt.Length || mlt[num2 + 1] != '"')
					{
						break;
					}
					flag = true;
					num2 = mlt.IndexOf('"', num2 + 2);
				}
				if (num2 + 1 < mlt.Length && mlt[num2 + 1] != ';')
				{
					throw new InvalidOperationException(string.Format("Unable to parse {0} as MLText. Missing ; after language identifier {1}", mlt, (cultureInfo != null) ? cultureInfo.ThreeLetterWindowsLanguageName : "@@@"));
				}
				text2 = mlt.Substring(pos + 1, num2 - pos - 1);
				if (flag)
				{
					text2 = text2.Replace("\"\"", "\"");
				}
				pos = num2 + 2;
			}
			yield return new KeyValuePair<CultureInfo, string>(cultureInfo, text2);
			for (; pos < mlt.Length && char.IsWhiteSpace(mlt[pos]); pos++)
			{
			}
		}
	}

	public static IEnumerable<string> MLTextFormatter(IEnumerable<KeyValuePair<CultureInfo, string>> ml, int maxLength, bool forceUseSingleQuotes = false)
	{
		StringBuilder lineSb = new StringBuilder(1024);
		StringBuilder elementSb = new StringBuilder(1024);
		bool sourceEmpty = true;
		foreach (KeyValuePair<CultureInfo, string> item in ml.OrderBy((KeyValuePair<CultureInfo, string> kv) => CultureToLcid(kv.Key)))
		{
			sourceEmpty = false;
			bool flag = !forceUseSingleQuotes && (item.Value.IndexOfAny(DoubleQuoteNeededChars) >= 0 || (item.Value.Length > 0 && (char.IsWhiteSpace(item.Value[0]) || item.Value[0] == '"' || char.IsWhiteSpace(item.Value[item.Value.Length - 1]))));
			if (item.Key == null)
			{
				elementSb.Append("@@@");
			}
			else
			{
				elementSb.Append(item.Key.ThreeLetterWindowsLanguageName);
			}
			elementSb.Append('=');
			if (flag | forceUseSingleQuotes)
			{
				char value;
				string oldValue;
				string newValue;
				if (flag)
				{
					value = '"';
					oldValue = "\"";
					newValue = "\"\"";
				}
				else
				{
					value = '\'';
					oldValue = "'";
					newValue = "''";
				}
				elementSb.Append(value);
				int length = elementSb.Length;
				elementSb.Append(item.Value);
				elementSb.Replace(oldValue, newValue, length, item.Value.Length);
				elementSb.Append(value);
			}
			else
			{
				elementSb.Append((item.Value == string.Empty) ? "\"\"" : item.Value);
			}
			if (lineSb.Length + 1 + elementSb.Length <= maxLength)
			{
				if (lineSb.Length == 0)
				{
					StringBuilder stringBuilder = lineSb;
					lineSb = elementSb;
					elementSb = stringBuilder;
				}
				else
				{
					lineSb.Append(';');
					lineSb.Append(elementSb);
					elementSb.Clear();
				}
			}
			else
			{
				if (lineSb.Length > 0)
				{
					yield return lineSb.ToString();
					lineSb.Clear();
				}
				StringBuilder stringBuilder = lineSb;
				lineSb = elementSb;
				elementSb = stringBuilder;
			}
		}
		if (sourceEmpty)
		{
			yield return string.Empty;
		}
		if (lineSb.Length > 0)
		{
			yield return lineSb.ToString();
		}
	}

	public static string AddOrUpdateLanguage(string multiLanguageString, CultureInfo culture, string value)
	{
		IEnumerable<KeyValuePair<CultureInfo, string>> enumerable = from kv in MLTextSplitter(multiLanguageString ?? string.Empty)
			where kv.Key != culture
			select kv;
		if (!string.IsNullOrEmpty(value))
		{
			enumerable = enumerable.Concat(new KeyValuePair<CultureInfo, string>[1]
			{
				new KeyValuePair<CultureInfo, string>(culture, value)
			});
		}
		return string.Join(";", MLTextFormatter(enumerable, 0));
	}

	public static string RemoveLanguage(string multiLanguageString, Predicate<CultureInfo> removeLanguage)
	{
		IEnumerable<KeyValuePair<CultureInfo, string>> ml = from kv in MLTextSplitter(multiLanguageString ?? string.Empty)
			where !removeLanguage(kv.Key)
			select kv;
		return string.Join(";", MLTextFormatter(ml, 0));
	}

	public static int CultureToLcid(CultureInfo culture)
	{
		return culture?.LCID ?? 1;
	}
}
