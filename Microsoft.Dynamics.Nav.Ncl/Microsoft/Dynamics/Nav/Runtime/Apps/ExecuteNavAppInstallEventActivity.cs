using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal sealed class ExecuteNavAppInstallEventActivity : ExecuteNavEventActivity
{
	public NavAppInstallationContext Context { get; }

	public ExecuteNavAppInstallEventActivity(NavAppInstallationContext installationContext, IEnumerable<string> companies, string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
		: base(companies, bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
		Context = installationContext;
	}

	protected override void InvokeEvent(INavUpgradeEventExecutorProvider eventExecutorProvider, string company)
	{
		NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(Context, eventExecutorProvider.CreateInstallEventExecutor);
		if (string.IsNullOrWhiteSpace(company))
		{
			navAppInstallationProcessor.RunNavAppInstallPerDatabaseEvent(runInSystemSession: false);
		}
		else
		{
			navAppInstallationProcessor.RunNavAppInstallPerCompanyEvent(company, runInSystemSession: false);
		}
	}
}
