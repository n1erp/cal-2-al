using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavMediaEvaluator : NavValueEvaluator<NavMedia>
{
	private static readonly NavMediaEvaluator instance = new NavMediaEvaluator();

	internal static NavMediaEvaluator Instance => instance;

	private NavMediaEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavMedia value, INavValueMetadata metadata, string source, int number)
	{
		value = new NavMedia(source);
		return true;
	}

	internal bool Evaluate(DataError dataError, out NavMedia value, string source, int number)
	{
		return Evaluate(dataError, out value, null, source, number);
	}

	protected override NavMedia GetDefaultValue(INavValueMetadata metadata)
	{
		return NavMedia.Default;
	}
}
