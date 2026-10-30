using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class DateTimeUtilities
{
	public const int CSideDateUndefined = 0;

	public const int CSideTimeUndefined = 0;

	public const long CSideDateTimeUndefined = 0L;

	public const int CSideDateFirstSupported = 737;

	public static readonly DateTime DateTimeUndefined = default;

	public static readonly DateTime DateTimeMinimum = new DateTime(1, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);

	public const int MillisecondsPerDay = 86400000;

	public static string DateTimeRoundTripFormatSpecifier = "O";

	internal static DateTime CSideDateToDateTime(int date)
	{
		return date switch
		{
			0 => default, 
			1 => DateTimeMinimum, 
			_ => new DateTime(((date - 737) / 2 + 2) * 864000000000L), 
		};
	}

	internal static bool TryParseDate(string value, out int date)
	{
		date = 0;
		if (value == "0D")
		{
			return true;
		}
		if (value.Length != 9)
		{
			return false;
		}
		if (!int.TryParse(value.Substring(0, 4), out var result) || !int.TryParse(value.Substring(4, 2), out var result2) || !int.TryParse(value.Substring(6, 2), out var result3))
		{
			return false;
		}
		if (result == 0 && result2 == 1 && result3 == 1)
		{
			date = 1;
			return true;
		}
		DateTime dateTime;
		try
		{
			dateTime = new DateTime(result, result2, result3);
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
		int num = (int)(dateTime.Ticks / 864000000000L);
		date = 737 + (num - 2) * 2;
		return true;
	}

	internal static string FormatCSideDate(int date, string formatString)
	{
		DateTime dateTime = CSideDateToDateTime(date);
		if (dateTime == DateTimeMinimum)
		{
			switch (formatString)
			{
			case "yyyyMMdd":
				return "00000101";
			case "yyyy-MM-dd":
				return "0000-01-01";
			case "o":
			case "O":
				return "0000-01-01T00:00:00.0000000";
			default:
				throw new ArgumentException(FormattableString.Invariant($"The format string '{formatString}' is not supported."));
			}
		}
		return dateTime.ToString(formatString, CultureInfo.InvariantCulture);
	}

	public static DateTime CSideTimeToDateTime(int value)
	{
		if (value == 0)
		{
			return DateTimeUndefined;
		}
		if (value < 0 || value > 86400000)
		{
			throw ExceptionUtilities.UnexpectedValue(value);
		}
		return DateTime.MinValue.AddTicks(10000L * (long)(value - 1));
	}

	internal static bool TryParseTime(string value, out int time)
	{
		time = 0;
		if (value == "0")
		{
			return true;
		}
		int result = 0;
		int num = value.IndexOf('.');
		if (num > 0)
		{
			int num2 = value.Length - num - 1;
			if (num2 > 3 || num2 < 1)
			{
				return false;
			}
			if (!int.TryParse(value.Substring(num + 1, num2), out result))
			{
				return false;
			}
			result = num2 switch
			{
				2 => result * 10, 
				3 => result, 
				_ => result * 100, 
			};
			value = value.Substring(0, num);
		}
		if (value.Length != 6)
		{
			return false;
		}
		if (!int.TryParse(value.Substring(0, 2), out var result2) || !int.TryParse(value.Substring(2, 2), out var result3) || !int.TryParse(value.Substring(4, 2), out var result4))
		{
			return false;
		}
		if (result2 > 23 || result3 > 59 || result4 > 59)
		{
			return false;
		}
		time = ((result2 * 60 + result3) * 60 + result4) * 1000 + result + 1;
		return true;
	}

	internal static string FormatCSideTime(int time, string formatString)
	{
		return CSideTimeToDateTime(time).ToString(formatString, CultureInfo.InvariantCulture);
	}
}
