using System;
using System.Activities;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class ExecuteNavAppUpgradeSessionActionActivity<T> : ExecuteNavUpgradeActivity where T : INavAppOperationContext
{
	public T Context { get; }

	public Action<T, NavSession> SessionAction { get; }

	public ExecuteNavAppUpgradeSessionActionActivity(T operationContext, string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, Action<T, NavSession> sessionAction, bool postUpgradeActivity)
		: base(bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
		Context = operationContext;
		SessionAction = sessionAction;
	}

	protected override void Execute(NativeActivityContext context, NavSession session)
	{
		INavDataUpgradeProgressMonitor extension = context.GetExtension<INavDataUpgradeProgressMonitor>();
		try
		{
			extension.ProcessUpgradeStepStarting(UpgradeStep, null, session.Id);
			SessionAction(Context, session);
			extension.ProcessUpgradeStepComplete(UpgradeStep, null);
		}
		catch (Exception exception)
		{
			extension.ReportEventError(UpgradeStep, null, exception, PostUpgradeActivity);
			throw;
		}
	}
}
