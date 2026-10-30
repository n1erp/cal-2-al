using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavChildSessionTaskTestRuntime : INavChildSessionTaskRuntime
{
	private readonly NavChildSessionTask task;

	private readonly NavSession session;

	public NavChildSessionTaskTestRuntime(NavChildSessionTask task, NavSession session)
	{
		this.task = task ?? throw new ArgumentNullException("task");
		this.session = session ?? throw new ArgumentNullException("task");
	}

	public void Run()
	{
		if (NavCurrentThread.Session != session)
		{
			throw new InvalidOperationException("Test child session can only run on the thread owning the parent session");
		}
		DiagnosticsResolver.GetMostSpecificInstance(session).SendTraceTag("00007VZ", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "About to run code in a simulated child session. Readonly: " + task.RunInReadOnlySession);
		task.ValidateTaskCanStart();
		task.Status = NavChildSessionTaskStatus.Running;
		if (!task.BeforeRunTask())
		{
			DiagnosticsResolver.GetMostSpecificInstance(session).SendTraceTag("00007W0", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Child session task prerequisites were not met. Aborting execution.");
			return;
		}
		ConnectionType clientConnectionType = session.ClientConnectionType;
		IClientCallback clientCallbackOverride = session.ClientCallbackOverride;
		NavCompany company = session.Company;
		Exception lastException = session.LastException;
		NavChildSessionTaskError navChildSessionTaskError = null;
		try
		{
			session.ClientConnectionType = ConnectionType.Background;
			if (!(clientCallbackOverride is HeadlessClientCallback))
			{
				session.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, session);
			}
			session.Company = null;
			session.OpenCompany(company.CompanyName);
			try
			{
				task.RunTaskInChildSession();
			}
			catch (NavBaseException exception)
			{
				LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
				navChildSessionTaskError = new NavChildSessionTaskError(session, exception);
			}
			finally
			{
				session.CloseCompany();
			}
		}
		finally
		{
			session.ClientConnectionType = clientConnectionType;
			session.ClientCallbackOverride = clientCallbackOverride;
			session.Company = company;
			session.LastException = lastException;
		}
		try
		{
			if (navChildSessionTaskError == null)
			{
				task.AfterRunTask();
			}
			else
			{
				task.AfterRunTaskError(navChildSessionTaskError);
			}
		}
		finally
		{
			task.Status = ((navChildSessionTaskError == null) ? NavChildSessionTaskStatus.Completed : NavChildSessionTaskStatus.Error);
		}
	}
}
