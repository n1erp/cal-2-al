using System;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class DateTimeHelper
{
	private static readonly DateTime[] datePeriodStartMinimumDate = new DateTime[5]
	{
		new DateTime(1, 1, 3),
		new DateTime(1, 1, 8),
		new DateTime(1, 2, 1),
		new DateTime(1, 4, 1),
		new DateTime(2, 1, 1)
	};

	private static readonly DateTime[] datePeriodStartMaximumDate = new DateTime[5]
	{
		new DateTime(9999, 12, 31),
		new DateTime(9999, 12, 20),
		new DateTime(9999, 12, 1),
		new DateTime(9999, 10, 1),
		new DateTime(9999, 1, 1)
	};

	private static readonly DateTime[] datePeriodEndMaximumDate = new DateTime[5]
	{
		new DateTime(9999, 12, 31),
		new DateTime(9999, 12, 26),
		new DateTime(9999, 12, 31),
		new DateTime(9999, 12, 31),
		new DateTime(9999, 12, 31)
	};

	private const string DefaultShortDateEvalOrder = "DMY";

	private const int DaysInWeek = 7;

	public const int MinWeekNo = 1;

	private const int MaxWeekNo = 53;

	public static void DateTimeToIsoYearWeek(DateTime dt, out int year, out int weekNo)
	{
		int num = DateTimeToDays(dt);
		int num2 = num % 7 - 3;
		int num3 = num - num2;
		year = DaysToDateTime(num3).Year;
		int num4 = DateTimeToDays(new DateTime(year, 1, 1));
		weekNo = (num3 - num4) / 7 + 1;
	}

	public static int DateTimeToIsoWeekNo(DateTime dt)
	{
		DateTimeToIsoYearWeek(dt, out var year, out var weekNo);
		return year * 100 + weekNo;
	}

	public static DateTime IsoYearWeekToDateTime(int year, int weekNo)
	{
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year");
		}
		if (weekNo < 1 || weekNo > 53)
		{
			throw new ArgumentOutOfRangeException("weekNo");
		}
		int num = DateTimeToDays(new DateTime(year, 1, 1));
		int num2 = IntDateNextThursday(num);
		int num3 = num2 - 3;
		int num4 = num3 + (weekNo - 1) * 7;
		if (weekNo == 53)
		{
			int num5 = IntDateDec28(year, num);
			if (num4 > num5)
			{
				throw new ArgumentOutOfRangeException("weekNo");
			}
		}
		return DaysToDateTime(num4);
	}

	public static DateTime IsoWeekNoToDateTime(int isoWeekNo)
	{
		return IsoYearWeekToDateTime(isoWeekNo / 100, isoWeekNo % 100);
	}

	public static int MaxWeekNoOfYear(int year)
	{
		if (year < 1 || year > 9999)
		{
			throw new ArgumentOutOfRangeException("year");
		}
		int num = DateTimeToDays(new DateTime(year, 1, 1));
		int num2 = IntDateNextThursday(num);
		int num3 = num2 - 3;
		int num4 = IntDateDec28(year, num);
		return (num4 - num3) / 7 + 1;
	}

	public static string GetDayMonthYearOrder(string shortDateSpecifier)
	{
		if (string.IsNullOrEmpty(shortDateSpecifier))
		{
			return "DMY";
		}
		StringBuilder stringBuilder = new StringBuilder(3);
		int length = shortDateSpecifier.Length;
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		while (num < length)
		{
			switch (shortDateSpecifier[num++])
			{
			case 'D':
			case 'd':
				if (!flag)
				{
					flag = true;
					stringBuilder.Append('D');
				}
				break;
			case 'M':
			case 'm':
				if (!flag2)
				{
					flag2 = true;
					stringBuilder.Append('M');
				}
				break;
			case 'Y':
			case 'y':
				if (!flag3)
				{
					flag3 = true;
					stringBuilder.Append('Y');
				}
				break;
			}
		}
		if (!flag || !flag2 || !flag3)
		{
			return "DMY";
		}
		return stringBuilder.ToString();
	}

	public static int[] GetDayMonthYearIndex(string dateTextEvaluationOrder)
	{
		int[] array = new int[3] { -1, -1, -1 };
		for (int i = 0; i < dateTextEvaluationOrder.Length; i++)
		{
			switch (dateTextEvaluationOrder[i])
			{
			case 'D':
				array[0] = i;
				break;
			case 'M':
				array[1] = i;
				break;
			case 'Y':
				array[2] = i;
				break;
			}
		}
		return array;
	}

	public static DateTime TruncateMilliseconds(DateTime dateTime)
	{
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);
	}

	internal static int DateTimeToDays(DateTime date)
	{
		return (int)(date.Ticks / 864000000000L);
	}

	internal static int DayOfWeekToNavDayNo(DayOfWeek dow)
	{
		if (dow != DayOfWeek.Sunday)
		{
			return (int)dow;
		}
		return 7;
	}

	internal static DateTime DaysToDateTime(int value, DateTimeKind valueKind)
	{
		return new DateTime(value * 864000000000L, valueKind);
	}

	internal static DateTime DaysToDateTime(int value)
	{
		return DaysToDateTime(value, DateTimeKind.Local);
	}

	internal static bool IsDateAtStartOfPeriod(DateTime date, DatePeriodType periodType)
	{
		if (date < datePeriodStartMinimumDate[(int)periodType] || date > datePeriodStartMaximumDate[(int)periodType])
		{
			return false;
		}
		switch (periodType)
		{
		case DatePeriodType.Date:
			return true;
		case DatePeriodType.Week:
			return date.DayOfWeek == DayOfWeek.Monday;
		case DatePeriodType.Month:
			return date.Day == 1;
		case DatePeriodType.Quarter:
			if (date.Day == 1)
			{
				return date.Month % 3 == 1;
			}
			return false;
		case DatePeriodType.Year:
			if (date.Day == 1)
			{
				return date.Month == 1;
			}
			return false;
		default:
			throw new NotSupportedException();
		}
	}

	internal static bool DatePeriodRoundDown(DateTime date, DatePeriodType periodType, out DateTime result)
	{
		if (date < datePeriodStartMinimumDate[(int)periodType] || date > datePeriodEndMaximumDate[(int)periodType])
		{
			result = date;
			return false;
		}
		switch (periodType)
		{
		case DatePeriodType.Date:
			result = date;
			return true;
		case DatePeriodType.Week:
		{
			int num = (int)(date.DayOfWeek + 7 - 1) % 7;
			result = ((num == 0) ? date : date.AddDays(-num));
			return true;
		}
		case DatePeriodType.Month:
			result = new DateTime(date.Year, date.Month, 1);
			return true;
		case DatePeriodType.Quarter:
		{
			int month = date.Month;
			result = new DateTime(date.Year, month - (month - 1) % 3, 1);
			return true;
		}
		case DatePeriodType.Year:
			result = new DateTime(date.Year, 1, 1);
			return true;
		default:
			throw new InvalidOperationException();
		}
	}

	internal static bool DatePeriodRoundUp(DateTime date, DatePeriodType periodType, out DateTime result)
	{
		if (date < datePeriodStartMinimumDate[(int)periodType] || date > datePeriodEndMaximumDate[(int)periodType])
		{
			result = date;
			return false;
		}
		switch (periodType)
		{
		case DatePeriodType.Date:
			result = date;
			return true;
		case DatePeriodType.Week:
		{
			int num = 6 - (int)(date.DayOfWeek + 7 - 1) % 7;
			result = ((num == 0) ? date : date.AddDays(num));
			return true;
		}
		case DatePeriodType.Month:
		{
			int year = date.Year;
			int month = date.Month;
			result = new DateTime(year, month, DateTime.DaysInMonth(year, month));
			return true;
		}
		case DatePeriodType.Quarter:
		{
			int year = date.Year;
			int month = date.Month;
			int month2 = month + (12 - month) % 3;
			result = new DateTime(year, month2, DateTime.DaysInMonth(year, month2));
			return true;
		}
		case DatePeriodType.Year:
			result = new DateTime(date.Year, 12, 31);
			return true;
		default:
			throw new InvalidOperationException();
		}
	}

	internal static DateTime DatePeriodStartMinimumDate(DatePeriodType periodType)
	{
		return datePeriodStartMinimumDate[(int)periodType];
	}

	internal static DateTime DatePeriodStartMaximumDate(DatePeriodType periodType)
	{
		return datePeriodStartMaximumDate[(int)periodType];
	}

	private static int IntDateNextThursday(int intDate)
	{
		int num = 6 - (intDate + 3) % 7;
		return intDate + num;
	}

	private static int IntDateDec28(int year, int intDateJan1)
	{
		int num = intDateJan1 + 361;
		if (DateTime.IsLeapYear(year))
		{
			num++;
		}
		return num;
	}
}
