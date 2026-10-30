using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class StringHelper
{
	public enum SplitOption
	{
		None,
		ReturnNullOnIndexOutOfRange,
		RemoveEmptyValuesAtEnd
	}

	public const char DefaultSplitChar = ',';

	public static string SingleSplit(string source, int index)
	{
		return SingleSplit(source, index, SplitOption.None, ',');
	}

	public static string SingleSplit(string source, int index, SplitOption options)
	{
		return SingleSplit(source, index, options, ',');
	}

	public static string SingleSplit(string source, int index, SplitOption options, char splitSymbol)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (index >= 0)
		{
			int num = 0;
			int num2 = source.Length;
			if ((options & SplitOption.RemoveEmptyValuesAtEnd) != 0)
			{
				while (num2 > 0 && source[num2 - 1] == ',')
				{
					num2--;
				}
				if (num2 == 0)
				{
					num2 = -1;
				}
			}
			while (num <= num2)
			{
				int i;
				for (i = num; i < num2 && source[i] != splitSymbol; i++)
				{
				}
				if (index == 0)
				{
					return source.Substring(num, i - num);
				}
				num = i + 1;
				index--;
			}
		}
		if ((options & SplitOption.ReturnNullOnIndexOutOfRange) != 0)
		{
			return null;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public static bool TryParseInt32FromString(string source, ref int pos, out int value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		int num = pos;
		value = 0;
		while (pos < source.Length)
		{
			int num2 = source[pos] - 48;
			if (num2 < 0 || num2 > 9 || (num == pos && num2 == 0))
			{
				break;
			}
			value = value * 10 + num2;
			pos++;
		}
		return pos != num;
	}

	public static int FindStringInStringArrayUsingCulture(CultureInfo ci, string[] strings, string searchTarget)
	{
		if (strings == null)
		{
			throw new ArgumentNullException("strings");
		}
		searchTarget = searchTarget.Trim();
		for (int i = 0; i < strings.Length; i++)
		{
			if (strings[i].Length > 0 && string.Compare(strings[i].Trim(), searchTarget, ignoreCase: true, ci) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindStringInStringArrayUsingCulture(int culture, string[] strings, string searchTarget)
	{
		return FindStringInStringArrayUsingCulture(CultureInfo.GetCultureInfo(culture), strings, searchTarget);
	}

	public static int FindStringInStringArrayUsingCurrentCulture(string[] strings, string searchTarget)
	{
		CultureInfo ci = ((NavCurrentThread.Session == null) ? CultureInfo.GetCultureInfo(NavEnvironment.DefaultLanguage) : NavCurrentThread.Session.WindowsCulture);
		return FindStringInStringArrayUsingCulture(ci, strings, searchTarget);
	}

	public static string ToUpperUsingNavCultureInfoSafe(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		NavEnvironment instance = NavEnvironment.Instance;
		if (instance != null)
		{
			NavSession session = NavCurrentThread.Session;
			if (session != null)
			{
				CultureInfo runtimeLanguage = session.RuntimeLanguage;
				if (runtimeLanguage != null)
				{
					return value.ToUpper(runtimeLanguage);
				}
			}
		}
		return value.ToUpper(CultureInfo.InvariantCulture);
	}

	private static bool IsQuoted(string str, char quoteChar = '"')
	{
		if (str.Length > 1 && str[0] == quoteChar)
		{
			return str[str.Length - 1] == quoteChar;
		}
		return false;
	}

	public static string Unquote(string arg, char quoteChar)
	{
		if (IsQuoted(arg, quoteChar))
		{
			return arg.Substring(1, arg.Length - 2);
		}
		return arg;
	}

	public static int Compare(string left, string right)
	{
		if (right == null)
		{
			if (left != null)
			{
				return 1;
			}
			return 0;
		}
		if (left == null)
		{
			return -1;
		}
		return string.Compare(left, right, ignoreCase: false, NavCurrentThread.Session.Culture);
	}
}
