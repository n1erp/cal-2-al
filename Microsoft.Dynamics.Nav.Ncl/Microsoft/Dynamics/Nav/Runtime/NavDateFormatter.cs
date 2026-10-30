using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> DateFieldNames = new ReadOnlyCollection<string>(new string[12]
	{
		"DAY", "MONTH", "MONTH TEXT", "QUARTER", "YEAR", "YEAR4", "WEEK", "WEEK YEAR", "WEEK YEAR4", "WEEKDAY",
		"WEEKDAY TEXT", "CLOSING"
	});

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		NavDate navDate = (NavDate)value;
		if (navDate.IsZeroOrEmpty)
		{
			return string.Empty;
		}
		if ((uint)formatNumber <= 7u || formatNumber == 9)
		{
			return NavFormatEvaluateHelper.FormatWithFormatNumber(navDate, length, formatNumber, formatsetting);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Date);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 7u || formatNumber == 9)
		{
			return NavCurrentThread.Session.FormatSettings.DateStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.Date);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return DateFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string result = null;
		NavDate navDate = (NavDate)value;
		options.Initialize(FormatSettings.DateTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		int weekNo;
		int year;
		switch ((char)(ushort)options.Field)
		{
		case '\u0003':
			result = GetDay(navDate).ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0004':
			result = navDate.Value.Month.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0005':
			result = settings.DateTextMonthNames[navDate.Value.Month - 1];
			break;
		case '\u0006':
			result = ((navDate.Value.Month + 2) / 3).ToString(CultureInfo.InvariantCulture);
			break;
		case '\a':
		{
			int dateRangeCentury = settings.DateRangeCentury;
			year = GetYear(navDate);
			if (IsInRange(year, dateRangeCentury, dateRangeCentury + 99))
			{
				year %= 100;
				result = ((year / 10 == 0) ? ("0" + year.ToString(CultureInfo.InvariantCulture)) : year.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				result = PadYear(year);
			}
			break;
		}
		case '\b':
			result = PadYear(GetYear(navDate));
			break;
		case '\t':
			DateTimeHelper.DateTimeToIsoYearWeek(navDate, out year, out weekNo);
			result = weekNo.ToString(CultureInfo.InvariantCulture);
			break;
		case '\n':
		{
			DateTimeHelper.DateTimeToIsoYearWeek(navDate, out year, out weekNo);
			int dateRangeCentury = settings.DateRangeCentury;
			if (IsInRange(year, dateRangeCentury, dateRangeCentury + 99))
			{
				year %= 100;
				result = ((year / 10 == 0) ? ("0" + year.ToString(CultureInfo.InvariantCulture)) : year.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				result = PadYear(year);
			}
			break;
		}
		case '\v':
			DateTimeHelper.DateTimeToIsoYearWeek(navDate, out year, out weekNo);
			result = PadYear(year);
			break;
		case '\f':
		{
			int num = (int)navDate.Value.DayOfWeek;
			if (num == 0)
			{
				num = 7;
			}
			result = num.ToString(CultureInfo.InvariantCulture);
			break;
		}
		case '\r':
			result = settings.DateTextWeekdayNames[(int)navDate.Value.DayOfWeek];
			break;
		case '\u000e':
			result = (NavDate.IsClosingDate(navDate) ? settings.ClosingDateCharacter : "");
			break;
		}
		return result;
	}

	private static int GetYear(NavDate dateValue)
	{
		if (dateValue == NavDateTimeHelper.DateTimeMinimum)
		{
			return 0;
		}
		return dateValue.Value.Year;
	}

	private static int GetDay(NavDate dateValue)
	{
		if (dateValue == NavDateTimeHelper.DateTimeMinimum)
		{
			return 1;
		}
		return dateValue.Value.Day;
	}

	internal static bool IsInRange(int value, int low, int high)
	{
		if (low <= value)
		{
			return value <= high;
		}
		return false;
	}

	internal static string PadYear(int year)
	{
		return year.ToString("D4", CultureInfo.InvariantCulture);
	}
}
