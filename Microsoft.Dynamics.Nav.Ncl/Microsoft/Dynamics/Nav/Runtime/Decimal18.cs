using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

public struct Decimal18 : IFormattable, IComparable, IComparable<Decimal18>, IEquatable<Decimal18>, IConvertible
{
	[StructLayout(LayoutKind.Explicit)]
	private struct DecimalUnion
	{
		[FieldOffset(0)]
		public decimal Decimal;

		[FieldOffset(0)]
		public Decimal18 Decimal18;

		[FieldOffset(0)]
		public uint Flags;

		[FieldOffset(2)]
		public byte Scale;

		[FieldOffset(3)]
		public sbyte Sign;

		[FieldOffset(4)]
		public uint High;

		[FieldOffset(8)]
		public ulong Medlo;
	}

	private static uint[] digitCountToValueHi = new uint[30]
	{
		0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
		0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
		5u, 54u, 542u, 5421u, 54210u, 542101u, 5421010u, 54210108u, 542101086u, 4294967295u
	};

	private static ulong[] digitCountToValueMedlo = new ulong[30]
	{
		0uL, 9uL, 99uL, 999uL, 9999uL, 99999uL, 999999uL, 9999999uL, 99999999uL, 999999999uL,
		9999999999uL, 99999999999uL, 999999999999uL, 9999999999999uL, 99999999999999uL, 999999999999999uL, 9999999999999999uL, 99999999999999999uL, 999999999999999999uL, 9999999999999999999uL,
		7766279631452241919uL, 3875820019684212735uL, 1864712049423024127uL, 200376420520689663uL, 2003764205206896639uL, 1590897978359414783uL, 15908979783594147839uL, 11515845246265065471uL, 4477988020393345023uL, 18446744073709551615uL
	};

	private static uint[] power10 = new uint[10] { 1u, 10u, 100u, 1000u, 10000u, 100000u, 1000000u, 10000000u, 100000000u, 1000000000u };

	public const int Precision = 18;

	public const MidpointRounding PrecisionLossRounding = MidpointRounding.AwayFromZero;

	public static readonly Decimal18 MinValue = -79228162514264337500000000000m;

	public static readonly Decimal18 MaxValue = 79228162514264337500000000000m;

	public static readonly Decimal18 Zero = 0m;

	public static readonly Decimal18 One = 1m;

	public static readonly Decimal18 MinusOne = -1m;

	private decimal value;

	public Decimal18(int value)
	{
		this.value = value;
	}

	public Decimal18(long value)
	{
		this.value = RoundToPrecision(value);
	}

	public Decimal18(uint value)
	{
		this.value = value;
	}

	public Decimal18(ulong value)
	{
		this.value = RoundToPrecision(value);
	}

	public Decimal18(float value)
	{
		this.value = RoundToPrecision((decimal)value);
	}

	public Decimal18(double value)
	{
		this.value = RoundToPrecision((decimal)value);
	}

	public Decimal18(decimal value)
	{
		this.value = RoundToPrecision(value);
	}

	internal Decimal18(decimal value, bool truncate)
	{
		this.value = RoundToPrecision(value, truncate);
	}

	public static Decimal18 Add(Decimal18 left, Decimal18 right)
	{
		return new Decimal18(decimal.Add(left.value, right.value));
	}

	public static Decimal18 Increment(Decimal18 value)
	{
		return new Decimal18(decimal.Add(value.value, 1m));
	}

	public static Decimal18 Subtract(Decimal18 left, Decimal18 right)
	{
		return new Decimal18(decimal.Subtract(left.value, right.value));
	}

	public static Decimal18 Decrement(Decimal18 value)
	{
		return new Decimal18(decimal.Subtract(value.value, 1m));
	}

	public static Decimal18 Multiply(Decimal18 left, Decimal18 right)
	{
		return new Decimal18(decimal.Multiply(left.value, right.value));
	}

	public static Decimal18 Divide(Decimal18 left, Decimal18 right)
	{
		return new Decimal18(decimal.Divide(left.value, right.value));
	}

	public static Decimal18 Modulus(Decimal18 left, Decimal18 right)
	{
		return new Decimal18(decimal.Remainder(left.value, right.value));
	}

	public static Decimal18 Ceiling(Decimal18 value)
	{
		return new Decimal18(decimal.Ceiling(value.value));
	}

	public static Decimal18 Floor(Decimal18 value)
	{
		return new Decimal18(decimal.Floor(value.value));
	}

	public static Decimal18 Plus(Decimal18 value)
	{
		return value;
	}

	public static Decimal18 Negate(Decimal18 value)
	{
		return new Decimal18(decimal.Negate(value.value));
	}

	public static Decimal18 Round(Decimal18 value)
	{
		return new Decimal18(decimal.Round(value.value, 0, MidpointRounding.AwayFromZero));
	}

	public static Decimal18 Round(Decimal18 value, int decimals)
	{
		return new Decimal18(decimal.Round(value.value, decimals, MidpointRounding.AwayFromZero));
	}

	public static Decimal18 Round(Decimal18 value, MidpointRounding mode)
	{
		return new Decimal18(decimal.Round(value.value, mode));
	}

