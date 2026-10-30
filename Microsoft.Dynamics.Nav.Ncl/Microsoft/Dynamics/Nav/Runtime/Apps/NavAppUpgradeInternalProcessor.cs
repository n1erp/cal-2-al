using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppUpgradeInternalProcessor
{
	public static ChangeNavAppStateResponse InternalUpgradeNavApp(NavAppUpgradePrecheckResult precheckResult, string name, NavSession session, NavTenant tenant, string activityId, LanguageSetting language)
	{
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		NavAppRuntimeMetadata targetApp = precheckResult.AppBeingUpgradedTo;
		INavAppReference oldAppBeingUpgradedFrom = precheckResult.OldAppBeingUpgradedFrom;
		List<NavAppRuntimeMetadata> source = tenant.NavAppGroup.OrderedAppMetadata.ToList();
		List<PackageId> list = (from i in source
			where (WrappedComparable<Guid>)(object)i.AppId != (WrappedComparable<Guid>)(object)targetApp.AppId
			select i.PackageId).ToList();
		list.Add(targetApp.PackageId);
		NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(list);
		NavEnvironment.Instance.NavAppGroupUpdater.PrepareAppGroup(orCreateGroupForAppCombination);
		ChangeNavAppStateResponse result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeSuccessful);
		List<string> companyNames = session.GetAllCompanies(useSecurityFilter: false).ToList();
		try
		{
			NavAppUpgradeContext appUpgradeContext = NavAppUpgradeContext.Create(tenant, orCreateGroupForAppCombination, targetApp, oldAppBeingUpgradedFrom, activityId);
			session.NavAppUpgradeManager = new NavAppUpgradeManager(appUpgradeContext);
			string content = language?.CultureInfo.Name ?? session.Culture.Name;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000013P", Category.Extensions, Verbosity.Normal, "Extension internal upgrade starting -- PackageId:{0}; AppId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Tenant:{5}; Culture:{6}", DiagnosticParameter.SystemMetadata(targetApp.PackageId), DiagnosticParameter.SystemMetadata(targetApp.AppId), DiagnosticParameter.SystemMetadata(targetApp.Name), DiagnosticParameter.SystemMetadata(targetApp.Publisher), DiagnosticParameter.SystemMetadata(targetApp.Version), DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(content));
			session.NavAppUpgradeManager.UpgradeNavApp(companyNames, language);
			NavGlobal.MetadataProvider.UpdateMetadataVersion();
			NavEnvironment.Instance.NavAppGroupUpdater.UpdateGroupForChangedTenant(tenant);
			tenant.StartNasIfNotRunning();
		}
		catch (NavBaseException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000013R", Category.Extensions, ex);
			ChangeNavAppStateResult result2;
			string error;
			if (ex is NavUpgradeActivityExecutionException)
			{
				result2 = ChangeNavAppStateResult.AppUpgradeCodeExecutionFailure;
				error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeCodeExecutionFailure, name, tenant.Id, ex.Message);
			}
			else
			{
				if (!(ex is NavAppException))
				{
					throw;
				}
				result2 = ChangeNavAppStateResult.UnknownAppError;
				error = ex.Message;
			}
			result = new ChangeNavAppStateResponse(result2, error);
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000013S", Category.Extensions, exception);
			throw;
		}
		finally
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000014J", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Internal upgrade ending");
		}
		return result;
	}
}
