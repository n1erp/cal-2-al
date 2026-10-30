using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

public static class StringExtensions
{
	public static string FormatWithResolvedCulture(this string format, params object[] args)
	{
		CultureInfo provider = NavCurrentThread.Session?.Culture ?? CultureInfo.CurrentCulture;
		return string.Format(provider, format, args);
	}

	public static string EscapeCurlyBraces(this string context)
	{
		return context.Replace("{", "{{").Replace("}", "}}");
	}
}
