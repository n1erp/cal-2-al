using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBigIntegerEvaluator : NavValueEvaluator<NavBigInteger>
{
	private static readonly NavBigIntegerEvaluator instance = new NavBigIntegerEvaluator();

	internal static NavBigIntegerEvaluator Instance => instance;

	private NavBigIntegerEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavBigInteger value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source, number);
		value = NavBigInteger.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out long value, string source, int number)
	{
		if (long.TryParse(source, NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number), out value))
		{
			return true;
		}
		string text = NavValueEvaluator.RemoveWhiteSpaceUntilFirstDigit(source);
		if (!string.IsNullOrEmpty(text) && long.TryParse(text, NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number), out value))
		{
			return true;
		}
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.BigInteger, source);
	}

	protected override NavBigInteger GetDefaultValue(INavValueMetadata metadata)
	{
		return NavBigInteger.Default;
	}
}
