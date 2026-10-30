using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavTextExtensions
{
	public static bool ALContains(string text, string value)
	{
		return text.Contains(value);
	}

	public static bool ALEndsWith(string text, string value)
	{
		return text.EndsWith(value, StringComparison.Ordinal);
	}

	public static int ALIndexOf(string text, string value)
	{
		return 1 + text.IndexOf(value, StringComparison.Ordinal);
	}

	public static int ALIndexOf(string text, string value, int startIndex)
	{
		try
		{
			return 1 + text.IndexOf(value, startIndex - 1, StringComparison.Ordinal);
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static int ALIndexOfAny(string text, string chars)
	{
		return 1 + text.IndexOfAny(chars.ToCharArray());
	}

	public static int ALIndexOfAny(string text, string chars, int startIndex)
	{
		return 1 + text.IndexOfAny(chars.ToCharArray(), startIndex - 1);
	}

	public static int ALIndexOfAny(string text, NavList<char> chars)
	{
		return 1 + text.IndexOfAny(chars.Value.ToArray());
	}

	public static int ALIndexOfAny(string text, NavList<char> chars, int startIndex)
	{
		return 1 + text.IndexOfAny(chars.Value.ToArray(), startIndex - 1);
	}

	public static int ALLastIndexOf(string text, string value)
	{
		return 1 + text.LastIndexOf(value, StringComparison.Ordinal);
	}

	public static int ALLastIndexOf(string text, string value, int startIndex)
	{
		try
		{
			return 1 + text.LastIndexOf(value, startIndex - 1, StringComparison.Ordinal);
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALPadLeft(string text, int count)
	{
		try
		{
			return new NavText(text.PadLeft(count));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALPadLeft(string text, int count, char value)
	{
		try
		{
			return new NavText(text.PadLeft(count, value));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALPadRight(string text, int count)
	{
		try
		{
			return new NavText(text.PadRight(count));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALPadRight(string text, int count, char value)
	{
		try
		{
			return new NavText(text.PadRight(count, value));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALRemove(string text, int startIndex)
	{
		try
		{
			return new NavText(text.Remove(startIndex - 1));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALRemove(string text, int startIndex, int count)
	{
		try
		{
			return new NavText(text.Remove(startIndex - 1, count));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALReplace(string text, string oldValue, string newValue)
	{
		try
		{
			return new NavText(text.Replace(oldValue, newValue));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavList<NavText> ALSplit(string text, params string[] separators)
	{
		IEnumerable<NavText> values = from x in text.Split(separators, StringSplitOptions.None)
			select new NavText(x);
		return new NavList<NavText>(values);
	}

	public static NavList<NavText> ALSplit(string text, NavList<NavText> separators)
	{
		return ALSplit(text, separators.Value.Select((NavText v) => v.Value).ToArray());
	}

	public static NavList<NavText> ALSplit(string text, NavList<char> separators)
	{
		IEnumerable<NavText> values = from x in text.Split(separators.Value.ToArray())
			select new NavText(x);
		return new NavList<NavText>(values);
	}

	public static bool ALStartsWith(string text, string value)
	{
		return text.StartsWith(value, StringComparison.Ordinal);
	}

	public static NavText ALSubstring(string text, int startIndex)
	{
		try
		{
			return new NavText(text.Substring(startIndex - 1));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALSubstring(string text, int startIndex, int count)
	{
		try
		{
			return new NavText(text.Substring(startIndex - 1, count));
		}
		catch (Exception exception)
		{
			throw MapNativeException(exception);
		}
	}

	public static NavText ALToLower(string text)
	{
		return new NavText(text.ToLowerInvariant());
	}

	public static NavText ALToUpper(string text)
	{
		return new NavText(text.ToUpperInvariant());
	}

	public static NavText ALTrim(string text)
	{
		return new NavText(text.Trim());
	}

	public static NavText ALTrimStart(string text)
	{
		return new NavText(text.TrimStart());
	}

	public static NavText ALTrimStart(string text, string chars)
	{
		return new NavText(text.TrimStart(chars.ToCharArray()));
	}

	public static NavText ALTrimEnd(string text)
	{
		return new NavText(text.TrimEnd());
	}

	public static NavText ALTrimEnd(string text, string chars)
	{
		return new NavText(text.TrimEnd(chars.ToCharArray()));
	}

	private static Exception MapNativeException(Exception exception)
	{
		if (exception is ArgumentOutOfRangeException)
		{
			string text = exception.Message;
			if (text.Contains("StartIndex cannot be less than zero."))
			{
				text = text.Replace("zero", "1");
			}
			return new NavNCLArgumentOutOfRangeException(text);
		}
		if (exception is ArgumentException)
		{
			return new NavNCLArgumentException(exception.Message);
		}
		return exception;
	}
}
