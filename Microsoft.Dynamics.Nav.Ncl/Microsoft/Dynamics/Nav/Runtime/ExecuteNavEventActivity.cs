using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class ExecuteNavEventActivity : ExecuteNavUpgradeActivity, INavErrorReporter
{
	private INavDataUpgradeProgressMonitor progressMonitor;

	private bool operationCompletedSuccessfully;

	private string currentCompany;

	public IEnumerable<string> Companies { get; }

	protected ExecuteNavEventActivity(IEnumerable<string> companies, string bookmarkName, NavUpgradeActivityStep upgradeStep, bool transactionalSession, bool postUpgradeActivity)
		: base(bookmarkName, upgradeStep, transactionalSession, postUpgradeActivity)
	{
		Companies = companies?.ToArray() ?? new string[1];
	}

	protected override void Execute(NativeActivityContext context, NavSession session)
	{
		INavUpgradeEventExecutorProvider extension = context.GetExtension<INavUpgradeEventExecutorProvider>();
		progressMonitor = context.GetExtension<INavDataUpgradeProgressMonitor>();
		foreach (string company in Companies)
		{
			operationCompletedSuccessfully = true;
			currentCompany = company;
			try
			{
				session.SetIsolatedEventErrorReporter(this);
				progressMonitor.ProcessUpgradeStepStarting(UpgradeStep, company, session.Id);
				InvokeEvent(extension, company);
				if (operationCompletedSuccessfully)
				{
					progressMonitor.ProcessUpgradeStepComplete(UpgradeStep, company);
				}
			}
			catch (NavBaseException exception)
			{
				ReportError(null, exception);
				throw;
			}
			catch (TargetException ex)
			{
				ReportError(null, ex);
				throw new NavUpgradeActivityExecutionException(ex.Message, ex);
			}
			finally
			{
				session.ClearIsolatedEventErrorReporter();
			}
		}
	}

	public void ReportError(string errorMessage, Exception exception)
	{
		operationCompletedSuccessfully = false;
		progressMonitor.ReportEventError(UpgradeStep, currentCompany, exception, PostUpgradeActivity);
	}

	protected abstract void InvokeEvent(INavUpgradeEventExecutorProvider eventExecutorProvider, string company);
}