	public static Decimal18 Round(Decimal18 value, int decimals, MidpointRounding mode)
	{
		return new Decimal18(decimal.Round(value.value, decimals, mode));
	}

	public static Decimal18 Truncate(Decimal18 value)
	{
		return new Decimal18(decimal.Truncate(value.value));
	}

	public static Decimal18 Abs(Decimal18 value)
	{
		DecimalUnion decimalUnion = new DecimalUnion
		{
			Decimal18 = value
		};
		if (decimalUnion.Sign < 0)
		{
			decimalUnion.Sign = 0;
		}
		return decimalUnion.Decimal18;
	}

	public static Decimal18 operator +(Decimal18 left, Decimal18 right)
	{
		return Add(left, right);
	}

	public static Decimal18 operator -(Decimal18 left, Decimal18 right)
	{
		return Subtract(left, right);
	}

	public static Decimal18 operator *(Decimal18 left, Decimal18 right)
	{
		return Multiply(left, right);
	}

	public static Decimal18 operator /(Decimal18 left, Decimal18 right)
	{
		return Divide(left, right);
	}

	public static Decimal18 operator %(Decimal18 left, Decimal18 right)
	{
		return Modulus(left, right);
	}

	public static Decimal18 operator ++(Decimal18 value)
	{
		return Increment(value);
	}

	public static Decimal18 operator --(Decimal18 value)
	{
		return Decrement(value);
	}

	public static Decimal18 operator -(Decimal18 value)
	{
		return Negate(value);
	}

	public static Decimal18 operator +(Decimal18 value)
	{
		return Plus(value);
	}

