using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDate : NavDateTimeValue
{
	private class NavDateNull : NavDate
	{
		public override bool IsNull => true;
	}

	private static readonly NavDate defaultValue = new NavDate();

	public static readonly DateTime SqlDateTimeUndefined = new DateTime(1753, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

	private static readonly DateTime SqlDateTimeFirstValid = new DateTime(1754, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

	private static readonly DateTime SqlDateTimeLastValid = new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Local);

	private object sqlWritableValue;

	private object sqlWhereClauseValue;

	private static readonly NavDate nullValue = new NavDateNull();

	public override NavNclType NclType => NavNclType.NavDate;

	public override int GetBytesSize => 4;

	public static NavDate Default => defaultValue;

	public static NavDate Null => nullValue;

	public static NavDate Undefined => defaultValue;

	private NavDate()
	{
	}

	private NavDate(DateTime initValue)
	{
		if (initValue.Kind != DateTimeKind.Local || (!IsNormalDate(initValue) && !IsClosingDate(initValue)))
		{
			throw new NavNCLDateInvalidException();
		}
		value = initValue;
	}

	public static NavDate Create(DateTime initValue)
	{
		if (initValue == NavDateTimeHelper.DateTimeUndefined)
		{
			return defaultValue;
		}
		return new NavDate(initValue);
	}

	internal static NavDate Create(byte[] initValue)
	{
		return Create(BitConverter.ToInt32(initValue, 0));
	}

	internal static NavDate CreateNavDate(byte[] data, int startIndex)
	{
		return Create(BitConverter.ToInt32(data, startIndex));
	}

	public static NavDate Create(int csideDate)
	{
		return Create(NavDateTimeHelper.CSideDateToDateTime(csideDate));
	}

	public static NavDate Create(uint csideDate)
	{
		return Create((int)csideDate);
	}

	public static NavDate CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Date);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Date);
		}
		return CreateNavDate(data, startIndex);
	}

	public static NavDate CreateFromObject(object value)
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
		if (value is int || value is long || value is Decimal18)
		{
			return Create(((IConvertible)value).ToInt32(CultureInfo.CurrentCulture));
		}
		if (value is byte[] initValue)
		{
			return Create(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavDate), value);
	}

	public static NavDate CreateDate(DateTime value, bool closing)
	{
		value = value.Date;
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			if (closing)
			{
				throw new NavNCLDateInvalidException();
			}
			return Undefined;
		}
		NavDate navDate = new NavDate();
		if (!closing)
		{
			navDate.value = new DateTime(value.Ticks, DateTimeKind.Local);
		}
		else
		{
			navDate.value = new DateTime(value.Ticks + NavDateTimeHelper.DateTimeClosingTicks, DateTimeKind.Local);
		}
		return navDate;
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		if (sqlWhereClauseValue == null)
		{
			if (IsZeroOrEmpty)
			{
				sqlWhereClauseValue = SqlDateTimeUndefined;
			}
			else if (value.CompareTo(SqlDateTimeFirstValid) < 0)
			{
				sqlWhereClauseValue = SqlDateTimeFirstValid;
			}
			else if (value.CompareTo(SqlDateTimeLastValid) > 0)
			{
				sqlWhereClauseValue = SqlDateTimeLastValid;
			}
			else
			{
				sqlWhereClauseValue = value;
			}
		}
		return sqlWhereClauseValue;
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		if (sqlWritableValue == null)
		{
			if (IsZeroOrEmpty)
			{
				sqlWritableValue = SqlDateTimeUndefined;
			}
			else
			{
				if (value.CompareTo(SqlDateTimeFirstValid) < 0 || value.CompareTo(SqlDateTimeLastValid) > 0)
				{
					throw new NavCSideException(22928068);
				}
				sqlWritableValue = value;
			}
		}
		return sqlWritableValue;
	}

	public override byte[] GetBytes()
	{
		return BitConverter.GetBytes(ToInt32());
	}

	public override int ToInt32()
	{
		return NavDateTimeHelper.DateTimeToCSideDate(Value);
	}

	public static bool IsNormalDate(DateTime value)
	{
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		return value.Ticks % 864000000000L == 0;
	}

	public static bool IsClosingDate(DateTime value)
	{
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		return value.Ticks % 864000000000L == NavDateTimeHelper.DateTimeClosingTicks;
	}

	public static NavDate operator +(NavDate value1, int value2)
	{
		return Add(value1, value2);
	}

	public static NavDate operator +(NavDate value1, long value2)
	{
		return value1 + ALCompiler.ToInt32(value2);
	}

	public static NavDate operator +(NavDate value1, Decimal18 value2)
	{
		return value1 + ALCompiler.ToInt32(value2);
	}

	public static NavDate operator -(NavDate value1, int value2)
	{
		return value1 + -value2;
	}

	public static NavDate operator -(NavDate value1, long value2)
	{
		return value1 + -value2;
	}

	public static NavDate operator -(NavDate value1, Decimal18 value2)
	{
		return value1 + -value2;
	}

	public static int operator -(NavDate value1, NavDate value2)
	{
		return Subtract(value1, value2);
	}

	public static NavDate Add(NavDate source, int days)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		DateTime dateTime = source.Value;
		if (dateTime == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		if (IsClosingDate(dateTime))
		{
			throw new NavNCLClosingDateOperationException();
		}
		int num = NavDateTimeHelper.DateTimeToCSideDate(dateTime) + days * 2;
		if (num == 0)
		{
			throw new NavNCLDateInvalidException();
		}
		NavDate navDate = new NavDate();
		navDate.value = NavDateTimeHelper.CSideDateToDateTime(num);
		return navDate;
	}

	public static int Subtract(NavDate value1, NavDate value2)
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
		if (IsClosingDate(dateTime) || IsClosingDate(dateTime2))
		{
			throw new NavNCLClosingDateOperationException();
		}
		return (NavDateTimeHelper.DateTimeToCSideDate(dateTime) - NavDateTimeHelper.DateTimeToCSideDate(dateTime2)) / 2;
	}
}
