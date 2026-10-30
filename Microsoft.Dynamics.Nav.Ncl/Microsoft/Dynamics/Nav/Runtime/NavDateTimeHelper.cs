using System;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavDateTimeHelper
{
	public const int CSideDateUndefined = 0;

	public const int CSideTimeUndefined = 0;

	public const long CSideDateTimeUndefined = 0L;

	public static readonly DateTime DateTimeUndefined = default;

	public const int CSideDateMinimum = 1;

	public static readonly DateTime DateTimeMinimum = new DateTime(1, 1, 2, 0, 0, 0, DateTimeKind.Local);

	public const string DateTimeMinimumDateXmlFormatPrefix = "0001-01-02T";

	public const long CSideDateTimeZero = 1L;

	public const int CSideDateMinimumClosing = 2;

	public const int CSideDateFirstSupported = 737;

	public static readonly DateTime DateTimeMinimumClosing = DateTimeMinimum.AddTicks(DateTimeClosingTicks);

	public static readonly DateTime DateTimeJan1st1601 = new DateTime(1601, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public const int CSideDateMaximumClosing = 7304850;

	public static readonly long DateTimeClosingTicks = new DateTime(1, 1, 1, 23, 59, 59, 0).Ticks;

	public const int MillisecondsPerDay = 86400000;

	public static DateTime CSideDateToDateTime(int csideDate, DateTimeKind csideDateKind)
	{
		if (csideDate <= 7304850)
		{
			if (csideDate >= 737)
			{
				DateTime result = DateTimeHelper.DaysToDateTime((csideDate - 737) / 2 + 2, csideDateKind);
				if ((csideDate & 1) == 0)
				{
					return result.AddTicks(DateTimeClosingTicks);
				}
				return result;
			}
			switch (csideDate)
			{
			case 0:
				return DateTimeUndefined;
			case 1:
				return DateTimeMinimum;
			case 2:
				return DateTimeMinimumClosing;
			}
		}
		throw new NavNCLDateInvalidException();
	}

	public static DateTime CSideDateToDateTime(int csideDate)
	{
		return CSideDateToDateTime(csideDate, DateTimeKind.Local);
	}

	public static int DateTimeToCSideDate(DateTime value)
	{
		if (value == DateTimeUndefined)
		{
			return 0;
		}
		int num = DateTimeHelper.DateTimeToDays(value);
		int num2 = ((value.Ticks % 864000000000L == DateTimeClosingTicks) ? 1 : 0);
		return num switch
		{
			0 => throw new NavNCLDateInvalidException(), 
			1 => 1 + num2, 
			_ => 737 + (num - 2) * 2 + num2, 
		};
	}

	public static DateTime CSideDateTimeToDateTime(long csideDateTime)
	{
		if (csideDateTime == 0L)
		{
			return DateTimeUndefined;
		}
		csideDateTime--;
		int csideDate = 1 + 2 * (int)(csideDateTime / 86400000);
		long num = csideDateTime % 86400000 * 10000;
		return new DateTime(CSideDateToDateTime(csideDate, DateTimeKind.Utc).Ticks + num, DateTimeKind.Utc);
	}

	public static long DateTimeToCSideDateTime(DateTime value)
	{
		if (value == DateTimeUndefined)
		{
			return 0L;
		}
		int num = DateTimeToCSideDate(value.Date);
		if (num == 0)
		{
			throw new NavNCLDateInvalidException();
		}
		long num2 = value.Ticks % 864000000000L;
		return 1 + (long)(num / 2) * 86400000L + num2 / 10000;
	}

	public static DateTime CSideTimeToDateTime(int value)
	{
		if (value == 0)
		{
			return DateTimeUndefined;
		}
		if (value < 0 || value > 86400000)
		{
			throw new NavNCLInvalidTimeException();
		}
		return DateTimeMinimum.AddTicks(10000L * (long)(value - 1));
	}

	public static int DateTimeToCSideTime(DateTime value)
	{
		if (value == DateTimeUndefined)
		{
			return 0;
		}
		return 1 + (int)(value.Ticks % 864000000000L / 10000);
	}

	public static DateTime RoundDateTimeToMilliseconds(DateTime dt)
	{
		return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind);
	}

	public static NavDateTime ToClosingValueIfTimeIsDateTimeMinimum(NavDateTime fromNavDateTime)
	{
		if (!(fromNavDateTime.TimePart == DateTimeMinimum))
		{
			return fromNavDateTime;
		}
		return NavDateTime.Create(fromNavDateTime.DatePart.value.AddTicks(DateTimeClosingTicks), DateTimeReferenceFrame.Client);
	}
}
