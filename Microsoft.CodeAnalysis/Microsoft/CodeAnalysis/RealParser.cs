using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.CodeAnalysis;

internal static class RealParser
{
	private abstract class FloatingPointType
	{
		public abstract ushort DenormalMantissaBits { get; }

		public ushort NormalMantissaBits => (ushort)(DenormalMantissaBits + 1);

		public abstract ushort ExponentBits { get; }

		public int MinBinaryExponent => 1 - MaxBinaryExponent;

		public abstract int MaxBinaryExponent { get; }

		public int OverflowDecimalExponent => (MaxBinaryExponent + 2 * NormalMantissaBits) / 3;

		public abstract int ExponentBias { get; }

		public ulong DenormalMantissaMask => (ulong)((1L << (int)DenormalMantissaBits) - 1);

		public ulong NormalMantissaMask => (ulong)((1L << (int)NormalMantissaBits) - 1);

		public abstract ulong Zero { get; }

		public abstract ulong Infinity { get; }

		public Status AssembleFloatingPointValue(ulong initialMantissa, int initialExponent, bool hasZeroTail, out ulong result)
		{
			uint num = CountSignificantBits(initialMantissa);
			int num2 = (int)(NormalMantissaBits - num);
			int num3 = initialExponent - num2;
			ulong num4 = initialMantissa;
			int num5 = num3;
			if (num3 > MaxBinaryExponent)
			{
				result = Infinity;
				return Status.Overflow;
			}
			if (num3 < MinBinaryExponent)
			{
				int num6 = num2 + num3 + ExponentBias - 1;
				num5 = -ExponentBias;
				if (num6 < 0)
				{
					num4 = RightShiftWithRounding(num4, -num6, hasZeroTail);
					if (num4 == 0L)
					{
						result = Zero;
						return Status.Undeflow;
					}
					if (num4 > DenormalMantissaMask)
					{
						num5 = initialExponent - (num6 + 1) - num2;
					}
				}
				else
				{
					num4 <<= num6;
				}
			}
			else if (num2 < 0)
			{
				num4 = RightShiftWithRounding(num4, -num2, hasZeroTail);
				if (num4 > NormalMantissaMask)
				{
					num4 >>= 1;
					num5++;
					if (num5 > MaxBinaryExponent)
					{
						result = Infinity;
						return Status.Overflow;
					}
				}
			}
			else if (num2 > 0)
			{
				num4 <<= num2;
			}
			num4 &= DenormalMantissaMask;
			ulong num7 = (ulong)((long)(num5 + ExponentBias) << (int)DenormalMantissaBits);
			result = num7 | num4;
			return Status.OK;
		}
	}

	private sealed class FloatFloatingPointType : FloatingPointType
	{
		public static FloatFloatingPointType Instance = new FloatFloatingPointType();

		public override ushort DenormalMantissaBits => 23;

		public override ushort ExponentBits => 8;

		public override int MaxBinaryExponent => 127;

		public override int ExponentBias => 127;

		public override ulong Zero => FloatToInt32Bits(0f);

		public override ulong Infinity => FloatToInt32Bits(float.PositiveInfinity);

		private FloatFloatingPointType()
		{
		}
	}

	private sealed class DoubleFloatingPointType : FloatingPointType
	{
		public static DoubleFloatingPointType Instance = new DoubleFloatingPointType();

		public override ushort DenormalMantissaBits => 52;

		public override ushort ExponentBits => 11;

		public override int MaxBinaryExponent => 1023;

		public override int ExponentBias => 1023;

		public override ulong Zero => (ulong)BitConverter.DoubleToInt64Bits(0.0);

		public override ulong Infinity => (ulong)BitConverter.DoubleToInt64Bits(double.PositiveInfinity);

		private DoubleFloatingPointType()
		{
		}
	}

	[DebuggerDisplay("0.{Mantissa}e{Exponent}")]
	private struct DecimalFloatingPointString
	{
		public int Exponent;

		public string Mantissa;

		public uint MantissaCount => (uint)Mantissa.Length;

