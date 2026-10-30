using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBigTextEvaluator : NavValueEvaluator<NavBigText>
{
	private static readonly NavBigTextEvaluator instance = new NavBigTextEvaluator();

	internal static NavBigTextEvaluator Instance => instance;

	private NavBigTextEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavBigText value, INavValueMetadata metadata, string source, int number)
	{
		value = new NavBigText(source);
		return true;
	}

	protected override NavBigText GetDefaultValue(INavValueMetadata metadata)
	{
		return NavBigText.Default;
	}
}
