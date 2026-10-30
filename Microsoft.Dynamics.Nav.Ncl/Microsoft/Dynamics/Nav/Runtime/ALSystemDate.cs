using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemDate
{
	public static NavDate ALToday => NavDate.CreateDate(NavDateTime.ClientToday, closing: false);

	public static NavTime ALTime => NavDateTime.Create(DateTimeNowProvider.Current.UtcNow).TimePart;

	public static NavDateTime ALCurrentDateTime => NavDateTime.Create(DateTimeNowProvider.Current.UtcNow);

	public static NavDate ALNormalDate(NavDate date)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		if (NavDate.IsNormalDate(date))
		{
			return date;
		}
		return NavDate.CreateDate(date.Value, closing: false);
	}

	public static NavDate ALClosingDate(NavDate date)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		if (NavDate.IsClosingDate(date))
		{
			return date;
		}
		return NavDate.CreateDate(date.Value, closing: true);
	}

	public static NavDate ALDT2Date(NavDateTime dateTime)
	{
		if (dateTime == null)
		{
			throw new ArgumentNullException("dateTime");
		}
		return dateTime.DatePart;
	}

	public static NavDate ALWorkDate()
	{
		return NavCurrentThread.Session.WorkDate;
	}

	public static NavDate ALWorkDate(NavDate workDate)
	{
		NavCurrentThread.Session.WorkDate = workDate;
		NavCurrentThread.Session.ClientCallbackOrNull?.WorkDateChanged(NavCurrentThread.Session.WorkDate);
		return NavCurrentThread.Session.WorkDate;
	}

	public static int ALDate2DMY(NavDate date, int component)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		DateTime value = date.Value;
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		if (value.Date == NavDateTimeHelper.DateTimeMinimum)
		{
			switch (component)
			{
			case 1:
				return 1;
			case 2:
				return 1;
			case 3:
				return 0;
			}
		}
		else
		{
			switch (component)
			{
			case 1:
				return value.Day;
			case 2:
				return value.Month;
			case 3:
				return value.Year;
			}
		}
		throw new NavNCLOutsidePermittedRangeException("DATE2DMY", 2, component, 1, 3);
	}

	public static NavDate ALDMY2Date(int day)
	{
		DateTime clientToday = NavDateTime.ClientToday;
		return ALDMY2Date(day, clientToday.Month, clientToday.Year);
	}

	public static NavDate ALDMY2Date(int day, int month)
	{
		return ALDMY2Date(day, month, NavDateTime.ClientToday.Year);
	}

	public static NavDate ALDMY2Date(int day, int month, int year, bool closing = false)
	{
		if (year == 0 && month == 1 && day == 1)
		{
			return NavDate.Create(NavDateTimeHelper.DateTimeMinimum);
		}
		try
		{
			return NavDate.CreateDate(new DateTime(year, month, day, 0, 0, 0, 0, DateTimeKind.Local), closing);
		}
		catch (ArgumentOutOfRangeException)
		{
			if (month < 1 || month > 12)
			{
				throw new NavNCLOutsidePermittedRangeException("DMY2Date", 2, month, 1, 12);
			}
			if (year < 1 || year > 9999)
			{
				throw new NavNCLOutsidePermittedRangeException("DMY2Date", 3, year, 1, 9999);
			}
			if (day < 1 || day > DateTime.DaysInMonth(year, month))
			{
				throw new NavNCLOutsidePermittedRangeException("DMY2Date", 1, day, 1, DateTime.DaysInMonth(year, month));
			}
			throw new NavNCLDateInvalidException();
		}
	}

	public static int ALDate2DWY(NavDate date, int component)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		DateTime value = date.Value;
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		if (value.Date == NavDateTimeHelper.DateTimeMinimum)
		{
			switch (component)
			{
			case 1:
				return 6;
			case 2:
				return 53;
			case 3:
				return -1;
			}
		}
		else
		{
			switch (component)
			{
			case 1:
			{
				int num = (int)value.DayOfWeek;
				if (num == 0)
				{
					num = 7;
				}
				return num;
			}
			case 2:
				return DateTimeHelper.DateTimeToIsoWeekNo(value) % 100;
			case 3:
				return DateTimeHelper.DateTimeToIsoWeekNo(value) / 100;
			}
		}
		throw new NavNCLOutsidePermittedRangeException("DATE2DWY", 2, component, 1, 3);
	}

	public static NavDate ALDWY2Date(int weekday)
	{
		NavDate aLToday = ALToday;
		return ALDWY2Date(weekday, ALDate2DWY(aLToday, 2), ALDate2DWY(aLToday, 3));
	}

	public static NavDate ALDWY2Date(int weekday, int week)
	{
		NavDate aLToday = ALToday;
		return ALDWY2Date(weekday, week, ALDate2DWY(aLToday, 3));
	}

	public static NavDate ALDWY2Date(int weekday, int week, int year)
	{
		if (weekday == 6 && week == 53 && year == -1)
		{
			return NavDate.Create(NavDateTimeHelper.DateTimeMinimum);
		}
		if (weekday < 1 || weekday > 7)
		{
			throw new NavNCLOutsidePermittedRangeException("DWY2Date", 1, weekday, 1, 7);
		}
		try
		{
			return NavDate.Create(DateTimeHelper.IsoYearWeekToDateTime(year, week).AddDays(weekday - 1));
		}
		catch (ArgumentOutOfRangeException)
		{
			if (week < 1 || week > 53)
			{
				throw new NavNCLOutsidePermittedRangeException("DWY2Date", 2, week, 1, 53);
			}
			if (year < 1 || year > 9999)
			{
				throw new NavNCLOutsidePermittedRangeException("DWY2Date", 3, year, 1, 9999);
			}
			throw new NavNCLDateInvalidException();
		}
	}

	public static NavDate ALVariant2Date(NavVariant variant)
	{
		if (variant == null)
		{
			throw NavNCLInvalidTypeException.CreateExpectedType(CultureInfo.CurrentCulture, typeof(NavDate));
		}
		if (variant.IsInitialized)
		{
			switch (variant.InnerValue.NclType)
			{
			case NavNclType.NavDate:
				return (NavDate)variant.InnerValue;
			case NavNclType.NavDateTime:
				return ((NavDateTime)variant.InnerValue).DatePart;
			}
		}
		throw NavNCLInvalidTypeException.CreateExpectedType(CultureInfo.CurrentCulture, typeof(NavDate));
	}

	public static NavDate ALCalcDate(NavDateFormula dateExpression)
	{
		if (dateExpression == null)
		{
			throw new ArgumentNullException("dateExpression");
		}
		return ALCalcDate(dateExpression, ALToday);
	}

	public static NavDate ALCalcDate(NavDateFormula dateExpression, NavDate dateValue)
	{
		if (dateExpression == null)
		{
			throw new ArgumentNullException("dateExpression");
		}
		if (dateValue == null)
		{
			throw new ArgumentNullException("dateValue");
		}
		return dateExpression.CalcDate(dateValue);
	}

	public static NavDate ALCalcDate(string dateExpression, NavDate dateValue)
	{
		if (dateExpression == null)
		{
			throw new ArgumentNullException("dateExpression");
		}
		if (dateValue == null)
		{
			throw new ArgumentNullException("dateValue");
		}
		return new NavDateFormula(dateExpression).CalcDate(dateValue);
	}

	public static NavDate ALCalcDate(object dateExpression, NavDate dateValue)
	{
		throw new NavNCLArgumentException(Lang.IllegalCalcDateArgument);
	}

	public static NavDate ALCalcDate(string dateExpression)
	{
		if (dateExpression == null)
		{
			throw new ArgumentNullException("dateExpression");
		}
		return ALCalcDate(dateExpression, ALToday);
	}

	public static NavDate ALCalcDate(object dateExpression)
	{
		throw new NavNCLArgumentException(Lang.IllegalCalcDateArgument);
	}

	public static NavTime ALVariant2Time(NavVariant variant)
	{
		if (variant == null)
		{
			throw NavNCLInvalidTypeException.CreateExpectedType(CultureInfo.CurrentCulture, typeof(NavTime));
		}
		if (variant.IsInitialized)
		{
			switch (variant.InnerValue.NclType)
			{
			case NavNclType.NavTime:
				return (NavTime)variant.InnerValue;
			case NavNclType.NavDateTime:
				return ((NavDateTime)variant.InnerValue).TimePart;
			}
		}
		throw NavNCLInvalidTypeException.CreateExpectedType(CultureInfo.CurrentCulture, typeof(NavTime));
	}

	public static NavTime ALDT2Time(NavDateTime dateTime)
	{
		if (dateTime == null)
		{
			throw new ArgumentNullException("dateTime");
		}
		return dateTime.TimePart;
	}

	public static NavDateTime ALRoundDateTime(NavDateTime dateTime)
	{
		return ALRoundDateTime(dateTime, 1000L, "=");
	}

	public static NavDateTime ALRoundDateTime(NavDateTime dateTime, long precision)
	{
		return ALRoundDateTime(dateTime, precision, "=");
	}

	public static NavDateTime ALRoundDateTime(NavDateTime dateTime, string direction)
	{
		return ALRoundDateTime(dateTime, 1000L, direction);
	}

	public static NavDateTime ALRoundDateTime(NavDateTime dateTime, long precision, string direction)
	{
		if (dateTime.IsZeroOrEmpty)
		{
			return dateTime;
		}
		if (precision <= 0)
		{
			throw new NavNCLArgumentException(precision, "ROUNDDATETIME", 2);
		}
		if (direction == null || direction.Length != 1 || "<=>".IndexOf(direction[0]) < 0)
		{
			throw new NavNCLArgumentException(direction, "ROUNDDATETIME", 3);
		}
		long num = NavDateTime.ConvertCSideDateTimeToClientLocal(NavDateTimeHelper.DateTimeToCSideDateTime(dateTime.Value));
		num--;
		long num2 = num % precision;
		switch (direction[0])
		{
		case '>':
			if (num2 > 0)
			{
				num = num - num2 + precision;
			}
			break;
		case '<':
			if (num2 > 0)
			{
				num -= num2;
			}
			break;
		case '=':
		{
			long num3 = num - num2;
			long num4 = ((num2 > 0) ? (num - num2 + precision) : num);
			long num5 = Math.Abs(num - num4);
			num = ((num5 <= num2) ? num4 : num3);
			break;
		}
		}
		num++;
		return NavDateTime.Create(NavDateTimeHelper.CSideDateTimeToDateTime(NavDateTime.ConvertCSideDateTimeToUtc(num, adjustNonexistentDateTime: true)));
	}

	public static NavDateTime ALCreateDateTime(NavDate date, NavTime time)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		if (time == null)
		{
			throw new ArgumentNullException("time");
		}
		if (date.IsZeroOrEmpty)
		{
			if (time.IsZeroOrEmpty)
			{
				return NavDateTime.Undefined;
			}
			throw new NavNCLDateInvalidException();
		}
		DateTime dateTime = date.ToDateTime();
		if (!time.IsZeroOrEmpty)
		{
			dateTime = dateTime.AddTicks(time.Value.TimeOfDay.Ticks);
		}
		if (dateTime < NavDateTimeHelper.DateTimeJan1st1601)
		{
			return NavDateTime.Create(new DateTime(dateTime.Ticks, DateTimeKind.Utc));
		}
		return NavDateTime.CreateFromClientTimeAllowingForNonexistentTime(dateTime);
	}

	public static NavVariant ALDaTi2Variant(ITreeObject parentOfResult, NavDate date, NavTime time)
	{
		if (date == null)
		{
			throw new ArgumentNullException("date");
		}
		if (time == null)
		{
			throw new ArgumentNullException("time");
		}
		return new NavVariant(parentOfResult, ALCreateDateTime(date, time));
	}
}
