using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

public struct CompanyId : IConvertible, IComparable<CompanyId>, IEquatable<CompanyId>
{
	public int Value { get; }

	public CompanyId(int value)
	{
		Value = value;
	}

	public bool Equals(CompanyId other)
	{
		return Value.Equals(other.Value);
	}

	public override string ToString()
	{
		return Value.ToString(CultureInfo.InvariantCulture);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CompanyId companyId))
		{
			return false;
		}
		return Value.Equals(companyId.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(CompanyId company1, CompanyId company2)
	{
		return company1.Value == company2.Value;
	}

	public static bool operator !=(CompanyId company1, CompanyId company2)
	{
		return company1.Value != company2.Value;
	}

	public static bool operator <(CompanyId company1, CompanyId company2)
	{
		return company1.Value < company2.Value;
	}

	public static bool operator >(CompanyId company1, CompanyId company2)
	{
		return company1.Value > company2.Value;
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Int32;
	}

	public bool ToBoolean(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public char ToChar(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public sbyte ToSByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public byte ToByte(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public short ToInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public ushort ToUInt16(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public int ToInt32(IFormatProvider provider)
	{
		return Value;
	}

	public uint ToUInt32(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public long ToInt64(IFormatProvider provider)
	{
		return Value;
	}

	public ulong ToUInt64(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public float ToSingle(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public double ToDouble(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public decimal ToDecimal(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public DateTime ToDateTime(IFormatProvider provider)
	{
		throw new InvalidCastException();
	}

	public object ToType(Type conversionType, IFormatProvider provider)
	{
		if (conversionType == typeof(int))
		{
			return Value;
		}
		if (conversionType == typeof(long))
		{
			return (long)Value;
		}
		throw new InvalidCastException();
	}

	public string ToString(IFormatProvider provider)
	{
		return Value.ToString(provider);
	}

	public int CompareTo(CompanyId other)
	{
		return Value.CompareTo(other.Value);
	}
}
