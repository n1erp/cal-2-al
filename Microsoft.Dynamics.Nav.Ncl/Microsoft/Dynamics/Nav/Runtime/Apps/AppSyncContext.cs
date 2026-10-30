using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AppSyncContext
{
	private static readonly Dictionary<NavAppSyncMode, SyncMode> modeMap = new Dictionary<NavAppSyncMode, SyncMode>
	{
		{
			NavAppSyncMode.Add,
			SyncMode.Sync
		},
		{
			NavAppSyncMode.Clean,
			SyncMode.ForceSync
		},
		{
			NavAppSyncMode.Development,
			SyncMode.Sync
		},
		{
			NavAppSyncMode.ForceSync,
			SyncMode.ForceSync
		}
	};

	public NavAppRuntimeMetadata AppForAdd { get; }

	public NavAppGroup TargetGroup { get; }

	public AppId AppId { get; }

	public Version Version { get; }

	public Version OldVersion { get; }

	public NavAppSyncMode AppSyncMode { get; }

	public SyncMode SyncEngineMode { get; }

	public string ActivityId { get; }

	public AppSyncContext(NavAppRuntimeMetadata appForAdd, NavAppGroup group, AppId appId, Version version, Version oldVersion, NavAppSyncMode appSyncMode, string activityId)
	{
		AppForAdd = appForAdd;
		TargetGroup = group;
		AppId = appId;
		Version = version;
		OldVersion = oldVersion;
		AppSyncMode = appSyncMode;
		ActivityId = activityId;
		SyncEngineMode = modeMap[appSyncMode];
	}
}
