using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitUpgradeTriggers : NavSystemCodeunit, INavUpgradeEventExecutor
{
	internal enum CodeunitMethod
	{
		OnCheckPreconditionsPerDatabase = 1,
		OnCheckPreconditionsPerCompany = 2,
		OnUpgradePerDatabase = 3,
		OnUpgradePerCompany = 4,
		OnValidateUpgradePerDatabase = 5,
		OnValidateUpgradePerCompany = 6,
		OnAfterUpgradeCommitPerDatabase = 99,
		OnAfterUpgradeCommitPerCompany = 100
	}

	private static readonly HashSet<string> installUpgradeEventMethodNames = new HashSet<string>
	{
		CodeunitMethod.OnCheckPreconditionsPerDatabase.ToString(),
		CodeunitMethod.OnCheckPreconditionsPerCompany.ToString(),
		CodeunitMethod.OnUpgradePerDatabase.ToString(),
		CodeunitMethod.OnUpgradePerCompany.ToString(),
		CodeunitMethod.OnValidateUpgradePerDatabase.ToString(),
		CodeunitMethod.OnValidateUpgradePerCompany.ToString(),
		CodeunitMethod.OnAfterUpgradeCommitPerDatabase.ToString(),
		CodeunitMethod.OnAfterUpgradeCommitPerCompany.ToString(),
		NavSystemCodeunitExtensionTriggers.CodeunitMethod.OnInstallAppPerDatabase.ToString(),
		NavSystemCodeunitExtensionTriggers.CodeunitMethod.OnInstallAppPerCompany.ToString()
	};

	private readonly Dictionary<NavUpgradeEvent, CodeunitMethod> upgradeEventMapping = new Dictionary<NavUpgradeEvent, CodeunitMethod>
	{
		{
			NavUpgradeEvent.OnCheckPreconditionsPerDatabase,
			CodeunitMethod.OnCheckPreconditionsPerDatabase
		},
		{
			NavUpgradeEvent.OnCheckPreconditionsPerCompany,
			CodeunitMethod.OnCheckPreconditionsPerCompany
		},
		{
			NavUpgradeEvent.OnUpgradePerDatabase,
			CodeunitMethod.OnUpgradePerDatabase
		},
		{
			NavUpgradeEvent.OnUpgradePerCompany,
			CodeunitMethod.OnUpgradePerCompany
		},
		{
			NavUpgradeEvent.OnValidateUpgradePerDatabase,
			CodeunitMethod.OnValidateUpgradePerDatabase
		},
		{
			NavUpgradeEvent.OnValidateUpgradePerCompany,
			CodeunitMethod.OnValidateUpgradePerCompany
		},
		{
			NavUpgradeEvent.OnAfterUpgradeCommitPerDatabase,
			CodeunitMethod.OnAfterUpgradeCommitPerDatabase
		},
		{
			NavUpgradeEvent.OnAfterUpgradeCommitPerCompany,
			CodeunitMethod.OnAfterUpgradeCommitPerCompany
		}
	};

	public static int Id { get; } = 2000000008;

	public NavSystemCodeunitUpgradeTriggers(TreeObject parent)
		: base(parent, Id)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public static bool IsInstallUpgradeMethodName(string methodName)
	{
		return installUpgradeEventMethodNames.Contains(methodName);
	}

	public void InvokeUpgradeEvent(NavUpgradeEvent upgradeEvent)
	{
		NavSession session = NavCurrentThread.Session;
		if (!session.SystemCodeunitFactory.InstallUpgradeSubscriptionObserver.AreInstallUpgradeEventsUsed)
		{
			return;
		}
		CodeunitMethod correspondingMethod = upgradeEventMapping[upgradeEvent];
		if ((uint)(upgradeEvent - 6) > 1u)
		{
			session.TransactionScope(() =>
			{
				Invoke((int)correspondingMethod, NavSystemCodeunit.EmptyObjectArray);
			});
		}
	}
}
