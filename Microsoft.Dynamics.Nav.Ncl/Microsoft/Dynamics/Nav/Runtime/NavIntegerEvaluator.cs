using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavIntegerEvaluator : NavValueEvaluator<NavInteger>
{
	private static readonly NavIntegerEvaluator instance = new NavIntegerEvaluator();

	internal static NavIntegerEvaluator Instance => instance;

	private NavIntegerEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavInteger value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source, number);
		value = NavInteger.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out int value, string source, int number)
	{
		if (int.TryParse(source, NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number), out value))
		{
			if (value == int.MinValue)
			{
				if (dataError == DataError.TrapError)
				{
					value = 0;
					return false;
				}
				throw NavNCLEvaluateException.CreateIntegerValueOutOfRange(CultureInfo.CurrentCulture, source, -2147483647, int.MaxValue);
			}
			return true;
		}
		if (dataError == DataError.TrapError)
		{
			value = 0;
			return false;
		}
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Integer, source);
	}

	protected override NavInteger GetDefaultValue(INavValueMetadata metadata)
	{
		return NavInteger.Default;
	}
}
