using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavCharEvaluator : NavValueEvaluator<NavChar>
{
	private static readonly NavCharEvaluator instance = new NavCharEvaluator();

	internal static NavCharEvaluator Instance => instance;

	private NavCharEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavChar value, INavValueMetadata metadata, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, source);
		value = NavChar.Create(value2);
		return result;
	}

	internal static bool Evaluate(DataError dataError, out char value, string source)
	{
		return NavValueEvaluator<NavChar>.TryEvaluateToCharOrByte(dataError, source, TryParse, (char tempChar) => tempChar, out value);
	}

	protected override NavChar GetDefaultValue(INavValueMetadata metadata)
	{
		return NavChar.Default;
	}

	private static bool TryParse(DataError dataError, string source, out char value)
	{
		value = '\0';
		if (!uint.TryParse(source, out var result))
		{
			return false;
		}
		value = ALCompiler.ToChar(result);
		return true;
	}
}
