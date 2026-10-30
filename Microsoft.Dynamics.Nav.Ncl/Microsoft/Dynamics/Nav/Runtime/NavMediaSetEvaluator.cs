using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavMediaSetEvaluator : NavValueEvaluator<NavMediaSet>
{
	private static readonly NavMediaSetEvaluator instance = new NavMediaSetEvaluator();

	internal static NavMediaSetEvaluator Instance => instance;

	private NavMediaSetEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavMediaSet value, INavValueMetadata metadata, string source, int number)
	{
		value = new NavMediaSet(source);
		return true;
	}

	internal bool Evaluate(DataError dataError, out NavMediaSet value, string source, int number)
	{
		return Evaluate(dataError, out value, null, source, number);
	}

	protected override NavMediaSet GetDefaultValue(INavValueMetadata metadata)
	{
		return NavMediaSet.Default;
	}
}
