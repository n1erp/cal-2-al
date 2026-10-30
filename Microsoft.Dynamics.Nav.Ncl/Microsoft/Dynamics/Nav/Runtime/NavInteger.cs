using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavInteger : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private class NavIntegerNull : NavInteger
	{
		public override bool IsNull => true;

		internal NavIntegerNull()
			: base(0)
		{
		}
	}

	private const int CacheLow = -5;

	private const int CacheHigh = 10;

	private static readonly NavInteger defaultValue = new NavInteger(0);

	private static readonly NavInteger minValue = new NavInteger(int.MinValue);

	private static readonly NavInteger maxValue = new NavInteger(int.MaxValue);

	private static readonly NavInteger[] cachedNavIntegers = new NavInteger[16]
	{
		new NavInteger(-5),
		new NavInteger(-4),
		new NavInteger(-3),
		new NavInteger(-2),
		new NavInteger(-1),
		defaultValue,
		new NavInteger(1),
		new NavInteger(2),
		new NavInteger(3),
		new NavInteger(4),
		new NavInteger(5),
		new NavInteger(6),
		new NavInteger(7),
		new NavInteger(8),
		new NavInteger(9),
		new NavInteger(10)
	};

	private readonly int value;

	private static readonly NavInteger nullValue = new NavIntegerNull();

	public override NavNclType NclType => NavNclType.NavInteger;

	public override int GetBytesSize => 4;

	public override object ValueAsObject => Value;

	public override bool IsZeroOrEmpty => value == 0;

	public static NavInteger Default => defaultValue;

	public static NavInteger Null => nullValue;

	public int Value => value;

	public static NavInteger MinValue => minValue;

	public static NavInteger MaxValue => maxValue;

	private NavInteger(int initValue)
	{
		value = initValue;
	}

	public static NavInteger Create(int value)
	{
		if (value >= -5 && value <= 10)
		{
			return cachedNavIntegers[value - -5];
		}
		return new NavInteger(value);
	}

	public static NavInteger Create(byte[] data)
	{
		return Create(data, 0, data.Length);
	}

	public static NavInteger Create(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 4)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Integer);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Integer);
		}
		return Create(BitConverter.ToInt32(data, startIndex));
	}

	public static NavInteger CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return Create(navValue.ToInt32());
		}
		if (value is string s)
		{
			if (int.TryParse(s, out var result))
			{
				return Create(result);
			}
		}
		else
		{
			if (value is IConvertible convertible)
			{
				return Create(convertible.ToInt32(CultureInfo.CurrentCulture));
			}
			if (value is byte[] data)
			{
				return Create(data, 0, 4);
			}
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavInteger), value);
	}

	public override void StoreToBytes(BinaryWriter writer)
	{
		if (writer != null)
		{
			writer.Write((int)NavType);
			writer.Write(4);
			writer.Write(value);
		}
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public override byte[] GetBytes()
	{
		return new byte[4]
		{
			(byte)(value & 0xFF),
			(byte)((value >> 8) & 0xFF),
			(byte)((value >> 16) & 0xFF),
			(byte)((value >> 24) & 0xFF)
		};
	}

	public override int ToInt32()
	{
		return Value;
	}

	public static implicit operator int(NavInteger value)
	{
		return value?.value ?? 0;
	}

	public static implicit operator long(NavInteger value)
	{
		if (value == null)
		{
			return 0L;
		}
		return value.value;
	}

	public static implicit operator Decimal18(NavInteger value)
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
		case NavNclType.NavOption:
			return Value == other.ToInt32();
		case NavNclType.NavBigInteger:
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

	public static bool operator ==(NavInteger right, int left)
	{
		if (right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavInteger right, int left)
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
			case NavNclType.NavOption:
				return Value.CompareTo(other.ToInt32());
			case NavNclType.NavBigInteger:
			case NavNclType.NavDuration:
				return ((long)Value).CompareTo(other.ToInt64());
			case NavNclType.NavDecimal:
				return ((Decimal18)Value).CompareTo(other.ToDecimal());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
