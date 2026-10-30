using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateTimeFormatter : NavValueFormatter
{
	private static readonly ReadOnlyCollection<string> DatetimeFieldNames = new ReadOnlyCollection<string>(new string[18]
	{
		"DAY", "MONTH", "MONTH TEXT", "QUARTER", "YEAR", "YEAR4", "WEEK", "WEEK YEAR", "WEEK YEAR4", "WEEKDAY",
		"WEEKDAY TEXT", "HOURS24", "HOURS12", "MINUTES", "SECONDS", "THOUSANDS", "AM/PM", "SECOND DEC."
	});

	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatSetting)
	{
		NavDateTime navDateTime = (NavDateTime)value;
		if (navDateTime.IsZeroOrEmpty)
		{
			return string.Empty;
		}
		if ((uint)formatNumber <= 3u || formatNumber == 9)
		{
			return NavFormatEvaluateHelper.FormatWithFormatNumber(navDateTime, length, formatNumber, formatSetting);
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.DateTime);
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 3u || formatNumber == 9)
		{
			return NavCurrentThread.Session.FormatSettings.DatetimeStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.DateTime);
	}

	internal override ReadOnlyCollection<string> GetFieldNames()
	{
		return DatetimeFieldNames;
	}

	internal override bool PadLeft(NavValue value)
	{
		return true;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		string text = null;
		NavDateTime navDateTime = (NavDateTime)value;
		options.Initialize(FormatSettings.DatetimeTypeFmt[options.Field - 3]);
		NavFormatEvaluateHelper.GetFieldFormatFromString(options, format, ref pos);
		int weekNo;
		int year;
		switch ((char)(ushort)options.Field)
		{
		case '\u0003':
			text = GetDay(navDateTime).ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0004':
			text = navDateTime.Value.Month.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0005':
			text = settings.DateTextMonthNames[navDateTime.Value.Month - 1];
			break;
		case '\u0006':
			text = ((navDateTime.Value.Month + 2) / 3).ToString(CultureInfo.InvariantCulture);
			break;
		case '\a':
		{
			int dateRangeCentury = settings.DateRangeCentury;
			year = GetYear(navDateTime);
			if (NavDateFormatter.IsInRange(year, dateRangeCentury, dateRangeCentury + 99))
			{
				year %= 100;
				text = ((year / 10 == 0) ? ("0" + year.ToString(CultureInfo.InvariantCulture)) : year.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				text = NavDateFormatter.PadYear(year);
			}
			break;
		}
		case '\b':
			text = NavDateFormatter.PadYear(GetYear(navDateTime));
			break;
		case '\t':
			DateTimeHelper.DateTimeToIsoYearWeek(navDateTime, out year, out weekNo);
			text = weekNo.ToString(CultureInfo.InvariantCulture);
			break;
		case '\n':
		{
			DateTimeHelper.DateTimeToIsoYearWeek(navDateTime, out year, out weekNo);
			int dateRangeCentury = settings.DateRangeCentury;
			if (NavDateFormatter.IsInRange(year, dateRangeCentury, dateRangeCentury + 99))
			{
				year %= 100;
				text = ((year / 10 == 0) ? ("0" + year.ToString(CultureInfo.InvariantCulture)) : year.ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				text = NavDateFormatter.PadYear(year);
			}
			break;
		}
		case '\v':
			DateTimeHelper.DateTimeToIsoYearWeek(navDateTime, out year, out weekNo);
			text = NavDateFormatter.PadYear(year);
			break;
		case '\f':
			text = ((int)navDateTime.Value.DayOfWeek).ToString(CultureInfo.InvariantCulture);
			break;
		case '\r':
			text = settings.DateTextWeekdayNames[(int)navDateTime.Value.DayOfWeek];
			break;
		case '\u000e':
			text = navDateTime.Value.Hour.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u000f':
			text = ((navDateTime.Value.Hour + 11) % 12 + 1).ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0010':
			text = navDateTime.Value.Minute.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0011':
			text = navDateTime.Value.Second.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0012':
			text = navDateTime.Value.Millisecond.ToString(CultureInfo.InvariantCulture);
			break;
		case '\u0013':
			text = ((navDateTime.Value.Hour < 12) ? settings.TimeAMDesignator : settings.TimePMDesignator);
			break;
		case '\u0014':
		{
			text = string.Empty;
			int num = navDateTime.Value.Millisecond;
			int num2 = 100;
			if (num != 0 || options.Length != 0)
			{
				int num3 = 1;
				text += ((options.Comma == '\u0001') ? settings.FieldFormatComma : options.Comma);
				while (num != 0)
				{
					text += (char)(num / num2 + 48);
					num -= (text[num3] - 48) * num2;
					num3++;
					num2 /= 10;
				}
			}
			break;
		}
		}
		return text;
	}

	private static int GetYear(NavDateTime datetimeValue)
	{
		if (datetimeValue == NavDateTimeHelper.DateTimeMinimum)
		{
			return 0;
		}
		return datetimeValue.Value.Year;
	}

	private static int GetDay(NavDateTime datetimeValue)
	{
		if (datetimeValue == NavDateTimeHelper.DateTimeMinimum)
		{
			return 1;
		}
		return datetimeValue.Value.Day;
	}
}
