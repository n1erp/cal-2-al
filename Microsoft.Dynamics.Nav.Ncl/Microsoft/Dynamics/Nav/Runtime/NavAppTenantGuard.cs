using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavAppTenantGuard
{
	public static void BlockInstallIfTenantIsInImproperState(NavTenant tenant)
	{
		if (tenant.State != NavTenantState.Mounted && tenant.State != NavTenantState.Operational && tenant.State != NavTenantState.OperationalSyncInProgress)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001FB", Category.Extensions, new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.InstallAppsBlocked, tenant.State)));
		}
	}

	public static void BlockUninstallIfTenantIsInImproperState(NavTenant tenant)
	{
		if (tenant.State != NavTenantState.Mounted && tenant.State != NavTenantState.Operational && tenant.State != NavTenantState.OperationalSyncInProgress && tenant.State != NavTenantState.OperationalDataUpgradePending)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001M0", Category.Extensions, new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.UninstallAppsBlocked, tenant.State)));
		}
	}

	public static void BlockSynchronizationIfTenantIsInImproperState(NavTenant tenant)
	{
		if (tenant.State != NavTenantState.Mounted && tenant.State != NavTenantState.OperationalWithSyncPending && tenant.State != NavTenantState.OperationalWithSyncFailure && tenant.State != NavTenantState.OperationalSyncInProgress && tenant.State != NavTenantState.OperationalDataUpgradePending && tenant.State != NavTenantState.OperationalWithDataUpgradeFailure && tenant.State != NavTenantState.OperationalDataUpgradeInProgress && tenant.State != NavTenantState.Operational)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001FC", Category.Extensions, new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.SyncAppsBlocked, tenant.State)));
		}
	}

	public static void BlockUpgradeIfTenantIsInImproperState(NavTenant tenant)
	{
		if (tenant.State != NavTenantState.Mounted && tenant.State != NavTenantState.OperationalDataUpgradePending && tenant.State != NavTenantState.OperationalDataUpgradeInProgress && tenant.State != NavTenantState.OperationalWithDataUpgradeFailure && tenant.State != NavTenantState.Operational)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001FD", Category.Extensions, new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeAppsBlocked, tenant.State)));
		}
	}
}
