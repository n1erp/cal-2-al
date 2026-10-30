using System;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavBoolean : NavValue, IEquatable<bool>
{
	private class NavBooleanNull : NavBoolean
	{
		public override bool IsNull => true;

		internal NavBooleanNull()
			: base(initValue: false)
		{
		}
	}

	private static readonly NavBoolean trueValue = new NavBoolean(initValue: true);

	private static readonly NavBoolean falseValue = new NavBoolean(initValue: false);

	private static readonly NavBoolean defaultValue = falseValue;

	private static readonly NavBoolean nullValue = new NavBooleanNull();

	private readonly bool value;

	public override NavNclType NclType => NavNclType.NavBoolean;

	public override int GetBytesSize => 1;

	public override object ValueAsObject => Value;

	public override bool IsZeroOrEmpty => !value;

	public static NavBoolean Default => defaultValue;

	public static NavBoolean Null => nullValue;

	public static NavBoolean True => trueValue;

	public static NavBoolean False => falseValue;

	public bool Value => value;

	private NavBoolean(bool initValue)
	{
		value = initValue;
	}

	public static NavBoolean Create(bool value)
	{
		if (!value)
		{
			return falseValue;
		}
		return trueValue;
	}

	public static NavBoolean Create(byte[] value)
	{
		return Create(BitConverter.ToBoolean(value, 0));
	}

	internal static NavBoolean CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 1 && length != 4)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Boolean);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Boolean);
		}
		return Create(BitConverter.ToBoolean(data, startIndex));
	}

	public static NavBoolean CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return Create(navValue.ToBoolean());
		}
		if (value is bool)
		{
			return Create((bool)value);
		}
		if (value is string)
		{
			bool result = false;
			if (bool.TryParse(value.ToString(), out result))
			{
				return Create(result);
			}
		}
		if (value is byte[] array)
		{
			return Create(array);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavBoolean), value);
	}

	public override int GetHashCode()
	{
		return ToInt32();
	}

	public override byte[] GetBytes()
	{
		return new byte[1] { ToByte() };
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
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	public override int ToInt32()
	{
		return ToByte();
	}

	public override bool ToBoolean()
	{
		return value;
	}

	public static implicit operator bool(NavBoolean value)
	{
		return value?.value ?? false;
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
		if (obj is bool)
		{
			return Value == (bool)obj;
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
		NavType navType = other.NavType;
		if (navType == NavType.Boolean)
		{
			return Value == other.ToBoolean();
		}
		return ThrowUnableToCompareException(other);
	}

	public bool Equals(bool other)
	{
		return Value == other;
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.Boolean)
			{
				return Value.CompareTo(other.ToBoolean());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
