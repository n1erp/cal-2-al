using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavALErrorInfoEvaluator : NavValueEvaluator<NavALErrorInfo>
{
	internal static readonly NavALErrorInfoEvaluator Instance = new NavALErrorInfoEvaluator();

	private NavALErrorInfoEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavALErrorInfo value, INavValueMetadata metadata, string source, int number)
	{
		return Evaluate(dataError, out value, source, number);
	}

	internal static bool Evaluate(DataError dataError, out NavALErrorInfo value, string source, int number)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			if (dataError == DataError.ThrowError)
			{
				throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.ALErrorInfo, source);
			}
			value = null;
			return false;
		}
		try
		{
			string[] array = source.Split(';');
			if (array.Length < 4)
			{
				value = null;
				return false;
			}
			ALErrorType errorType = (ALErrorType)int.Parse(array[0], NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number));
			ALVerbosity verbosity = (ALVerbosity)int.Parse(array[1], NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number));
			DataClassification classification = (DataClassification)int.Parse(array[2], NavValueEvaluator.GetNumberStyles(number), NavValueEvaluator.GetFormatProvider(number));
			string message = string.Join(";", array.Skip(3));
			value = new NavALErrorInfo(errorType, verbosity, classification, message);
			return true;
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
		if (dataError == DataError.ThrowError)
		{
			throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.ALErrorInfo, source);
		}
		value = null;
		return false;
	}

	protected override NavALErrorInfo GetDefaultValue(INavValueMetadata metadata)
	{
		return NavALErrorInfo.Default;
	}
}
