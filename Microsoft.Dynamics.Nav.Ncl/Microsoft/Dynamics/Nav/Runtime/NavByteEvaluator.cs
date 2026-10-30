using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavByteEvaluator : NavValueEvaluator<NavByte>
{
	private static readonly NavByteEvaluator instance = new NavByteEvaluator();

	internal static NavByteEvaluator Instance => instance;

	private NavByteEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavByte value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source);
		value = NavByte.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out byte value, string source)
	{
		return NavValueEvaluator<NavByte>.TryEvaluateToCharOrByte(dataError, source, TryParse, NCLManagedAdapter.TextCharToByte, out value);
	}

	protected override NavByte GetDefaultValue(INavValueMetadata metadata)
	{
		return NavByte.Default;
	}

	private static bool TryParse(DataError dataError, string source, out byte value)
	{
		value = 0;
		if (!ushort.TryParse(source, out var result))
		{
			return false;
		}
		if (result > 255)
		{
			if (dataError == DataError.TrapError)
			{
				return false;
			}
			throw NavNCLEvaluateException.CreateIntegerValueOutOfRange(CultureInfo.CurrentCulture, source, 0, 255);
		}
		value = ALCompiler.ToByte(result);
		return true;
	}
}
