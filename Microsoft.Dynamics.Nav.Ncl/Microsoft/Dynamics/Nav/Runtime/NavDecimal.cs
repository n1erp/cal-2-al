using System;
using System.Data.SqlTypes;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDecimal : NavValue, IEquatable<byte>, IEquatable<int>, IEquatable<long>, IEquatable<Decimal18>
{
	private class NavDecimalNull : NavDecimal
	{
		public override bool IsNull => true;

		internal NavDecimalNull()
			: base(Decimal18.Zero)
		{
		}
	}

	private static readonly NavDecimal maxValue = new NavDecimal(NavDecimalHelper.MaxBcdReal);

	private static readonly NavDecimal minValue = new NavDecimal(NavDecimalHelper.MinBcdReal);

	private static readonly NavDecimal defaultValue = new NavDecimal(Decimal18.Zero);

	private static readonly NavDecimal one = new NavDecimal(Decimal18.One);

	private static readonly NavDecimal minusOne = new NavDecimal(Decimal18.MinusOne);

	private readonly Decimal18 value;

	private object sqlValue;

	private static readonly NavDecimal nullValue = new NavDecimalNull();

	public override NavNclType NclType => NavNclType.NavDecimal;

	public override int GetBytesSize => 10;

	public override object ValueAsObject => Value;

	public override object ClientObject => (decimal)Value;

	public override bool IsZeroOrEmpty => value == 0;

	public static NavDecimal Default => defaultValue;

	public static NavDecimal Null => nullValue;

	public Decimal18 Value => value;

	public static NavDecimal MaxValue => maxValue;

	public static NavDecimal MinValue => minValue;

	private NavDecimal(Decimal18 value)
	{
		this.value = value;
	}

	private NavDecimal(Decimal18 value, object sqlDecimal)
		: this(value)
	{
		sqlValue = sqlDecimal;
	}

	public static NavDecimal Create(Decimal18 value, object sqlDecimal)
	{
		int num = Decimal18.Compare(value, Decimal18.Zero);
		if (num > 0)
		{
			if (Decimal18.Equals(value, Decimal18.One))
			{
				return one;
			}
			return new NavDecimal(value, sqlDecimal);
		}
		if (num < 0)
		{
			if (Decimal18.Equals(value, Decimal18.MinusOne))
			{
				return minusOne;
			}
			return new NavDecimal(value, sqlDecimal);
		}
		return defaultValue;
	}

	public static NavDecimal Create(Decimal18 value)
	{
		return Create(value, null);
	}

	internal static NavDecimal Create(double value)
	{
		return Create((Decimal18)value);
	}

	internal static NavDecimal Create(int value)
	{
		return Create((Decimal18)value);
	}

	public static NavDecimal Create(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 12)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.Decimal);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.Decimal);
		}
		return Create(NavDecimalHelper.NavDecimalToDecimal(data, startIndex));
	}

	internal static NavDecimal Create(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length != 12)
		{
			throw new ArgumentException("NavDecimalToDecimal");
		}
		return Create(NavDecimalHelper.NavDecimalToDecimal(data));
	}

	public static NavDecimal CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavValue navValue)
		{
			return Create(navValue.ToDecimal());
		}
		if (value is IConvertible convertible)
		{
			return Create(convertible.ToDecimal(CultureInfo.CurrentCulture));
		}
		if (value is byte[] data)
		{
			return Create(data);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavDecimal), value);
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		if (sqlValue == null)
		{
			Decimal18 @decimal = value;
			NavDecimalHelper.ValidateDecimalRange(@decimal);
			sqlValue = new SqlDecimal((decimal)@decimal);
		}
		return sqlValue;
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		return GetSqlWhereClauseValue(sqlDataType);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override int ToInt32()
	{
		return ALCompiler.ToInt32(Value);
	}

	public override long ToInt64()
	{
		return ALCompiler.ToInt64(Value);
	}

	public override Decimal18 ToDecimal()
	{
		return Value;
	}

	public override byte[] GetBytes()
	{
		return NavDecimalHelper.DecimalToNavDecimal(value);
	}

	public static implicit operator Decimal18(NavDecimal value)
	{
		return value?.value ?? ((Decimal18)0);
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
		case NavType.Decimal:
		case NavType.Char:
		case NavType.Integer:
		case NavType.Byte:
		case NavType.Option:
		case NavType.BigInteger:
		case NavType.Duration:
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

	public static bool operator ==(NavDecimal right, Decimal18 left)
	{
		if ((object)right == null)
		{
			return left == 0;
		}
		return right.Value == left;
	}

	public static bool operator !=(NavDecimal right, Decimal18 left)
	{
		return !(right == left);
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			switch (other.NavType)
			{
			case NavType.Decimal:
			case NavType.Char:
			case NavType.Integer:
			case NavType.Byte:
			case NavType.Option:
			case NavType.BigInteger:
			case NavType.Duration:
				return Value.CompareTo(other.ToDecimal());
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	private static int CompareNavDecimalWithDecimal(NavDecimal navDecimal, decimal dec)
	{
		if ((object)navDecimal == null)
		{
			return decimal.Compare(0m, dec);
		}
		return decimal.Compare((decimal)navDecimal.Value, dec);
	}

	public static bool operator <(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) < 0;
	}

	public static bool operator <(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) > 0;
	}

	public static bool operator <=(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) <= 0;
	}

	public static bool operator <=(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) >= 0;
	}

	public static bool operator ==(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) == 0;
	}

	public static bool operator ==(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) == 0;
	}

	public static bool operator !=(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) != 0;
	}

	public static bool operator !=(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) != 0;
	}

	public static bool operator >=(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) >= 0;
	}

	public static bool operator >=(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) <= 0;
	}

	public static bool operator >(NavDecimal navDecimal, decimal dec)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) > 0;
	}

	public static bool operator >(decimal dec, NavDecimal navDecimal)
	{
		return CompareNavDecimalWithDecimal(navDecimal, dec) < 0;
	}

	private static int CompareNavDecimalWithNavDecimal(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		if ((object)navDecimal1 == navDecimal2)
		{
			return 0;
		}
		if ((object)navDecimal1 == null)
		{
			return -navDecimal2.Value.CompareTo(Decimal18.Zero);
		}
		if ((object)navDecimal2 == null)
		{
			return navDecimal1.Value.CompareTo(Decimal18.Zero);
		}
		return navDecimal1.Value.CompareTo(navDecimal2.Value);
	}

	public static bool operator <(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) < 0;
	}

	public static bool operator <=(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) <= 0;
	}

	public static bool operator ==(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) == 0;
	}

	public static bool operator !=(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) != 0;
	}

	public static bool operator >=(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) >= 0;
	}

	public static bool operator >(NavDecimal navDecimal1, NavDecimal navDecimal2)
	{
		return CompareNavDecimalWithNavDecimal(navDecimal1, navDecimal2) > 0;
	}
}
