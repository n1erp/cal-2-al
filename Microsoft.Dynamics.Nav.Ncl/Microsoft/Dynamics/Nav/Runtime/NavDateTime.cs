using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDateTime : NavDateTimeValue
{
	private class NavDateTimeNull : NavDateTime
	{
		public override bool IsNull => true;
	}

	private static readonly NavDateTime defaultValue = new NavDateTime();

	private static readonly NavDateTime maxValue = Create(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc));

	private static readonly NavDateTime minValue = Create(NavDateTimeHelper.DateTimeMinimum);

	public static readonly DateTime SqlDateTimeUtcUndefined = new DateTime(1753, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	private static readonly DateTime SqlDateTimeUtcFirstValid = new DateTime(1754, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	private static readonly DateTime SqlDateTimeUtcLastValid = new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Utc);

	private object sqlWhereClauseValue;

	private object sqlWritableValue;

	private static readonly NavDateTime nullValue = new NavDateTimeNull();

	public override NavNclType NclType => NavNclType.NavDateTime;

	public override int GetBytesSize => 8;

	public static NavDateTime Default => Undefined;

	public static NavDateTime MinValue => minValue;

	public static NavDateTime MaxValue => maxValue;

	public static NavDateTime Now => Create(DateTimeNowProvider.Current.UtcNow);

	public static DateTime ClientToday => Now.ClientLocalValue.Date;

	public static NavDateTime Undefined => defaultValue;

	public static NavDateTime Null => nullValue;

	public DateTime ClientLocalValue => ConvertToLocalTime(value, DateTimeReferenceFrame.Client);

	public NavDate DatePart => NavDate.CreateDate(ClientLocalValue, closing: false);

	public NavTime TimePart => NavTime.CreateTime(ClientLocalValue);

	protected NavDateTime()
	{
	}

	private NavDateTime(DateTime initValue, DateTimeReferenceFrame referenceFrame, bool unspecifiedAsLocal)
		: this(initValue, referenceFrame, unspecifiedAsLocal, adjustNonexistentDateTime: false)
	{
	}

	private NavDateTime(DateTime initValue, DateTimeReferenceFrame referenceFrame, bool unspecifiedAsLocal, bool adjustNonexistentDateTime)
	{
		if (initValue.Kind == DateTimeKind.Unspecified && !unspecifiedAsLocal && initValue >= NavDateTimeHelper.DateTimeJan1st1601)
		{
			throw new NavNCLDateInvalidException();
		}
		switch (initValue.Kind)
		{
		case DateTimeKind.Utc:
			value = NavDateTimeHelper.RoundDateTimeToMilliseconds(initValue);
			break;
		case DateTimeKind.Unspecified:
		case DateTimeKind.Local:
			value = ConvertToUTc(NavDateTimeHelper.RoundDateTimeToMilliseconds(initValue), referenceFrame, adjustNonexistentDateTime);
			break;
		}
	}

	public static NavDateTime Create(DateTime initValue)
	{
		return Create(initValue, unspecifiedAsLocal: false);
	}

	public static NavDateTime Create(DateTime initValue, bool unspecifiedAsLocal)
	{
		return Create(initValue, DateTimeReferenceFrame.Client, unspecifiedAsLocal);
	}

	public static NavDateTime Create(DateTime initValue, DateTimeReferenceFrame referenceFrame)
	{
		return Create(initValue, referenceFrame, unspecifiedAsLocal: false);
	}

	public static NavDateTime Create(DateTime initValue, DateTimeReferenceFrame referenceFrame, bool unspecifiedAsLocal)
	{
		if (initValue == NavDateTimeHelper.DateTimeUndefined)
		{
			return defaultValue;
		}
		return new NavDateTime(initValue, referenceFrame, unspecifiedAsLocal);
	}

	public static NavDateTime CreateFromClientTimeAllowingForNonexistentTime(DateTime value)
	{
		if (value == NavDateTimeHelper.DateTimeUndefined)
		{
			return defaultValue;
		}
		return new NavDateTime(value, DateTimeReferenceFrame.Client, unspecifiedAsLocal: false, adjustNonexistentDateTime: true);
	}

	public static NavDateTime Create(long initValue)
	{
		return Create(NavDateTimeHelper.CSideDateTimeToDateTime(initValue));
	}

	internal static NavDateTime Create(byte[] initValue)
	{
		return Create(NavDateTimeHelper.CSideDateTimeToDateTime(BitConverter.ToInt64(initValue, 0)));
	}

	public static NavDateTime CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 8)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.DateTime);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.DateTime);
		}
		return Create(NavDateTimeHelper.CSideDateTimeToDateTime(BitConverter.ToInt64(data, startIndex)));
	}

	public static NavDateTime CreateFromObject(object value)
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
			return Create((DateTime)value, DateTimeReferenceFrame.Client, unspecifiedAsLocal: true);
		}
		if (value is int || value is long || value is Decimal18)
		{
			return Create(((IConvertible)value).ToInt32(CultureInfo.CurrentCulture));
		}
		if (value is byte[] initValue)
		{
			return Create(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavDateTime), value);
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		if (sqlWhereClauseValue == null)
		{
			if (IsZeroOrEmpty)
			{
				sqlWhereClauseValue = SqlDateTimeUtcUndefined;
			}
			else if (value.CompareTo(SqlDateTimeUtcFirstValid) < 0)
			{
				sqlWhereClauseValue = SqlDateTimeUtcFirstValid;
			}
			else if (value.CompareTo(SqlDateTimeUtcLastValid) > 0)
			{
				sqlWhereClauseValue = SqlDateTimeUtcLastValid;
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
				sqlWritableValue = SqlDateTimeUtcUndefined;
			}
			else
			{
				if (value.CompareTo(SqlDateTimeUtcFirstValid) < 0 || value.CompareTo(SqlDateTimeUtcLastValid) > 0)
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
		return BitConverter.GetBytes(ToInt64());
	}

	public override long ToInt64()
	{
		return NavDateTimeHelper.DateTimeToCSideDateTime(Value);
	}

	public static NavDateTime operator +(NavDateTime value1, int value2)
	{
		return Add(value1, value2);
	}

	public static NavDateTime operator +(NavDateTime value1, long value2)
	{
		return Add(value1, value2);
	}

	public static NavDateTime operator +(NavDateTime value1, Decimal18 value2)
	{
		return Add(value1, ALCompiler.ToInt64(value2));
	}

	public static NavDateTime operator +(NavDateTime value1, NavDuration value2)
	{
		if (value2 == null)
		{
			throw new ArgumentNullException("value2");
		}
		return Add(value1, value2.Value);
	}

	public static NavDateTime operator -(NavDateTime value1, int value2)
	{
		return value1 + -value2;
	}

	public static NavDateTime operator -(NavDateTime value1, long value2)
	{
		return value1 + -value2;
	}

	public static NavDateTime operator -(NavDateTime value1, Decimal18 value2)
	{
		return value1 + -value2;
	}

	public static NavDateTime operator -(NavDateTime value1, NavDuration value2)
	{
		if (value2 == null)
		{
			throw new ArgumentNullException("value2");
		}
		return value1 + -value2.Value;
	}

	public static NavDuration operator -(NavDateTime value1, NavDateTime value2)
	{
		return new NavDuration(Subtract(value1, value2));
	}

	internal static long ConvertCSideDateTimeToClientLocal(long csideDateTime)
	{
		DateTime dateTime = NavDateTimeHelper.CSideDateTimeToDateTime(csideDateTime);
		if (dateTime >= NavDateTimeHelper.DateTimeJan1st1601)
		{
			DateTime dateTime2 = ConvertToLocalTime(dateTime, DateTimeReferenceFrame.Client);
			csideDateTime = NavDateTimeHelper.DateTimeToCSideDateTime(dateTime2);
		}
		return csideDateTime;
	}

	internal static long ConvertCSideDateTimeToUtc(long csideDateTime, bool adjustNonexistentDateTime)
	{
		DateTime dateTime = NavDateTimeHelper.CSideDateTimeToDateTime(csideDateTime);
		if (dateTime >= NavDateTimeHelper.DateTimeJan1st1601)
		{
			DateTime dateTime2 = ConvertToUTc(dateTime, DateTimeReferenceFrame.Client, adjustNonexistentDateTime);
			csideDateTime = NavDateTimeHelper.DateTimeToCSideDateTime(dateTime2);
		}
		return csideDateTime;
	}

	public static NavDateTime Add(NavDateTime value1, long milliseconds)
	{
		if (value1 == null)
		{
			throw new ArgumentNullException("value1");
		}
		DateTime dateTime = value1.Value;
		if (dateTime == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		long num = NavDateTimeHelper.DateTimeToCSideDateTime(dateTime) + milliseconds;
		if (num == 0L)
		{
			throw new NavNCLDateInvalidException();
		}
		NavDateTime navDateTime = new NavDateTime();
		navDateTime.value = NavDateTimeHelper.CSideDateTimeToDateTime(num);
		return navDateTime;
	}

	public static long Subtract(NavDateTime value1, NavDateTime value2)
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
			return 0L;
		}
		if (dateTime == NavDateTimeHelper.DateTimeUndefined || dateTime2 == NavDateTimeHelper.DateTimeUndefined)
		{
			throw new NavNCLDateInvalidException();
		}
		return NavDateTimeHelper.DateTimeToCSideDateTime(dateTime) - NavDateTimeHelper.DateTimeToCSideDateTime(dateTime2);
	}

	private static DateTime ConvertToLocalTime(DateTime dateTime, DateTimeReferenceFrame referenceFrame)
	{
		if (dateTime == NavDateTimeHelper.DateTimeUndefined)
		{
			return Undefined;
		}
		if (dateTime >= NavDateTimeHelper.DateTimeJan1st1601)
		{
			return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(dateTime, (referenceFrame == DateTimeReferenceFrame.Client) ? NavCurrentThread.Session.ClientTimeZone : TimeZoneInfo.Local), DateTimeKind.Local);
		}
		return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
	}

	private static DateTime ConvertToUTc(DateTime dateTime, DateTimeReferenceFrame referenceFrame, bool adjustInvalidDateTime)
	{
		if (dateTime == NavDateTimeHelper.DateTimeUndefined)
		{
			return Undefined;
		}
		if (dateTime >= NavDateTimeHelper.DateTimeJan1st1601)
		{
			dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
			TimeZoneInfo clientTimeZone = NavCurrentSessionWrapper.ClientTimeZone;
			if (clientTimeZone.IsInvalidTime(dateTime))
			{
				if (!adjustInvalidDateTime)
				{
					throw new NavNCLDateInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidDateTimeForTimeZone, dateTime.ToString(CultureInfo.CurrentCulture)));
				}
				dateTime = AdjustForInvalidClientTime(dateTime, clientTimeZone);
			}
			return TimeZoneInfo.ConvertTimeToUtc(dateTime, (referenceFrame == DateTimeReferenceFrame.Server) ? TimeZoneInfo.Local : clientTimeZone);
		}
		return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
	}

	public static DateTime AdjustForInvalidClientTime(DateTime clientDateTime, TimeZoneInfo clientTimeZone)
	{
		TimeZoneInfo.AdjustmentRule adjustmentRule = clientTimeZone.GetAdjustmentRules().First((TimeZoneInfo.AdjustmentRule r) => r.DateStart <= clientDateTime && r.DateEnd >= clientDateTime);
		return clientDateTime + adjustmentRule.DaylightDelta;
	}
}
