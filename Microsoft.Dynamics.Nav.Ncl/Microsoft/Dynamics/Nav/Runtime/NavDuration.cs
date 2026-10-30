using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDuration : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private static readonly NavDuration defaultDuration = new NavDuration(0L);

	private static readonly NavDuration minValue = new NavDuration(long.MinValue);

	private static readonly NavDuration maxValue = new NavDuration(long.MaxValue);

	private long value;

	public override NavNclType NclType => NavNclType.NavDuration;

	public override int GetBytesSize => 8;

	public override object ValueAsObject => Value;

	public override object ClientObject => (TimeSpan)this;

	public override bool IsZeroOrEmpty => value == 0;

	public static NavDuration Default => defaultDuration;

	public static NavDuration MinValue => minValue;

	public static NavDuration MaxValue => maxValue;

	public long Value => value;

	public NavDuration(long initValue)
	{
		value = initValue;
	}

	public NavDuration(TimeSpan initValue)
	{
		value = (long)initValue.TotalMilliseconds;
	}

	internal NavDuration(byte[] initValue)
	{
		value = BitConverter.ToInt64(initValue, 0);
	}

	internal NavDuration(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 8)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Duration);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Duration);
		}
		value = BitConverter.ToInt64(data, startIndex);
	}

	public static NavDuration CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavDuration(data, startIndex, length);
	}

	public static NavDuration CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return new NavDuration(navValue.ToInt64());
		}
		if (value is TimeSpan)
		{
			return new NavDuration((TimeSpan)value);
		}
		if (value is IConvertible convertible)
		{
			return new NavDuration(convertible.ToInt64(CultureInfo.CurrentCulture));
		}
		if (value is byte[] initValue)
		{
			return new NavDuration(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavDuration), value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override byte[] GetBytes()
	{
		return BitConverter.GetBytes(Value);
	}

	public override int ToInt32()
	{
		return (int)Value;
	}

	public override long ToInt64()
	{
		return Value;
	}

	public static implicit operator long(NavDuration value)
	{
		return value?.Value ?? 0;
	}

	public static implicit operator Decimal18(NavDuration value)
	{
		if (value == null)
		{
			return 0;
		}
		return value.Value;
	}

	public static implicit operator NavDuration(long value)
	{
		return new NavDuration(value);
	}

	public static implicit operator TimeSpan(NavDuration value)
	{
		if (value == null)
		{
			return Default;
		}
		if (TimeSpan.MinValue.TotalMilliseconds == (double)value.Value)
		{
			return TimeSpan.MinValue;
		}
		if (TimeSpan.MaxValue.TotalMilliseconds == (double)value.Value)
		{
			return TimeSpan.MaxValue;
		}
		return TimeSpan.FromMilliseconds(value.Value);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == this)
		{
			return true;
		}
		NavValue other = obj as NavValue;
		if (obj != null)
		{
			return Equals(other);
		}
		if (obj is byte)
		{
			return Value == (byte)obj;
		}
		if (obj is int)
		{
			return Value == (int)obj;
		}
		if (obj is long)
		{
			return Value == (long)obj;
		}
		if (obj is Decimal18)
		{
			return Value == (Decimal18)obj;
		}
		return ThrowUnableToCompareException(obj);
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		switch (other.NavType)
		{
		case NavType.Char:
		case NavType.Integer:
		case NavType.Byte:
		case NavType.Option:
		case NavType.BigInteger:
		case NavType.Duration:
			return Value == other.ToInt64();
		case NavType.Decimal:
			return Value == other.ToDecimal();
		default:
			return ThrowUnableToCompareException(other);
		}
	}

	public bool Equals(byte other)
	{
		return Value == other;
	}

	public bool Equals(int other)
	{
		return Value == other;
	}

	public bool Equals(long other)
	{
		return Value == other;
	}

	public bool Equals(Decimal18 other)
	{
		return Value == other;
	}

	public static bool operator ==(NavDuration right, long left)
	{
		if (right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavDuration right, long left)
	{
		return !(right == left);
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			switch (other.NavType)
			{
			case NavType.Char:
			case NavType.Integer:
			case NavType.Byte:
			case NavType.Option:
			case NavType.BigInteger:
			case NavType.Duration:
				return Value.CompareTo(other.ToInt64());
			case NavType.Decimal:
				return ((Decimal18)Value).CompareTo(other.ToDecimal());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	public string Format(FormatSettings formatSettings, int standardFormatNumber)
	{
		if (value == 0L && standardFormatNumber != 9)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		TimeSpan timeSpan;
		if (value < 0)
		{
			stringBuilder.Append('-');
			timeSpan = TimeSpan.FromMilliseconds(-value);
		}
		else
		{
			timeSpan = TimeSpan.FromMilliseconds(value);
		}
		if (standardFormatNumber == 9)
		{
			stringBuilder.Append('P').Append(timeSpan.Days).Append('D')
				.Append('T')
				.Append(timeSpan.Hours)
				.Append('H')
				.Append(timeSpan.Minutes)
				.Append('M')
				.Append(timeSpan.Seconds)
				.Append((0.001 * (double)timeSpan.Milliseconds).ToString(".0##", CultureInfo.InvariantCulture))
				.Append('S');
		}
		else
		{
			bool expressUnitsAsNumbers = standardFormatNumber == 2;
			AppendDurationComponent(DurationComponent.Day, timeSpan.Days, expressUnitsAsNumbers, stringBuilder, formatSettings);
			AppendDurationComponent(DurationComponent.Hour, timeSpan.Hours, expressUnitsAsNumbers, stringBuilder, formatSettings);
			AppendDurationComponent(DurationComponent.Minute, timeSpan.Minutes, expressUnitsAsNumbers, stringBuilder, formatSettings);
			AppendDurationComponent(DurationComponent.Second, timeSpan.Seconds, expressUnitsAsNumbers, stringBuilder, formatSettings);
			AppendDurationComponent(DurationComponent.Millisecond, timeSpan.Milliseconds, expressUnitsAsNumbers, stringBuilder, formatSettings);
			stringBuilder.Length--;
		}
		return stringBuilder.ToString();
	}

	private static void AppendDurationComponent(DurationComponent durationComponent, int value, bool expressUnitsAsNumbers, StringBuilder stringBuilder, FormatSettings formatSettings)
	{
		if (value != 0)
		{
			if (value > 1)
			{
				durationComponent |= DurationComponent.Plural;
			}
			stringBuilder.Append(value).Append(' ');
			if (expressUnitsAsNumbers)
			{
				stringBuilder.Append((int)durationComponent);
			}
			else
			{
				stringBuilder.Append(formatSettings.GetDurationUnit(durationComponent));
			}
			stringBuilder.Append(' ');
		}
	}
}
