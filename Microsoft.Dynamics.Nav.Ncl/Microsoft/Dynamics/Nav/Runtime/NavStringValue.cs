using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavStringValue : NavValue
{
	public const int MaxDefinedLength = 2048;

	public abstract string Value { get; }

	public abstract int MaxLength { get; }

	public byte this[int index] => ALSystemString.GetCharAsByte(Value, index);

	public override bool IsZeroOrEmpty => string.IsNullOrEmpty(Value);

	public static implicit operator string(NavStringValue value)
	{
		return value?.Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override string ToString()
	{
		return Value;
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		return ValueAsObject;
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		if (Value.Length > maxLength)
		{
			throw new NavCSideException(22928068);
		}
		return ValueAsObject;
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
		if (obj is string text)
		{
			return Value == text;
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
		if (other is NavStringValue)
		{
			return Value == other.ToString();
		}
		return ThrowUnableToCompareException(other);
	}

	public bool Equals(string other)
	{
		return string.Equals(Value, other);
	}

	public static bool operator ==(NavStringValue left, string right)
	{
		return left?.Equals(right) ?? (right == null);
	}

	public static bool operator !=(NavStringValue left, string right)
	{
		return !(left == right);
	}

	public override int CompareTo(NavValue other)
	{
		if (other is NavStringValue other2)
		{
			return CompareTo(other2);
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	public int CompareTo(string other)
	{
		if (other == null)
		{
			return -1;
		}
		return string.Compare(Value, other, ignoreCase: false, NavCurrentThread.Session.Culture);
	}

	public virtual int CompareTo(NavStringValue other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		if ((object)other == this)
		{
			return 0;
		}
		return string.Compare(Value, other.Value, ignoreCase: false, NavCurrentThread.Session.Culture);
	}

	public static int Compare(NavStringValue left, NavStringValue right)
	{
		if ((object)right == null)
		{
			if ((object)left != null)
			{
				return 1;
			}
			return 0;
		}
		if ((object)left == null)
		{
			return -1;
		}
		if ((object)left == right)
		{
			return 0;
		}
		return left.CompareTo(right);
	}

	public static int Compare(NavStringValue left, string right)
	{
		if (right == null)
		{
			if ((object)left != null)
			{
				return 1;
			}
			return 0;
		}
		return left?.CompareTo(right) ?? (-1);
	}

	public static bool operator <(NavStringValue left, NavStringValue right)
	{
		return Compare(left, right) < 0;
	}

	public static bool operator <(NavStringValue left, string right)
	{
		return Compare(left, right) < 0;
	}

	public static bool operator <(string left, NavStringValue right)
	{
		return Compare(right, left) > 0;
	}

	public static bool operator <=(NavStringValue left, NavStringValue right)
	{
		return Compare(left, right) <= 0;
	}

	public static bool operator <=(NavStringValue left, string right)
	{
		return Compare(left, right) <= 0;
	}

	public static bool operator <=(string left, NavStringValue right)
	{
		return Compare(right, left) >= 0;
	}

	public static bool operator >=(NavStringValue left, NavStringValue right)
	{
		return Compare(left, right) >= 0;
	}

	public static bool operator >=(NavStringValue left, string right)
	{
		return Compare(left, right) >= 0;
	}

	public static bool operator >=(string left, NavStringValue right)
	{
		return Compare(right, left) <= 0;
	}

	public static bool operator >(NavStringValue left, NavStringValue right)
	{
		return Compare(left, right) > 0;
	}

	public static bool operator >(NavStringValue left, string right)
	{
		return Compare(left, right) > 0;
	}

	public static bool operator >(string left, NavStringValue right)
	{
		return Compare(right, left) < 0;
	}

	protected static T GetDefaultForMaxLength<T>(T[] cachedValues, int maxLength, Func<int, T> createDefault) where T : class
	{
		if (maxLength >= cachedValues.Length)
		{
			return createDefault(maxLength);
		}
		T val = cachedValues[maxLength];
		if (val == null)
		{
			val = (cachedValues[maxLength] = createDefault(maxLength));
		}
		return val;
	}

	protected static string Truncate(string value, int maxLength)
	{
		if (value != null && value.Length > maxLength)
		{
			value = value.Substring(0, maxLength);
		}
		return value;
	}

	protected static string GetStringFromBytes(byte[] data, int startIndex, int length)
	{
		int i;
		for (i = startIndex; i < startIndex + length && (data[i] != 0 || data[i + 1] != 0); i += 2)
		{
		}
		length = i - startIndex;
		return Encoding.Unicode.GetString(data, startIndex, length);
	}
}
