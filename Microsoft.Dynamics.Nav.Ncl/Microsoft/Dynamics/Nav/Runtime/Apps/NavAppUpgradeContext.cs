using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppUpgradeContext : INavAppOperationContext
{
	public bool TransactionalPhaseCompleted { get; private set; }

	public NavTenant Tenant { get; }

	public NavAppGroup TargetGroup { get; }

	public NavAppRuntimeMetadata AppBeingTargeted { get; }

	public INavAppReference OldAppBeingUpgradedFrom { get; }

	public string ActivityId { get; }

	private NavAppUpgradeContext(NavTenant tenant, NavAppGroup targetGroup, NavAppRuntimeMetadata appBeingTargeted, INavAppReference oldApp, string activityId)
	{
		AppBeingTargeted = appBeingTargeted;
		OldAppBeingUpgradedFrom = oldApp;
		TargetGroup = targetGroup;
		ActivityId = activityId;
		Tenant = tenant;
		TransactionalPhaseCompleted = false;
	}

	public static NavAppUpgradeContext Create(NavTenant tenant, NavAppGroup targetGroup, NavAppRuntimeMetadata newApp, INavAppReference oldApp, string activityId)
	{
		return new NavAppUpgradeContext(tenant, targetGroup, newApp, oldApp, activityId);
	}

	public void SetTransactionalPhaseComplete()
	{
		TransactionalPhaseCompleted = true;
	}
}
