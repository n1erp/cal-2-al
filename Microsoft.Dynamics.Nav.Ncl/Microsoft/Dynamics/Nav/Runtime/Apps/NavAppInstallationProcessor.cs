using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public sealed class NavAppInstallationProcessor : NavAppOperationProcessor<NavAppInstallationContext>
{
	private readonly Func<INavAppInstallEventExecutor> executorGenerator;

	public NavAppInstallationProcessor(NavAppInstallationContext context, Func<INavAppInstallEventExecutor> eventExecutorGenerator = null)
		: base(context)
	{
		executorGenerator = eventExecutorGenerator ?? ((Func<INavAppInstallEventExecutor>)(() => NavCurrentThread.Session.SystemCodeunitFactory.ExtensionTriggers));
	}

	public void RunNavAppInstallSpecialRecordActions(bool runInSystemSession, bool doNotLoadData, NavSqlConnectionScope tenantDatabaseConnectionScope = null)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			if (tenantDatabaseConnectionScope == null)
			{
				using (NavSqlConnectionScope tenantDatabaseScope = NavSqlConnectionScope.CreateForceNewConnection(Context.Tenant.Database, TransactionType.Update))
				{
					RunInstallSpecialRecordActions(tenantDatabaseScope, doNotLoadData);
					return;
				}
			}
			RunInstallSpecialRecordActions(tenantDatabaseConnectionScope, doNotLoadData: true);
		});
	}

	public void RunAppDatabaseInstallRecordsUpdate(bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			HandleInstallAppDatabase();
		});
	}

	public void RunNavAppInstallPerCompanyEvent(string companyName, bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			if (!string.IsNullOrWhiteSpace(companyName))
			{
				RaisePerCompanyInstallEvent(session, new string[1] { companyName });
			}
		});
	}

	public void RunNavAppInstallPerDatabaseEvent(bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			RaisePerDatabaseInstallEvent();
		});
	}

	public void ClearGlobalCache(bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			ClearGlobalCache();
		});
	}

	public void InstallNavApp(bool doNotLoadData)
	{
		RunActionInSession(runInNewSystemSession: true, (NavSession session) =>
		{
			DoTransactionPartOfInstall(session, doNotLoadData);
		});
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008E", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB update");
		HandleInstallAppDatabase();
	}

	public void UninstallNavApp(bool doNotSaveData)
	{
		RunActionInSession(runInNewSystemSession: true, (NavSession session) =>
		{
			DoTransactionPartOfUninstall(session, doNotSaveData);
		});
		HandleUninstallAppDatabaseRemoval();
	}

	private void DoTransactionPartOfUninstall(NavSession systemSession, bool doNotSaveData)
	{
		NavAppRuntimeMetadata appBeingTargeted = Context.AppBeingTargeted;
		NavTenant tenant = Context.Tenant;
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenant.Database, TransactionType.Update);
		systemSession.EnsureWriteTransactionStarted();
		try
		{
			if (doNotSaveData)
			{
				NavAppInstallationManagement.PurgeAppTablesForTenant(appBeingTargeted, Context.Tenant.NavAppGroup, tenant);
			}
			HandleSpecialRecordsForUninstall(appBeingTargeted, !doNotSaveData, tenant, navSqlConnectionScope);
		}
		catch (Exception)
		{
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
			throw;
		}
		finally
		{
			ClearGlobalCache();
		}
	}

	internal static void HandleSpecialRecordsForInstall(NavAppRuntimeMetadata metadata, NavTenant tenant, NavSqlConnectionScope tenantDatabaseScope)
	{
		Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(metadata.PackageId);
		NavAppInstallationManagement.AddInstalledAppRecord(tenant, tenantDatabaseScope, (INavAppMetadataReference)(object)metadata);
		new DatabaseUninstalledAppRepository(tenant.Database, tenantDatabaseScope).DeleteAllForApp(metadata.AppId);
		NavAppPermissionManagement.AddPermissionArtifacts(tenant, tenantDatabaseScope, packageBlob, metadata.AppId);
		NavAppWebServiceManagement.AddWebServices(tenant, tenantDatabaseScope, packageBlob, metadata.AppId);
		NavAppControlAddInManagement.AddControlAddIns(tenant, tenantDatabaseScope, packageBlob, metadata.AppId);
		NavAppReportLayoutManagement.AddCustomReportLayouts(tenant, packageBlob, metadata.AppId);
		NavAppProfileManagement.AddProfileArtifacts(tenant, tenantDatabaseScope, metadata.AppId, metadata.PackageId);
	}

	internal static void HandleSpecialRecordsForUninstall(NavAppRuntimeMetadata metadata, bool saveData, NavTenant tenant, NavSqlConnectionScope tenantDatabaseScope)
	{
		Stream packageBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageBlob(metadata.PackageId);
		NavAppInstallationManagement.RemoveInstalledAppRecord(tenant, tenantDatabaseScope, metadata.AppId);
		new DatabaseUninstalledAppRepository(tenant.Database, tenantDatabaseScope).InsertOrUpdate(new UninstalledAppRecord
		{
			AppId = metadata.AppId,
			Name = metadata.Name,
			Publisher = metadata.Publisher,
			Version = metadata.Version,
			TimeOfUninstall = DateTime.Now,
			HasData = saveData
		});
		NavAppPermissionManagement.RemovePermissionArtifacts(tenant, tenantDatabaseScope, metadata.AppId);
		NavAppWebServiceManagement.RemoveWebServices(tenant, tenantDatabaseScope, packageBlob, metadata.AppId);
		NavAppControlAddInManagement.RemoveControlAddins(tenant, tenantDatabaseScope, metadata.AppId);
		NavAppReportLayoutManagement.RemoveCustomReportLayouts(tenant, tenantDatabaseScope, metadata.AppId);
		NavAppProfileManagement.RemoveProfileArtifacts(tenant, tenantDatabaseScope, metadata.AppId);
		if (!saveData)
		{
			IsolatedStorageRepository.DeleteApplicationData(tenantDatabaseScope, ((WrappedValue<Guid>)(object)metadata.AppId).Value);
			NavAppSettingManagement.RemoveApplicationSettings(tenantDatabaseScope, metadata.AppId);
		}
	}

	private void DoTransactionPartOfInstall(NavSession systemSession, bool doNotLoadData)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Context.Tenant.Database, TransactionType.Update);
		systemSession.EnsureWriteTransactionStarted();
		try
		{
			RunInstallSpecialRecordActions(navSqlConnectionScope, doNotLoadData);
			RaiseEventsForTransactionalPhase(systemSession, systemSession.GetAllCompanies(useSecurityFilter: false).ToList());
		}
		catch (Exception)
		{
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
			throw;
		}
		finally
		{
			ClearGlobalCache();
		}
	}

	private void RunInstallSpecialRecordActions(NavSqlConnectionScope tenantDatabaseScope, bool doNotLoadData)
	{
		NavAppRuntimeMetadata appBeingTargeted = Context.AppBeingTargeted;
		NavAppGroup targetGroup = Context.TargetGroup;
		NavTenant tenant = Context.Tenant;
		if (doNotLoadData)
		{
			NavAppInstallationManagement.PurgeAppTablesForTenant(appBeingTargeted, targetGroup, tenant);
		}
		NavAppInstallationManagement.UpdateCompanionTablesForApp(appBeingTargeted, targetGroup, tenant);
		HandleSpecialRecordsForInstall(appBeingTargeted, tenant, tenantDatabaseScope);
	}

	private void ClearGlobalCache()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000018S", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing global cache");
		Context.Tenant.Database.DatabaseState.ClearGlobalCache();
	}

	private void HandleInstallAppDatabase()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001O1", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB update");
		NavAppInstallationManagement.InstallAppToApplicationDatabaseWithRetries(NavGlobal.AppDatabase, Context.Tenant.Id, (INavAppMetadataReference)(object)Context.AppBeingTargeted);
	}

	private void HandleUninstallAppDatabaseRemoval()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008K", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB update");
		NavAppInstallationManagement.UninstallAppFromApplicationDatabaseWithRetries(NavGlobal.AppDatabase, Context.Tenant.Id, Context.AppBeingTargeted.PackageId);
	}

	private void RaiseEventsForTransactionalPhase(NavSession session, IList<string> companyNames)
	{
		MonitorType monitorTypes = (MonitorType)458755;
		using (DiagnosticsResolver.CreateTraceScope("00001EG", DiagnosticParameter.SystemMetadata("InstallAppPerCompany Code"), monitorTypes, Category.Extensions, createNewActivity: false))
		{
			RaisePerCompanyInstallEvent(session, companyNames);
		}
		using (DiagnosticsResolver.CreateTraceScope("00001EH", DiagnosticParameter.SystemMetadata("InstallAppPerDatabase Code"), monitorTypes, Category.Extensions, createNewActivity: false))
		{
			RaisePerDatabaseInstallEvent();
		}
	}

	private void RaisePerCompanyInstallEvent(NavSession session, IList<string> companyNames)
	{
		foreach (string companyName in companyNames)
		{
			try
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EC", Category.Extensions, Verbosity.Normal, "Opening company for install events -- Company:{0}", DiagnosticParameter.Oii(companyName));
				session.OpenCompany(companyName);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001ED", Category.Extensions, Verbosity.Normal, "Raising per-company install event -- Event:OnInstallAppPerCompany; Company:{0}", DiagnosticParameter.Oii(companyName));
				executorGenerator().InvokeOnInstallAppPerCompany();
			}
			finally
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EE", Category.Extensions, Verbosity.Normal, "Closing company for install events -- Company:{0}", DiagnosticParameter.Oii(companyName));
				session.CloseCompany();
			}
		}
	}

	private void RaisePerDatabaseInstallEvent()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EF", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Raising per-database install event -- Event:OnInstallAppPerDatabase");
		executorGenerator().InvokeOnInstallAppPerDatabase();
	}

	protected override void SetupSessionAndExecuteAction(NavSession systemSession, Action<NavSession> sessionAction)
	{
		try
		{
			systemSession.NavAppUpgradeSessionManager = new LoadPackageDataOnlyNavAppDataArchiveManager(systemSession, Context);
			systemSession.SetAppInstallationContext(Context);
			sessionAction(systemSession);
		}
		finally
		{
			systemSession.NavAppUpgradeSessionManager = null;
			systemSession.ClearAppInstallationContext();
		}
	}
}
