using System;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemNumeric
{
	public static int ALAbs(int value)
	{
		return Math.Abs(value);
	}

	public static long ALAbs(long value)
	{
		return Math.Abs(value);
	}

	public static Decimal18 ALAbs(Decimal18 value)
	{
		return Decimal18.Abs(value);
	}

	public static Decimal18 ALRound(Decimal18 value)
	{
		return NavDecimalHelper.NavDecimalRound(value);
	}

	public static Decimal18 ALRound(Decimal18 value, Decimal18 precision)
	{
		return NavDecimalHelper.NavDecimalRound(value, precision);
	}

	public static Decimal18 ALRound(Decimal18 value, Decimal18 precision, string direction)
	{
		if (direction == null)
		{
			throw new ArgumentNullException("direction");
		}
		if (precision == 0)
		{
			throw new NavNCLOutsidePermittedRangeException("ROUND", 2, (decimal)Decimal18.Abs(precision), 1m, 2147483647m);
		}
		if (direction.Length == 1)
		{
			switch (direction[0])
			{
			case '=':
				return NavDecimalHelper.NavDecimalRound(value, precision, NavDecimalHelper.RoundMethod.RoundNear);
			case '<':
				return NavDecimalHelper.NavDecimalRound(value, precision, NavDecimalHelper.RoundMethod.RoundDown);
			case '>':
				return NavDecimalHelper.NavDecimalRound(value, precision, NavDecimalHelper.RoundMethod.RoundUp);
			}
		}
		throw new NavNCLInvalidValueException();
	}

	public static Decimal18 ALPower(Decimal18 value, Decimal18 power)
	{
		return NavDecimalHelper.NavDecimalPower(value, power);
	}

	public static Decimal18 ALTruncate(Decimal18 value)
	{
		return Decimal18.Truncate(value);
	}

	public static void ALRandomize()
	{
		NavCurrentThread.Session.Random = new Random();
	}

	public static void ALRandomize(int seed)
	{
		NavCurrentThread.Session.Random = new Random(seed);
	}

	public static int ALRandom(int maxNumber)
	{
		if (maxNumber < 0)
		{
			maxNumber = -maxNumber;
		}
		if (maxNumber == 0)
		{
			maxNumber = 1;
		}
		return NavCurrentThread.Session.Random.Next(maxNumber) + 1;
	}
}
