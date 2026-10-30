using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppDataUpgradeActivities
{
	public NavAppJointUpgradeContext Context { get; }

	public Activity HandleSpecialRecords { get; set; }

	public Activity CheckPreconditions { get; set; }

	public Activity OnUpgrade { get; set; }

	public Activity ValidateUpgrade { get; set; }

	public Activity ClearGlobalCache { get; set; }

	public Activity SetTransactionalPhaseComplete { get; set; }

	public Activity AfterUpgradeCommit { get; set; }

	public Activity UpdateAppDatabase { get; set; }

	public NavAppDataUpgradeActivities(NavAppJointUpgradeContext context)
	{
		Context = context;
	}
}
