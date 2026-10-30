using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTenantManagementTasks : ITenantManagementService
{
	public DataTable GetApplication()
	{
		DataTable dataTable = new DataTable("Nav Application");
		dataTable.Locale = CultureInfo.CurrentCulture;
		dataTable.Columns.Add("Database server");
		dataTable.Columns.Add("Database name");
		dataTable.Columns.Add("ApplicationVersion");
		dataTable.Columns.Add("ApplicationFamily");
		if (NavEnvironment.Instance.Tenants.IsSystemTenantCreated)
		{
			NavSystemTenant systemTenant = NavEnvironment.Instance.Tenants.SystemTenant;
			dataTable.Rows.Add(systemTenant.Database.DatabaseServer.ServerName, systemTenant.Database.DatabaseName, systemTenant.Database.SqlDatabaseProperties.ApplicationVersion, systemTenant.Database.SqlDatabaseProperties.ApplicationFamily);
		}
		return dataTable;
	}

	public void SetApplication(Version applicationVersion, string applicationFamily, bool force, bool incrementApplicationVersion)
	{
		NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
		if (incrementApplicationVersion)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.IncrementApplicationVersion();
		}
		else
		{
			if (applicationVersion > NavSqlTenantProperties.UninitializedVersion)
			{
				NavGlobal.AppDatabase.SqlDatabaseProperties.UpdateDatabasePropertiesField("applicationversion", applicationVersion.ToString(), force);
			}
			if (applicationFamily != null)
			{
				NavGlobal.AppDatabase.SqlDatabaseProperties.UpdateDatabasePropertiesField("applicationfamily", applicationFamily, force);
			}
		}
		NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
	}

	public void MountApplication(MountApplicationArguments mountApplicationArguments)
	{
		bool value = ServerUserSettings.Instance.Multitenant.Value;
		ServerUserSettings.Instance.Multitenant.Value = true;
		try
		{
			if (NavEnvironment.Instance.Tenants.IsSystemTenantCreated)
			{
				throw new NavApplicationAlreadyMountedException("The application has already been mounted on the server instance.");
			}
			try
			{
				NavEnvironment.Instance.Tenants.AddSystemTenant(mountApplicationArguments.DatabaseServer, mountApplicationArguments.DatabaseName, mountApplicationArguments.DatabaseUserName, mountApplicationArguments.DatabasePassword, verifyConnection: true, null, new DatabasePasswordPublicEncryptionKeyStorage());
			}
			catch (Exception ex)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The NAV application could not be mounted for database '{0}' on database server '{1}' due to the following error: {2}", mountApplicationArguments.DatabaseName, mountApplicationArguments.DatabaseServer, ex.Message);
				throw new NavApplicationMountFailedException(message);
			}
			NavEnvironment.Instance.Tenants.PerformInitialMountingOfKnownTenants();
		}
		catch (Exception)
		{
			ServerUserSettings.Instance.Multitenant.Value = value;
			throw;
		}
	}

	public NavTenantRuntimeSettingsList GetTenants(string tenantId, bool checkSpecifiedTenant = false)
	{
		bool useTenantDatabaseToCalculateState = false;
		bool refreshState = true;
		IEnumerable<NavTenant> source;
		if (!string.IsNullOrEmpty(tenantId))
		{
			useTenantDatabaseToCalculateState = true;
			NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true, enablePolling: true);
			if (checkSpecifiedTenant)
			{
				try
				{
					tenantById?.Database?.ValidateDatabaseState();
				}
				catch (NavSqlDatabaseSyncException)
				{
					refreshState = false;
				}
				catch (NavBaseException ex2) when (NavTenant.IsSqlConnectionException(ex2) || tenantById.State == NavTenantState.Failed || tenantById.IsDisposed)
				{
					refreshState = false;
					tenantById.Diagnostics.SendExceptionTag("00008Z3", Category.SqlConnection, ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Failed to ValidateDatabaseState using the tenant database"));
					NavEnvironment.Instance.Tenants.SetTenantToFailedState(tenantById.TenantSettings);
					tenantById.DetailedState = new NavTenantDatabaseSyncStatus(0, 0, ex2.Message);
				}
			}
			source = new List<NavTenant> { tenantById };
		}
		else
		{
			source = NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: false, includeFailedTenants: true);
		}
		return new NavTenantRuntimeSettingsList((from t in source
			select t.GetRuntimeTenantSettings(useTenantDatabaseToCalculateState, refreshState && t.State != NavTenantState.Failed) into t
			orderby t.Id
			select t).ToList());
	}

	public string SetTenant(SetTenantArguments arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (arguments is BufferTenantArguments bufferTenant)
		{
			return SetBufferTenant(bufferTenant);
		}
		throw new NavNCLNotSupportedOperationException();
	}

	private static TenantId SetBufferTenant(BufferTenantArguments bufferTenantArguments)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VQ", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Turning tenant {bufferTenantArguments.TenantId} in database {bufferTenantArguments.TenantDatabaseId} into a buffer tenant."));
		string tenantDatabaseId = bufferTenantArguments.TenantDatabaseId;
		TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(tenantDatabaseId);
		if (tenantDatabaseConfiguration == null)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VT", Category.Tenant, NavTenantDatabaseException.CreateForDatabaseEntryNotFound(tenantDatabaseId));
		}
		if (NavEnvironment.Instance.Tenants.TenantsOnTheSameSqlDatabase(tenantDatabaseId).Any((NavTenant t) => t.Id == bufferTenantArguments.TenantId && t.State != NavTenantState.Dismounted))
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VR", Category.Tenant, NavSetTenantException.CreateForSettingMountedTenantToBuffer(new TenantId(bufferTenantArguments.TenantId)));
		}
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(tenantDatabaseConfiguration.DatabaseServer, tenantDatabaseConfiguration.DatabaseUserName, tenantDatabaseConfiguration.DatabasePassword);
		NavTenantSettings tenantSettings = new NavTenantSettings(bufferTenantArguments.TenantId, bufferTenantArguments.TenantDatabaseId)
		{
			DatabaseName = tenantDatabaseConfiguration.DatabaseName,
			DatabaseServer = tenantDatabaseConfiguration.DatabaseServer,
			DatabaseUserName = tenantDatabaseConfiguration.DatabaseUserName,
			ProtectedDatabasePassword = tenantDatabaseConfiguration.DatabasePassword
		};
		NavTenant tenant = new NavTenant(tenantSettings, orAddDatabaseServerByName);
		int num = 10000;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (true)
		{
			try
			{
				CheckTenantDatabaseVersion(tenant);
				using NavTenant navTenant = new NavTenant(tenantSettings, orAddDatabaseServerByName, NavTenantState.Dismounted);
				NavSqlTenantProperties tenantProperties = navTenant.Database.TenantProperties;
				if (tenantProperties.IsNewTenant)
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000XC", Category.Tenant, NavTenantNotFoundException.CreateForGeneralNotFound(bufferTenantArguments.TenantId));
				}
				if (tenantProperties.TenantType != NavTenantType.Normal)
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VU", Category.Tenant, NavSetTenantException.CreateForSettingNonNormalTenantToBuffer(new TenantId(bufferTenantArguments.TenantId)));
				}
				tenantProperties.SetTenantType(NavTenantType.Buffer);
				TenantId tenantId = new TenantId(Guid.NewGuid().ToString("N"));
				tenantProperties.ChangeDismountedTenantId(tenantId);
				return tenantId;
			}
			catch (NavTenantNotFoundException)
			{
				throw;
			}
			catch (NavSetTenantException)
			{
				throw;
			}
			catch (Exception) when (stopwatch.ElapsedMilliseconds <= bufferTenantArguments.TimeoutSeconds * 1000)
			{
				Thread.Sleep(num);
				num = Math.Min(num * 2, 300000);
			}
		}
	}

	public IEnumerable<NavAppTableModificationResponse> GetNavAppTableModifications(GetNavAppInfoArguments getNavAppInfoArguments)
	{
		string name = getNavAppInfoArguments.Name;
		string appId = getNavAppInfoArguments.AppId;
		string publisher = getNavAppInfoArguments.Publisher;
		Version version = getNavAppInfoArguments.Version;
		Version compatibilityId = getNavAppInfoArguments.CompatibilityId;
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		return from c in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata()
			where (string.IsNullOrEmpty(name) || c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(appId) || ((object)c.AppId).ToString().Equals(appId, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || c.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || version.Equals(c.Version)) && (compatibilityId == null || compatibilityId.Equals(c.CompatibilityId))
			select NavAppTypeExtensions.ToServerResponseType(c);
	}

	public IEnumerable<NavAppTableModificationResponse> GetNavAppTableModificationsForTenant(string tenantId)
	{
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		return NavAppGroup.BaseGroup.OrderedAppMetadata.Select((NavAppRuntimeMetadata c) => NavAppTypeExtensions.ToServerResponseType(c));
	}

	public void NewTenant(NewTenantArguments arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VI", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Creating new tenant {arguments.Settings.Id} in tenant database ID {arguments.Settings.TenantDatabaseId}"));
		TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(arguments.Settings.TenantDatabaseId);
		if (tenantDatabaseConfiguration == null)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VJ", Category.Tenant, new NavTenantDatabaseException(string.Format(CultureInfo.CurrentCulture, Lang.TenantsCanOnlyBeCreatedInMountedDatabases, arguments.Settings.TenantDatabaseId)));
		}
		arguments.Settings.DatabaseServer = tenantDatabaseConfiguration.DatabaseServer;
		arguments.Settings.DatabaseName = tenantDatabaseConfiguration.DatabaseName;
		arguments.Settings.DatabaseUserName = tenantDatabaseConfiguration.DatabaseUserName;
		arguments.Settings.ProtectedDatabasePassword = tenantDatabaseConfiguration.DatabasePassword;
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(tenantDatabaseConfiguration.DatabaseServer, tenantDatabaseConfiguration.DatabaseUserName, tenantDatabaseConfiguration.DatabasePassword);
		int num = 10000;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (true)
		{
			try
			{
				using NavTenant navTenant = new NavTenant(arguments.Settings, orAddDatabaseServerByName);
				if (!navTenant.Database.IsSharedTenantSchema && navTenant.Database.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() != 0)
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000X7", Category.Tenant, NavNewTenantException.Create(arguments.Settings.TenantDatabaseId));
				}
				CheckTenantDatabaseVersion(navTenant);
				if (!navTenant.Database.TenantProperties.RegisterNewTenant())
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VK", Category.Tenant, NavTenantAlreadyExistsException.Create(arguments.Settings.Id, arguments.Settings.TenantDatabaseId));
				}
				if (navTenant.Database.IsSharedTenantSchema)
				{
					navTenant.Database.DatabaseSync.CreateSequencesForTenantSystemAndNotPerCompanyTables();
				}
				break;
			}
			catch (NavNewTenantException)
			{
				throw;
			}
			catch (NavTenantAlreadyExistsException)
			{
				throw;
			}
			catch (Exception) when (stopwatch.ElapsedMilliseconds <= arguments.TimeoutSeconds * 1000)
			{
				Thread.Sleep(num);
				num = Math.Min(num * 2, 300000);
			}
		}
	}

	public int GetNumberOfBufferTenants(GetNumberOfBufferTenantsArguments arguments)
	{
		DatabaseInfo databaseInfo = null;
		if (!string.IsNullOrEmpty(arguments.DatabaseInfo?.Name))
		{
			databaseInfo = arguments.DatabaseInfo;
		}
		else if (!string.IsNullOrEmpty(arguments.DatabaseId))
		{
			databaseInfo = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(arguments.DatabaseId)?.ToDatabaseInfo();
			if (databaseInfo == null)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, arguments.DatabaseId);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000088A", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, message);
				throw new NavTenantDatabaseException(message);
			}
		}
		if (databaseInfo != null)
		{
			return BufferTenantManagement.GetNumberOfBufferTenants(databaseInfo.Name, databaseInfo.Server, databaseInfo.UserName, databaseInfo.Password, arguments.IncludeObsoleteBufferTenants);
		}
		int num = 0;
		foreach (NavDatabaseServer allServer in NavEnvironment.Instance.DatabaseServers.AllServers)
		{
			HashSet<string> seenDatabases = new HashSet<string>();
			foreach (NavDatabase item in allServer.DatabasesOnServer)
			{
				if (IsUnseenTenantDatabase(item, seenDatabases))
				{
					num += BufferTenantManagement.GetNumberOfBufferTenants(item, arguments.IncludeObsoleteBufferTenants);
				}
			}
		}
		return num;
	}

	public NavTenantSettings RegisterTenant(RegisterTenantArguments arguments)
	{
		NavTenantDatabaseManagementTasks navTenantDatabaseManagementTasks = new NavTenantDatabaseManagementTasks();
		if (string.IsNullOrEmpty(arguments.TenantDatabaseId))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000TH", Category.Tenant, Verbosity.Normal, "Atempting to register a tenant in any mounted database under the new ID {0}.", DiagnosticParameter.Oii(arguments.RegisteredTenantId));
			string applicationVersion = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
			foreach (TenantDatabaseConfiguration allTenantDatabaseConfiguration in NavEnvironment.Instance.TenantDatabases.Configuration.AllTenantDatabaseConfigurations)
			{
				if (navTenantDatabaseManagementTasks.GetTenantDatabase(allTenantDatabaseConfiguration.Id).Single().State != NavTenantDatabaseState.Operational)
				{
					continue;
				}
				using NavTenant navTenant = CreateTempTenant(allTenantDatabaseConfiguration);
				if (BufferTenantManagement.RegisterBufferTenant(navTenant.Database, arguments.RegisteredTenantId, applicationVersion))
				{
					return new NavTenantSettings(arguments.RegisteredTenantId, allTenantDatabaseConfiguration.Id);
				}
			}
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000TC", Category.Tenant, NavNoBufferTenantsAvailableException.Create());
		}
		TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(arguments.TenantDatabaseId);
		if (tenantDatabaseConfiguration == null)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000VM", Category.Tenant, NavTenantDatabaseException.CreateForDatabaseEntryNotFound(arguments.TenantDatabaseId));
		}
		NavTenantDatabaseState state = navTenantDatabaseManagementTasks.GetTenantDatabase(tenantDatabaseConfiguration.Id).Single().State;
		if (state != NavTenantDatabaseState.Operational)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000016P", Category.Tenant, NavTenantDatabaseException.CreateForCannotRegisterIfDatabaseNotOperational(arguments.TenantDatabaseId, state));
		}
		using NavTenant navTenant2 = CreateTempTenant(tenantDatabaseConfiguration, arguments.BufferTenantId ?? "temp");
		if (string.IsNullOrEmpty(arguments.BufferTenantId))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000TI", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Atempting to register a tenant in database {0} on server {1} under the new ID {2}.", tenantDatabaseConfiguration.DatabaseName, tenantDatabaseConfiguration.DatabaseServer, arguments.RegisteredTenantId);
			string applicationVersion2 = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
			if (BufferTenantManagement.RegisterBufferTenant(navTenant2.Database, arguments.RegisteredTenantId, applicationVersion2))
			{
				return new NavTenantSettings(arguments.RegisteredTenantId, arguments.TenantDatabaseId);
			}
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000TD", Category.Tenant, NavNoBufferTenantsAvailableException.CreateForDatabase(arguments.TenantDatabaseId, applicationVersion2));
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000TJ", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Atempting to register tenant {0} in database {1} on server {2} under the new ID {3}.", arguments.BufferTenantId, tenantDatabaseConfiguration.DatabaseName, tenantDatabaseConfiguration.DatabaseServer, arguments.RegisteredTenantId);
		BufferTenantManagement.RegisterBufferTenant(navTenant2.Database, arguments.RegisteredTenantId, arguments.BufferTenantId, arguments.Force);
		return new NavTenantSettings(arguments.RegisteredTenantId, arguments.TenantDatabaseId);
	}

	private static NavTenant CreateTempTenant(TenantDatabaseConfiguration config, string tenantId = "temp")
	{
		NavTenantSettings tenantSettings = new NavTenantSettings(tenantId, config.Id);
		NavEnvironment.Instance.TenantDatabases.Configuration.FillDatabaseInformation(tenantSettings);
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(config.DatabaseServer, config.DatabaseUserName, config.DatabasePassword);
		return new NavTenant(tenantSettings, orAddDatabaseServerByName);
	}

	private static bool IsUnseenTenantDatabase(NavDatabase database, ICollection<string> seenDatabases)
	{
		if (database.Tenant.TenantType != NavTenantType.Normal)
		{
			return false;
		}
		if (seenDatabases.Contains(database.DatabaseName))
		{
			return false;
		}
		seenDatabases.Add(database.DatabaseName);
		return true;
	}

	public int CopyTenantData(CopyTenantDataArguments arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		NavTenant mountedTenant = GetMountedTenant(arguments.SourceTenantId);
		NavTenant mountedTenant2 = GetMountedTenant(arguments.DestinationTenantId);
		if (!string.Equals(mountedTenant.TenantSettings.DatabaseServer, mountedTenant2.TenantSettings.DatabaseServer, StringComparison.OrdinalIgnoreCase) || !string.Equals(mountedTenant.TenantSettings.DatabaseName, mountedTenant2.TenantSettings.DatabaseName, StringComparison.OrdinalIgnoreCase))
		{
			throw new NavTenantDataCopyException("It is only possible to copy tenant data to a tenant that is in the same tenant database.");
		}
		if (mountedTenant.Database.TenantProperties.TenantDataVersion != NavSqlTenantProperties.UninitializedVersion.ToString() && (NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion != mountedTenant.Database.TenantProperties.TenantDataVersion || mountedTenant.Database.TenantProperties.TenantDataVersion != mountedTenant2.Database.TenantProperties.TenantDataVersion))
		{
			throw new NavTenantDataCopyException(string.Format(CultureInfo.InvariantCulture, "The tenant data cannot be copied because the application version of the source tenant, destination tenant, and {3} instance must be identical. Source tenant application version: {1}, Destination tenant application version: {2}, Server instance application version: {0}.", NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, mountedTenant.Database.TenantProperties.TenantDataVersion, mountedTenant2.Database.TenantProperties.TenantDataVersion, ProductNames.ProductNameServer));
		}
		return mountedTenant2.Database.CopyTenantData(mountedTenant, arguments.Async);
	}

	public int RemoveTenant(RemoveTenantArguments arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		NavTenant tenant = GetMountedTenant(arguments.TenantId);
		if (!tenant.Database.IsSharedTenantSchema)
		{
			throw new NavTenantRemoveException("You can only use the Remove-NAVTenant cmdlet to remove a tenant when the tenant database uses shared schema. If the tenant database does not use shared schema, you can dismount the tenant, and then delete the database manually, for example, by using SQL Server Management Studio.");
		}
		switch (tenant.Database.TenantProperties.TenantType)
		{
		case NavTenantType.Deleted:
			throw new NavTenantRemoveException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} is already removed. Use Get-NAVTenant to get further information.", tenant.Id));
		case NavTenantType.Deleting:
			if ((DateTime.UtcNow - tenant.Database.TenantProperties.LastModified).TotalDays < 1.0)
			{
				throw new NavTenantRemoveException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} is currently being removed. Use Get-NAVTenant to get further information.", tenant.Id));
			}
			break;
		}
		if (arguments.ForceImmediateDeletion)
		{
			return tenant.Database.RemoveTenant(arguments.Async);
		}
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession systemSession) =>
		{
			tenant.Database.TenantProperties.SetTenantType(NavTenantType.SoftDeleted);
			NavTaskSchedulerUtility.ScheduleSystemTask(SystemTaskActionId.RemoveTenant, new RemoveTenantSystemTask.RemoveTenantSystemTaskParameters(), DateTime.UtcNow);
		}), true, false, false, (LanguageSetting)null);
		return 0;
	}

	public bool MountTenant(MountTenantArguments mountTenantArguments)
	{
		if (!NavEnvironment.Instance.Tenants.IsSystemTenantCreated)
		{
			throw new NavApplicationNotMountedException(string.Format(CultureInfo.CurrentCulture, Lang.ApplicationNotMounted, ProductNames.ProductNameShort));
		}
		try
		{
			NavEnvironment.Instance.TenantDatabases.Configuration.FillDatabaseInformation(mountTenantArguments.TenantSettings);
			CleanUpOrphanedDeveloperExtensionsOnMount(mountTenantArguments.TenantSettings);
			WireUpTenantInstalledAppsOnMountAndRollbackIfFailed(mountTenantArguments.TenantSettings);
			NavEnvironment.Instance.Tenants.AddTenant(new NavTenantMountingParameters
			{
				TenantSettings = mountTenantArguments.TenantSettings,
				OverwriteTenantIdInDatabase = mountTenantArguments.OverwriteTenantIdInDatabase,
				VerifyDatabaseConnection = true,
				VerifyServerInstanceKey = mountTenantArguments.VerifyServerInstanceKey,
				EnsureDatabaseExclusiveStateWhileOverwritingTenantId = mountTenantArguments.SetSingleUserWhileOverwritingTenantId,
				AllowAsync = mountTenantArguments.Async
			});
		}
		catch (NavBaseException ex)
		{
			if (ex.InnerException != null && ((SqlException)ex.InnerException).Number == 5070)
			{
				if (mountTenantArguments.ThrowOnSingleUserError)
				{
					throw;
				}
				return false;
			}
			throw;
		}
		NavEnvironment.Instance.Tenants.Configuration.AddTenantConfiguration(mountTenantArguments.TenantSettings);
		return true;
	}

	public NavTenantSettings DismountTenant(DismountTenantArguments arguments = null, string tenantId = null)
	{
		tenantId = arguments.TenantId;
		NavTenant tenant = null;
		if (!NavEnvironment.Instance.Tenants.TryGetTenantById(tenantId, out tenant, includeSystemTenant: false, includeFailed: true))
		{
			return null;
		}
		tenantId = tenant.Id;
		NavTenantSettings result = null;
		RunActionWithElevatedExclusiveAccessIfTicketAvailiable(tenant, arguments.ExclusiveAccessTicket, () =>
		{
			NavTenant navTenant = tenant;
			if (navTenant != null && navTenant.IsDatabaseInitialized)
			{
				bool flag = true;
				if (!string.IsNullOrEmpty(tenant.TenantSettings.TenantDatabaseId))
				{
					flag = NavEnvironment.Instance.Tenants.TenantsOnTheSameSqlDatabase(tenant.TenantSettings.TenantDatabaseId).Count() == 1;
				}
				if (flag && tenant.Database != null)
				{
					DeadlockAnalyzer.CleanupEventSessionForDatabase(tenant.Database);
				}
			}
			NavEnvironment.Instance.Tenants.OnTenantDismounting(tenantId);
			NavEnvironment.Instance.Tenants.Configuration.RemoveTenantConfiguration(tenantId);
			UnregisterTenantInTenantDatabaseIfDeleted(tenant);
			IncrementConnectionPoolBumperToken(tenant);
			result = NavEnvironment.Instance.Tenants.RemoveTenantAndReturn(tenantId);
			NavEnvironment.Instance.Tenants.OnTenantDismounted(tenantId);
			NavAppInstallationManagement.PurgeInstalledAndPublishedTenantAppsFromApplicationDatabase(NavEnvironment.Instance.Tenants.SystemTenant.Database, tenantId);
		});
		return result;
	}

	private static void IncrementConnectionPoolBumperToken(NavTenant tenant)
	{
		if (tenant != null && tenant.TenantSettings != null && tenant.Database != null && string.IsNullOrEmpty(tenant.TenantSettings.TenantDatabaseId))
		{
			tenant.Database.DatabaseServer.SqlConnectionPool.IncrementBumperToken(tenant.Database);
		}
	}

	private static void UnregisterTenantInTenantDatabaseIfDeleted(NavTenant tenant)
	{
		try
		{
			if (tenant != null)
			{
				NavDatabase database = tenant.Database;
				if (database != null && database.TenantProperties.TenantType == NavTenantType.Deleted)
				{
					bool flag = tenant.Database.TenantProperties.UnregisterDeletedTenant();
					tenant.Diagnostics.SendTraceTag("00001KD", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Tenant {tenant.Id} unregister ended. Result = {flag}"));
				}
			}
		}
		catch (Exception ex) when (ex is NavBaseException || (ex is SqlException ex2 && !NavSqlReturnCode.IsTransientError(ex2.Number)))
		{
			tenant.Diagnostics.SendExceptionTag("00001R7", Category.Tenant, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("Exception occurred while trying to dismount."));
		}
	}

	private static void RunActionWithElevatedExclusiveAccessIfTicketAvailiable(NavTenant tenant, string exclusiveAccessTicket, Action action)
	{
		if (string.IsNullOrEmpty(exclusiveAccessTicket))
		{
			action();
			return;
		}
		using (new ExclusiveTenantActionElevatedScope(new ExclusiveTenantActionScope(tenant, exclusiveAccessTicket, NavEnvironment.Instance.Tenants.Configuration)))
		{
			action();
		}
	}

	public int SynchronizeTenantDatabase(SynchronizeTenantDatabaseArguments synchronizeTenantDatabaseArguments)
	{
		NavTenant mountedTenant = GetMountedTenant(synchronizeTenantDatabaseArguments.TenantId);
		DiagnosticsResolver.GetMostSpecificInstance(mountedTenant).SendTraceTag("00009IT", Category.DatabaseSync, Verbosity.Normal, "Initiating SynchronizeTenantDatabase." + Environment.NewLine + "TenantId: {0}, DatabaseName: {1}", DiagnosticParameter.Oii(mountedTenant.Id), DiagnosticParameter.Oii(mountedTenant.Database.DatabaseName));
		if (mountedTenant.Database.IsSharedTenantSchema && mountedTenant.Database.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() > 1)
		{
			throw NavSyncTenantInDatabaseWithMultipleTenantsException.Create(mountedTenant.Id, mountedTenant.TenantSettings.TenantDatabaseId);
		}
		NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(mountedTenant.Database);
		int result = mountedTenant.Database.SynchronizeTenantDatabase(synchronizeTenantDatabaseArguments.SyncMode, synchronizeTenantDatabaseArguments.Async, synchronizeTenantDatabaseArguments.PerTable);
		mountedTenant.StartNasIfNotRunning();
		return result;
	}

	public int ConvertTenantDatabaseToSharedSchema(ConvertTenantDatabaseToSharedSchemaArguments convertTenantDatabaseToSharedSchemaArguments)
	{
		NavTenant mountedTenant = GetMountedTenant(convertTenantDatabaseToSharedSchemaArguments.TenantId);
		NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(mountedTenant.Database);
		return mountedTenant.Database.ConvertTenantDatabase(convertTenantDatabaseToSharedSchemaArguments.Async, convertTenantDatabaseToSharedSchemaArguments.PerTable);
	}

	public int GetCurrentSynchronizationTaskId(string tenantId)
	{
		NavTenant mountedTenant = GetMountedTenant(tenantId);
		return mountedTenant.Database.CurrentSynchronizationTaskId;
	}

	public TestTenantDatabaseSchemaResponse TestTenantDatabaseSchema(string tenantId)
	{
		return DatabaseTableSchemaValidator.ValidateSchema(tenantId);
	}

	public IEnumerable<TableSynchSetupForDataUpgradeResponse> GetTableSynchSetupForDataUpgradeFromDatabaseInfo(GetTableSynchSetupForDataUpgradeArguments arguments)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000115", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Getting Table Synch Setup for Tenant Database '{0}' and Application Database '{1}'.", arguments.DatabaseName, NavGlobal.AppDatabase.DatabaseName);
		string databaseUserName = (string.IsNullOrEmpty(arguments.UserName) ? NavGlobal.AppDatabase.DatabaseServer.UserName : arguments.UserName);
		ProtectedDatabasePassword databasePassword = arguments.Password ?? NavGlobal.AppDatabase.DatabaseServer.Password;
		NavDatabaseServer databaseServer = new NavDatabaseServer(arguments.DatabaseServerName, databaseUserName, databasePassword);
		NavTenantSettings tenantSettings = new NavTenantSettings("default", arguments.DatabaseName, arguments.DatabaseServerName, databaseUserName, databasePassword);
		NavTenant navTenant = new NavTenant(tenantSettings, databaseServer);
		NavDatabase database = navTenant.Database;
		bool sortingPropertiesChanged = database.TenantDatabaseProperties.Collation != NavGlobal.AppDatabase.SqlSortingProperties.Collation;
		bool databaseConvertedFromNav = database.TenantDatabaseProperties.VersionNo < 1000;
		return from c in database.DatabaseSync.GetTableChanges(sortingPropertiesChanged, databaseConvertedFromNav)
			select c.ToServerResponseType();
	}

	public NavDataUpgradeProcessExecutionDetails GetTenantDatabaseUpgradeStatus(string tenantId)
	{
		NavTenant mountedTenant = GetMountedTenant(tenantId);
		return mountedTenant.Database.UpgradeManager.GetLegacyUpgradeInformation();
	}

	public NavDataUpgradeProcessExecutionState RetrieveTenantDataUpgradeStatus(string tenantId)
	{
		NavTenant mountedTenant = GetMountedTenant(tenantId);
		NavDataUpgradeProcessExecutionState upgradeInformation = mountedTenant.Database.UpgradeManager.GetUpgradeInformation();
		upgradeInformation.IsTenantInExclusiveAccess = mountedTenant.IsInExclusiveAccessMode;
		return upgradeInformation;
	}

	public NavDataUpgradeProcessExecutionState PollForCompletedTenantDataUpgradeStatus(string tenantId)
	{
		NavDataUpgradeProcessExecutionState navDataUpgradeProcessExecutionState = RetrieveTenantDataUpgradeStatus(tenantId);
		for (int i = 0; i < 10; i++)
		{
			if (navDataUpgradeProcessExecutionState.State != NavWorkflowState.NotStarted)
			{
				break;
			}
			Thread.Sleep(TimeSpan.FromSeconds(2.0));
			navDataUpgradeProcessExecutionState = RetrieveTenantDataUpgradeStatus(tenantId);
		}
		while (navDataUpgradeProcessExecutionState.State == NavWorkflowState.InProgress || navDataUpgradeProcessExecutionState.IsTenantInExclusiveAccess)
		{
			int num = (int)(navDataUpgradeProcessExecutionState.NumericProgress * 100.0);
			if (num != 100)
			{
				Thread.Sleep(1000);
			}
			navDataUpgradeProcessExecutionState = RetrieveTenantDataUpgradeStatus(tenantId);
		}
		return navDataUpgradeProcessExecutionState;
	}

	public void StartTenantDatabaseUpgrade(StartTenantDatabaseUpgradeArguments startTenantDatabaseUpgradeArguments)
	{
		if ((startTenantDatabaseUpgradeArguments.SkipIfAlreadyUpgraded && startTenantDatabaseUpgradeArguments.SkipAppVersionCheck) || (startTenantDatabaseUpgradeArguments.SingleTransaction && startTenantDatabaseUpgradeArguments.ContinueOnErrorEnabled))
		{
			throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidParameterSet));
		}
		NavTenant tenant = GetMountedTenant(startTenantDatabaseUpgradeArguments.TenantId);
		if (!FeatureSwitches.Instance.DisableTenantStateCheckOnSessionCreate && !tenant.CanCreateSession(allowNonoperationalStateConnection: true))
		{
			throw tenant.Diagnostics.SendExceptionTag("00000YJ", Category.Runtime, NavTenantNotAccessibleException.Create(tenant.Id));
		}
		if (IsTenantDataUpgradeNeeded(tenant, startTenantDatabaseUpgradeArguments.SkipAppVersionCheck, startTenantDatabaseUpgradeArguments.SkipIfAlreadyUpgraded, startTenantDatabaseUpgradeArguments.ExcludeExtensions))
		{
			if (!tenant.Database.GetIsDatabaseInSync())
			{
				throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessCannotStartDatabaseNotSynchronized);
			}
			tenant.RefreshState();
			if (tenant.State != NavTenantState.Operational && tenant.State != NavTenantState.OperationalWithDataUpgradeFailure && tenant.State != NavTenantState.OperationalDataUpgradePending)
			{
				throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeProcessCannotStartDueToCurrentTenantState, tenant.State));
			}
			if (!startTenantDatabaseUpgradeArguments.SkipUserSessionCheck)
			{
				CheckTenantSessions(tenant);
			}
			IEnumerable<string> companyNames = from nextToken in tenant.Database.GetCompanyTokens()
				select tenant.Database.GetCompanyNameFromToken(nextToken);
			tenant.Database.UpgradeManager.StartUpgrade(companyNames, startTenantDatabaseUpgradeArguments.Lcid, startTenantDatabaseUpgradeArguments.ParallelExecutionEnabled, startTenantDatabaseUpgradeArguments.ContinueOnErrorEnabled, startTenantDatabaseUpgradeArguments.SkipCompanyInitialization, startTenantDatabaseUpgradeArguments.SingleTransaction, startTenantDatabaseUpgradeArguments.ExcludeExtensions, !startTenantDatabaseUpgradeArguments.SkipUserSessionCheck);
		}
	}

	private static bool IsTenantDataUpgradeNeeded(NavTenant tenant, bool skipAppVersionCheck = false, bool skipIfAlreadyUpgraded = false, bool excludeExtensions = false)
	{
		NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
		tenant.Database.TenantDatabaseProperties.Refresh();
		if (string.IsNullOrEmpty(tenant.Database.TenantProperties.TenantDataVersion))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Y0", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, Lang.SkipEmptyDataTenantVersionCheck);
			return true;
		}
		Version version = new Version(tenant.Database.TenantProperties.TenantDataVersion);
		Version version2 = new Version(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
		if (version >= version2)
		{
			if (skipIfAlreadyUpgraded)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001GM", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, Lang.UpgradeProcessNotNecessaryDueToApplicationVersion, version, version2);
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000WV", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, Lang.UpgradeProcessCannotStartDueToApplicationVersion, version, version2);
			}
			if ((version == version2) & skipAppVersionCheck)
			{
				return true;
			}
			if (!excludeExtensions && version == version2 && NeedToUpgradeExtensions(tenant))
			{
				return true;
			}
			if (skipIfAlreadyUpgraded)
			{
				return false;
			}
			throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeProcessCannotStartDueToApplicationVersion, version, version2) + Lang.CanForceDataUpgrade);
		}
		NavSqlTenantDatabaseVersion tenantDatabaseVersion = tenant.Database.TenantDatabaseVersion;
		string nextSupportedApplicationVersion = tenantDatabaseVersion.GetNextSupportedApplicationVersion(tenant.Database.TenantProperties.TenantDataVersion);
		Version version3 = (string.IsNullOrEmpty(nextSupportedApplicationVersion) ? null : new Version(nextSupportedApplicationVersion));
		if (version3 == null)
		{
			string message = string.Format(CultureInfo.InvariantCulture, "Cannot start the data upgrade process because tenant version is already the latest application version the tenant database supports ({0}). Call Sync-NAVTenantDatabase to sync the tenant database to the current version of the application ({1}).", version, version2);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000117", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
			throw new NavUpgradeProcessExecutionException(message);
		}
		if (version2 != version3)
		{
			string text = string.Format(CultureInfo.CurrentCulture, Lang.NextSupportedApplicationVersionToUpgrade, version3);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000WW", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, Lang.UpgradeProcessCannotStartDueToNextApplicationVersion + text, version3, version2, ProductNames.ProductNameServer);
			if (!skipAppVersionCheck)
			{
				throw new NavUpgradeProcessExecutionException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeProcessCannotStartDueToNextApplicationVersion, version2, version, ProductNames.ProductNameServer) + text + Lang.CanForceDataUpgrade);
			}
		}
		return true;
	}

	private static bool NeedToUpgradeExtensions(NavTenant tenant)
	{
		NavAppPackagePoolSelector poolSelector = NavAppPackagePoolSelector.CreateForUpgradeContextFactory(NavEnvironment.Instance.NavAppMetadataRetriever, (NavSqlConnectionScope tenantConScope) => new DatabaseAppSyncTrackingRepository(tenantConScope.Connection.Database, tenantConScope));
		NavAppWorkflowUpgradeContextFactory navAppWorkflowUpgradeContextFactory = new NavAppWorkflowUpgradeContextFactory(poolSelector, NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase, (NavSqlConnectionScope tenantConScope, string tenantId) => new DatabaseUninstalledAppRepository(tenantConScope.Connection.Database, tenantConScope).GetAllForTenant(tenantId), NavGlobal.NavAppGroupResolver);
		return navAppWorkflowUpgradeContextFactory.IsAppsUpgradeNeeded(tenant, skipUninstalledV2Extensions: true);
	}

	private static void CheckTenantSessions(NavTenant tenant)
	{
		IEnumerable<NavSession> enumerable = tenant.AllSessions.Where((NavSession session) => session != NavCurrentThread.Session && session.Id != -1 && !session.AdminSession);
		IList<NavSession> list = (enumerable as IList<NavSession>) ?? enumerable.ToList();
		if (list.Any())
		{
			NavSession navSession = list.First();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VF", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Check tenant sessions: tenant {0} has {1} sessions, first one: {2} (AppId {3}, Company {4}, IsSystem {5}).", tenant.Id, list.Count, navSession.Id, navSession.AppId, navSession.CompanyName, navSession.IsSystemSession);
			throw new NavUpgradeProcessExecutionException(Lang.UpgradeProcessCannotStartDueToExistingSessions);
		}
	}

	public void ResumeTenantDatabaseUpgrade(ResumeTenantDatabaseUpgradeArguments resumeTenantDatabaseUpgradeArguments)
	{
		NavTenant mountedTenant = GetMountedTenant(resumeTenantDatabaseUpgradeArguments.TenantId);
		if (resumeTenantDatabaseUpgradeArguments.CodeunitId == 0)
		{
			mountedTenant.Database.UpgradeManager.ResumeUpgrade();
		}
		else
		{
			mountedTenant.Database.UpgradeManager.ResumeUpgrade(resumeTenantDatabaseUpgradeArguments.CodeunitId, resumeTenantDatabaseUpgradeArguments.MethodName, resumeTenantDatabaseUpgradeArguments.CompanyName);
		}
	}

	public void StopTenantDatabaseUpgrade(string tenantId)
	{
		NavTenant mountedTenant = GetMountedTenant(tenantId);
		mountedTenant.Database.UpgradeManager.StopUpgrade();
	}

	public void UpdateScheduledTaskList(string tenantId)
	{
		NavTenant mountedTenant = GetMountedTenant(tenantId);
		if (!mountedTenant.Database.IsNewTenantDatabase)
		{
			mountedTenant.RunActionWhenTenantIsMounted((NavTenant t) =>
			{
				NavEnvironment.Instance.TaskScheduler.ForceTaskListSynchronization(t.Id);
			});
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000058", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Tried to mount a tenant but got a sync error. Might be a completely empty database.");
		}
	}

	public string SetTenantExclusiveAccessMode(SetTenantExclusiveAccessModeArguments arguments)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000YM", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "SetTenantExclusiveAccessMode for tenant: {0} with justification: {1}.", arguments.TenantId, arguments.Justification);
		if (string.IsNullOrWhiteSpace(arguments.Justification))
		{
			throw new NavNCLExclusiveActionException("Justification for an exclusive access operation must be provided.");
		}
		NavTenant mountedTenant = GetMountedTenant(arguments.TenantId);
		mountedTenant.MoveTenantToExclusiveAccessMode();
		mountedTenant.StopAllSessions();
		TenantConfiguration tenantConfiguration = NavEnvironment.Instance.Tenants.Configuration.SetTenantConfigurationStateAndWaitWhilePropagated(mountedTenant.Id, TenantConfigurationState.ExclusiveAccess);
		return tenantConfiguration.CalculateExclusiveAccessTicket();
	}

	public void ReleaseTenantExclusiveAccessMode(ReleaseTenantExclusiveAccessModeArguments arguments)
	{
		NavTenant mountedTenant = GetMountedTenant(arguments.TenantId, includeFailed: true);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000YN", Category.Tenant, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "ReleaseTenantExclusiveAccessMode for tenant: {0}, force={1}, justification: {2}.", arguments.TenantId, arguments.Force, arguments.Justification);
		string exclusiveAccessTicket = null;
		if (arguments.Force)
		{
			if (string.IsNullOrWhiteSpace(arguments.Justification))
			{
				throw new NavNCLExclusiveActionException("Justification for an exclusive access operation must be provided.");
			}
		}
		else
		{
			if (string.IsNullOrWhiteSpace(arguments.ExclusiveAccessTicket))
			{
				throw new NavNCLExclusiveActionException("Exclusive access ticket must be provided.");
			}
			exclusiveAccessTicket = arguments.ExclusiveAccessTicket;
		}
		NavEnvironment.Instance.Tenants.Configuration.SetTenantConfigurationStateAndWaitWhilePropagated(mountedTenant.Id, TenantConfigurationState.Active, null, null, exclusiveAccessTicket);
		mountedTenant.RestoreStateFromExclusiveAccessMode();
		mountedTenant.RefreshState();
	}

	public int MoveTenant(MoveTenantArguments arguments)
	{
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		NavTenant mountedTenant = GetMountedTenant(arguments.TenantId);
		if (!mountedTenant.Database.GetIsDatabaseInSync())
		{
			throw new NavTenantDataMoveException(Lang.MoveTenantFailedDatabaseNotSynchronized);
		}
		return mountedTenant.Database.MoveTenantData(new TenantDatabaseId(arguments.DestinationTenantDatabaseId), arguments.Async, arguments.ImmediateSourceTenantRemoval);
	}

	public static bool TenantHasPendingChanges(NavTenant tenant)
	{
		if (tenant == null)
		{
			throw new ArgumentNullException("tenant");
		}
		try
		{
			tenant.Database.ValidateDatabaseState();
		}
		catch (NavBaseException)
		{
		}
		finally
		{
			tenant.RefreshState();
		}
		return tenant.Database.DatabaseSync.AnyPendingTableChanges == PendingTableChangesState.PendingChanges;
	}

	internal static NavTenant GetMountedTenant(string tenantId = null, bool includeFailed = false, bool enablePolling = true)
	{
		NavTenant navTenant = null;
		if (!string.IsNullOrEmpty(tenantId))
		{
			try
			{
				navTenant = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed, enablePolling);
			}
			catch (NavTenantNotFoundException)
			{
				if (!NavEnvironment.Instance.Tenants.IsSingleTenancyMode)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000056", Category.Tenant, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "Tenant {0} does not exist or could not be mounted within the alloted timeframe", tenantId);
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000057", Category.Tenant, Verbosity.Warning, PrivacyClassification.OrganizationIdentifiableInformation, "Specified tenant {0} does not exist in single tenancy mode, attempting to use default tenant", tenantId);
			}
		}
		if (navTenant == null)
		{
			navTenant = NavEnvironment.Instance.Tenants.DefaultTenant;
		}
		if (includeFailed && navTenant.FailedToMount)
		{
			NavServerEventSource.Log.WriteTenantMountFailedEvent(NavEnvironment.ServiceInstanceName, tenantId, string.Format(CultureInfo.CurrentCulture, "{0}\n{1}", string.Format(CultureInfo.InvariantCulture, "Tenant ID: {0}", tenantId), string.Format(CultureInfo.InvariantCulture, "The tenant '{0}' is mounted but cannot be accessed as an error occurred during the mounting process. See the Windows Event Log for error information regarding the specific tenant. The tenant can be dismounted using the Dismount-NAVTenant cmdlet.", tenantId)));
		}
		return navTenant;
	}

	private static void CleanUpOrphanedDeveloperExtensionsOnMount(NavTenantSettings tenantSettings)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Cleaning up orphaned developer extensions on mount -- Tenant:{0}", tenantSettings.Id);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000022F", DiagnosticParameter.Oii(content), (MonitorType)196609, Category.Extensions, createNewActivity: false))
		{
			List<INavAppMetadataReference> source = new List<INavAppMetadataReference>();
			IEnumerable<NavAppRuntimeMetadata> allMetadata = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata();
			IEnumerable<NavAppRuntimeMetadata> apps = allMetadata.Where((NavAppRuntimeMetadata x) => NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(x).IsDeveloperExtensionForTenant(tenantSettings.Id));
			IEnumerable<NavAppRuntimeMetadata> enumerable = NavAppOrderResolver.DetermineProperOrderForApps(apps).Reverse().ToList();
			NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(tenantSettings.DatabaseServer, tenantSettings.DatabaseUserName, tenantSettings.ProtectedDatabasePassword);
			using (NavTenant navTenant = new NavTenant(tenantSettings, orAddDatabaseServerByName))
			{
				NavDatabase database = navTenant.Database;
				if (!database.IsSharedTenantSchema || !database.TenantProperties.IsNewTenant)
				{
					using NavSqlConnectionScope connectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Browse);
					AppPublishedAppStorage appPublishedAppStorage = new AppPublishedAppStorage(database, connectionScope);
					source = appPublishedAppStorage.GetAll();
				}
			}
			foreach (NavAppRuntimeMetadata developerApp in enumerable)
			{
				if (!source.Any((INavAppMetadataReference t) => (WrappedComparable<Guid>)(object)((INavAppReference)t).AppId == (WrappedComparable<Guid>)(object)developerApp.AppId))
				{
					Result val = NavAppUnpublisher.Unpublish(((WrappedValue<Guid>)(object)developerApp.PackageId).Value, developerApp.Name, developerApp.Publisher, developerApp.Version);
					if (val.Success)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000022D", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removed orphaned developer extension for tenant {0}. AppId={1}, Name={2}, Publisher={3}, Version={4}", tenantSettings.Id, developerApp.AppId, developerApp.Name, developerApp.Publisher, developerApp.Version.ToString());
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000022E", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Failed to remove orphaned developer extension for tenant {0}. AppId={1}, Name={2}, Publisher={3}, Version={4}, Error={5}", tenantSettings.Id, developerApp.AppId, developerApp.Name, developerApp.Publisher, developerApp.Version.ToString(), val.Error.Message);
					}
				}
			}
		}
	}

	private static void WireUpTenantInstalledAppsOnMountAndRollbackIfFailed(NavTenantSettings tenantSettings)
	{
		NavGlobal.AppDatabase?.ChangeListener?.ForceHandleTrackingEvent();
		string content = string.Format(CultureInfo.InvariantCulture, "Wiring up tenant's installed extensions on mount -- Tenant:{0}", tenantSettings.Id);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000009B", DiagnosticParameter.Oii(content), (MonitorType)196609, Category.Extensions, createNewActivity: false))
		{
			string id = tenantSettings.Id;
			string tenantId = NavTenant.ConvertTenantIdToExpectedCase(id);
			NavDatabase database = NavEnvironment.Instance.Tenants.SystemTenant.Database;
			NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(tenantSettings.DatabaseServer, tenantSettings.DatabaseUserName, tenantSettings.ProtectedDatabasePassword);
			using NavTenant navTenant = new NavTenant(tenantSettings, orAddDatabaseServerByName);
			NavDatabase database2 = navTenant.Database;
			if (database2.IsSharedTenantSchema && database2.TenantProperties.IsNewTenant)
			{
				return;
			}
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.Update);
			using NavSqlConnectionScope navSqlConnectionScope2 = NavSqlConnectionScope.CreateForceNewConnection(database2, TransactionType.Update);
			List<NavAppRuntimeMetadata> list = ((!NavEnvironment.Instance.StandaloneInstance) ? NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().Where((NavAppRuntimeMetadata m) =>
			{
				NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(m);
				return !publishProperties.IsDeveloperExtension() || publishProperties.IsDeveloperExtensionForTenant(tenantSettings.Id);
			}).ToList() : NavAppInstallationManagement.GetAppsPublishedToApplicationDatabase(navSqlConnectionScope, tenantSettings.Id));
			AppPublishReferenceStorage appPublishReferenceStorage = new AppPublishReferenceStorage(NavGlobal.AppDatabase, navSqlConnectionScope);
			AppPublishedAppStorage appPublishedAppStorage = new AppPublishedAppStorage(database2, navSqlConnectionScope2);
			List<INavAppMetadataReference> all = appPublishedAppStorage.GetAll();
			List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(navSqlConnectionScope2);
			List<AppSyncTrackingRecord> tenantDbSynced = new DatabaseAppSyncTrackingRepository(database2, navSqlConnectionScope2).GetAll(hasBaseline: false).ToList();
			IList<NavAppRuntimeMetadata> list2 = NavAppMountHelper.GetAppsToMarkAsInstalled(list, all, appsInstalledForTenantInTenantDatabase, tenantDbSynced, id).ToList();
			IList<NavAppRuntimeMetadata> list3 = NavAppMountHelper.GetAppsToUpdatePackageIdFor(list2, appsInstalledForTenantInTenantDatabase, id).ToList();
			IList<NavAppRuntimeMetadata> list4 = NavAppMountHelper.GetAppsToMarkAsPublished(list, all, list2, id).ToList();
			if (!NavEnvironment.Instance.StandaloneInstance)
			{
				VerifyAppCombinationIsValid(list2);
			}
			appPublishReferenceStorage.DeletePublishReferencesForTenant(tenantId);
			foreach (NavAppRuntimeMetadata item in list3)
			{
				NavAppInstallationManagement.UpdatePackageIdAndContentHashInForAppInTenantDatabase((INavAppMetadataReference)(object)item, navSqlConnectionScope2);
			}
			foreach (NavAppRuntimeMetadata item2 in list4)
			{
				appPublishReferenceStorage.InsertOrUpdatePublishReference(tenantId, item2.PackageId);
				appPublishedAppStorage.InsertOrUpdate(item2);
			}
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange(tenantId);
			NavAppInstallationManagement.PurgeAndInsertNewTenantAppInstallationRecords(database, list2.Select((NavAppRuntimeMetadata a) => a.PackageId).ToList(), id);
		}
	}

	private static void VerifyAppCombinationIsValid(IEnumerable<NavAppRuntimeMetadata> appsinTenantDatabase)
	{
		try
		{
			NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(appsinTenantDatabase.Select((NavAppRuntimeMetadata m) => m.PackageId));
			NavGlobal.NCLMetadata.InitializeAppGroup(orCreateGroupForAppCombination);
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000009D", Category.Extensions, exception);
			throw;
		}
	}

	private static void CheckTenantDatabaseVersion(NavTenant tenant)
	{
		if (!tenant.Database.TenantDatabaseProperties.TableExists || !tenant.Database.TenantProperties.TableExists || !tenant.Database.IsCurrentVersion)
		{
			throw new NavTenantDatabaseException(string.Format(CultureInfo.InvariantCulture, "The {0} database has version {1} which is not supported by this version of {2}. Run Sync-NAVTenantDatabase to upgrade or initialize the tenant database.", tenant.TenantSettings.TenantDatabaseId, tenant.Database.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
		}
	}
}
