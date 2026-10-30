using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavTaskSchedulerUtility
{
	internal static void CancelAllTasksForCompany(string tenantId, string companyName)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000175 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000175);
			NavText navText = NavText.Create(companyName);
			navRecord.ALSetRange(10, navText, navText);
			if (!navRecord.ALFindFirst(DataError.TrapError))
			{
				return;
			}
			Guid task;
			do
			{
				task = navRecord.GetFieldValueSafe(1, NavType.GUID).ToGuid();
				NavCurrentThread.Session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope conScope) =>
				{
					NavEnvironment.Instance.TaskScheduler.CancelTask(task, tenantId);
				}));
				navRecord.ALDelete(DataError.TrapError);
			}
			while (navRecord.ALNext() != 0);
		}
	}

	internal static Guid ScheduleSystemTask(SystemTaskActionId systemTaskActionId, SystemTaskParameters parameters, DateTime notBefore)
	{
		NavSession session = NavCurrentThread.Session;
		Guid taskId = Guid.NewGuid();
		NavSystemScheduledTask systemScheduledTask = new NavSystemScheduledTask(taskId, systemTaskActionId, parameters.GetSerializedAsJson(), session.Tenant.Id, notBefore);
		session.EnsureWriteTransactionStarted();
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Tenant.Database, TransactionType.Update))
		{
			LogRecordOperation(systemScheduledTask, parameters, "Inserting system scheduled task.");
			NavSqlSystemScheduledTask.Insert(conScope, systemScheduledTask);
		}
		session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope appConScope) =>
		{
			NavEnvironment.Instance.TaskScheduler.CreateTask(taskId, NavCurrentThread.Session.Tenant.Id, systemScheduledTask.NotBefore, isReady: true, NavTaskSchedulerTaskType.System);
			session.Diagnostics.SendTraceTag("00001GS", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task commit action executed adding system scheduled task to application database for id = {0}", taskId);
		}));
		session.Diagnostics.SendTraceTag("00001GT", Category.TaskScheduling, Verbosity.Verbose, "System task with task id {0} queued for scheduling using NavTaskScheduler.ScheduleSystemTask by {1}", DiagnosticParameter.SystemMetadata(taskId), DiagnosticParameter.Euii(session.User?.DisplayName));
		return taskId;
	}

	internal static bool CancelSystemTask(Guid taskId)
	{
		NavSession session = NavCurrentThread.Session;
		session.EnsureWriteTransactionStarted();
		bool result;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Database, TransactionType.Update))
		{
			result = NavSqlSystemScheduledTask.Delete(conScope, taskId);
		}
		session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope appConScope) =>
		{
			NavEnvironment.Instance.TaskScheduler.CancelTask(taskId, NavCurrentThread.Session.Tenant.Id);
			session.Diagnostics.SendTraceTag("00001GU", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task commit action executed deleting system scheduled task in application database for id = {0}", taskId);
		}));
		session.Diagnostics.SendTraceTag("00001GV", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task queued for delete by NavTaskScheduler.CancelSystemTask id = {0}", taskId);
		return result;
	}

	private static void LogRecordOperation(NavSystemScheduledTask record, SystemTaskParameters parameters, string message)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UL", Category.TaskScheduling, Verbosity.Verbose, message + " Record with taskId {0} for tenant {1} to be executed after {2} with parameters: {3}", DiagnosticParameter.SystemMetadata(record.TaskId), DiagnosticParameter.Oii(record.TenantId), DiagnosticParameter.SystemMetadata(record.NotBefore), DiagnosticParameter.Oii(parameters.GetNonPersonalDataParametersForTelemetry()));
	}
}
