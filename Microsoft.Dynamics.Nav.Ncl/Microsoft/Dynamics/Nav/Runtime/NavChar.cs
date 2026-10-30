using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavChar : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private static readonly NavChar defaultValue = new NavChar('\0');

	private readonly char value;

	public static NavChar Default => defaultValue;

	public char Value => value;

	public override NavNclType NclType => NavNclType.NavChar;

	public override int GetBytesSize => 2;

	public override object ValueAsObject => value;

	public override bool IsZeroOrEmpty => value == '\0';

	private NavChar(char value)
	{
		this.value = value;
	}

	public static NavChar Create(int initValue)
	{
		if (initValue != 0)
		{
			return new NavChar(ALCompiler.ToChar(initValue));
		}
		return defaultValue;
	}

	public static NavChar Create(byte[] initValue)
	{
		if (initValue == null)
		{
			throw new ArgumentNullException("initValue");
		}
		if (initValue.Length != 2)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The initialization value was either too large or too small for a {0}.", "NavChar"));
		}
		return Create((initValue[1] << 8) + initValue[0]);
	}

	private NavChar(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 2)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Char);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Char);
		}
		value = ALCompiler.ToChar((data[startIndex + 1] << 8) + data[startIndex]);
	}

	public static NavChar CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavChar(data, startIndex, length);
	}

	public static NavChar CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is char)
		{
			return Create((char)value);
		}
		if (value is string { Length: 1 } text)
		{
			return Create(text[0]);
		}
		if (value is byte[] initValue)
		{
			return Create(initValue);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavChar), value);
	}

	public override int GetHashCode()
	{
		return value;
	}

	public override byte[] GetBytes()
	{
		return new byte[2]
		{
			(byte)(value & 0xFF),
			(byte)(((int)value >> 8) & 0xFF)
		};
	}

	public override void StoreToBytes(BinaryWriter writer)
	{
		if (writer != null)
		{
			writer.Write((int)NavType);
			writer.Write(2);
			writer.Write(GetBytes());
		}
	}

	public override int ToInt32()
	{
		return value;
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public static implicit operator char(NavChar value)
	{
		return value?.value ?? '\0';
	}

	public static implicit operator int(NavChar value)
	{
		return value?.value ?? '\0';
	}

	public static implicit operator long(NavChar value)
	{
		if (value == null)
		{
			return 0L;
		}
		return value.value;
	}

	public static implicit operator Decimal18(NavChar value)
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
		if (obj is char)
		{
			return Value == (char)obj;
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
		case NavType.Char:
			return Value == other.ToInt32();
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

	public bool Equals(char other)
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

	public static bool operator ==(NavChar right, int left)
	{
		if (right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavChar right, int left)
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
			case NavType.Char:
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
