using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public static class DateTimeParsingHelper
{
	private const string DateTimeRoundTripFormatSpecifier = "O";

	public static bool TryParseExactDateTime(string source, ClientSettings settings, out DateTime result)
	{
		string[] formats = CreateExactDateTimePatterns(settings.ShortDatePattern, settings.LongTimePattern, settings.TimeSeparator, settings.DecimalSeparator);
		return DateTime.TryParseExact(source, formats, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactDateTime(string source, CultureInfo culture, out DateTime result)
	{
		string[] formats = CreateExactDateTimePatterns(culture.DateTimeFormat.ShortDatePattern, culture.DateTimeFormat.LongTimePattern, culture.DateTimeFormat.TimeSeparator, culture.NumberFormat.NumberDecimalSeparator);
		return DateTime.TryParseExact(source, formats, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactDateTime(string source, out DateTime result)
	{
		return TryParseExactDateTimeLiteral(source, out result);
	}

	public static bool TryParseExactTime(string source, ClientSettings settings, out DateTime result)
	{
		string[] formats = CreateExactTimePatterns(settings.LongTimePattern, settings.DecimalSeparator);
		return DateTime.TryParseExact(source, formats, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactTime(string source, CultureInfo culture, out DateTime result)
	{
		string[] formats = CreateExactTimePatterns(culture.DateTimeFormat.LongTimePattern, culture.NumberFormat.NumberDecimalSeparator);
		return DateTime.TryParseExact(source, formats, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactTime(string source, out DateTime result)
	{
		return TryParseExactDateTimeLiteral(source, out result);
	}

	public static bool TryParseExactDate(string source, ClientSettings settings, out DateTime result)
	{
		return DateTime.TryParseExact(source, settings.ShortDatePattern, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactDate(string source, CultureInfo culture, out DateTime result)
	{
		return DateTime.TryParseExact(source, culture.DateTimeFormat.ShortDatePattern, null, DateTimeStyles.None, out result);
	}

	public static bool TryParseExactDate(string source, out DateTime result)
	{
		return TryParseExactDateTimeLiteral(source, out result);
	}

	private static bool TryParseExactDateTimeLiteral(string source, out DateTime result)
	{
		if (!DateTime.TryParseExact(source, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result))
		{
			return false;
		}
		if (result.Kind != DateTimeKind.Unspecified)
		{
			return false;
		}
		result = new DateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, result.Millisecond, DateTimeKind.Local);
		return true;
	}

	private static string[] CreateExactDateTimePatterns(string shortDatePattern, string longTimePattern, string timeSeparator, string decimalSeparator)
	{
		return new string[3]
		{
			string.Format(CultureInfo.InvariantCulture, "{0} {1}", shortDatePattern, longTimePattern),
			string.Format(CultureInfo.InvariantCulture, "{0} {1}", shortDatePattern, longTimePattern.Replace(string.Format(CultureInfo.InvariantCulture, "{0}ss", timeSeparator), "")),
			string.Format(CultureInfo.InvariantCulture, "{0} {1}", shortDatePattern, longTimePattern.Replace("ss", string.Format(CultureInfo.InvariantCulture, "ss{0}fff", decimalSeparator)))
		};
	}

	private static string[] CreateExactTimePatterns(string longTimePattern, string decimalSeparator)
	{
		return new string[2]
		{
			longTimePattern,
			string.Format(CultureInfo.InvariantCulture, "{0}", longTimePattern.Replace("ss", string.Format(CultureInfo.InvariantCulture, "ss{0}fff", decimalSeparator)))
		};
	}
}
