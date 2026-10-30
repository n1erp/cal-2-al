using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTime : NavDateTimeValue
{
	private static readonly NavTime defaultValue = new NavTime();

	private static readonly NavTime minValue = Create(0, 0, 0, 0);

	private static readonly NavTime maxValue = Create(23, 59, 59, 999);

	public static readonly DateTime SqlTimeUndefined = new DateTime(1753, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

	private const int SqlTimeYear = 1754;

	private const int SqlTimeMonth = 1;

	private const int SqlTimeDay = 1;

	private object sqlValue;

	public override NavNclType NclType => NavNclType.NavTime;

	public override int GetBytesSize => 4;

	public static NavTime Default => Undefined;

	public static NavTime MinValue => minValue;

	public static NavTime MaxValue => maxValue;

	public static NavTime Undefined => defaultValue;

	private NavTime()
	{
	}

	private NavTime(DateTime initValue)
	{
		if (initValue.Kind != DateTimeKind.Local || initValue.Date != NavDateTimeHelper.DateTimeMinimum)
		{
			throw new NavNCLInvalidTimeException();
		}
		value = NavDateTimeHelper.RoundDateTimeToMilliseconds(initValue);
	}

	public static NavTime Create(DateTime initValue)
	{
		if (initValue == NavDateTimeHelper.DateTimeUndefined)
		{
			return defaultValue;
		}
		return new NavTime(initValue);
	}

	public static NavTime Create(int hours, int minutes, int seconds, int milliseconds)
	{
		try
		{
			return Create(NavDateTimeHelper.DateTimeMinimum.AddTicks(new TimeSpan(0, hours, minutes, seconds, milliseconds).Ticks));
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new NavNCLInvalidTimeException();
		}
	}

	public static NavTime Create(int csideTime)
	{
		return Create(NavDateTimeHelper.CSideTimeToDateTime(csideTime));
	}

	public static NavTime Create(uint csideTime)
	{
		return Create((int)csideTime);
	}

	internal static NavTime Create(byte[] initValue)
	{
		return Create(BitConverter.ToInt32(initValue, 0));
	}

	internal static NavTime Create(byte[] data, int startIndex)
	{
		return Create(BitConverter.ToInt32(data, startIndex));
	}

	public static NavTime CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 4)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Time);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Time);
		}
		return Create(data, startIndex);
	}

	public static NavTime CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return Create(navValue.ToDateTime());
		}
		if (value is DateTime)
		{
			return Create((DateTime)value);
		}
		if (value is IConvertible convertible)
		{
			return Create(convertible.ToInt32(CultureInfo.CurrentCulture));
		}
		if (value is byte[] initValue)
		{
			return Create(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavTime), value);
	}

	public static NavTime CreateTime(DateTime value)
	{
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			return Undefined;
		}
		NavTime navTime = new NavTime();
		DateTime dateTime = NavDateTimeHelper.RoundDateTimeToMilliseconds(value);
		navTime.value = NavDateTimeHelper.DateTimeMinimum.AddTicks(dateTime.TimeOfDay.Ticks);
		return navTime;
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		return GetSqlWritableValue(sqlDataType, 0);
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		if (sqlValue == null)
		{
			if ((object)this == Undefined)
			{
				sqlValue = SqlTimeUndefined;
			}
			else
			{
				sqlValue = new DateTime(1754, 1, 1, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Local);
			}
		}
		return sqlValue;
	}

	public override byte[] GetBytes()
	{
		uint num = Convert.ToUInt32(ToInt32());
		return BitConverter.GetBytes(num);
	}

	public override int ToInt32()
	{
		return NavDateTimeHelper.DateTimeToCSideTime(Value);
	}

	public static NavTime operator +(NavTime value1, int value2)
	{
		return Add(value1, value2);
	}

	public static NavTime operator +(NavTime value1, long value2)
	{
		return Add(value1, ALCompiler.ToInt32(value2));
	}

	public static NavTime operator +(NavTime value1, Decimal18 value2)
	{
		return Add(value1, ALCompiler.ToInt32(value2));
	}

	public static NavTime operator -(NavTime value1, int value2)
	{
		return value1 + -value2;
	}

	public static NavTime operator -(NavTime value1, long value2)
	{
		return value1 + -value2;
	}

	public static NavTime operator -(NavTime value1, Decimal18 value2)
	{
		return value1 + -value2;
	}

	public static int operator -(NavTime value1, NavTime value2)
	{
		return Subtract(value1, value2);
	}

	public static NavTime Add(NavTime value1, int milliseconds)
	{
		if (value1 == null)
		{
			throw new ArgumentNullException("value1");
		}
		DateTime dateTime = value1.Value;
		if (value1 == Undefined)
		{
			throw new NavNCLInvalidTimeException();
		}
		int num = (int)(dateTime.Ticks % 864000000000L / 10000) + milliseconds % 86400000;
		num = (num + 86400000) % 86400000;
		NavTime navTime = new NavTime();
		navTime.value = NavDateTimeHelper.DateTimeMinimum.AddMilliseconds(num);
		return navTime;
	}

	public static int Subtract(NavTime value1, NavTime value2)
	{
		if (value1 == null)
		{
			throw new ArgumentNullException("value1");
		}
		if (value2 == null)
		{
			throw new ArgumentNullException("value2");
		}
		DateTime dateTime = value1.Value;
		DateTime dateTime2 = value2.Value;
		if (dateTime == dateTime2)
		{
			return 0;
		}
		if (dateTime == NavDateTimeHelper.DateTimeUndefined || dateTime2 == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		return (int)(dateTime - dateTime2).TotalMilliseconds;
	}
}
