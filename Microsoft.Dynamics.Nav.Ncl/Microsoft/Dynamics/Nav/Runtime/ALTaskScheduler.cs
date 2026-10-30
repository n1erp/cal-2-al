using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALTaskScheduler
{
	public static Guid ALCreateTask(int codeunitId, int failureCodeunitId, bool isReady = true, string companyName = null, NavDateTime notBefore = null, NavRecordId recordId = null)
	{
		NavSession session = NavCurrentThread.Session;
		CheckCodeUnit(session, codeunitId);
		CheckCodeUnit(session, failureCodeunitId);
		if (IsTestIsolation(session))
		{
			throw new NavTestStartSessionNotAllowedException();
		}
		if (!CanCreateTask(session))
		{
			throw new NavCreateScheduledTasksNotAllowedException();
		}
		if (string.IsNullOrWhiteSpace(companyName))
		{
			companyName = session.CompanyName;
		}
		CheckRecordExists(session, recordId, companyName);
		Guid id = Guid.NewGuid();
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000175 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000175);
			navRecord.SetFieldValue(1, new NavGuid(id));
			navRecord.SetFieldValue(2, new NavGuid(ALDatabase.ALUserSecurityId()));
			navRecord.SetFieldValue(3, new NavText(ALDatabase.ALUserID));
			navRecord.SetFieldValue(4, NavInteger.Create(session.GlobalLanguage));
			navRecord.SetFieldValue(5, NavInteger.Create(session.RegionalSettings.WindowsLCID));
			navRecord.SetFieldValue(6, NavText.Create(session.ClientTimeZone.Id));
			navRecord.SetFieldValue(7, NavText.Create(session.AppId));
			navRecord.SetFieldValue(10, new NavText(companyName));
			navRecord.SetFieldValue(11, NavBoolean.Create(isReady));
			if (notBefore != null)
			{
				navRecord.SetFieldValue(12, notBefore);
			}
			navRecord.SetFieldValue(20, NavInteger.Create(codeunitId));
			navRecord.SetFieldValue(21, NavInteger.Create(failureCodeunitId));
			if (recordId != null)
			{
				navRecord.SetFieldValue(22, recordId);
			}
			navRecord.ALInsert();
		}
		session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope conScope) =>
		{
			session.Diagnostics.SendTraceTag("00000ZM", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task commit action executed adding scheduled task to application database for id = {0}", id);
			NavEnvironment.Instance.TaskScheduler.CreateTask(id, NavCurrentThread.Session.Tenant.Id, (notBefore == null || notBefore.IsZeroOrEmpty) ? DateTime.UtcNow : notBefore.ToDateTime(), isReady, NavTaskSchedulerTaskType.Codeunit);
		}));
		session.Diagnostics.SendTraceTag("00000ZN", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task queued for scheduling by AL TaskScheduler.CreateTask id = {0}", id);
		return id;
	}

	private static void CheckCodeUnit(NavSession session, int codeUnitId)
	{
		if (codeUnitId == 0)
		{
			return;
		}
		try
		{
			NavGlobal.NCLMetadata.GetMetaCodeunitById(codeUnitId);
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
			session.Diagnostics.SendTraceTag("00000GW", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Code unit {0} could not be found", codeUnitId);
			throw;
		}
	}

	private static void CheckRecordExists(NavSession session, NavRecordId recordId, string companyName)
	{
		if (recordId == null || recordId.IsZeroOrEmpty)
		{
			return;
		}
		try
		{
			using NavRecordRef navRecordRef = new NavRecordRef(session, recordId.TableNo, isTemporary: false, companyName);
			navRecordRef.InitFromRecordId(recordId);
			navRecordRef.Record.ALFind();
		}
		catch (NavCSideRecordNotFoundException)
		{
			session.Diagnostics.SendTraceTag("0000191", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Record {0} could not be found", recordId);
			throw;
		}
	}

	public static bool ALCanCreateTask()
	{
		return CanCreateTask(NavCurrentThread.Session);
	}

	private static bool CanCreateTask(NavSession session)
	{
		if (!session.User.IsDelegatedUser && !session.Authenticator.IsDeviceUser)
		{
			return !IsTestIsolation(session);
		}
		return false;
	}

	private static bool IsTestIsolation(NavSession session)
	{
		if (session.TestExecution != null)
		{
			if (session.TestExecution.CommitTestCodeunits)
			{
				return !session.TestExecution.CommitTestFunctions;
			}
			return true;
		}
		return false;
	}

	public static bool ALTaskExists(Guid task)
	{
		NavCurrentThread.Session.Diagnostics.SendTraceTag("00000ZO", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "AL TaskScheduler.TaskExists executed on id = {0}", task);
		return NavEnvironment.Instance.TaskScheduler.TaskExists(task, NavCurrentThread.Session.Tenant.Id);
	}

	public static bool ALCancelTask(Guid task)
	{
		bool result = false;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000175 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000175);
			if (navRecord.ALGet(DataError.TrapError, NavGuid.CreateFromObject(task)) && navRecord.ALDelete(DataError.TrapError))
			{
				result = true;
			}
		}
		session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope conScope) =>
		{
			session.Diagnostics.SendTraceTag("00000ZP", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task commit action executed deleting scheduled task in application database for id = {0}", task);
			NavEnvironment.Instance.TaskScheduler.CancelTask(task, NavCurrentThread.Session.Tenant.Id);
		}));
		session.Diagnostics.SendTraceTag("00000ZQ", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task queued for delete by AL TaskScheduler.CancelTask id = {0}", task);
		return result;
	}

	public static bool ALSetTaskReady(Guid task, NavDateTime notBefore = null)
	{
		bool flag = false;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000175 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000175);
			if (navRecord.ALGet(DataError.TrapError, NavGuid.CreateFromObject(task)))
			{
				navRecord.SetFieldValueSafe(11, NavType.Boolean, NavBoolean.True);
				if (notBefore == null)
				{
					notBefore = (NavDateTime)navRecord.GetFieldValueSafe(12, NavType.DateTime);
				}
				flag = navRecord.ALModify(DataError.TrapError);
			}
		}
		if (flag)
		{
			session.UncommittedTaskScheduleActions.AddTaskScheduleAction(new TaskScheduleAppAction((NavSqlConnectionScope conScope) =>
			{
				session.Diagnostics.SendTraceTag("00000ZR", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task commit action executed setting scheduled task ready in application database for id = {0}", task);
				NavEnvironment.Instance.TaskScheduler.SetReady(task, NavCurrentThread.Session.Tenant.Id, notBefore.IsZeroOrEmpty ? DateTime.UtcNow : notBefore.ToDateTime());
			}));
			session.Diagnostics.SendTraceTag("00000ZS", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Task queued for setting task ready by AL TaskScheduler.SetTaskReady id = {0}", task);
		}
		return flag;
	}
}
