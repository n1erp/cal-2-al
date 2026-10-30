using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavOption : NavValue
{
	private class NavOptionNull : NavOption
	{
		public override bool IsNull => true;

		internal NavOptionNull(NCLOptionMetadata metadata)
			: base(metadata, 0)
		{
		}
	}

	private readonly NCLOptionMetadata metadata;

	private readonly int value;

	public static char[] OptionDelimiter = new char[1] { ',' };

	public int Value => value;

	public override NavNclType NclType => NavNclType.NavOption;

	public override object ValueAsObject => value;

	public override int GetBytesSize => 4;

	public override bool IsZeroOrEmpty => value == 0;

	public override NCLOptionMetadata NavOptionMetadata => metadata;

	private NavOption(NCLOptionMetadata metadata, int value)
	{
		this.metadata = metadata;
		this.value = value;
	}

	public static NavOption Create(NCLOptionMetadata metadata, int value)
	{
		return metadata.CreateNavOption(value);
	}

	internal static NavOption CreateBypassCache(NCLOptionMetadata metadata, int value)
	{
		return new NavOption(metadata, value);
	}

	public static NavOption CreateFromBytes(NCLOptionMetadata metadata, byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 4)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Option);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Option);
		}
		return metadata.CreateNavOption(BitConverter.ToInt32(data, startIndex));
	}

	public static NavOption CreateFromObject(NCLOptionMetadata metadata, object value)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		if (value == null)
		{
			return metadata.CreateNavOption(0);
		}
		if (value is NavValue navValue)
		{
			return metadata.CreateNavOption(navValue.ToInt32());
		}
		if (value is IConvertible convertible)
		{
			return metadata.CreateNavOption(convertible.ToInt32(CultureInfo.CurrentCulture));
		}
		if (value is byte[] data)
		{
			return CreateFromBytes(metadata, data, 0, 4);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavOption), value);
	}

	public NavOption Default()
	{
		return new NavOption(NavOptionMetadata, 0);
	}

	public static NavOption Null(NCLOptionMetadata metadata)
	{
		return new NavOptionNull(metadata);
	}

	public NavOption CreateInstance(int initValue)
	{
		if (initValue == value)
		{
			return this;
		}
		return NavOptionMetadata.CreateNavOption(initValue);
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

	public override void StoreToBytes(BinaryWriter writer)
	{
		if (writer != null)
		{
			writer.Write((int)NavType);
			writer.Write(4);
			writer.Write(value);
		}
	}

	public override int ToInt32()
	{
		return value;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public static implicit operator int(NavOption value)
	{
		return value?.value ?? 0;
	}

	public static implicit operator long(NavOption value)
	{
		if ((object)value == null)
		{
			return 0L;
		}
		return value.value;
	}

	public static implicit operator Decimal18(NavOption value)
	{
		if ((object)value == null)
		{
			return 0;
		}
		return value.value;
	}

	public static NavOption Add(NavOption right, NavOption left)
	{
		return new NavOption(right.NavOptionMetadata, right.value + left.value);
	}

	public static NavOption operator +(NavOption right, NavOption left)
	{
		return Add(right, left);
	}

	public static NavOption Subtract(NavOption right, NavOption left)
	{
		return new NavOption(right.NavOptionMetadata, right.value - left.value);
	}

	public static NavOption operator -(NavOption right, NavOption left)
	{
		return Subtract(right, left);
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
		case NavType.Char:
		case NavType.Integer:
		case NavType.Byte:
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

	public static bool operator ==(NavOption right, int left)
	{
		if ((object)right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavOption right, int left)
	{
		return !(right == left);
	}

	public static bool operator ==(NavOption right, NavOption left)
	{
		if ((object)right == null)
		{
			return (object)left == null;
		}
		if ((object)left != null)
		{
			return right.Value == left.Value;
		}
		return false;
	}

	public static bool operator !=(NavOption right, NavOption left)
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
				return Value.CompareTo(other.ToInt32());
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
