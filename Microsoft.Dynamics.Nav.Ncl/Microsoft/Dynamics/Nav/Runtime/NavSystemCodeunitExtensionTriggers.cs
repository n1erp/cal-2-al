namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitExtensionTriggers : NavSystemCodeunit, INavAppInstallEventExecutor
{
	internal enum CodeunitMethod
	{
		OnInstallAppPerDatabase = 1,
		OnInstallAppPerCompany
	}

	public NavSystemCodeunitExtensionTriggers(TreeObject parent)
		: base(parent, 2000000010)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public void InvokeOnInstallAppPerDatabase()
	{
		NavSession session = NavCurrentThread.Session;
		if (session.SystemCodeunitFactory.InstallUpgradeSubscriptionObserver.AreInstallUpgradeEventsUsed)
		{
			session.TransactionScope(() =>
			{
				Invoke(1, NavSystemCodeunit.EmptyObjectArray);
			});
		}
	}

	public void InvokeOnInstallAppPerCompany()
	{
		NavSession session = NavCurrentThread.Session;
		if (session.SystemCodeunitFactory.InstallUpgradeSubscriptionObserver.AreInstallUpgradeEventsUsed)
		{
			session.TransactionScope(() =>
			{
				Invoke(2, NavSystemCodeunit.EmptyObjectArray);
			});
		}
	}
}
