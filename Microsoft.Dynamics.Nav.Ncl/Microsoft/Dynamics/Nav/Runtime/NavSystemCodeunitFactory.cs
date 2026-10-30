using Microsoft.Dynamics.Nav.EventSubscription;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavSystemCodeunitFactory
{
	private readonly TreeObject parent;

	private NavSystemCodeunitVersionTriggers versionTriggers;

	private NavSystemCodeunitCompanyTriggers companyTriggers;

	private NavSystemCodeunitUIHelperTriggers uiHelperTriggers;

	private NavSystemCodeunitReportingTriggers reportingTriggers;

	private NavSystemCodeunitSystemActionTriggers systemActionTriggers;

	private NavSystemCodeunitFilterHelperTriggers filterHelperTriggers;

	private NavSystemCodeunitUpgradeTriggers upgradeTriggers;

	private NavSystemCodeunitExtensionTriggers extensionTriggers;

	private NavSystemCodeunitGlobalTriggers globalTriggers;

	private NavSystemCodeunitDebuggerTriggers debuggerTriggers;

	public NavInstallUpgradeSubscriptionObserver InstallUpgradeSubscriptionObserver { get; } = NavGlobal.EventSubscriptionMetadata.GetSubscriptionObserverInstance<NavInstallUpgradeSubscriptionObserver>();

	public NavSystemCodeunitVersionTriggers VersionTriggers
	{
		get
		{
			if (versionTriggers == null)
			{
				versionTriggers = new NavSystemCodeunitVersionTriggers(parent);
			}
			return versionTriggers;
		}
	}

	public NavSystemCodeunitCompanyTriggers CompanyTriggers
	{
		get
		{
			if (companyTriggers == null)
			{
				companyTriggers = new NavSystemCodeunitCompanyTriggers(parent);
			}
			return companyTriggers;
		}
	}

	public NavSystemCodeunitUIHelperTriggers UIHelperTriggers
	{
		get
		{
			if (uiHelperTriggers == null)
			{
				uiHelperTriggers = new NavSystemCodeunitUIHelperTriggers(parent);
			}
			return uiHelperTriggers;
		}
	}

	public NavSystemCodeunitReportingTriggers ReportingTriggers
	{
		get
		{
			if (reportingTriggers == null)
			{
				reportingTriggers = new NavSystemCodeunitReportingTriggers(parent);
			}
			return reportingTriggers;
		}
	}

	public NavSystemCodeunitSystemActionTriggers SystemActionTriggers
	{
		get
		{
			if (systemActionTriggers == null)
			{
				systemActionTriggers = new NavSystemCodeunitSystemActionTriggers(parent);
			}
			return systemActionTriggers;
		}
	}

	public NavSystemCodeunitFilterHelperTriggers FilterHelperTriggers
	{
		get
		{
			if (filterHelperTriggers == null)
			{
				filterHelperTriggers = new NavSystemCodeunitFilterHelperTriggers(parent);
			}
			return filterHelperTriggers;
		}
	}

	public NavSystemCodeunitUpgradeTriggers UpgradeTriggers
	{
		get
		{
			if (upgradeTriggers == null)
			{
				upgradeTriggers = new NavSystemCodeunitUpgradeTriggers(parent);
			}
			return upgradeTriggers;
		}
	}

	public NavSystemCodeunitExtensionTriggers ExtensionTriggers
	{
		get
		{
			if (extensionTriggers == null)
			{
				extensionTriggers = new NavSystemCodeunitExtensionTriggers(parent);
			}
			return extensionTriggers;
		}
	}

	public NavSystemCodeunitGlobalTriggers GlobalTriggers
	{
		get
		{
			if (globalTriggers == null)
			{
				globalTriggers = new NavSystemCodeunitGlobalTriggers(parent);
			}
			return globalTriggers;
		}
	}

	public NavSystemCodeunitDebuggerTriggers DebuggerTriggers
	{
		get
		{
			if (debuggerTriggers == null)
			{
				debuggerTriggers = new NavSystemCodeunitDebuggerTriggers(parent);
			}
			return debuggerTriggers;
		}
	}

	public NavSystemCodeunitFactory(TreeObject parent)
	{
		this.parent = parent;
	}
}
