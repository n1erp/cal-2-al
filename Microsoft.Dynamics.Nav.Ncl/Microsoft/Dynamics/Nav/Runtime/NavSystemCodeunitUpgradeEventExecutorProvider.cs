namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitUpgradeEventExecutorProvider : INavUpgradeEventExecutorProvider
{
	public INavUpgradeEventExecutor CreateUpgradeEventExecutor()
	{
		return NavCurrentThread.Session.SystemCodeunitFactory.UpgradeTriggers;
	}

	public INavAppInstallEventExecutor CreateInstallEventExecutor()
	{
		return NavCurrentThread.Session.SystemCodeunitFactory.ExtensionTriggers;
	}
}
