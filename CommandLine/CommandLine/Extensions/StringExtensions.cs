using System.Globalization;

namespace CommandLine.Extensions;

internal static class StringExtensions
{
	public static string Spaces(this int value)
	{
		return new string(' ', value);
	}

	public static bool IsNumeric(this string value)
	{
		decimal result;
		return decimal.TryParse(value, out result);
	}

	public static string FormatInvariant(this string value, params object[] arguments)
	{
		return string.Format(CultureInfo.InvariantCulture, value, arguments);
	}

	public static string FormatLocal(this string value, params object[] arguments)
	{
		return string.Format(CultureInfo.CurrentCulture, value, arguments);
	}
}
