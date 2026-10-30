using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class TaskRunner
{
	protected readonly TaskScheduleInfo ScheduleInfo;

	protected readonly NavTenant Tenant;

	private long startTick;

	private Task task;

	internal TaskRunState RunState { get; private set; }

	internal bool IsCompleted
	{
		get
		{
			if (RunState != TaskRunState.Completed)
			{
				return RunState == TaskRunState.CompletedDoneNotSet;
			}
			return true;
		}
	}

	internal bool IsFaulted
	{
		get
		{
			if (RunState != TaskRunState.FailedRetry && RunState != TaskRunState.FailedRetryNotSet)
			{
				return task.Status == TaskStatus.Faulted;
			}
			return true;
		}
	}

	internal bool IsTaskFinished
	{
		get
		{
			if (!task.IsCanceled && !task.IsCompleted)
			{
				return task.IsFaulted;
			}
			return true;
		}
	}

	internal bool HasFinalStateBeenSet
	{
		get
		{
			if (task.Status != TaskStatus.RanToCompletion)
			{
				return false;
			}
			if (RunState != TaskRunState.FailedRetry && RunState != TaskRunState.Completed)
			{
				return RunState == TaskRunState.Failed;
			}
			return true;
		}
	}

	protected TaskRunner(TaskScheduleInfo scheduleInfo, NavTenant tenant)
	{
		ScheduleInfo = scheduleInfo;
		Tenant = tenant;
		RunState = TaskRunState.Created;
	}

	protected abstract bool ShouldDeleteTaskOnException();

	protected void HandleSessionTermination(NavSessionTerminatedException sessionTerminatedException, NavSession session)
	{
		session.Diagnostics.SendExceptionTag("000003Q", Category.TaskScheduling, sessionTerminatedException);
		LogWriterHelper.LogExceptionEvent(sessionTerminatedException);
		session.EndTransaction(commit: false);
		if (NavEnvironment.Instance.InServerShutdown || NavCurrentThread.Session.Tenant.IsTenantDismounting)
		{
			SetRetryOrDoneIfRetryLimitExceeded(throwOnError: true);
			return;
		}
		session.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.ForceStopScheduledTaskComment, ScheduleInfo.Id);
		SetFailedAndRemove(session, setSessionState: false);
	}

	protected void LogExceptionMessage(NavSession session, Exception exception, string traceState, bool isSuppressedException)
	{
		isSuppressedException |= (exception as NavBaseException)?.SuppressMessage ?? false;
		Verbosity verbosity = (isSuppressedException ? Verbosity.Normal : Verbosity.Error);
		session.Diagnostics.SendExceptionTag("000014P", Category.TaskScheduling, exception, verbosity, DiagnosticParameter.SystemMetadata("Task " + GetTraceText() + " Scheduling State " + traceState));
		if (!isSuppressedException)
		{
			LogWriterHelper.LogExceptionEvent(exception);
		}
	}

	protected void SetFailedAndRemove(NavSession session, bool setSessionState)
	{
		try
		{
			session.BeginTransaction();
			RemoveTaskScheduleFromTenant(session);
			SetDone(throwOnError: false);
			session.Diagnostics.SendTraceTag("00000GX", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Task {ScheduleInfo.Id} has been removed as it faulted"));
		}
		finally
		{
			session.EndTransaction(commit: true);
		}
		if (setSessionState)
		{
			SetSessionCommentTaskFailed(session);
		}
	}

	protected void StartExecutionTimeCounter()
	{
		startTick = NavPerformanceCounterSetter.GetPerformanceCounterTicks();
	}

	protected abstract void RemoveTaskScheduleFromTenant(NavSession session);

	protected void SetInitializing()
	{
		RunState = TaskRunState.Initializing;
	}

	protected void SetRunning()
	{
		RunState = TaskRunState.Running;
	}

	protected void SetFailed()
	{
		RunState = TaskRunState.Failed;
	}

	internal Task RunAsync()
	{
		task = NavTaskFactory.RunTask((Action)InternalRun);
		return task;
	}

	internal void InternalRun()
	{
		using (new NavAppUsageEventSuppressionScope(NavCurrentThread.Session, "Task Scheduler"))
		{
			StartExecutionTimeCounter();
			SetInitializing();
			try
			{
				TaskSpecificInternalRun();
			}
			catch (Exception ex)
			{
				if (ex is ObjectDisposedException exception && (Tenant.IsTenantDismounting || Tenant.IsDisposed))
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000B3", Category.TaskScheduling, exception, Verbosity.Verbose);
					return;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000003P", Category.TaskScheduling, ex);
				if (RunState == TaskRunState.Initializing)
				{
					if (!ScheduleInfo.ExceedsRetryLimit())
					{
						SetRetryOrDoneIfRetryLimitExceeded(throwOnError: false);
						return;
					}
					SetDone(throwOnError: true);
					if (ShouldDeleteTaskOnException())
					{
						NavTaskSchedulerHelpers.RunAsSystemSession(Tenant, (NavSession systemSesssion) =>
						{
							systemSesssion.BeginTransaction();
							RemoveTaskScheduleFromTenant(systemSesssion);
							systemSesssion.EndTransaction(commit: true);
						});
					}
					SetFailed();
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000B4", Category.TaskScheduling, ex);
			}
			finally
			{
				if (!IsCompleted)
				{
					NavEnvironment.Instance.PerformanceCounterSetter.IncrementTaskErrorsPerformanceCounter();
				}
			}
		}
	}

	protected abstract void TaskSpecificInternalRun();

	internal bool SetRetryOrDoneIfRetryLimitExceeded(bool throwOnError)
	{
		if (RunState == TaskRunState.FailedRetry)
		{
			return true;
		}
		RunState = TaskRunState.FailedRetryNotSet;
		if (!ScheduleInfo.ExceedsRetryLimit())
		{
			if (!ScheduleInfo.SetReadyWithRetry(throwOnError))
			{
				return false;
			}
			RunState = TaskRunState.FailedRetry;
		}
		else
		{
			Verbosity verbosity = ((ScheduleInfo.TaskType == NavTaskSchedulerTaskType.System) ? Verbosity.Warning : Verbosity.Normal);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001GY", Category.TaskScheduling, verbosity, PrivacyClassification.OrganizationIdentifiableInformation, "{0} task {1} for tenant {2} has reached the maximum retry count and will be set to done.", ScheduleInfo.TaskType, ScheduleInfo.Id, ScheduleInfo.TenantId);
			if (!SetDone(throwOnError))
			{
				return false;
			}
		}
		UpdateTaskExecutionTime();
		return true;
	}

	internal bool SetDone(bool throwOnError)
	{
		if (RunState == TaskRunState.Completed)
		{
			return true;
		}
		RunState = TaskRunState.CompletedDoneNotSet;
		if (!ScheduleInfo.SetDone(throwOnError))
		{
			return false;
		}
		RunState = TaskRunState.Completed;
		UpdateTaskExecutionTime();
		return true;
	}

	protected void SetSessionCommentStartScheduledTask(NavSession session)
	{
		session.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.StartScheduledTaskComment, ScheduleInfo.Id);
	}

	protected void SetSessionCommentTaskFailed(NavSession session)
	{
		session.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.FailedScheduledTaskComment, ScheduleInfo.Id);
	}

	internal string GetTraceText()
	{
		return "TaskId=" + ScheduleInfo.Id.ToString();
	}

	private void UpdateTaskExecutionTime()
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdateTaskExecutionTime(startTick);
	}
}
