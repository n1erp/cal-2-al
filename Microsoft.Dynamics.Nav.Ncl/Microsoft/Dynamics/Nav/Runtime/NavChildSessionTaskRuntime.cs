using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavChildSessionTaskRuntime : INavChildSessionTaskRuntime
{
	private readonly NavChildSessionTask task;

	private readonly NavSession parentSession;

	public NavChildSessionTaskRuntime(NavChildSessionTask task, NavSession parentSession)
	{
		this.task = task ?? throw new ArgumentNullException("task");
		this.parentSession = parentSession ?? throw new ArgumentNullException("task");
	}

	public void Run()
	{
		DiagnosticsResolver.GetMostSpecificInstance(parentSession).SendTraceTag("00007VX", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "About to run a child session. Readonly: " + task.RunInReadOnlySession + ", Timeout: " + task.Timeout.TotalSeconds + "s.");
		task.ValidateTaskCanStart();
		task.Status = NavChildSessionTaskStatus.Running;
		if (task.CancellationTokenSource.IsCancellationRequested || !task.BeforeRunTask())
		{
			DiagnosticsResolver.GetMostSpecificInstance(parentSession).SendTraceTag("00007VY", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Child session task prerequisites were not met. Aborting execution.");
			return;
		}
		NavSession session = NavCurrentThread.Session;
		NavSession navSession = null;
		NavChildSessionTaskError navChildSessionTaskError = null;
		try
		{
			navSession = new NavSession(parentSession.Tenant, parentSession.ClientTimeZone, parentSession.Authenticator.Clone(), parentSession.AppId);
			navSession.ClientConnectionType = ConnectionType.Background;
			navSession.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, navSession);
			navSession.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.StartSessionParentComment, parentSession.Id);
			navSession.AccessLock.BeginExclusiveExecution();
			navSession.Open();
			DiagnosticsResolver.GetMostSpecificInstance(parentSession).SendTraceTag("00007SO", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, TraceStringHelper.CreateALFunctionStatementTrace(parentSession, "StartChildSession (Session ID: " + navSession.Id.ToString(CultureInfo.InvariantCulture) + "). Parent: " + parentSession.Id));
			navSession.RegionalSettings = parentSession.RegionalSettings;
			navSession.WorkDate = parentSession.WorkDate;
			navSession.OpenCompany(parentSession.CompanyName);
			if (task.Timeout.Milliseconds > 0)
			{
				task.CancellationTokenSource.CancelAfter(task.Timeout);
			}
			try
			{
				NavCurrentThread.Session = navSession;
				task.RunTaskInChildSession();
				if (navSession.IsTransactionActive())
				{
					navSession.EndTransaction(!task.RunInReadOnlySession);
				}
			}
			catch (NavSessionTerminatedException exception)
			{
				LogWriterHelper.LogExceptionEvent(exception);
			}
			catch (NavBaseException ex)
			{
				navSession.SessionComment = ex.Message;
				LogWriterHelper.LogExceptionEvent(ex, EventLogEntryType.Error);
				navChildSessionTaskError = new NavChildSessionTaskError(navSession, ex);
			}
			NavCurrentThread.Session = session;
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
			NavCurrentThread.Session = session;
			navSession.Dispose();
			NavCurrentThread.Session = session;
		}
	}
}
