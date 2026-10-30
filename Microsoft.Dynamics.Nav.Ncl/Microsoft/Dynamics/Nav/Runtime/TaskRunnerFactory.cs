using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TaskRunnerFactory
{
	public static TaskRunner CreateTaskRunner(TaskScheduleInfo scheduleInfo, ISystemTaskFactory systemTaskFactory)
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		mostSpecificInstance.SendTraceTag("000002X", Category.TaskScheduling, Verbosity.Normal, "CreateTaskRunInfo: {0}", DiagnosticParameter.Oii(scheduleInfo.GetFullTraceText()));
		string text = string.Format(CultureInfo.InvariantCulture, "TaskId={0}, TenantId={1}", scheduleInfo.Id, scheduleInfo.TenantId);
		if (!NavEnvironment.Instance.Tenants.TryGetTenantById(scheduleInfo.TenantId, out var tenant))
		{
			throw new NavTenantNotFoundException();
		}
		if (!scheduleInfo.SetStarting())
		{
			mostSpecificInstance.SendTraceTag("000002Z", Category.TaskScheduling, Verbosity.Verbose, "Task picked up on another NST: {0}", DiagnosticParameter.Oii(text));
			return null;
		}
		try
		{
			return scheduleInfo.TaskType switch
			{
				NavTaskSchedulerTaskType.Codeunit => (TaskRunner)CreateTaskRunnerCodeUnit(scheduleInfo, tenant, text), 
				NavTaskSchedulerTaskType.System => CreateTaskRunnerSystem(scheduleInfo, tenant, text, systemTaskFactory), 
				_ => throw new InvalidOperationException("Trying to create task runner of unknown task type."), 
			};
		}
		catch (Exception exception)
		{
			mostSpecificInstance.SendExceptionTag("0000034", Category.TaskScheduling, exception);
			LogWriterHelper.LogExceptionEvent(exception);
			return null;
		}
	}

	internal static TaskRunnerCodeUnit CreateTaskRunnerCodeUnit(TaskScheduleInfo scheduleInfo, NavTenant tenant, string traceText)
	{
		return NavTaskSchedulerHelpers.RunCodeAsSystemSessionInTenant(tenant, (NavSession session, NavRecord record) =>
		{
			session.Diagnostics.SendTraceTag("0000030", Category.TaskScheduling, Verbosity.Normal, "Read tenant TaskSchedule: {0}", DiagnosticParameter.Oii(traceText));
			try
			{
				if (!record.ALGet(DataError.TrapError, new NavGuid(scheduleInfo.Id)))
				{
					session.Diagnostics.SendTraceTag("0000031", Category.TaskScheduling, Verbosity.Warning, "Tenant TaskSchedule not found {0}", DiagnosticParameter.Oii(traceText));
					scheduleInfo.SetDone(throwOnError: true);
					return (TaskRunnerCodeUnit)null;
				}
			}
			catch (NavBaseException)
			{
				session.Diagnostics.SendTraceTag("0000032", Category.TaskScheduling, Verbosity.Warning, "Unable to read tenant TaskSchedule - Retry: {0}", DiagnosticParameter.Oii(traceText));
				scheduleInfo.SetReadyWithRetry(throwOnError: true);
				return (TaskRunnerCodeUnit)null;
			}
			if (scheduleInfo.SetRunning())
			{
				session.Diagnostics.SendTraceTag("0000033", Category.TaskScheduling, Verbosity.Normal, "Create TaskRunnerCodeUnit: {0}", DiagnosticParameter.Oii(traceText));
				return new TaskRunnerCodeUnit(scheduleInfo, tenant, record.GetFieldValueSafe(2, NavType.GUID).ToGuid(), record.GetFieldValueSafe(3, NavType.Text).ToString(), record.GetFieldValueSafe(4, NavType.Integer).ToInt32(), record.GetFieldValueSafe(5, NavType.Integer).ToInt32(), record.GetFieldValueSafe(10, NavType.Text).ToString(), record.GetFieldValueSafe(20, NavType.Integer).ToInt32(), record.GetFieldValueSafe(21, NavType.Integer).ToInt32(), (NavRecordId)record.GetFieldValueSafe(22, NavType.RecordID), record.GetFieldValueSafe(6, NavType.Text).ToString(), record.GetFieldValueSafe(7, NavType.Text).ToString());
			}
			return (TaskRunnerCodeUnit)null;
		}, allowAppsDisabledMode: false);
	}

	internal static TaskRunnerSystem CreateTaskRunnerSystem(TaskScheduleInfo scheduleInfo, NavTenant tenant, string traceText, ISystemTaskFactory systemTaskFactory)
	{
		NavSystemScheduledTask systemScheduledTask;
		try
		{
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenant.Database, updateOrlockingActivity: true);
			systemScheduledTask = NavSqlSystemScheduledTask.GetSystemScheduledTask(conScope, scheduleInfo.Id);
		}
		catch (NavBaseException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001IE", Category.TaskScheduling, Verbosity.Warning, "Unable to read tenant SystemScheduledTask - Retry: {0}", DiagnosticParameter.Oii(traceText));
			scheduleInfo.SetReadyWithRetry(throwOnError: true);
			return null;
		}
		if (systemScheduledTask == null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001IF", Category.TaskScheduling, Verbosity.Warning, "Tenant SystemScheduledTask not found  {0}", DiagnosticParameter.Oii(traceText));
			scheduleInfo.SetDone(throwOnError: true);
			return null;
		}
		if (!scheduleInfo.SetRunning())
		{
			return null;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001GW", Category.TaskScheduling, Verbosity.Normal, "Create TaskRunnerSystem: {0}", DiagnosticParameter.Oii(traceText));
		ISystemTask systemTask = systemTaskFactory.GetSystemTask(systemScheduledTask.SystemTaskActionId);
		if (systemTask == null)
		{
			scheduleInfo.SetDone(throwOnError: false);
			return null;
		}
		systemTask.Initialize(systemScheduledTask.Parameters);
		return new TaskRunnerSystem(scheduleInfo, tenant, systemTask);
	}
}
