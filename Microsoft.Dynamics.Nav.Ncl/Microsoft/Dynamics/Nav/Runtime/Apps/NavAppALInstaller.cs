using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppALInstaller
{
	private readonly NavSession session;

	public NavAppALInstaller()
	{
		session = NavCurrentThread.Session;
		if (session == null || !session.IsOpen)
		{
			throw new InvalidOperationException("Current Nav session must exist and be open");
		}
	}

	public string ALGetAppDependenciesToInstallString(Guid appPackageId)
	{
		NavAppRuntimeMetadata target = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(PackageId.op_Implicit(appPackageId));
		return NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			NavAppInstallPreCheckHelper navAppInstallPreCheckHelper = null;
			ChangeNavAppStateResult result;
			string error;
			if (NavAppRequiresUpgrade(target))
			{
				NavAppUpgradePrecheckResult navAppUpgradePrecheckResult = NavAppUpgradePrecheck.UpgradeAppPrecheck(target.Name, target.Publisher, target.Version, session.Tenant, session.LCID);
				result = navAppUpgradePrecheckResult.Result;
				error = navAppUpgradePrecheckResult.Error;
			}
			else
			{
				navAppInstallPreCheckHelper = NavManagementTasks.InstallNavAppPrecheck(target.Name, target.Publisher, target.Version, session.Tenant, force: false, doNotLoadData: false, session, performLanguageCheck: false);
				result = navAppInstallPreCheckHelper.Result;
				error = navAppInstallPreCheckHelper.Error;
			}
			string[] value = new string[0];
			switch (result)
			{
			case ChangeNavAppStateResult.DependenciesFoundWithoutForceSet:
				if (navAppInstallPreCheckHelper != null)
				{
					value = navAppInstallPreCheckHelper.DependenciesToInstall.Select((NavAppRuntimeMetadata d) => string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", d.Name, d.Publisher, d.Version)).ToArray();
				}
				break;
			default:
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QP", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension ALInstall :: Precheck Failure -- Name:{0}; Publisher:{1}; Version:{2}; Tenant:{3}; Result:{4}; Error:{5}", target.Name, target.Publisher, target.Version, session.Tenant.Id, result, error);
				throw new NavALException(error);
			case ChangeNavAppStateResult.AppChangeSuccessful:
			case ChangeNavAppStateResult.AppAlreadyChangedWarning:
			case ChangeNavAppStateResult.AppNotSynced:
			case ChangeNavAppStateResult.SyncedVersionTooOld:
			case ChangeNavAppStateResult.UnsatisfiedDependenciesForInstall:
				break;
			}
			string text = string.Join("; ", value);
			if (text.Length > 2048)
			{
				text = text.Substring(0, 2048) + "...";
			}
			return text;
		}, FormatAdditionalInfo(target), null, "ALGetAppDependenciesToInstallString");
	}

	public void ALInstallNavApp(Guid appPackageId, int lcid)
	{
		NavAppRuntimeMetadata target = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(PackageId.op_Implicit(appPackageId));
		NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			int lCID = session.LCID;
			try
			{
				session.LCID = lcid;
				NavManagementTasks navManagementTasks = new NavManagementTasks(session);
				if (target.ExtensionType == NavExtensionType.ModernDev)
				{
					List<NavAppRuntimeMetadata> published = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(session.Tenant.Id).ToList();
					List<NavAppRuntimeMetadata> installed = session.Tenant.NavAppGroup.OrderedAppMetadata.ToList();
					IEnumerable<UninstalledAppRecord> allForTenant;
					using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(session.Tenant.Database, TransactionType.Browse))
					{
						DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(session.Tenant.Database, conScope);
						allForTenant = databaseUninstalledAppRepository.GetAllForTenant(session.Tenant.Id);
					}
					List<AppSyncTrackingRecord> synced = NavAppInstallationManagement.GetAppsSyncedForTenantInTenantDatabase(session.Tenant.Database).ToList();
					List<NavAppRuntimeMetadata> list = NavAppV2DependencyResolver.PackagesAppDependsOn(target, (IEnumerable<INavAppReference>)installed, allForTenant, published, synced, checkSyncState: false).SatisfiedDependencies.ToList();
					list.Add(target);
					TrySyncExtensionBeforeInstall(list);
				}
				ChangeNavAppStateResponse changeNavAppStateResponse = ((!NavAppRequiresUpgrade(target)) ? navManagementTasks.InstallNavApp(new InstallNavAppArguments(target.Name, target.Publisher, target.Version, session.Tenant.Id, doNotLoadData: false, force: true, "AL")) : navManagementTasks.StartNavAppDataUpgrade(new StartNavAppDataUpgradeArguments(target.Name, target.Publisher, target.Version, session.Tenant.Id, force: true, "AL")));
				ChangeNavAppStateResult result = changeNavAppStateResponse.Result;
				if (result != ChangeNavAppStateResult.AppChangeSuccessful && result != ChangeNavAppStateResult.AppAlreadyChangedWarning)
				{
					throw new NavALException(changeNavAppStateResponse.Error);
				}
			}
			finally
			{
				session.LCID = lCID;
			}
		}, FormatAdditionalInfo(target), null, "ALInstallNavApp");
	}

	public string ALGetDependentAppsToUninstallString(Guid appPackageId)
	{
		NavAppRuntimeMetadata target = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(PackageId.op_Implicit(appPackageId));
		return NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			NavManagementTasks navManagementTasks = new NavManagementTasks(session);
			NavAppUninstallPreCheckHelper navAppUninstallPreCheckHelper = navManagementTasks.UninstallAppPrecheck(target.Name, target.Publisher, target.Version, session.Tenant, force: false);
			string[] value = new string[0];
			switch (navAppUninstallPreCheckHelper.Result)
			{
			case ChangeNavAppStateResult.DependenciesFoundWithoutForceSet:
				value = navAppUninstallPreCheckHelper.DependentsToUninstall.Select((NavAppRuntimeMetadata d) => string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", d.Name, d.Publisher, d.Version)).ToArray();
				break;
			default:
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QQ", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension ALUninstall :: Precheck Failure -- Name:{0}; Publisher:{1}; Version:{2}; Tenant:{3}; Result:{4}; Error:{5}", target.Name, target.Publisher, target.Version, session.Tenant.Id, navAppUninstallPreCheckHelper.Result, navAppUninstallPreCheckHelper.Error);
				throw new NavALException(navAppUninstallPreCheckHelper.Error);
			case ChangeNavAppStateResult.AppChangeSuccessful:
			case ChangeNavAppStateResult.AppAlreadyChangedWarning:
				break;
			}
			string text = string.Join("; ", value);
			if (text.Length > 2048)
			{
				text = text.Substring(0, 2048) + "...";
			}
			return text;
		}, FormatAdditionalInfo(target), null, "ALGetDependentAppsToUninstallString");
	}

	public void ALUninstallNavApp(Guid appPackageId)
	{
		NavAppRuntimeMetadata target = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(PackageId.op_Implicit(appPackageId));
		NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			NavManagementTasks navManagementTasks = new NavManagementTasks(session);
			ChangeNavAppStateResponse changeNavAppStateResponse = navManagementTasks.UninstallNavApp(new UninstallNavAppArguments(target.Name, target.Publisher, target.Version, session.Tenant.Id, doNotSaveData: false, force: true, "AL"));
			ChangeNavAppStateResult result = changeNavAppStateResponse.Result;
			if (result != ChangeNavAppStateResult.AppChangeSuccessful && result != ChangeNavAppStateResult.AppAlreadyChangedWarning)
			{
				throw new NavALException(changeNavAppStateResponse.Error);
			}
		}, FormatAdditionalInfo(target), null, "ALUninstallNavApp");
	}

	public void ALUnpublishNavTenantApp(Guid appPackageId)
	{
		NavAppRuntimeMetadata target = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(PackageId.op_Implicit(appPackageId));
		NavAppDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			string id = session.Tenant.Id;
			NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(target);
			bool flag = publishProperties != null && publishProperties.IsDeveloperExtensionForTenant(id);
			Result val = NavAppUnpublisher.Unpublish(appPackageId, null, null, null, id);
			UnpublishNavAppResponse unpublishNavAppResponse = (val.Success ? UnpublishNavAppResponse.CreateForSuccess() : UnpublishNavAppResponse.CreateForError(ErrorExtensions.ToSingleMessage(val.Error)));
			if (unpublishNavAppResponse.DidErrorOccur)
			{
				throw new NavALException(unpublishNavAppResponse.Error);
			}
			if (!flag)
			{
				IEnumerable<string> tenantIdsWithAppPublished = NavEnvironment.Instance.NavAppMetadataRetriever.GetTenantIdsWithAppPublished(PackageId.op_Implicit(appPackageId));
				if (!tenantIdsWithAppPublished.Any())
				{
					Result val2 = NavAppUnpublisher.Unpublish(appPackageId, null, null, null);
					UnpublishNavAppResponse unpublishNavAppResponse2 = (val2.Success ? UnpublishNavAppResponse.CreateForSuccess() : UnpublishNavAppResponse.CreateForError(ErrorExtensions.ToSingleMessage(val2.Error)));
					if (unpublishNavAppResponse2.DidErrorOccur)
					{
						throw new NavALException(unpublishNavAppResponse2.Error);
					}
				}
			}
			if (session.CurrentDesignerExtension != null && session.CurrentDesignerExtension.Manifest.AppId.Equals(((WrappedValue<Guid>)(object)target.AppId).Value))
			{
				session.ApplicationExtension = Guid.Empty;
				session.CurrentDesignerExtension = null;
			}
		}, FormatAdditionalInfo(target), null, "ALUnpublishNavTenantApp");
	}

	private void TrySyncExtensionBeforeInstall(IEnumerable<NavAppRuntimeMetadata> appsToSync)
	{
		if (session.Tenant.Database.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() > 1)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppALInstallTooManyTenants));
		}
		IEnumerable<AppSyncTrackingRecord> all;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(session.Tenant.Database))
		{
			all = new DatabaseAppSyncTrackingRepository(session.Tenant.Database, conScope).GetAll();
		}
		foreach (NavAppRuntimeMetadata extension in appsToSync)
		{
			try
			{
				if (!all.Any((AppSyncTrackingRecord s) => (WrappedComparable<Guid>)(object)s.AppId == (WrappedComparable<Guid>)(object)extension.AppId && s.Version >= extension.Version))
				{
					SyncNavAppResponse syncNavAppResponse = NavAppSyncManagementTasks.SyncNavApp(new SyncNavAppArguments
					{
						Name = extension.Name,
						Publisher = extension.Publisher,
						Version = extension.Version,
						TenantId = session.Tenant.Id,
						Mode = NavAppSyncMode.Add,
						Origin = "AL"
					});
					if (syncNavAppResponse.Result != SyncNavAppStatus.AppSyncSuccessful && syncNavAppResponse.Result != SyncNavAppStatus.AppAlreadyChangedWarning)
					{
						throw new NavAppSyncException(syncNavAppResponse.Error);
					}
				}
			}
			catch (Exception ex)
			{
				throw new NavALException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppALInstallError, extension.FriendlyDisplayName, ex.Message));
			}
		}
	}

	private bool NavAppRequiresUpgrade(NavAppRuntimeMetadata targetApp)
	{
		if (targetApp.ExtensionType != NavExtensionType.ModernDev)
		{
			return false;
		}
		NavTenant tenant = session.Tenant;
		List<UninstalledAppRecord> source = (from u in NavAppInstallationManagement.GetAppsUninstalledForTenantInTenantDatabase(tenant.Database, tenant.Id)
			where u.HasData
			select u).ToList();
		UninstalledAppRecord uninstalledAppRecord = source.FirstOrDefault((UninstalledAppRecord app) => (WrappedComparable<Guid>)(object)app.AppId == (WrappedComparable<Guid>)(object)targetApp.AppId);
		if (uninstalledAppRecord != null)
		{
			return uninstalledAppRecord.Version < targetApp.Version;
		}
		return false;
	}

	private string FormatAdditionalInfo(NavAppRuntimeMetadata target)
	{
		return string.Format(CultureInfo.InvariantCulture, "Target :: Name:{0}; Publisher:{1}; Version:{2}; Tenant:{3}", target.Name, target.Publisher, target.Version, session.Tenant.Id);
	}
}
