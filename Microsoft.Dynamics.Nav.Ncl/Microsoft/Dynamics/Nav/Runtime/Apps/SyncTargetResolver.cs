using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncTargetResolver
{
	private readonly IEnumerable<NavAppRuntimeMetadata> allPublished;

	private readonly IEnumerable<AppSyncTrackingRecord> allSynced;

	private readonly IEnumerable<NavAppRuntimeMetadata> allInstalled;

	public SyncTargetResolver(IEnumerable<NavAppRuntimeMetadata> allPublished, IEnumerable<AppSyncTrackingRecord> allSynced, IEnumerable<NavAppRuntimeMetadata> allInstalled)
	{
		this.allPublished = allPublished;
		this.allSynced = allSynced;
		this.allInstalled = allInstalled;
	}

	public SyncPrecheckHelper ResolveSyncTargetForAdd(string name, string publisher, Version version)
	{
		List<NavAppRuntimeMetadata> list = (from m in allPublished
			where string.IsNullOrEmpty(name) || m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			where string.IsNullOrEmpty(publisher) || m.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
			where version == null || m.Version == version
			select m).ToList();
		if (!list.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppNotPublished, Lang.NavAppPublishCheck_UnknownApp);
		}
		if (list.Count > 1)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AmbiguousApp, Lang.NavAppPublishCheck_AmbiguousApp);
		}
		NavAppRuntimeMetadata publishedMatch = list.Single();
		if (publishedMatch.ExtensionType != NavExtensionType.ModernDev)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.SyncNotSupported, string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncNotSupported, publishedMatch.Name));
		}
		AppSyncTrackingRecord appSyncTrackingRecord = allSynced.FirstOrDefault((AppSyncTrackingRecord m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)publishedMatch.AppId);
		if (appSyncTrackingRecord?.Version == publishedMatch.Version)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppAlreadyChangedWarning, string.Format(CultureInfo.CurrentCulture, Lang.NavAppAlreadySyncedWarning, name));
		}
		if (appSyncTrackingRecord?.Version > publishedMatch.Version)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.HigherVersionSyncedError, string.Format(CultureInfo.CurrentCulture, Lang.NavAppHigherVersionSyncedWarning, name));
		}
		return SyncPrecheckHelper.CreateForSuccess(publishedMatch, publishedMatch.AppId, publishedMatch.Version, appSyncTrackingRecord?.Version);
	}

	public SyncPrecheckHelper ResolveSyncTargetForDevelopment(string name, string publisher)
	{
		List<NavAppRuntimeMetadata> list = (from m in allPublished
			where string.IsNullOrEmpty(name) || m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			where string.IsNullOrEmpty(publisher) || m.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
			select m).ToList();
		if (!list.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppNotPublished, Lang.NavAppPublishCheck_UnknownApp);
		}
		if (list.Count > 1)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AmbiguousApp, Lang.NavAppPublishCheck_AmbiguousApp);
		}
		NavAppRuntimeMetadata publishedMatch = list.Single();
		if (publishedMatch.ExtensionType != NavExtensionType.ModernDev)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.SyncNotSupported, string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncNotSupported, publishedMatch.Name));
		}
		AppSyncTrackingRecord appSyncTrackingRecord = allSynced.FirstOrDefault((AppSyncTrackingRecord m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)publishedMatch.AppId);
		if (appSyncTrackingRecord?.Version > publishedMatch.Version)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.HigherVersionSyncedError, string.Format(CultureInfo.CurrentCulture, Lang.NavAppHigherVersionSyncedWarning, name));
		}
		return SyncPrecheckHelper.CreateForSuccess(publishedMatch, publishedMatch.AppId, publishedMatch.Version, appSyncTrackingRecord?.Version);
	}

	public SyncPrecheckHelper ResolveSyncTargetForClean(string name, string publisher)
	{
		var list = (from m in allSynced
			where string.IsNullOrEmpty(name) || m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			where string.IsNullOrEmpty(publisher) || m.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
			group m by new { m.Name, m.Publisher }).ToList();
		if (!list.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppAlreadyChangedWarning, string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_NotSynced, name));
		}
		if (list.Count > 1)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AmbiguousApp, Lang.NavAppPublishCheck_AmbiguousApp);
		}
		AppSyncTrackingRecord syncedMatch = list.FirstOrDefault()?.FirstOrDefault();
		List<NavAppRuntimeMetadata> source = allInstalled.Where((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)syncedMatch.AppId).ToList();
		if (source.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppStillInstalledError, Lang.NavAppSync_CannotRemoveUntilUninstalled);
		}
		return SyncPrecheckHelper.CreateForSuccess(null, syncedMatch?.AppId, null, syncedMatch?.Version);
	}

	public SyncPrecheckHelper ResolveSyncTargetForForce(string name, string publisher, Version version)
	{
		List<NavAppRuntimeMetadata> list = (from m in allPublished
			where string.IsNullOrEmpty(name) || m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
			where string.IsNullOrEmpty(publisher) || m.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
			where version == null || m.Version == version
			select m).ToList();
		if (!list.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppNotPublished, Lang.NavAppPublishCheck_UnknownApp);
		}
		if (list.Count > 1)
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AmbiguousApp, Lang.NavAppPublishCheck_AmbiguousApp);
		}
		NavAppRuntimeMetadata publishedMatch = list.Single();
		AppSyncTrackingRecord appSyncTrackingRecord = allSynced.FirstOrDefault((AppSyncTrackingRecord m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)publishedMatch.AppId);
		List<NavAppRuntimeMetadata> source = allInstalled.Where((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)publishedMatch.AppId).ToList();
		if (source.Any())
		{
			return SyncPrecheckHelper.CreateForError(SyncNavAppStatus.AppStillInstalledError, Lang.NavAppSync_CannotForceSyncUntilUninstalled);
		}
		return SyncPrecheckHelper.CreateForSuccess(publishedMatch, publishedMatch.AppId, publishedMatch.Version, appSyncTrackingRecord?.Version);
	}
}
