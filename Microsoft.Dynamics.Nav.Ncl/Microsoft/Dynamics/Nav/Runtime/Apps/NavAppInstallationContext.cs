using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppInstallationContext : INavAppOperationContext
{
	public NavAppGroup TargetGroup { get; }

	public NavTenant Tenant { get; }

	public NavAppRuntimeMetadata AppBeingTargeted { get; }

	public bool IsInstalling { get; }

	public NavAppSyncOptions SyncOptions { get; }

	public SyncMode SyncMode { get; }

	public string ActivityId { get; }

	public bool HasData { get; }

	private NavAppInstallationContext(NavTenant tenant, NavAppGroup targetGroup, NavAppRuntimeMetadata appBeingTargeted, bool isInstalling, NavAppSyncOptions syncOptions, string activityId, bool hasData)
	{
		Tenant = tenant;
		TargetGroup = targetGroup;
		AppBeingTargeted = appBeingTargeted;
		IsInstalling = isInstalling;
		SyncOptions = syncOptions;
		ActivityId = activityId;
		SyncMode = (isInstalling ? SyncMode.Sync : SyncMode.ForceSync);
		HasData = hasData;
	}

	public static NavAppInstallationContext CreateForInstall(NavTenant tenant, NavAppGroup targetGroup, NavAppRuntimeMetadata appBeingChangedRuntimeMetadata, NavAppSyncOptions syncOptions, string activityId, bool hasData)
	{
		return new NavAppInstallationContext(tenant, targetGroup, appBeingChangedRuntimeMetadata, isInstalling: true, syncOptions, activityId, hasData);
	}

	public static NavAppInstallationContext CreateForUninstall(NavTenant tenant, NavAppGroup targetGroup, NavAppRuntimeMetadata appBeingChangedRuntimeMetadata, NavAppSyncOptions syncOptions, string activityId)
	{
		return new NavAppInstallationContext(tenant, targetGroup, appBeingChangedRuntimeMetadata, isInstalling: false, syncOptions, activityId, hasData: true);
	}
}
