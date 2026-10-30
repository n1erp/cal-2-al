using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal sealed class NullExecuteNavUpgradeActivity : ExecuteNavUpgradeActivity
{
	public NullExecuteNavUpgradeActivity(string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
		: base(bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
	}

	protected override void Execute(NativeActivityContext context, NavSession session)
	{
	}
}
