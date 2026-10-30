using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTenantDatabaseManagementTasks : ITenantDatabaseManagementService
{
	public void DismountTenantDatabase(string id)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QZ", Category.TenantDatabaseManagement, Verbosity.Normal, "Dismounting tenant database with id={0}", DiagnosticParameter.Oii(id));
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new NavNCLArgumentException(id, "DismountTenantDatabase", 1);
		}
		TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(id);
		if (tenantDatabaseConfiguration != null)
		{
			NavTenant navTenant = NavEnvironment.Instance.Tenants.TenantsOnTheSameSqlDatabase(id).FirstOrDefault();
			if (navTenant != null)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The tenant database '{0}' cannot be dismounted because it has a mounted tenant: tenant ID = {1}.", id, navTenant.Id);
				NavTenantDatabaseException ex = new NavTenantDatabaseException(message);
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000012P", Category.TenantDatabaseManagement, ex);
				throw ex;
			}
		}
		if (!NavEnvironment.Instance.TenantDatabases.Configuration.RemoveTenantDatabaseConfiguration(id))
		{
			string message2 = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, id);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QW", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message2);
			throw new NavTenantDatabaseException(message2);
		}
		NavEnvironment.Instance.DatabaseServers.GetDatabaseServerByName(tenantDatabaseConfiguration.DatabaseServer, tenantDatabaseConfiguration.DatabaseUserName, tenantDatabaseConfiguration.DatabasePassword)?.SqlConnectionPool?.IncrementBumperToken(tenantDatabaseConfiguration.DatabaseName);
	}

	public IList<NavTenantDatabaseSettings> GetTenantDatabase(string id = null, bool forceRefresh = false)
	{
		IList<NavTenantDatabaseSettings> result = new List<NavTenantDatabaseSettings>();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000R0", Category.TenantDatabaseManagement, Verbosity.Normal, "Getting tenant database with id={0}", DiagnosticParameter.Oii(id));
		if (string.IsNullOrEmpty(id))
		{
			IList<TenantDatabaseConfiguration> allTenantDatabaseConfigurations = NavEnvironment.Instance.TenantDatabases.Configuration.AllTenantDatabaseConfigurations;
			if (forceRefresh)
			{
				allTenantDatabaseConfigurations.ForEach((TenantDatabaseConfiguration configuration) =>
				{
					try
					{
						result.Add(GetTenantDatabase(configuration.Id).Single());
					}
					catch (Exception)
					{
						result.Add(new NavTenantDatabaseSettings(configuration.Id, configuration.ToDatabaseInfo(), new List<string> { "undefined" }, "undefined", new NavTenantDatabaseSettingsDetailedState(NavTenantDatabaseState.Failed)));
					}
				});
			}
			else
			{
				allTenantDatabaseConfigurations.ForEach((TenantDatabaseConfiguration configuration) =>
				{
					result.Add(new NavTenantDatabaseSettings(configuration.Id, configuration.ToDatabaseInfo()));
				});
			}
		}
		else
		{
			TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(id);
			if (tenantDatabaseConfiguration == null)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, id);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SD", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message);
				throw new NavTenantDatabaseException(message);
			}
			using NavExternalTenant navExternalTenant = new NavExternalTenant(tenantDatabaseConfiguration, NavDatabaseType.Tenant);
			KeyValuePair<string, List<string>> supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(navExternalTenant.Database);
			NavTenantDatabaseSettingsDetailedState initialState = TenantDatabaseConfiguration.CalculateTenantDatabaseState(navExternalTenant.Database, supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily);
			result.Add(new NavTenantDatabaseSettings(tenantDatabaseConfiguration.Id, tenantDatabaseConfiguration.ToDatabaseInfo(), supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, initialState, navExternalTenant.Database.IsSharedTenantSchema));
		}
		return result;
	}

	public NavTenantDatabaseSettings MountTenantDatabase(MountTenantDatabaseArguments mountTenantDatabaseArguments)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000R1", Category.TenantDatabaseManagement, Verbosity.Normal, "Mount tenant database with id={0}", DiagnosticParameter.Oii(mountTenantDatabaseArguments.TenantDatabaseSettings.Id));
		if (string.IsNullOrEmpty(mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Server))
		{
			mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Server = ServerUserSettings.Instance.ServerName;
		}
		if (mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo?.Password != null)
		{
			NavEnvironment.Instance.Tenants.CheckEncryptionKey(null, mountTenantDatabaseArguments.VerifyServerInstanceKey);
		}
		TenantDatabaseConfiguration tenantDatabaseConfiguration = new TenantDatabaseConfiguration(mountTenantDatabaseArguments.TenantDatabaseSettings.Id, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Server, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Name, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.UserName, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Password);
		KeyValuePair<string, List<string>> supportedFamilyAndVersions;
		NavTenantDatabaseSettingsDetailedState initialState;
		bool? isSharedSchema;
		using (NavExternalTenant navExternalTenant = new NavExternalTenant(tenantDatabaseConfiguration, NavDatabaseType.Tenant))
		{
			CheckTenantDatabaseVersion(navExternalTenant.Database);
			supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(navExternalTenant.Database);
			if (supportedFamilyAndVersions.Value != null)
			{
				List<string> value = supportedFamilyAndVersions.Value;
				if (!TenantDatabaseConfiguration.IsApplicationFamilyCompatible(supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily) || !TenantDatabaseConfiguration.IsApplicationVersionMountable(value, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion))
				{
					string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseNotReadyForUseWrongApplication, navExternalTenant.Database.NormalizedDatabaseName, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily, string.Join(",", value), string.Join(",", supportedFamilyAndVersions.Key));
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000R3", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message);
					throw new NavTenantDatabaseException(message);
				}
			}
			initialState = TenantDatabaseConfiguration.CalculateTenantDatabaseState(navExternalTenant.Database, supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily);
			isSharedSchema = navExternalTenant.Database.IsSharedTenantSchema;
		}
		try
		{
			NavEnvironment.Instance.TenantDatabases.Configuration.AddTenantDatabaseConfiguration(new TenantDatabaseConfiguration(mountTenantDatabaseArguments.TenantDatabaseSettings.Id, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Server, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Name, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.UserName, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Password));
		}
		catch (NavCSideDuplicateKeyException ex)
		{
			string message2;
			switch ((ex.InnerException is SqlException ex2) ? new int?(ex2.Number) : ((int?)null))
			{
			case 2627:
				message2 = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryAlreadyMounted, mountTenantDatabaseArguments.TenantDatabaseSettings.Id);
				break;
			case 2601:
				message2 = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseMountedWithAnotherId, mountTenantDatabaseArguments.TenantDatabaseSettings.DatabaseInfo.Name);
				break;
			default:
				throw;
			}
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000QX", Category.TenantDatabaseManagement, new NavTenantDatabaseException(message2));
		}
		return new NavTenantDatabaseSettings(tenantDatabaseConfiguration.Id, tenantDatabaseConfiguration.ToDatabaseInfo(), supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, initialState, isSharedSchema);
	}

	public static void CheckTenantDatabaseVersion(NavDatabase tenantDatabase)
	{
		if (tenantDatabase.TenantDatabaseProperties.VersionNo > (int)NavSqlTenantPropertiesSync.CurrentVersion && !tenantDatabase.TenantDatabaseVersion.CheckIfSupportsApplicationVersion(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion))
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnSupportedTenantDatabaseVersion, tenantDatabase.DatabaseName, tenantDatabase.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
		}
	}

	public NavTenantDatabaseSettings GetTenantDatabaseSynchronizeStatus(string id)
	{
		NavTenantDatabaseSettings navTenantDatabaseSettings = null;
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavEnvironment.Instance.Tenants.SystemTenant.Database))
		{
			TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(navSqlConnectionScope.Connection.Database);
			TenantDatabaseConfiguration tenantDatabase = tenantDatabaseConfigurationStorage.GetTenantDatabase(navSqlConnectionScope, id);
			using NavExternalTenant navExternalTenant = new NavExternalTenant(tenantDatabase, NavDatabaseType.Tenant);
			using NavSqlConnectionScope tenantDatabaseLockScope = NavSqlConnectionScope.CreateForceNewConnection(navExternalTenant.Database, TransactionType.Update);
			KeyValuePair<string, List<string>> supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(navExternalTenant.Database);
			navTenantDatabaseSettings = new NavTenantDatabaseSettings(tenantDatabase.Id, tenantDatabase.ToDatabaseInfo(), supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, new NavTenantDatabaseSettingsDetailedState(), navExternalTenant.Database.IsSharedTenantSchema);
			using NavSqlDatabaseLock navSqlDatabaseLock = NavDatabase.AcquireSyncTenantDatabaseCrossProcessLock(tenantDatabaseLockScope);
			if (!navSqlDatabaseLock.IsLockAcquired())
			{
				navTenantDatabaseSettings.State = NavTenantDatabaseState.SyncInProgress;
				navTenantDatabaseSettings.DetailedState = new NavTenantDatabaseSettingsDetailedState(navTenantDatabaseSettings.State);
			}
			else
			{
				NavTenantDatabaseSettingsDetailedState navTenantDatabaseSettingsDetailedState = TenantDatabaseConfiguration.CalculateTenantDatabaseState(navExternalTenant.Database, supportedFamilyAndVersions.Value, supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily);
				navTenantDatabaseSettings.State = navTenantDatabaseSettingsDetailedState.State;
				navTenantDatabaseSettings.DetailedState = navTenantDatabaseSettingsDetailedState;
			}
		}
		return navTenantDatabaseSettings;
	}

	public int SynchronizeTenantsDatabase(SynchronizeTenantsDatabaseArguments synchronizeTenantsDatabaseArguments)
	{
		string id = synchronizeTenantsDatabaseArguments.Id;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000R2", Category.TenantDatabaseManagement, Verbosity.Normal, "Synchronizing tenant database with id={0}", DiagnosticParameter.Oii(id));
		if (!NavEnvironment.Instance.Tenants.IsSystemTenantCreated)
		{
			throw new NavApplicationNotMountedException(string.Format(CultureInfo.CurrentCulture, Lang.ApplicationNotMounted, ProductNames.ProductNameShort));
		}
		try
		{
			TenantDatabaseConfiguration tenantDatabaseConfiguration = null;
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavEnvironment.Instance.Tenants.SystemTenant.Database))
			{
				TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(navSqlConnectionScope.Connection.Database);
				tenantDatabaseConfiguration = tenantDatabaseConfigurationStorage.GetTenantDatabase(navSqlConnectionScope, id);
				if (tenantDatabaseConfiguration == null)
				{
					string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, id);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SE", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message);
					throw new NavTenantDatabaseException(message);
				}
			}
			using NavExternalTenant navExternalTenant = new NavExternalTenant(tenantDatabaseConfiguration, NavDatabaseType.Tenant);
			CanTenantDatabaseSynchronize(navExternalTenant.Database);
			using NavSqlConnectionScope tenantDatabaseLockScope = NavSqlConnectionScope.CreateForceNewConnection(navExternalTenant.Database, TransactionType.Browse);
			using NavSqlDatabaseLock navSqlDatabaseLock = NavDatabase.AcquireSyncTenantDatabaseCrossProcessLock(tenantDatabaseLockScope);
			if (!navSqlDatabaseLock.IsLockAcquired())
			{
				string message2 = string.Format(CultureInfo.InvariantCulture, "The tenant database {0} cannot be synchronized because the schema synchronization is already running on this server.", id);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RS", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message2);
				throw new NavTenantDatabaseException(message2);
			}
			bool flag = synchronizeTenantsDatabaseArguments.Async;
			if (flag)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000S5", Category.TenantDatabaseManagement, Verbosity.Normal, "Synchronization of the tenant database {0} was requested to run asynchroniously, but it is currently not supported. Continuing synchroniously.", DiagnosticParameter.Oii(id));
				flag = false;
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RT", Category.TenantDatabaseManagement, Verbosity.Normal, "Begin SynchronizeTenantsDatabase: {0}", DiagnosticParameter.Oii(navExternalTenant.Database.NormalizedDatabaseName));
			int result = navExternalTenant.Database.SynchronizeTenantDatabase(synchronizeTenantsDatabaseArguments.SyncMode, flag, synchronizeTenantsDatabaseArguments.PerTable, synchronizeTenantsDatabaseArguments.SharedSchema);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RU", Category.TenantDatabaseManagement, Verbosity.Normal, "End SynchronizeTenantsDatabase: {0}", DiagnosticParameter.Oii(navExternalTenant.Database.NormalizedDatabaseName));
			if (synchronizeTenantsDatabaseArguments.SyncMode == SyncMode.CheckOnly)
			{
				return result;
			}
			return result;
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000RW", Category.TenantDatabaseManagement, ex);
			throw new NavTenantDatabaseException(ex.Message);
		}
	}

	internal static void CanTenantDatabaseSynchronize(NavDatabase database)
	{
		KeyValuePair<string, List<string>> supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(database);
		NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
		if (!TenantDatabaseConfiguration.IsApplicationFamilyCompatible(supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily) || !TenantDatabaseConfiguration.IsApplicationVersionSynchronizable(supportedFamilyAndVersions.Value, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion))
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseCannotBeSynchronized, database.NormalizedDatabaseName, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily, string.Join(",", supportedFamilyAndVersions.Value), supportedFamilyAndVersions.Key);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000T4", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message);
			throw new NavTenantDatabaseException(message);
		}
	}

	public SyncNavAppResponse SyncNavApp(SyncNavAppArguments syncNavAppArguments)
	{
		return NavAppSyncManagementTasks.SyncNavApp(syncNavAppArguments);
	}
}
