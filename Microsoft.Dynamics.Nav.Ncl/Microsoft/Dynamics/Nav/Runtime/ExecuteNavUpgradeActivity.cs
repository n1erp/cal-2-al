using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class ExecuteNavUpgradeActivity : NativeActivity
{
	public NavUpgradeActivityStep UpgradeStep { get; }

	public string BookmarkName { get; }

	public bool TransactionalSession { get; }

	public bool PostUpgradeActivity { get; }

	protected ExecuteNavUpgradeActivity(string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
	{
		BookmarkName = bookmarkName;
		UpgradeStep = upgradeStep;
		DisplayName = bookmarkName;
		TransactionalSession = transactionalSession;
		PostUpgradeActivity = postUpgradeActivity;
	}

	protected sealed override void Execute(NativeActivityContext context)
	{
		INavWorkflowTenantSessionManager extension = context.GetExtension<INavWorkflowTenantSessionManager>();
		if (extension == null)
		{
			throw new InvalidWorkflowException("Activity requires a INavWorkflowTenantSessionManager extension instance");
		}
		if (TransactionalSession)
		{
			extension.RunActionInSystemSession((NavSession session) =>
			{
				Execute(context, session);
			});
		}
		else
		{
			extension.RunActionInSystemSessionWithoutTransaction((NavSession session) =>
			{
				Execute(context, session);
			});
		}
	}

	protected abstract void Execute(NativeActivityContext context, NavSession session);
}
