using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavNotificationEvaluator : NavValueEvaluator<NavNotification>
{
	internal static readonly NavNotificationEvaluator Instance = new NavNotificationEvaluator();

	private NavNotificationEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavNotification value, INavValueMetadata metadata, string source, int number)
	{
		value = new NavNotification(source);
		return true;
	}

	internal bool Evaluate(DataError dataError, out NavNotification value, string source, int number)
	{
		return Evaluate(dataError, out value, null, source, number);
	}

	protected override NavNotification GetDefaultValue(INavValueMetadata metadata)
	{
		return NavNotification.Default;
	}
}
