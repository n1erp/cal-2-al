using System;
using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDataUpgradeSessionManagementConfigurationActivity : NativeActivity
{
	private readonly Func<NativeActivityContext, NavWorkflowTenantSessionManager> getSessionManager;

	public bool EnableSingleSessionMode { get; private set; }

	public bool DisableSingleSessionMode { get; private set; }

	private NavDataUpgradeSessionManagementConfigurationActivity(Func<NativeActivityContext, NavWorkflowTenantSessionManager> sessionManagerRetriever, string displayName = null)
	{
		if (displayName != null)
		{
			DisplayName = displayName;
		}
		getSessionManager = sessionManagerRetriever;
	}

	internal static NavDataUpgradeSessionManagementConfigurationActivity StartSingleSessionMode(string displayName = null, Func<NativeActivityContext, NavWorkflowTenantSessionManager> sessionManagerRetriever = null)
	{
		sessionManagerRetriever = sessionManagerRetriever ?? ((Func<NativeActivityContext, NavWorkflowTenantSessionManager>)((NativeActivityContext context) => context.GetExtension<INavWorkflowTenantSessionManager>() as NavWorkflowTenantSessionManager));
		return new NavDataUpgradeSessionManagementConfigurationActivity(sessionManagerRetriever, displayName)
		{
			EnableSingleSessionMode = true
		};
	}

	internal static NavDataUpgradeSessionManagementConfigurationActivity StopSingleSessionMode(string displayName = null, Func<NativeActivityContext, NavWorkflowTenantSessionManager> sessionManagerRetriever = null)
	{
		sessionManagerRetriever = sessionManagerRetriever ?? ((Func<NativeActivityContext, NavWorkflowTenantSessionManager>)((NativeActivityContext context) => context.GetExtension<INavWorkflowTenantSessionManager>() as NavWorkflowTenantSessionManager));
		return new NavDataUpgradeSessionManagementConfigurationActivity(sessionManagerRetriever, displayName)
		{
			DisableSingleSessionMode = false
		};
	}

	protected override void Execute(NativeActivityContext context)
	{
		NavWorkflowTenantSessionManager navWorkflowTenantSessionManager = getSessionManager(context);
		if (EnableSingleSessionMode)
		{
			navWorkflowTenantSessionManager.StartSingleSessionMode();
		}
		if (DisableSingleSessionMode)
		{
			navWorkflowTenantSessionManager.StopSingleSessionMode();
		}
	}
}
