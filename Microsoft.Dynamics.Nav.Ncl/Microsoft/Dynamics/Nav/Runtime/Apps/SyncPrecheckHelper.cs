using System;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncPrecheckHelper
{
	public NavAppRuntimeMetadata AppForAdd { get; }

	public AppId AppId { get; }

	public Version TargetVersion { get; }

	public Version OldVersion { get; }

	public SyncNavAppStatus Status { get; }

	public string Error { get; }

	private SyncPrecheckHelper(SyncNavAppStatus status, string error)
	{
		Status = status;
		Error = error;
	}

	private SyncPrecheckHelper(NavAppRuntimeMetadata appForAdd, AppId appId, Version targetVersion, Version oldVersion)
	{
		Status = SyncNavAppStatus.InProgress;
		AppForAdd = appForAdd;
		AppId = appId;
		TargetVersion = targetVersion;
		OldVersion = oldVersion;
	}

	public static SyncPrecheckHelper CreateForSuccess(NavAppRuntimeMetadata appForAdd, AppId appId, Version targetVersion, Version oldVersion)
	{
		return new SyncPrecheckHelper(appForAdd, appId, targetVersion, oldVersion);
	}

	public static SyncPrecheckHelper CreateForError(SyncNavAppStatus status, string error)
	{
		return new SyncPrecheckHelper(status, error);
	}

	public static SyncPrecheckHelper SyncNavAppPreCheck(SyncTargetResolver syncTargetResolver, string name, string publisher, Version version, NavAppSyncMode appSyncMode)
	{
		return appSyncMode switch
		{
			NavAppSyncMode.Add => syncTargetResolver.ResolveSyncTargetForAdd(name, publisher, version), 
			NavAppSyncMode.Development => syncTargetResolver.ResolveSyncTargetForDevelopment(name, publisher), 
			NavAppSyncMode.Clean => syncTargetResolver.ResolveSyncTargetForClean(name, publisher), 
			NavAppSyncMode.ForceSync => syncTargetResolver.ResolveSyncTargetForForce(name, publisher, version), 
			_ => throw new InvalidOperationException("Unsupported sync mode: " + appSyncMode), 
		};
	}
}
