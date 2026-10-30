using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBooleanEvaluator : NavValueEvaluator<NavBoolean>
{
	private static readonly NavBooleanEvaluator instance = new NavBooleanEvaluator();

	private static readonly string[] TrueFalseOptions = new string[2] { "FALSE", "TRUE" };

	internal static NavBooleanEvaluator Instance => instance;

	private NavBooleanEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavBoolean value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source, number);
		value = NavBoolean.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out bool value, string source, int number)
	{
		string[] array = ((number == 9 || NavCurrentSessionWrapper.FormatSettings.BooleanOptions == null) ? TrueFalseOptions : NavCurrentSessionWrapper.FormatSettings.BooleanOptions);
		value = false;
		source = source.Trim();
		if (source.Length > 0)
		{
			int result = 0;
			if (int.TryParse(source, NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number), out result))
			{
				switch (result)
				{
				case 1:
					value = true;
					return true;
				case 0:
					return true;
				}
			}
			if (array[0].StartsWith(source, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (array[1].StartsWith(source, StringComparison.OrdinalIgnoreCase))
			{
				value = true;
				return true;
			}
			if (number != 9)
			{
				if (TrueFalseOptions[0].StartsWith(source, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (TrueFalseOptions[1].StartsWith(source, StringComparison.OrdinalIgnoreCase))
				{
					value = true;
					return true;
				}
			}
		}
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Boolean, source);
	}

	protected override NavBoolean GetDefaultValue(INavValueMetadata metadata)
	{
		return NavBoolean.Default;
	}
}
