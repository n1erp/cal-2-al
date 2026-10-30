using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TaskRunnerSystem : TaskRunner
{
	private readonly ISystemTask systemTask;

	private static readonly NavMetric3Dimensions TaskRunTimeMetric = new NavMetric3Dimensions("SystemTaskRunTimeMs", "TaskId", "TenantId", "Result");

	public TaskRunnerSystem(TaskScheduleInfo scheduleInfo, NavTenant tenant, ISystemTask systemTask)
		: base(scheduleInfo, tenant)
	{
		this.systemTask = systemTask;
	}

	protected override void TaskSpecificInternalRun()
	{
		using NavSession navSession = ((ITenantSessionHandler)Tenant).CreateSystemSession();
		using (NavThread.AddCurrentThreadToScheduler(NavEnvironment.Instance.ThreadScheduler))
		{
			navSession.AccessLock.BeginExclusiveExecution();
			navSession.Open(false);
			Stopwatch stopwatch = new Stopwatch();
			string traceState = "SystemSessionStarted";
			try
			{
				NavCurrentThread.Session = navSession;
				navSession.Diagnostics.SendTraceTag("00001GX", Category.TaskScheduling, Verbosity.Normal, "TaskScheduling.SystemTaskInternalRun {0}, SystemTask={1}, Parameters={2}", DiagnosticParameter.Oii(ScheduleInfo.GetFullTraceText()), DiagnosticParameter.SystemMetadata(systemTask.GetType().Name), DiagnosticParameter.Oii(systemTask.GetNonPersonalDataParametersForTelemetry()));
				SetSessionCommentStartScheduledTask(navSession);
				SetRunning();
				navSession.BeginTransaction();
				stopwatch.Start();
				systemTask.Run(navSession);
				TaskRunTimeMetric.LogValue(stopwatch.ElapsedMilliseconds, ScheduleInfo.Id.ToString(), ScheduleInfo.TenantId, "Success");
				traceState = "SystemTaskExecuted";
				RemoveTaskScheduleFromTenant(navSession);
				traceState = "SystemTaskRemoved";
				navSession.EndTransaction(commit: true);
				SetDone(throwOnError: true);
			}
			catch (NavSessionTerminatedException ex)
			{
				TaskRunTimeMetric.LogValue(stopwatch.ElapsedMilliseconds, ScheduleInfo.Id.ToString(), ScheduleInfo.TenantId, "Terminated");
				LogExceptionMessage(navSession, ex, traceState, isSuppressedException: false);
				HandleSessionTermination(ex, navSession);
			}
			catch (Exception exception)
			{
				TaskRunTimeMetric.LogValue(stopwatch.ElapsedMilliseconds, ScheduleInfo.Id.ToString(), ScheduleInfo.TenantId, "Failed");
				LogExceptionMessage(navSession, exception, traceState, isSuppressedException: false);
				navSession.EndTransaction(commit: false);
				if (!systemTask.ShouldRetry() || (SetRetryOrDoneIfRetryLimitExceeded(throwOnError: true) && ScheduleInfo.IsDone))
				{
					SetFailedAndRemove(navSession, setSessionState: true);
				}
			}
			finally
			{
				NavCurrentThread.Session = null;
			}
		}
	}

	protected override void RemoveTaskScheduleFromTenant(NavSession session)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Database, updateOrlockingActivity: true);
		NavSqlSystemScheduledTask.Delete(conScope, ScheduleInfo.Id);
	}

	protected override bool ShouldDeleteTaskOnException()
	{
		return true;
	}
}
