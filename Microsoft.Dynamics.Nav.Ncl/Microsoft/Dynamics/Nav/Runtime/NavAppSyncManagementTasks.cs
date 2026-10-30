using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavAppSyncManagementTasks
{
	public static SyncNavAppResponse SyncNavApp(SyncNavAppArguments args)
	{
		SyncNavAppResponse syncNavAppResponse = new SyncNavAppResponse(SyncNavAppStatus.InProgress);
		string activityId = Guid.NewGuid().ToString();
		string content = string.Format(CultureInfo.InvariantCulture, "Extension sync request -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; TenantDatabaseId:{5}; Origin:{6}", NavAppTraceHelper.CreateActivityIdString(activityId), args.Name, args.Publisher, args.Version, args.TenantId, args.TenantDatabaseId, args.Origin);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016R", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			if (!NavEnvironment.Instance.Tenants.IsSystemTenantCreated)
			{
				throw new NavApplicationNotMountedException(string.Format(CultureInfo.CurrentCulture, Lang.ApplicationNotMounted, ProductNames.ProductNameShort));
			}
			try
			{
				using NavSqlConnectionScope appDbConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase);
				NavTenant navTenant = ResolveTenantForSyncNavApp(appDbConnectionScope, args.TenantId, args.TenantDatabaseId);
				try
				{
					NavAppTenantGuard.BlockSynchronizationIfTenantIsInImproperState(navTenant);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y1", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Checking whether database can sync");
					NavTenantDatabaseManagementTasks.CanTenantDatabaseSynchronize(navTenant.Database);
					NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y2", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Making sure the base is in sync");
					if (NavTenantManagementTasks.TenantHasPendingChanges(navTenant))
					{
						string message = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_MetadataOutOfSync, args.Name);
						throw new NavAppSyncException(message);
					}
					if (args.Mode == NavAppSyncMode.ForceSync && navTenant.Database.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() > 1)
					{
						string message2 = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_ForceSyncWithMultipleTenants);
						throw new NavAppSyncException(message2);
					}
					IEnumerable<NavAppRuntimeMetadata> enumerable = ResolvePublishedAppsForSyncNavApp(navTenant.Id);
					IEnumerable<NavAppRuntimeMetadata> allInstalled = new List<NavAppRuntimeMetadata>();
					if (navTenant.Database.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() == 1)
					{
						allInstalled = navTenant.NavAppGroup.OrderedAppMetadata;
					}
					List<AppSyncTrackingRecord> list;
					using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(navTenant.Database))
					{
						list = new DatabaseAppSyncTrackingRepository(navTenant.Database, conScope).GetAll().ToList();
					}
					SyncTargetResolver syncTargetResolver = new SyncTargetResolver(enumerable, list, allInstalled);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y3", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Running precheck");
					SyncPrecheckHelper syncPrecheckHelper = SyncPrecheckHelper.SyncNavAppPreCheck(syncTargetResolver, args.Name, args.Publisher, args.Version, args.Mode);
					NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
					object[] array = new object[7];
					NavAppRuntimeMetadata appForAdd = syncPrecheckHelper.AppForAdd;
					array[0] = ((appForAdd != null) ? appForAdd.PackageId : null);
					array[1] = syncPrecheckHelper.AppId;
					NavAppRuntimeMetadata appForAdd2 = syncPrecheckHelper.AppForAdd;
					array[2] = ((appForAdd2 != null) ? appForAdd2.Name : null);
					NavAppRuntimeMetadata appForAdd3 = syncPrecheckHelper.AppForAdd;
					array[3] = ((appForAdd3 != null) ? appForAdd3.Publisher : null);
					array[4] = syncPrecheckHelper.TargetVersion;
					array[5] = syncPrecheckHelper.Status;
					array[6] = syncPrecheckHelper.Error;
					mostSpecificInstance.SendTraceTag("00006Y4", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Sync Precheck -- PackageId:{0}; AppId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Status:{5}; Error:{6}", array);
					if (syncPrecheckHelper.Status != SyncNavAppStatus.InProgress)
					{
						syncNavAppResponse = new SyncNavAppResponse(syncPrecheckHelper.Status, syncPrecheckHelper.Error);
					}
					else
					{
						SyncAppGroupResolver syncAppGroupResolver = new SyncAppGroupResolver(enumerable.Where((NavAppRuntimeMetadata app) => app.ExtensionType == NavExtensionType.ModernDev), list, NavEnvironment.Instance.NavAppGroupResolver);
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y5", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Resolving and initializing app groups for sync");
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000017Y", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Resolving App Groups");
						NavAppGroup navAppGroup = syncAppGroupResolver.ResolveGroup(syncPrecheckHelper.AppForAdd, args.Mode);
						NavGlobal.NCLMetadata.InitializeAppGroup(navAppGroup);
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000017Z", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Resolved Group -- Group:{0}", navAppGroup.GroupId);
						AppSyncContext appSyncContext = new AppSyncContext(syncPrecheckHelper.AppForAdd, navAppGroup, syncPrecheckHelper.AppId, syncPrecheckHelper.TargetVersion, syncPrecheckHelper.OldVersion, args.Mode, activityId);
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y6", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SynchronizeTenantDatabase");
						syncNavAppResponse = navTenant.Database.SynchronizeTenantDatabaseForApp(appSyncContext);
					}
				}
				finally
				{
					if (navTenant is NavExternalTenant)
					{
						navTenant.Dispose();
					}
				}
			}
			catch (NavAppSyncValidationException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000231", Category.Extensions, ex);
				syncNavAppResponse = new SyncNavAppResponse(SyncNavAppStatus.SyncValidationFailed, ex.Message);
			}
			catch (Exception ex2)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000155", Category.Extensions, ex2);
				syncNavAppResponse = new SyncNavAppResponse(SyncNavAppStatus.UnknownAppError, ex2.Message);
			}
			finally
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015Q", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension sync request ending -- {0}; Result:{1}; Error:{2}", NavAppTraceHelper.CreateActivityIdString(activityId), syncNavAppResponse?.Result, syncNavAppResponse?.Error);
			}
		}
		return syncNavAppResponse;
	}

	private static NavTenant ResolveTenantForSyncNavApp(NavSqlConnectionScope appDbConnectionScope, string tenantId, string tenantDatabaseId)
	{
		if (!string.IsNullOrEmpty(tenantDatabaseId))
		{
			TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(appDbConnectionScope.Connection.Database);
			TenantDatabaseConfiguration tenantDatabase = tenantDatabaseConfigurationStorage.GetTenantDatabase(appDbConnectionScope, tenantDatabaseId);
			if (tenantDatabase == null)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, tenantDatabaseId);
				throw new NavTenantDatabaseException(message);
			}
			return new NavExternalTenant(tenantDatabase, NavDatabaseType.Tenant);
		}
		if (!string.IsNullOrEmpty(tenantId))
		{
			return NavEnvironment.Instance.Tenants.GetTenantById(tenantId);
		}
		if (NavEnvironment.Instance.Tenants.Count == 1)
		{
			return NavEnvironment.Instance.Tenants.AllTenants().First();
		}
		throw new NavAppSyncException(Lang.NavAppSync_AmbiguousTenantDatabase);
	}

	private static IEnumerable<NavAppRuntimeMetadata> ResolvePublishedAppsForSyncNavApp(string tenantId)
	{
		return NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().Where((NavAppRuntimeMetadata rm) =>
		{
			string developerTenantId = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(rm).DeveloperTenantId;
			return string.IsNullOrEmpty(developerTenantId) || string.Equals(developerTenantId, tenantId, StringComparison.OrdinalIgnoreCase);
		});
	}
}
