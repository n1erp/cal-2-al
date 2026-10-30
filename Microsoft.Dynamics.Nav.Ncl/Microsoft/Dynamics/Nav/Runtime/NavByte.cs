using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavByte : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private static readonly NavByte defaultValue = new NavByte(0);

	private readonly byte value;

	public static NavByte Default => defaultValue;

	public byte Value => value;

	public override NavNclType NclType => NavNclType.NavByte;

	public override int GetBytesSize => 1;

	public override object ValueAsObject => value;

	public override bool IsZeroOrEmpty => value == 0;

	private NavByte(byte value)
	{
		this.value = value;
	}

	public static NavByte Create(byte initValue)
	{
		if (initValue != 0)
		{
			return new NavByte(initValue);
		}
		return defaultValue;
	}

	public static NavByte Create(byte[] initValue)
	{
		if (initValue == null)
		{
			throw new ArgumentNullException("initValue");
		}
		if (initValue.Length != 1)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The initialization value was either too large or too small for a {0}.", "NavByte"));
		}
		return Create(initValue[0]);
	}

	private NavByte(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 1)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Byte);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Byte);
		}
		value = data[startIndex];
	}

	public static NavByte CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavByte(data, startIndex, length);
	}

	public static NavByte CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is byte)
		{
			return Create((byte)value);
		}
		if (value is string { Length: 1 } text)
		{
			return Create(NCLManagedAdapter.TextCharToByte(text[0]));
		}
		if (value is byte[] initValue)
		{
			return Create(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavByte), value);
	}

	public override int GetHashCode()
	{
		return value;
	}

	public override byte[] GetBytes()
	{
		return new byte[1] { value };
	}

	public override void StoreToBytes(BinaryWriter writer)
	{
		if (writer != null)
		{
			writer.Write((int)NavType);
			writer.Write(1);
			writer.Write(value);
		}
	}

	public override byte ToByte()
	{
		return value;
	}

	public override int ToInt32()
	{
		return value;
	}

	public static implicit operator byte(NavByte value)
	{
		return value?.value ?? 0;
	}

	public static implicit operator int(NavByte value)
	{
		return value?.value ?? 0;
	}

	public static implicit operator long(NavByte value)
	{
		if (value == null)
		{
			return 0L;
		}
		return value.value;
	}

	public static implicit operator Decimal18(NavByte value)
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
		switch (other.NavType)
		{
		case NavType.Byte:
			return Value == other.ToByte();
		case NavType.Integer:
		case NavType.Option:
			return Value == other.ToInt32();
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

	public static bool operator ==(NavByte right, int left)
	{
		if (right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavByte right, int left)
	{
		return !(right == left);
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			switch (other.NavType)
			{
			case NavType.Byte:
				return Value.CompareTo(other.ToByte());
			case NavType.Integer:
			case NavType.Option:
				return ((int)Value).CompareTo(other.ToInt32());
			case NavType.BigInteger:
			case NavType.Duration:
				return ((long)Value).CompareTo(other.ToInt64());
			case NavType.Decimal:
				return ((Decimal18)Value).CompareTo(other.ToDecimal());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