	public static implicit operator Decimal18(sbyte value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(byte value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(char value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(short value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(ushort value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(int value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(uint value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(long value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(ulong value)
	{
		return new Decimal18(value);
	}

	public static implicit operator Decimal18(decimal value)
	{
		return new Decimal18(value);
	}

	public static explicit operator Decimal18(float value)
	{
		return new Decimal18(value);
	}

	public static explicit operator Decimal18(double value)
	{
		return new Decimal18(value);
	}

	public static explicit operator sbyte(Decimal18 value)
	{
		return decimal.ToSByte(value.value);
	}

	public static explicit operator byte(Decimal18 value)
	{
		return decimal.ToByte(value.value);
	}

	public static explicit operator short(Decimal18 value)
	{
		return decimal.ToInt16(value.value);
	}

	public static explicit operator ushort(Decimal18 value)
	{
		return decimal.ToUInt16(value.value);
	}

	public static explicit operator int(Decimal18 value)
	{
		return decimal.ToInt32(value.value);
	}

	public static explicit operator uint(Decimal18 value)
	{
		return decimal.ToUInt32(value.value);
	}

	public static explicit operator long(Decimal18 value)
	{
		return decimal.ToInt64(value.value);
	}

	public static explicit operator ulong(Decimal18 value)
	{
		return decimal.ToUInt64(value.value);
	}

	public static explicit operator float(Decimal18 value)
	{
		return decimal.ToSingle(value.value);
	}

	public static explicit operator double(Decimal18 value)
	{
		return decimal.ToDouble(value.value);
	}

	public static explicit operator decimal(Decimal18 value)
	{
		return value.value;
	}

	public bool Equals(Decimal18 other)
	{
		return value.Equals(other.value);
	}

	public override bool Equals(object obj)
	{
		if (obj is Decimal18)
		{
			return decimal.Equals(value, ((Decimal18)obj).value);
		}
		return false;
	}

	public static bool Equals(Decimal18 left, Decimal18 right)
	{
		return decimal.Equals(left.value, right.value);
	}

	public static int Compare(Decimal18 left, Decimal18 right)
	{
		return decimal.Compare(left.value, right.value);
	}

	public int CompareTo(Decimal18 other)
	{
		return decimal.Compare(value, other.value);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (!(obj is Decimal18))
		{
			throw new ArgumentException(obj.GetType().Name);
		}
		return decimal.Compare(value, ((Decimal18)obj).value);
	}

	public static bool operator <(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) < 0;
	}

	public static bool operator <=(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) <= 0;
	}

	public static bool operator ==(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) == 0;
	}

	public static bool operator !=(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) != 0;
	}

	public static bool operator >=(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) >= 0;
	}

	public static bool operator >(Decimal18 left, Decimal18 right)
	{
		return Compare(left, right) > 0;
	}

	public static sbyte ToSByte(Decimal18 value)
	{
		return decimal.ToSByte(value.value);
	}

	public static byte ToByte(Decimal18 value)
	{
		return decimal.ToByte(value.value);
	}

	public static short ToInt16(Decimal18 value)
	{
		return decimal.ToInt16(value.value);
	}

	public static ushort ToUInt16(Decimal18 value)
	{
		return decimal.ToUInt16(value.value);
	}

	public static int ToInt32(Decimal18 value)
	{
		return decimal.ToInt32(value.value);
	}

	public static uint ToUInt32(Decimal18 value)
	{
		return decimal.ToUInt32(value.value);
	}

	public static long ToInt64(Decimal18 value)
	{
		return decimal.ToInt64(value.value);
	}

	public static ulong ToUInt64(Decimal18 value)
	{
		return decimal.ToUInt64(value.value);
	}

	public static float ToSingle(Decimal18 value)
	{
		return decimal.ToSingle(value.value);
	}

	public static double ToDouble(Decimal18 value)
	{
		return decimal.ToDouble(value.value);
	}

	public static decimal ToDecimal(Decimal18 value)
	{
		return value.value;
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Object;
	}

	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return ((IConvertible)value).ToBoolean(provider);
	}

	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return ((IConvertible)value).ToSByte(provider);
	}

	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return ((IConvertible)value).ToByte(provider);
	}

	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return ((IConvertible)value).ToInt16(provider);
	}

	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return ((IConvertible)value).ToUInt16(provider);
	}

	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return ((IConvertible)value).ToInt32(provider);
	}

	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return ((IConvertible)value).ToUInt32(provider);
	}

	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return ((IConvertible)value).ToInt64(provider);
	}

	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return ((IConvertible)value).ToUInt64(provider);
	}

	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return ((IConvertible)value).ToSingle(provider);
	}

	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return ((IConvertible)value).ToDouble(provider);
	}

	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return value;
	}

	string IConvertible.ToString(IFormatProvider provider)
	{
		return ((IConvertible)value).ToString(provider);
	}

	char IConvertible.ToChar(IFormatProvider provider)
	{
		return ((IConvertible)value).ToChar(provider);
	}

	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return ((IConvertible)value).ToDateTime(provider);
	}

	object IConvertible.ToType(Type conversionType, IFormatProvider provider)
	{
		return ((IConvertible)value).ToType(conversionType, provider);
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return value.ToString(formatProvider);
	}

	public string ToString(string format)
	{
		return value.ToString(format);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return value.ToString(format, formatProvider);
	}

	public static Decimal18 Parse(string value)
	{
		return new Decimal18(decimal.Parse(value));
	}

	public static Decimal18 Parse(string value, NumberStyles style)
	{
		return new Decimal18(decimal.Parse(value, style));
	}

	public static Decimal18 Parse(string value, IFormatProvider formatProvider)
	{
		return new Decimal18(decimal.Parse(value, formatProvider));
	}

	public static Decimal18 Parse(string value, NumberStyles style, IFormatProvider formatProvider)
	{
		return new Decimal18(decimal.Parse(value, style, formatProvider));
	}

	public static bool TryParse(string value, out Decimal18 result)
	{
		return TryParse(value, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result);
	}

	public static bool TryParse(string value, NumberStyles style, IFormatProvider formatProvider, out Decimal18 result)
	{
		if (decimal.TryParse(value, style, formatProvider, out var result2))
		{
			result = new Decimal18(result2, truncate: true);
			return true;
		}
		result = Zero;
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public static byte GetScale(decimal value)
	{
		DecimalUnion decimalUnion = new DecimalUnion
		{
			Decimal = value
		};
		return decimalUnion.Scale;
	}

	private static decimal RoundToPrecision(decimal d)
	{
		return RoundToPrecision(d, truncate: false);
	}

	private static decimal RoundToPrecision(decimal d, bool truncate)
	{
		DecimalUnion decimalUnion = new DecimalUnion
		{
			Decimal = d
		};
		if (decimalUnion.High == 0 && decimalUnion.Medlo <= digitCountToValueMedlo[18])
		{
			return decimalUnion.Decimal;
		}
		uint num;
		for (num = 19u; decimalUnion.High > digitCountToValueHi[num] || (decimalUnion.High == digitCountToValueHi[num] && decimalUnion.Medlo > digitCountToValueMedlo[num]); num++)
		{
		}
		uint flags = decimalUnion.Flags;
		uint num2 = num - 18;
		decimalUnion.Scale = (byte)num2;
		if (truncate)
		{
			decimalUnion.Decimal = decimal.Truncate(decimalUnion.Decimal);
		}
		else
		{
			decimalUnion.Decimal = decimal.Round(decimalUnion.Decimal, 0, MidpointRounding.AwayFromZero);
		}
		decimalUnion.Flags = flags;
		if (decimalUnion.Scale >= num2)
		{
			decimalUnion.Scale -= (byte)num2;
			while (decimalUnion.Scale > 0 && decimalUnion.Medlo % 10 == 0L)
			{
				decimalUnion.Medlo /= 10uL;
				decimalUnion.Scale--;
			}
			return decimalUnion.Decimal;
		}
		if (decimalUnion.Scale > 0)
		{
			num2 -= decimalUnion.Scale;
			decimalUnion.Scale = 0;
		}
		while (num2 != 0)
		{
			uint num3 = ((num2 > 9) ? 9u : num2);
			num2 -= num3;
			try
			{
				decimalUnion.Decimal *= (decimal)power10[num3];
			}
			catch (OverflowException)
			{
				return (decimalUnion.Sign < 0) ? MinValue.value : MaxValue.value;
			}
		}
		return decimalUnion.Decimal;
	}
}
