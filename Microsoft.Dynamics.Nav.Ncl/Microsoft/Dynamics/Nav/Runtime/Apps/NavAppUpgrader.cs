using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppUpgrader
{
	public static ChangeNavAppStateResponse UpgradeNavApp(StartNavAppDataUpgradeArguments startNavAppDataUpgradeArguments)
	{
		bool force = startNavAppDataUpgradeArguments.Force;
		string tenantId = startNavAppDataUpgradeArguments.TenantId;
		string name = startNavAppDataUpgradeArguments.Name;
		string publisher = startNavAppDataUpgradeArguments.Publisher;
		string origin = startNavAppDataUpgradeArguments.Origin;
		LanguageSetting language = ((startNavAppDataUpgradeArguments.Language != null) ? new LanguageSetting(startNavAppDataUpgradeArguments.Language) : null);
		string activityId = Guid.NewGuid().ToString();
		Version version = startNavAppDataUpgradeArguments.Version;
		ChangeNavAppStateResponse changeNavAppStateResponse = null;
		string content = string.Format(CultureInfo.InvariantCulture, "Extension upgrade request -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; Force:{5}; Origin:{6}", NavAppTraceHelper.CreateActivityIdString(activityId), name, publisher, version, tenantId, force, origin);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016Q", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			try
			{
				NavSession session = NavCurrentThread.Session;
				NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true);
				NavAppTenantGuard.BlockUpgradeIfTenantIsInImproperState(tenantById);
				lock (tenantById.ExtensionInstallUninstallLockObject)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013D", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "In-process tenant lock acquired");
					using NavSqlConnectionScope tenantDatabaseLockScope = NavSqlConnectionScope.CreateForceNewConnection(tenantById.Database, TransactionType.UpdateNoLocks);
					using NavSqlDatabaseLock navSqlDatabaseLock = NavDatabase.AcquireNavAppTenantCrossProcessLock(tenantDatabaseLockScope, tenantById.Id);
					if (!navSqlDatabaseLock.IsLockAcquired())
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013E", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension Cross-Process lock acquisition failed");
						changeNavAppStateResponse = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeInProgress, string.Format(CultureInfo.CurrentCulture, Lang.NavAppTenantInUseUpgrade, name, tenantById.Id));
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013F", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Cross-process tenant lock acquired");
						NavAppUpgradePrecheckResult navAppUpgradePrecheckResult = NavAppUpgradePrecheck.UpgradeAppPrecheck(name, publisher, version, tenantById, session.LCID);
						if (navAppUpgradePrecheckResult.Result != ChangeNavAppStateResult.AppChangeSuccessful)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013G", Category.Extensions, Verbosity.Normal, "Precheck failed -- Result:{0}; Error:{1}", DiagnosticParameter.SystemMetadata(navAppUpgradePrecheckResult.Result), DiagnosticParameter.SystemMetadata(navAppUpgradePrecheckResult.Error));
							changeNavAppStateResponse = new ChangeNavAppStateResponse(navAppUpgradePrecheckResult.Result, navAppUpgradePrecheckResult.Error);
						}
						else
						{
							changeNavAppStateResponse = NavAppUpgradeInternalProcessor.InternalUpgradeNavApp(navAppUpgradePrecheckResult, name, session, tenantById, activityId, language);
						}
					}
				}
			}
			catch (Exception ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000013H", Category.Extensions, ex);
				changeNavAppStateResponse = new ChangeNavAppStateResponse(ChangeNavAppStateResult.UnknownAppError, ex.Message);
				throw;
			}
			finally
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013J", Category.Extensions, Verbosity.Normal, "Extension upgrade request ending -- {0}; Result:{1}; Error:{2}", DiagnosticParameter.SystemMetadata(NavAppTraceHelper.CreateActivityIdString(activityId)), DiagnosticParameter.SystemMetadata(changeNavAppStateResponse?.Result), DiagnosticParameter.SystemMetadata(changeNavAppStateResponse?.Error));
			}
		}
		return changeNavAppStateResponse;
	}
}
