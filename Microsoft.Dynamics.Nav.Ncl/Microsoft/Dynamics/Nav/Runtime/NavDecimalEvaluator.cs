using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDecimalEvaluator : NavValueEvaluator<NavDecimal>
{
	private static readonly NavDecimalEvaluator instance = new NavDecimalEvaluator();

	internal static NavDecimalEvaluator Instance => instance;

	private NavDecimalEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavDecimal value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source, number);
		value = NavDecimal.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out Decimal18 value, string source, int number)
	{
		if (Decimal18.TryParse(source, NavValueEvaluator.GetNumberStyles(number) | NumberStyles.AllowExponent, NavValueEvaluator.GetFormatProvider(number), out value) && value >= NavDecimalHelper.MinBcdReal && value <= NavDecimalHelper.MaxBcdReal)
		{
			return true;
		}
		value = NavDecimal.Default;
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Decimal, source);
	}

	protected override NavDecimal GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDecimal.Default;
	}
}
