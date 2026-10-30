using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavDecimalHelper
{
	public enum RoundMethod
	{
		RoundDown = -1,
		RoundNear,
		RoundUp
	}

	public static readonly Decimal18 MaxBcdReal = 999999999999999.99m;

	public static readonly Decimal18 MinBcdReal = -999999999999999.99m;

	public const int MaxSafeBcdStringLength = 15;

	private static Decimal18[] pwr10 = InitPower10Array();

	private static Decimal18[] InitPower10Array()
	{
		Decimal18[] array = new Decimal18[29];
		array[0] = 1;
		for (int i = 1; i < 29; i++)
		{
			array[i] = array[i - 1] * 10m;
		}
		return array;
	}

	private static Decimal18 DecodeBCD(byte signAndExponent, byte bcdMantissaByte1, uint bcdMantissaByte25, uint bcdMantissaByte69)
	{
		if (signAndExponent == 0)
		{
			return 0;
		}
		int num = (signAndExponent & 0x7F) - 64;
		if (Math.Abs(num) > 27)
		{
			throw new OverflowException("NavDecimalToDecimal");
		}
		long num2;
		if (bcdMantissaByte1 != 0)
		{
			num2 = DecodeBCD1(bcdMantissaByte1) + (long)DecodeBCD4(bcdMantissaByte25) * 100L + (long)DecodeBCD4(bcdMantissaByte69) * 10000000000L;
			num -= 17;
		}
		else if (bcdMantissaByte25 != 0)
		{
			num2 = DecodeBCD4(bcdMantissaByte25) + (long)DecodeBCD4(bcdMantissaByte69) * 100000000L;
			num -= 15;
		}
		else
		{
			num2 = DecodeBCD4(bcdMantissaByte69);
			num -= 7;
		}
		if ((signAndExponent & 0x80) != 0)
		{
			num2 = -num2;
		}
		if (num >= 0)
		{
			return num2 * pwr10[num];
		}
		if (num >= -28)
		{
			return num2 / pwr10[-num];
		}
		return num2 / pwr10[20] / pwr10[-(num + 20)];
	}

	private static uint DecodeBCD4(uint bcd)
	{
		if (bcd == 0)
		{
			return 0u;
		}
		return (bcd & 0xF) + ((bcd >> 4) & 0xF) * 10 + ((bcd >> 8) & 0xF) * 100 + ((bcd >> 12) & 0xF) * 1000 + ((bcd >> 16) & 0xF) * 10000 + ((bcd >> 20) & 0xF) * 100000 + ((bcd >> 24) & 0xF) * 1000000 + (bcd >> 28) * 10000000;
	}

	private static uint DecodeBCD1(uint bcd)
	{
		if (bcd == 0)
		{
			return 0u;
		}
		return (bcd & 0xF) + ((bcd >> 4) & 0xF) * 10;
	}

	public static Decimal18 GetDefaultRoundPrecision()
	{
		return NavCurrentThread.Session?.GetDefaultRoundPrecision() ?? FallbackRoundPrecision();
	}

	public static Decimal18 FallbackRoundPrecision()
	{
		int num = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalDigits;
		if (num < 0 || num >= pwr10.Length)
		{
			num = 2;
		}
		return 1m / pwr10[num];
	}

	public static Decimal18 NavDecimalToDecimal(byte[] bcdReal)
	{
		return NavDecimalToDecimal(bcdReal, 0);
	}

	public static Decimal18 NavDecimalToDecimal(byte[] bcdReal, int startIndex)
	{
		if (bcdReal == null)
		{
			throw new ArgumentNullException("bcdReal", "NavDecimalToDecimal");
		}
		if (bcdReal.Length < 12 + startIndex || startIndex < 0)
		{
			throw new ArgumentException("NavDecimalToDecimal");
		}
		if (bcdReal[startIndex] == 0)
		{
			return 0;
		}
		return DecodeBCD(bcdReal[startIndex], bcdReal[startIndex + 1], (uint)(bcdReal[startIndex + 2] + (bcdReal[startIndex + 3] << 8) + (bcdReal[startIndex + 4] << 16) + (bcdReal[startIndex + 5] << 24)), (uint)(bcdReal[startIndex + 6] + (bcdReal[startIndex + 7] << 8) + (bcdReal[startIndex + 8] << 16) + (bcdReal[startIndex + 9] << 24)));
	}

	public static byte[] DecimalToNavDecimal(Decimal18 dec)
	{
		byte[] array = new byte[12];
		if (dec == 0)
		{
			return array;
		}
		string text = dec.ToString("E17", CultureInfo.InvariantCulture);
		int num;
		int num2;
		if (text[0] == '-')
		{
			num = 128;
			num2 = 1;
		}
		else
		{
			num = 0;
			num2 = 0;
		}
		int num3 = (text[num2 + 22] - 48) * 10 + (text[num2 + 23] - 48);
		if (text[num2 + 20] == '-')
		{
			num3 = -num3;
		}
		if (num3 >= 14 && (num3 > 14 || dec < MinBcdReal || dec > MaxBcdReal))
		{
			throw new NavNCLOutsidePermittedRangeException("Numeric", 0, (decimal)dec, (decimal)MinBcdReal, (decimal)MaxBcdReal);
		}
		array[0] = (byte)(num + (num3 + 64));
		array[9] = (byte)((text[num2++] - 48 << 4) + (text[++num2] - 48));
		for (int num4 = 8; num4 > 0; num4--)
		{
			array[num4] = (byte)((text[++num2] - 48 << 4) + (text[++num2] - 48));
		}
		return array;
	}

	public static Decimal18 NavDecimalRound(Decimal18 dec)
	{
		return NavDecimalRound(dec, GetDefaultRoundPrecision(), RoundMethod.RoundNear);
	}

	public static Decimal18 NavDecimalRound(Decimal18 dec, Decimal18 precision)
	{
		return NavDecimalRound(dec, precision, RoundMethod.RoundNear);
	}

	public static Decimal18 NavDecimalRound(Decimal18 value, Decimal18 precision, RoundMethod method)
	{
		if (precision <= Decimal18.Zero)
		{
			throw new NavNCLOutsidePermittedRangeException("ROUND", 2, (decimal)Decimal18.Abs(precision), 1m, 2147483647m);
		}
		bool flag = value < 0;
		decimal num = (decimal)Decimal18.Abs(value);
		decimal num2 = (decimal)precision;
		try
		{
			num /= num2;
		}
		catch (OverflowException)
		{
			return value;
		}
		switch (method)
		{
		case RoundMethod.RoundDown:
			num = decimal.Truncate(num);
			break;
		case RoundMethod.RoundNear:
			num = decimal.Truncate(num + 0.5m);
			break;
		case RoundMethod.RoundUp:
		{
			decimal num3 = num;
			num = decimal.Truncate(num);
			if (num != num3)
			{
				num += 1m;
			}
			break;
		}
		default:
			throw new ArgumentException("Unknown Round method");
		}
		num *= num2;
		if (flag)
		{
			num = decimal.Negate(num);
		}
		return num;
	}

	public static Decimal18 NavDecimalPower(Decimal18 dec, Decimal18 exponent)
	{
		if (exponent >= 0 && exponent < int.MaxValue)
		{
			int num = (int)exponent;
			if (num == exponent)
			{
				Decimal18 result = 1;
				while (true)
				{
					if ((num & 1) == 1)
					{
						result *= dec;
					}
					if (num < 2)
					{
						break;
					}
					dec *= dec;
					num >>= 1;
				}
				return result;
			}
		}
		return (Decimal18)Math.Pow((double)dec, (double)exponent);
	}

	public static void ValidateDecimalRange(Decimal18 dec)
	{
		if (dec < MinBcdReal || dec > MaxBcdReal)
		{
			throw new NavNCLOutsidePermittedRangeException("Decimal", 0, (decimal)dec, (decimal)MinBcdReal, (decimal)MaxBcdReal);
		}
	}
}
