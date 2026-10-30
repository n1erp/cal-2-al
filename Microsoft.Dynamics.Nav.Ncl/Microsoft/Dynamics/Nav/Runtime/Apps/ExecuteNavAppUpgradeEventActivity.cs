using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class ExecuteNavAppUpgradeEventActivity : ExecuteNavUpgradeEventActivity
{
	public NavAppUpgradeContext Context { get; }

	public ExecuteNavAppUpgradeEventActivity(NavAppUpgradeContext upgradeContext, IEnumerable<string> companies, NavUpgradeEvent upgradeEvent, string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
		: base(companies, upgradeEvent, bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
		Context = upgradeContext;
	}

	protected override void InvokeEvent(INavUpgradeEventExecutorProvider eventExecutorProvider, string company)
	{
		NavAppUpgradeManager navAppUpgradeManager = new NavAppUpgradeManager(Context, eventExecutorProvider.CreateUpgradeEventExecutor);
		navAppUpgradeManager.RunNavAppUpgradeEvent(company, runInSystemSession: false, UpgradeEvent);
	}
}
