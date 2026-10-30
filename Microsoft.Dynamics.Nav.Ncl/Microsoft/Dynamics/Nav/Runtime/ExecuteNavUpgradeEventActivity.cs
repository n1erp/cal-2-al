using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExecuteNavUpgradeEventActivity : ExecuteNavEventActivity
{
	public NavUpgradeEvent UpgradeEvent { get; }

	public ExecuteNavUpgradeEventActivity(IEnumerable<string> companies, NavUpgradeEvent upgradeEvent, string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
		: base(companies, bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
		UpgradeEvent = upgradeEvent;
	}

	protected override void InvokeEvent(INavUpgradeEventExecutorProvider eventExecutorProvider, string company)
	{
		INavUpgradeEventExecutor navUpgradeEventExecutor = eventExecutorProvider.CreateUpgradeEventExecutor();
		navUpgradeEventExecutor.InvokeUpgradeEvent(UpgradeEvent);
	}
}
