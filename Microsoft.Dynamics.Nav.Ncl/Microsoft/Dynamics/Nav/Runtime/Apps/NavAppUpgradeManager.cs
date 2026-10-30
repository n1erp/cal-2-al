using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppUpgradeManager : NavAppOperationProcessor<NavAppUpgradeContext>
{
	private readonly Func<INavUpgradeEventExecutor> executorGenerator;

	public NavAppUpgradeManager(NavAppUpgradeContext appUpgradeContext, Func<INavUpgradeEventExecutor> eventExecutorGenerator = null)
		: base(appUpgradeContext)
	{
		executorGenerator = eventExecutorGenerator ?? ((Func<INavUpgradeEventExecutor>)(() => NavCurrentThread.Session.SystemCodeunitFactory.UpgradeTriggers));
	}

	public void UpgradeNavApp(IEnumerable<string> companyNames, LanguageSetting language)
	{
		RunActionInSession(runInNewSystemSession: true, (NavSession session) =>
		{
			DoTransactionalPartOfUpgrade(session, companyNames);
		}, language);
		Context.SetTransactionalPhaseComplete();
		RunActionInSessionWithoutTransaction((NavSession session) =>
		{
			DoAfterCommitPartOfUpgrade(session, companyNames);
		}, language);
		UpdateAppDatabaseInstalledRecords();
	}

	public void RunAppDatabaseInstallRecordsUpdate(bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			UpdateAppDatabaseInstalledRecords();
		});
	}

	public void RunNavAppUpgradeEvent(string companyName, bool runInSystemSession, NavUpgradeEvent upgradeEvent)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			if (!string.IsNullOrWhiteSpace(companyName))
			{
				RaisePerCompanyUpgradeEvent(session, upgradeEvent, new string[1] { companyName });
			}
			else
			{
				RaisePerDatabaseUpgradeEvent(upgradeEvent);
			}
		});
	}

	public void RunNavAppUpgradeSpecialActions(bool runInSystemSession, NavSqlConnectionScope tenantDatabaseConnectionScope = null)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			if (tenantDatabaseConnectionScope == null)
			{
				using (NavSqlConnectionScope tenantDatabaseScope = NavSqlConnectionScope.CreateForceNewConnection(Context.Tenant.Database, TransactionType.Update))
				{
					RunUpgradeSpecialRecordActions(tenantDatabaseScope, purgeArchives: true);
					return;
				}
			}
			RunUpgradeSpecialRecordActions(tenantDatabaseConnectionScope, purgeArchives: true);
		});
	}

	public void ClearGlobalCache(bool runInSystemSession)
	{
		RunActionInSession(runInSystemSession, (NavSession session) =>
		{
			ClearGlobalCache();
		});
	}

	private void DoTransactionalPartOfUpgrade(NavSession session, IEnumerable<string> companies)
	{
		try
		{
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000019H", DiagnosticParameter.SystemMetadata("Performing Data Upgrade"), MonitorType.None, Category.Extensions, createNewActivity: false))
			{
				NavDatabase database = Context.Tenant.Database;
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update);
				try
				{
					INavAppReference oldAppBeingUpgradedFrom = Context.OldAppBeingUpgradedFrom;
					INavAppReference obj = ((oldAppBeingUpgradedFrom is INavAppMetadataReference) ? oldAppBeingUpgradedFrom : null);
					bool flag = (obj != null && ((INavAppMetadataReference)obj).ExtensionType == NavExtensionType.ModernDev) || ArchiveVersionDoNotMatchContext(database);
					RunUpgradeSpecialRecordActions(navSqlConnectionScope, flag);
					RaiseEventsForTransactionalPhase(session, companies);
					if (!flag)
					{
						session.NavAppUpgradeSessionManager.VerifyAppTablesHaveBeenHandled();
						PurgeOldArchiveTables(database, ((WrappedValue<Guid>)(object)Context.OldAppBeingUpgradedFrom.AppId).Value);
					}
				}
				catch (Exception exception)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000142", Category.Extensions, exception);
					navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
					throw;
				}
				finally
				{
					ClearGlobalCache();
				}
			}
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001VK", Category.Extensions, ex);
			throw new NavUpgradeActivityExecutionException(ex.Message, ex);
		}
	}

	private bool ArchiveVersionDoNotMatchContext(NavDatabase database)
	{
		INavAppReference oldAppBeingUpgradedFrom = Context.OldAppBeingUpgradedFrom;
		Version version = new DatabaseNavAppDataArchiveRepository(database).GetRecordsForApp(((WrappedValue<Guid>)(object)oldAppBeingUpgradedFrom.AppId).Value).FirstOrDefault()?.AppVersion;
		if (!(version == null))
		{
			return version != oldAppBeingUpgradedFrom.Version;
		}
		return true;
	}

	private void RaiseEventsForTransactionalPhase(NavSession session, IEnumerable<string> companyNames)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		RaisePerCompanyUpgradeEvent(session, NavUpgradeEvent.OnCheckPreconditionsPerCompany, companyNames);
		RaisePerDatabaseUpgradeEvent(NavUpgradeEvent.OnCheckPreconditionsPerDatabase);
		RaisePerCompanyUpgradeEvent(session, NavUpgradeEvent.OnUpgradePerCompany, companyNames);
		RaisePerDatabaseUpgradeEvent(NavUpgradeEvent.OnUpgradePerDatabase);
		RaisePerCompanyUpgradeEvent(session, NavUpgradeEvent.OnValidateUpgradePerCompany, companyNames);
		RaisePerDatabaseUpgradeEvent(NavUpgradeEvent.OnValidateUpgradePerDatabase);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074O", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension upgrade code -- Tenant:{0}; Elapsed:{1}", session.Tenant.Id, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void PurgeOldArchiveTables(NavDatabase database, Guid appId)
	{
		NavSqlDdlCommands.PurgeAppArchiveRecordsAndTablesForApp(database, appId);
	}

	private void DoAfterCommitPartOfUpgrade(NavSession session, IEnumerable<string> companyNames)
	{
		try
		{
			MonitorType monitorTypes = (MonitorType)458755;
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001EQ", DiagnosticParameter.SystemMetadata("Performing Data Upgrade (Post)"), MonitorType.None, Category.Extensions, createNewActivity: false))
			{
				using (DiagnosticsResolver.CreateTraceScope("00001ER", DiagnosticParameter.SystemMetadata("OnAfterUpgradeCommitPerCompany Code"), monitorTypes, Category.Extensions, createNewActivity: false))
				{
					RaisePerCompanyUpgradeEvent(session, NavUpgradeEvent.OnAfterUpgradeCommitPerCompany, companyNames);
				}
				using (DiagnosticsResolver.CreateTraceScope("00001ES", DiagnosticParameter.SystemMetadata("OnAfterUpgradeCommitPerDatabase Code"), monitorTypes, Category.Extensions, createNewActivity: false))
				{
					RaisePerDatabaseUpgradeEvent(NavUpgradeEvent.OnAfterUpgradeCommitPerDatabase);
				}
			}
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001EI", Category.Extensions, ex);
			throw new NavUpgradeActivityExecutionException(ex.Message, ex);
		}
	}

	private void RunUpgradeSpecialRecordActions(NavSqlConnectionScope tenantDatabaseScope, bool purgeArchives)
	{
		Context.Tenant.Database.TenantDatabaseProperties.Refresh();
		Context.Tenant.Database.TenantProperties.Refresh();
		NavAppInstallationManagement.UpdateCompanionTablesForApp(Context.AppBeingTargeted, Context.TargetGroup, Context.Tenant);
		INavAppReference oldAppBeingUpgradedFrom = Context.OldAppBeingUpgradedFrom;
		INavAppMetadataReference val = (INavAppMetadataReference)(object)((oldAppBeingUpgradedFrom is INavAppMetadataReference) ? oldAppBeingUpgradedFrom : null);
		if (val != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074P", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Handling special records for in-place upgrade");
			NavAppUpgradeSpecialRecordsHandler.HandleSpecialRecordsForUpgrade(Context.Tenant, tenantDatabaseScope, Context.AppBeingTargeted, (INavAppReference)(object)val);
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074Q", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Handling special records for install-like upgrade");
			NavAppInstallationProcessor.HandleSpecialRecordsForInstall(Context.AppBeingTargeted, Context.Tenant, tenantDatabaseScope);
		}
		if (purgeArchives)
		{
			PurgeOldArchiveTables(tenantDatabaseScope.Connection.Database, ((WrappedValue<Guid>)(object)Context.AppBeingTargeted.AppId).Value);
		}
	}

	private void UpdateAppDatabaseInstalledRecords()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013Q", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB update");
		INavAppReference oldAppBeingUpgradedFrom = Context.OldAppBeingUpgradedFrom;
		INavAppMetadataReference val = (INavAppMetadataReference)(object)((oldAppBeingUpgradedFrom is INavAppMetadataReference) ? oldAppBeingUpgradedFrom : null);
		NavAppRuntimeMetadata appBeingTargeted = Context.AppBeingTargeted;
		if (val != null)
		{
			NavAppInstallationManagement.UpdateAppInApplicationDatabaseWithRetries(NavGlobal.AppDatabase, Context.Tenant.Id, (INavAppMetadataReference)(object)appBeingTargeted, val);
		}
		else
		{
			NavAppInstallationManagement.InstallAppToApplicationDatabaseWithRetries(NavGlobal.AppDatabase, Context.Tenant.Id, (INavAppMetadataReference)(object)appBeingTargeted.AsNavAppMetadata());
		}
	}

	private void RaisePerDatabaseUpgradeEvent(NavUpgradeEvent upgradeEvent)
	{
		MonitorType monitorTypes = (MonitorType)458755;
		using (DiagnosticsResolver.CreateTraceScope("00001ET", DiagnosticParameter.SystemMetadata(FormattableString.Invariant($"{upgradeEvent} Code")), monitorTypes, Category.Extensions, createNewActivity: false))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EK", Category.Extensions, Verbosity.Normal, "Raising per-database upgrade event -- Event:{0}", DiagnosticParameter.SystemMetadata(upgradeEvent));
			executorGenerator().InvokeUpgradeEvent(upgradeEvent);
		}
	}

	private void RaisePerCompanyUpgradeEvent(NavSession session, NavUpgradeEvent upgradeEvent, IEnumerable<string> companies)
	{
		MonitorType monitorTypes = (MonitorType)458755;
		using (DiagnosticsResolver.CreateTraceScope("00001EU", DiagnosticParameter.SystemMetadata(FormattableString.Invariant($"{upgradeEvent} Code")), monitorTypes, Category.Extensions, createNewActivity: false))
		{
			foreach (string company in companies)
			{
				try
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EL", Category.Extensions, Verbosity.Normal, "Opening company for upgrade events -- Company:{0}", DiagnosticParameter.Oii(company));
					session.OpenCompany(company);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EM", Category.Extensions, Verbosity.Normal, "Raising per-company upgrade event -- Event:{0}; Company:{1}", DiagnosticParameter.SystemMetadata(upgradeEvent), DiagnosticParameter.Oii(company));
					executorGenerator().InvokeUpgradeEvent(upgradeEvent);
				}
				finally
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EN", Category.Extensions, Verbosity.Normal, "Closing company for upgrade events -- Company:{0}", DiagnosticParameter.Oii(company));
					session.CloseCompany();
				}
			}
		}
	}

	private void ClearGlobalCache()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000143", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing global cache");
		Context.Tenant.Database.DatabaseState.ClearGlobalCache();
	}

	protected override void SetupSessionAndExecuteAction(NavSession upgradeSession, Action<NavSession> sessionAction)
	{
		try
		{
			upgradeSession.NavAppUpgradeSessionManager = new NavAppDataArchiveTenantUpgradeManager(upgradeSession, Context);
			upgradeSession.SetAppUpgradeContext(Context);
			sessionAction(upgradeSession);
		}
		finally
		{
			upgradeSession.NavAppUpgradeSessionManager = null;
			upgradeSession.ClearAppUpgradeContext();
		}
	}
}
