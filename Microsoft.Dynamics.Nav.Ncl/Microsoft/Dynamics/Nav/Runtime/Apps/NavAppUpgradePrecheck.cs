using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppUpgradePrecheck
{
	private class AccumulatedPrecheckData
	{
		public NavTenant Tenant { get; set; }

		public List<NavAppRuntimeMetadata> Published { get; set; }

		public List<AppSyncTrackingRecord> Synced { get; set; }

		public List<INavAppMetadataReference> Installed { get; set; }

		public List<UninstalledAppRecord> Uninstalled { get; set; }

		public NavAppRuntimeMetadata AppBeingUpgradedTo { get; set; }

		public INavAppReference OldAppBeingUpgradedFrom { get; set; }

		public ChangeNavAppStateResult Result { get; set; }

		public string Error { get; set; }
	}

	public static NavAppUpgradePrecheckResult UpgradeAppPrecheck(string name, string publisher, Version version, NavTenant tenant, int lcid)
	{
		List<Action<AccumulatedPrecheckData>> list = new List<Action<AccumulatedPrecheckData>>();
		list.Add((AccumulatedPrecheckData d) =>
		{
			CheckLanguage(d, lcid);
		});
		list.Add((AccumulatedPrecheckData d) =>
		{
			CheckPublishedStateAndResolve(d, name, publisher, version);
		});
		list.Add(CheckSyncedState);
		list.Add(CheckInstalledAndUninstalledState);
		list.Add(CheckDependencies);
		List<Action<AccumulatedPrecheckData>> list2 = list;
		AccumulatedPrecheckData accumulatedPrecheckData = new AccumulatedPrecheckData
		{
			Tenant = tenant
		};
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		foreach (Action<AccumulatedPrecheckData> item in list2)
		{
			item(accumulatedPrecheckData);
			if (accumulatedPrecheckData.Result != ChangeNavAppStateResult.AppChangeSuccessful)
			{
				break;
			}
		}
		return new NavAppUpgradePrecheckResult
		{
			Result = accumulatedPrecheckData.Result,
			Error = accumulatedPrecheckData.Error,
			AppBeingUpgradedTo = accumulatedPrecheckData.AppBeingUpgradedTo,
			OldAppBeingUpgradedFrom = accumulatedPrecheckData.OldAppBeingUpgradedFrom
		};
	}

	private static void CheckLanguage(AccumulatedPrecheckData data, int lcid)
	{
		if (!WindowsLanguageDataProvider.IsAvailableLanguage(lcid))
		{
			data.Result = ChangeNavAppStateResult.LanguageNotSupported;
			try
			{
				data.Error = string.Format(CultureInfo.CurrentCulture, Lang.LanguageNotEnabledOrMissingLocalization, CultureInfo.GetCultureInfo(lcid).Name);
			}
			catch (Exception)
			{
				data.Error = string.Format(CultureInfo.CurrentCulture, Lang.ExceptionLanguageNotInstalled, lcid);
			}
		}
	}

	private static void CheckPublishedStateAndResolve(AccumulatedPrecheckData data, string name, string publisher, Version version)
	{
		data.Published = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().ToList();
		List<NavAppRuntimeMetadata> list = data.Published.Where((NavAppRuntimeMetadata app) => (string.IsNullOrEmpty(name) || string.Equals(app.Name, name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || string.Equals(app.Publisher, publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || app.Version == version)).ToList();
		if (list.Count > 1)
		{
			data.Result = ChangeNavAppStateResult.MultipleAppsFound;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppPublishCheck_AmbiguousApp);
			return;
		}
		NavAppRuntimeMetadata val = list.SingleOrDefault();
		if (val != null && val.ExtensionType != NavExtensionType.ModernDev)
		{
			data.Result = ChangeNavAppStateResult.UpgradeNotSupported;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeNotSupported, name);
		}
		if (val == null)
		{
			data.Result = ChangeNavAppStateResult.AppNotFound;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppPublishCheck_UnknownApp);
		}
		data.AppBeingUpgradedTo = val;
	}

	private static void CheckSyncedState(AccumulatedPrecheckData data)
	{
		NavAppRuntimeMetadata target = data.AppBeingUpgradedTo;
		data.Synced = NavAppInstallationManagement.GetAppsSyncedForTenantInTenantDatabase(data.Tenant.Database).ToList();
		AppSyncTrackingRecord appSyncTrackingRecord = data.Synced.FirstOrDefault((AppSyncTrackingRecord synced) => (WrappedComparable<Guid>)(object)synced.AppId == (WrappedComparable<Guid>)(object)target.AppId);
		if (appSyncTrackingRecord == null)
		{
			data.Result = ChangeNavAppStateResult.AppNotSynced;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_NotSynced, target.TechnicalDisplayName);
		}
		else if (target.Version > appSyncTrackingRecord.Version)
		{
			data.Result = ChangeNavAppStateResult.SyncedVersionTooOld;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_SyncedVersionTooOld, target.TechnicalDisplayName, appSyncTrackingRecord.Version);
		}
		else if (target.Version < appSyncTrackingRecord.BaselineVersion)
		{
			data.Result = ChangeNavAppStateResult.SyncBaselineVersionTooNew;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_BaselineVersionTooNew, target.TechnicalDisplayName, appSyncTrackingRecord.BaselineVersion);
		}
	}

	private static void CheckInstalledAndUninstalledState(AccumulatedPrecheckData data)
	{
		NavAppRuntimeMetadata target = data.AppBeingUpgradedTo;
		NavTenant tenant = data.Tenant;
		data.Installed = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(tenant.Database);
		data.Uninstalled = (from u in NavAppInstallationManagement.GetAppsUninstalledForTenantInTenantDatabase(tenant.Database, tenant.Id)
			where u.HasData
			select u).ToList();
		INavAppMetadataReference val = data.Installed.FirstOrDefault((INavAppMetadataReference installed) => (WrappedComparable<Guid>)(object)((INavAppReference)installed).AppId == (WrappedComparable<Guid>)(object)target.AppId);
		UninstalledAppRecord uninstalledAppRecord = data.Uninstalled.FirstOrDefault((UninstalledAppRecord uninstalled) => (WrappedComparable<Guid>)(object)uninstalled.AppId == (WrappedComparable<Guid>)(object)target.AppId);
		Version version = ((val != null) ? ((INavAppReference)val).Version : null) ?? uninstalledAppRecord?.Version;
		if (val != null && val.ExtensionType != NavExtensionType.ModernDev)
		{
			data.Result = ChangeNavAppStateResult.UpgradeMustUninstallV1Extension;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeCheck_MustUninstallV1Extensions, target.TechnicalDisplayName, version);
		}
		INavAppReference val2 = (INavAppReference)(object)val;
		data.OldAppBeingUpgradedFrom = (INavAppReference)(((object)val2) ?? ((object)uninstalledAppRecord));
		if (version != null && target.Version <= version)
		{
			data.Result = ChangeNavAppStateResult.UpgradeTargetOlderThanCurrent;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeCheck_TargetVersionMustBeNewer, target.TechnicalDisplayName, version);
		}
		else if (version == null)
		{
			data.Result = ChangeNavAppStateResult.UpgradeNoOldVersionFound;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeCheck_NoPreviousVersionFound, target.TechnicalDisplayName);
		}
	}

	private static void CheckDependencies(AccumulatedPrecheckData data)
	{
		List<string> list = new List<string>();
		foreach (NavAppRuntimeDependencyDefinition dependencyDefinition in data.AppBeingUpgradedTo.Dependencies)
		{
			INavAppMetadataReference val = data.Installed.FirstOrDefault((INavAppMetadataReference i) => (WrappedComparable<Guid>)(object)((INavAppReference)i).AppId == (WrappedComparable<Guid>)(object)dependencyDefinition.AppId);
			if (val == null || ((INavAppReference)val).Version < dependencyDefinition.Version)
			{
				string item = string.Format(CultureInfo.InvariantCulture, "{0} by {1} {2}", dependencyDefinition.Name, dependencyDefinition.Publisher, dependencyDefinition.Version);
				list.Add(item);
			}
		}
		if (list.Any())
		{
			string arg = Environment.NewLine + string.Join(Environment.NewLine, list);
			data.Result = ChangeNavAppStateResult.UnsatisfiedDependenciesForUpgrade;
			data.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradeCheck_UnsatisfiedDependencies, data.AppBeingUpgradedTo.TechnicalDisplayName, arg);
		}
	}
}