		public static DecimalFloatingPointString FromSource(string source)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			int i;
			for (i = 0; i < source.Length && source[i] == '0'; i++)
			{
			}
			int num2 = 0;
			for (; i < source.Length && source[i] >= '0' && source[i] <= '9'; i++)
			{
				if (source[i] == '0')
				{
					num2++;
				}
				else
				{
					stringBuilder.Append('0', num2);
					num2 = 0;
					stringBuilder.Append(source[i]);
				}
				num++;
			}
			if (i < source.Length && source[i] == '.')
			{
				for (i++; i < source.Length && source[i] >= '0' && source[i] <= '9'; i++)
				{
					if (source[i] == '0')
					{
						num2++;
						continue;
					}
					stringBuilder.Append('0', num2);
					num2 = 0;
					stringBuilder.Append(source[i]);
				}
			}
			DecimalFloatingPointString result = new DecimalFloatingPointString
			{
				Mantissa = stringBuilder.ToString()
			};
			if (i < source.Length && (source[i] == 'e' || source[i] == 'E'))
			{
				char c = '\0';
				i++;
				if (i < source.Length && (source[i] == '-' || source[i] == '+'))
				{
					c = source[i];
					i++;
				}
				int num3 = i;
				int num4 = i;
				while (i < source.Length && source[i] >= '0' && source[i] <= '9')
				{
					num4 = ++i;
				}
				int result2 = 0;
				if (!int.TryParse(source.Substring(num3, num4 - num3), out result2) || result2 > 1073741824)
				{
					num = ((c == '-') ? (-1073741824) : 1073741824);
				}
				else
				{
					num = ((c != '-') ? (num + result2) : (num - result2));
				}
			}
			result.Exponent = num;
			return result;
		}
	}

	private enum Status
	{
		OK,
		NoDigits,
		Undeflow,
		Overflow
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct FloatUnion
	{
		[FieldOffset(0)]
		public uint IntData;

		[FieldOffset(0)]
		public float FloatData;
	}

	private static readonly BigInteger s_bigZero = BigInteger.Zero;

	private static readonly BigInteger s_bigOne = BigInteger.One;

	private static readonly BigInteger s_bigTwo = new BigInteger(2);

	private static readonly BigInteger s_bigTen = new BigInteger(10);

	public static bool TryParseDouble(string s, out double d)
	{
		DecimalFloatingPointString data = DecimalFloatingPointString.FromSource(s);
		DoubleFloatingPointType instance = DoubleFloatingPointType.Instance;
		Status status = ConvertDecimalToFloatingPointBits(data, instance, out var result);
		d = BitConverter.Int64BitsToDouble((long)result);
		return status != Status.Overflow;
	}

	public static bool TryParseFloat(string s, out float f)
	{
		DecimalFloatingPointString data = DecimalFloatingPointString.FromSource(s);
		FloatFloatingPointType instance = FloatFloatingPointType.Instance;
		Status status = ConvertDecimalToFloatingPointBits(data, instance, out var result);
		f = Int32BitsToFloat((uint)result);
		return status != Status.Overflow;
	}

	private static Status ConvertDecimalToFloatingPointBits(DecimalFloatingPointString data, FloatingPointType type, out ulong result)
	{
		if (data.Mantissa.Length == 0)
		{
			result = type.Zero;
			return Status.NoDigits;
		}
		uint num = (uint)(type.NormalMantissaBits + 1);
		int num2 = Math.Max(0, data.Exponent);
		uint num3 = Math.Min((uint)num2, data.MantissaCount);
		uint num4 = (uint)num2 - num3;
		uint integer_first_index = 0u;
		uint num5 = num3;
		uint num6 = num5;
		uint mantissaCount = data.MantissaCount;
		uint num7 = mantissaCount - num6;
		BigInteger number = AccumulateDecimalDigitsIntoBigInteger(data, integer_first_index, num5);
		if (num4 != 0)
		{
			if (num4 > type.OverflowDecimalExponent)
			{
				result = type.Infinity;
				return Status.Overflow;
			}
			MultiplyByPowerOfTen(ref number, num4);
		}
		uint num8 = CountSignificantBits(number, out var dataBytes);
		if (num8 >= num || num7 == 0)
		{
			return ConvertBigIntegerToFloatingPointBits(dataBytes, num8, num7 != 0, type, out result);
		}
		uint num9 = ((data.Exponent < 0) ? (num7 + (uint)(-data.Exponent)) : num7);
		if (num8 == 0 && num9 - (int)data.MantissaCount > type.OverflowDecimalExponent)
		{
			result = type.Zero;
			return Status.Undeflow;
		}
		BigInteger number2 = AccumulateDecimalDigitsIntoBigInteger(data, num6, mantissaCount);
		BigInteger number3 = s_bigOne;
		MultiplyByPowerOfTen(ref number3, num9);
		uint num10 = CountSignificantBits(number2);
		uint num11 = CountSignificantBits(number3);
		uint num12 = ((num11 > num10) ? (num11 - num10) : 0u);
		if (num12 != 0)
		{
			ShiftLeft(ref number2, num12);
		}
		uint num13 = num - num8;
		uint num14 = num13;
		if (num8 != 0)
		{
			if (num12 > num14)
			{
				return ConvertBigIntegerToFloatingPointBits(dataBytes, num8, num7 != 0, type, out result);
			}
			num14 -= num12;
		}
		uint num15 = ((number2 < number3) ? (num12 + 1) : num12);
		ShiftLeft(ref number2, num14);
		ulong num16 = (ulong)BigInteger.DivRem(number2, number3, out var remainder);
		bool flag = remainder.IsZero;
		uint num17 = CountSignificantBits(num16);
		if (num17 > num13)
		{
			int num18 = (int)(num17 - num13);
			flag = flag && (num16 & (ulong)((1L << num18) - 1)) == 0;
			num16 >>= num18;
		}
		ulong initialMantissa = ((ulong)number << (int)num13) + num16;
		int initialExponent = (int)((num8 != 0) ? (num8 - 2) : (0 - num15 - 1));
		return type.AssembleFloatingPointValue(initialMantissa, initialExponent, flag, out result);
	}

	private static Status ConvertBigIntegerToFloatingPointBits(byte[] integerValueAsBytes, uint integerBitsOfPrecision, bool hasNonzeroFractionalPart, FloatingPointType type, out ulong result)
	{
		ushort denormalMantissaBits = type.DenormalMantissaBits;
		bool flag = !hasNonzeroFractionalPart;
		int num = (int)(integerBitsOfPrecision - 1) / 8;
		int num2 = Math.Max(0, num - 8 + 1);
		int initialExponent = denormalMantissaBits + num2 * 8;
		ulong num3 = 0uL;
		for (int num4 = num; num4 >= num2; num4--)
		{
			num3 <<= 8;
			num3 |= integerValueAsBytes[num4];
		}
		int num5 = num2 - 1;
		while (flag && num5 >= 0)
		{
			if (integerValueAsBytes[num5] != 0)
			{
				flag = false;
			}
			num5--;
		}
		return type.AssembleFloatingPointValue(num3, initialExponent, flag, out result);
	}

	private static BigInteger AccumulateDecimalDigitsIntoBigInteger(DecimalFloatingPointString data, uint integer_first_index, uint integer_last_index)
	{
		if (integer_first_index == integer_last_index)
		{
			return s_bigZero;
		}
		return BigInteger.Parse(data.Mantissa.Substring((int)integer_first_index, (int)(integer_last_index - integer_first_index)));
	}

	private static uint CountSignificantBits(ulong data)
	{
		uint num = 0u;
		while (data != 0L)
		{
			data >>= 1;
			num++;
		}
		return num;
	}

	private static uint CountSignificantBits(byte data)
	{
		uint num = 0u;
		while (data != 0)
		{
			data >>= 1;
			num++;
		}
		return num;
	}

	private static uint CountSignificantBits(BigInteger data, out byte[] dataBytes)
	{
		if (data.IsZero)
		{
			dataBytes = new byte[1];
			return 0u;
		}
		dataBytes = data.ToByteArray();
		for (int num = dataBytes.Length - 1; num >= 0; num--)
		{
			byte b = dataBytes[num];
			if (b != 0)
			{
				return (uint)(8 * num) + CountSignificantBits(b);
			}
		}
		return 0u;
	}

	private static uint CountSignificantBits(BigInteger data)
	{
		byte[] dataBytes;
		return CountSignificantBits(data, out dataBytes);
	}

	private static ulong RightShiftWithRounding(ulong value, int shift, bool hasZeroTail)
	{
		if (shift >= 64)
		{
			return 0uL;
		}
		ulong num = (ulong)((1L << shift - 1) - 1);
		ulong num2 = (ulong)(1L << shift - 1);
		ulong num3 = (ulong)(1L << shift);
		bool lsbBit = (value & num3) != 0;
		bool roundBit = (value & num2) != 0;
		bool hasTailBits = !hasZeroTail || (value & num) != 0;
		return (value >> shift) + (ulong)(ShouldRoundUp(lsbBit, roundBit, hasTailBits) ? 1 : 0);
	}

	private static bool ShouldRoundUp(bool lsbBit, bool roundBit, bool hasTailBits)
	{
		if (roundBit)
		{
			return hasTailBits | lsbBit;
		}
		return false;
	}

	private static void ShiftLeft(ref BigInteger number, uint shift)
	{
		BigInteger bigInteger = BigInteger.Pow(s_bigTwo, (int)shift);
		number *= bigInteger;
	}

	private static void MultiplyByPowerOfTen(ref BigInteger number, uint power)
	{
		BigInteger bigInteger = BigInteger.Pow(s_bigTen, (int)power);
		number *= bigInteger;
	}

	private static uint FloatToInt32Bits(float f)
	{
		FloatUnion floatUnion = new FloatUnion
		{
			FloatData = f
		};
		return floatUnion.IntData;
	}

	private static float Int32BitsToFloat(uint i)
	{
		FloatUnion floatUnion = new FloatUnion
		{
			IntData = i
		};
		return floatUnion.FloatData;
	}
}
