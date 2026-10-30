using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavBigInteger : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private class NavBigIntegerNull : NavBigInteger
	{
		public override bool IsNull => true;

		internal NavBigIntegerNull()
			: base(0L)
		{
		}
	}

	private const long CacheLow = -5L;

	private const long CacheHigh = 10L;

	private static readonly NavBigInteger defaultValue = new NavBigInteger(0L);

	private static readonly NavBigInteger maxValue = new NavBigInteger(long.MaxValue);

	private static readonly NavBigInteger minValue = new NavBigInteger(long.MinValue);

	private static readonly NavBigInteger[] cachedNavBigIntegers = new NavBigInteger[16]
	{
		new NavBigInteger(-5L),
		new NavBigInteger(-4L),
		new NavBigInteger(-3L),
		new NavBigInteger(-2L),
		new NavBigInteger(-1L),
		defaultValue,
		new NavBigInteger(1L),
		new NavBigInteger(2L),
		new NavBigInteger(3L),
		new NavBigInteger(4L),
		new NavBigInteger(5L),
		new NavBigInteger(6L),
		new NavBigInteger(7L),
		new NavBigInteger(8L),
		new NavBigInteger(9L),
		new NavBigInteger(10L)
	};

	private static readonly NavBigInteger nullValue = new NavBigIntegerNull();

	private readonly long value;

	private object sqlValue;

	public override NavNclType NclType => NavNclType.NavBigInteger;

	public override object ValueAsObject => Value;

	public override int GetBytesSize => 8;

	public override bool IsZeroOrEmpty => value == 0;

	public static NavBigInteger Default => defaultValue;

	public static NavBigInteger Null => nullValue;

	public static NavBigInteger MaxValue => maxValue;

	public static NavBigInteger MinValue => minValue;

	public long Value => value;

	private NavBigInteger(long initValue)
	{
		value = initValue;
	}

	private NavBigInteger(long initValue, object sqlValue)
		: this(initValue)
	{
		this.sqlValue = sqlValue;
	}

	public static NavBigInteger Create(long value, object sqlValue)
	{
		if (value >= -5 && value <= 10)
		{
			return cachedNavBigIntegers[value - -5];
		}
		return new NavBigInteger(value, sqlValue);
	}

	public static NavBigInteger Create(long value)
	{
		return Create(value, null);
	}

	public static NavBigInteger Create(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 8)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.BigInteger);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.BigInteger);
		}
		return Create(BitConverter.ToInt64(data, startIndex));
	}

	public static NavBigInteger CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return Create(navValue.ToInt64());
		}
		if (value is IConvertible convertible)
		{
			return Create(convertible.ToInt64(CultureInfo.CurrentCulture));
		}
		if (value is byte[] data)
		{
			return Create(data, 0, 8);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavBigInteger), value);
	}

	private unsafe static byte[] TimestampToByteArray(long timestamp)
	{
		byte[] array = new byte[8];
		byte* ptr = (byte*)(&timestamp);
		for (int i = 0; i < 8; i++)
		{
			array[i] = ptr[7 - i];
		}
		return array;
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		if (sqlDataType == SqlDataType.BigInt)
		{
			return value;
		}
		if (sqlValue == null)
		{
			sqlValue = TimestampToByteArray(value);
		}
		return sqlValue;
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		if (sqlDataType == SqlDataType.BigInt || sqlDataType == SqlDataType.Default)
		{
			return value;
		}
		return GetBytes();
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override byte[] GetBytes()
	{
		return BitConverter.GetBytes(Value);
	}

	public override void StoreToBytes(BinaryWriter writer)
	{
		if (writer != null)
		{
			writer.Write((int)NavType);
			writer.Write(8);
			writer.Write(value);
		}
	}

	public override int ToInt32()
	{
		return (int)Value;
	}

	public override long ToInt64()
	{
		return Value;
	}

	public static implicit operator long(NavBigInteger value)
	{
		return value?.value ?? 0;
	}

	public static implicit operator Decimal18(NavBigInteger value)
	{
		if (value == null)
		{
			return 0;
		}
		return value.value;
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
		if (obj is NavValue other)
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
		switch (other.NclType)
		{
		case NavNclType.NavByte:
		case NavNclType.NavInteger:
		case NavNclType.NavBigInteger:
		case NavNclType.NavOption:
		case NavNclType.NavDuration:
			return Value == other.ToInt64();
		case NavNclType.NavDecimal:
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

	public static bool operator ==(NavBigInteger right, long left)
	{
		if (right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavBigInteger right, long left)
	{
		return !(right == left);
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			switch (other.NclType)
			{
			case NavNclType.NavByte:
			case NavNclType.NavInteger:
			case NavNclType.NavBigInteger:
			case NavNclType.NavOption:
			case NavNclType.NavDuration:
				return Value.CompareTo(other.ToInt64());
			case NavNclType.NavDecimal:
				return ((Decimal18)Value).CompareTo(other.ToDecimal());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
