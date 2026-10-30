using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavCSideServiceTasks : ICSideService
{
	public IEnumerable<string> GetTenants()
	{
		return from tenant in NavEnvironment.Instance.Tenants.AllTenants()
			select tenant.Id;
	}

	public void StartTenantUpgrade(string tenantId, bool parallelExecutionEnabled, bool continueOnErrorEnabled, bool skipCompanyInitialization, bool skipAppVersionCheck)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		NavSession session = NavCurrentThread.Session;
		if (!CheckIfAdminOrSuperInAllCompanies(session))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		new NavTenantManagementTasks().StartTenantDatabaseUpgrade(new StartTenantDatabaseUpgradeArguments(tenantForCSideTask.Id, 0, parallelExecutionEnabled, continueOnErrorEnabled, skipCompanyInitialization, singleTransaction: false, skipAppVersionCheck));
	}

	public void ResumeTenantUpgrade(string tenantId, int codeunitId, string methodName, string companyName)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		NavSession session = NavCurrentThread.Session;
		if (!CheckIfAdminOrSuperInAllCompanies(session))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		new NavTenantManagementTasks().ResumeTenantDatabaseUpgrade(new ResumeTenantDatabaseUpgradeArguments(tenantForCSideTask.Id, codeunitId, methodName, companyName));
	}

	public void StopTenantUpgrade(string tenantId)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		NavSession session = NavCurrentThread.Session;
		if (!CheckIfAdminOrSuperInAllCompanies(session))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		new NavTenantManagementTasks().StopTenantDatabaseUpgrade(tenantForCSideTask.Id);
	}

	public NavDataUpgradeProcessExecutionDetails GetTenantUpgradeStatus(string tenantId)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		NavSession session = NavCurrentThread.Session;
		if (!CheckIfAdminOrSuperInAllCompanies(session))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		return new NavTenantManagementTasks().GetTenantDatabaseUpgradeStatus(tenantForCSideTask.Id);
	}

	public void ValidateTableLayoutChanges(string tenantId, int tableId, string baseMetadata)
	{
		GetTenantForCSideTask(tenantId)?.Database.DatabaseSync.ValidateTableLayoutChanges(tableId, baseMetadata);
	}

	private static void CheckPermissionsForSynchronization(NavTenant tenant)
	{
		NavSession session = NavCurrentThread.Session;
		tenant.Database.DatabaseSync.PerformSynchronizationAction(() =>
		{
			if (!CheckIfAdminOrSuperInAllCompanies(session))
			{
				throw NavNCLSuperRoleRequiredException.Create();
			}
		});
	}

	private static bool CheckIfAdminOrSuperInAllCompanies(NavSession navSession)
	{
		if ((!navSession.Authenticator.CanIdentify || !NavAdministration.IsNavAdministrator()) && !navSession.AdminSession)
		{
			return navSession.Permissions.IsSuperForAllCompanies;
		}
		return true;
	}

	public void SynchronizeTables(string tenantId, int[] tableIds, bool force)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		if (tenantForCSideTask == null)
		{
			return;
		}
		CheckPermissionsForSynchronization(tenantForCSideTask);
		NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(tenantForCSideTask.Database);
		SyncMode syncMode = ((!force) ? SyncMode.Sync : SyncMode.ForceSync);
		if (tenantForCSideTask.Database.IsNewTenantDatabase)
		{
			tenantForCSideTask.Database.SynchronizeTenantDatabase(syncMode, async: false);
			return;
		}
		NavGlobal.AppDatabase.InitializeChangeListener();
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		string content = "SynchronizeTables: " + ((tableIds.Length == 1) ? ("Table " + tableIds[0]) : (tableIds.Length + " tables"));
		using (DiagnosticsResolver.CreateTimingScope("00000R9", Category.DatabaseSync, DiagnosticParameter.SystemMetadata(content)))
		{
			tenantForCSideTask.Database.DatabaseSync.SynchronizeTables(tableIds, syncMode);
		}
	}

	public void SynchronizeAllTables(string tenantId, bool force)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		if (tenantForCSideTask != null)
		{
			CheckPermissionsForSynchronization(tenantForCSideTask);
			NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(tenantForCSideTask.Database);
			SyncMode syncMode = ((!force) ? SyncMode.Sync : SyncMode.ForceSync);
			tenantForCSideTask.Database.SynchronizeTenantDatabase(syncMode, async: true);
		}
	}

	public NavTenantRuntimeSettings SynchronizationState(string tenantId)
	{
		return GetTenantForCSideTask(tenantId)?.GetRuntimeTenantSettings();
	}

	public void SynchronizationCheck(string tenantId)
	{
		NavTenant tenantForCSideTask = GetTenantForCSideTask(tenantId);
		if (tenantForCSideTask != null)
		{
			CheckPermissionsForSynchronization(tenantForCSideTask);
			NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(tenantForCSideTask.Database);
			tenantForCSideTask.Database.SynchronizeTenantDatabase(SyncMode.CheckOnly, async: true);
		}
	}

	private static NavTenant GetTenantForCSideTask(string tenantId)
	{
		IEnumerable<NavTenant> source = NavEnvironment.Instance.Tenants.AllTenants();
		NavTenant navTenant;
		try
		{
			navTenant = source.SingleOrDefault();
		}
		catch (InvalidOperationException)
		{
			throw new NavTenantNotFoundException("There are multiple tenants mounted on the server. Validating table layout changes to protect against data-loss is only supported with a single tenant mounted on the server.");
		}
		if (navTenant == null)
		{
			throw new NavTenantNotFoundException("There are no tenants mounted on the server. Validating table layout changes to protect against data loss is only supported with a single tenant mounted on the server.");
		}
		if (!string.IsNullOrWhiteSpace(tenantId) && string.Compare(tenantId, navTenant.Id, StringComparison.OrdinalIgnoreCase) != 0)
		{
			throw new NavTenantNotFoundException(string.Format(CultureInfo.InvariantCulture, "The tenant '{0}' specified in Tools/Options is not the mounted or default tenant '{1}'.", tenantId, navTenant.Id));
		}
		return navTenant;
	}

	public bool PrepareAddInsDownload(string assemblyFullName, bool clientIs64BitProcess)
	{
		throw new NotImplementedException();
	}

	public bool PrepareAddInsDatabaseDownload(string databaseServer, string databaseName, string userName, string password, string assemblyFullName)
	{
		throw new NotImplementedException();
	}

	public StreamTransfer Download()
	{
		throw new NotImplementedException();
	}

	public ReadOnlyCollection<string> GetAddInsAssemblyNames()
	{
		throw new NotImplementedException();
	}
}
