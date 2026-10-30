using System;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public sealed class NavAppJointUpgradeContext : INavAppOperationContext
{
	public JointUpgradeContextType ContextType { get; }

	public INavAppOperationContext Context { get; }

	public NavTenant Tenant => Context.Tenant;

	public NavAppGroup TargetGroup => Context.TargetGroup;

	public NavAppRuntimeMetadata AppBeingTargeted => Context.AppBeingTargeted;

	public string ActivityId => Context.ActivityId;

	private NavAppJointUpgradeContext(JointUpgradeContextType type, INavAppOperationContext context)
	{
		ContextType = type;
		Context = context ?? throw new ArgumentNullException("context");
	}

	public static NavAppJointUpgradeContext CreateForInstall(NavAppInstallationContext context)
	{
		return new NavAppJointUpgradeContext(JointUpgradeContextType.Install, context);
	}

	public static NavAppJointUpgradeContext CreateForUpgrade(NavAppUpgradeContext context)
	{
		return new NavAppJointUpgradeContext(JointUpgradeContextType.Upgrade, context);
	}
}
