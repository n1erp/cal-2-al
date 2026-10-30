using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TaskRunnerCodeUnit : TaskRunner
{
	private readonly string company;

	private readonly int failureCodeunit;

	private readonly NavRecordId recordId;

	private readonly int runCodeunit;

	private readonly TimeZoneInfo timeZone;

	private readonly Guid user;

	private readonly string userAppId;

	private readonly int userFormatId;

	private readonly int userLanguageId;

	private readonly string userName;

	internal Action<int, NavRecordRef> CustomRunCodeunit { get; set; }

	public TaskRunnerCodeUnit(TaskScheduleInfo scheduleInfo, NavTenant tenant, Guid user, string userName, int userLanguageId, int userFormatId, string company, int runCodeunit, int failureCodeunit, NavRecordId recordId, string timeZoneId, string userAppId)
		: base(scheduleInfo, tenant)
	{
		this.user = user;
		this.userName = userName;
		this.userLanguageId = userLanguageId;
		this.userAppId = userAppId;
		this.userFormatId = userFormatId;
		this.company = company;
		this.runCodeunit = runCodeunit;
		this.failureCodeunit = failureCodeunit;
		this.recordId = recordId;
		if (timeZoneId != null)
		{
			try
			{
				timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			}
			catch (Exception)
			{
			}
		}
		timeZone = timeZone ?? tenant.TenantSettings.DefaultTimeZone;
	}

	protected override void TaskSpecificInternalRun()
	{
		using NavSession navSession = ((ITenantSessionHandler)Tenant).CreateTaskSchedulerSession(user, userName, timeZone, userAppId);
		NavCurrentThread.Session = navSession;
		using (navSession.Diagnostics.CreateTraceScope("00001FW", DiagnosticParameter.SystemMetadata("TaskScheduling.CodeUnitInternalRun"), MonitorType.None, Category.TaskScheduling))
		{
			navSession.Diagnostics.SendTraceTag("000004J", Category.TaskScheduling, Verbosity.Normal, "TaskScheduling.CodeUnitInternalRun {0}, Company={1}, UserId={2}, UserName={3}, CU={4}, FCU={5}, RID={6}, LangId={7}, FormatId={8}", DiagnosticParameter.Oii(ScheduleInfo.GetFullTraceText()), DiagnosticParameter.Oii(company), DiagnosticParameter.Eupi(user), DiagnosticParameter.Euii(userName), DiagnosticParameter.SystemMetadata(runCodeunit), DiagnosticParameter.SystemMetadata(failureCodeunit), DiagnosticParameter.SystemMetadata((recordId == null) ? string.Empty : recordId.ToString()), DiagnosticParameter.SystemMetadata(userLanguageId), DiagnosticParameter.SystemMetadata(userFormatId));
			using (NavThread.AddCurrentThreadToScheduler(NavEnvironment.Instance.ThreadScheduler))
			{
				navSession.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, navSession);
				navSession.ClientTimeZone = timeZone;
				SetSessionCommentStartScheduledTask(navSession);
				NavRecordRef navRecordRef = null;
				try
				{
					navSession.AccessLock.BeginExclusiveExecution();
					navSession.Open(useUserPersonalization: false);
					string text = "SessionOpened";
					ClientSettings regionalSettings = navSession.RegionalSettings;
					regionalSettings.Refresh(CultureInfo.GetCultureInfo(userFormatId), CultureInfo.GetCultureInfo(userLanguageId));
					navSession.RegionalSettings = regionalSettings;
					navSession.BeginTransaction();
					try
					{
						navSession.OpenCompany(company);
						text = "CompanyOpened";
					}
					catch (NavSessionTerminatedException ex)
					{
						navSession.Diagnostics.SendExceptionTag("00000M4", Category.TaskScheduling, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("OpenCompany Session Terminated, Task Scheduling State " + text));
						HandleSessionTermination(ex, navSession);
						return;
					}
					catch (Exception exception)
					{
						LogExceptionMessage(navSession, exception, text, isSuppressedException: false);
						navSession.EndTransaction(commit: false);
						SetRetryOrDoneIfRetryLimitExceeded(throwOnError: true);
						if (ScheduleInfo.IsDone)
						{
							SetFailedAndRemove(navSession, setSessionState: true);
						}
						return;
					}
					try
					{
						if (recordId != null && !recordId.IsZeroOrEmpty)
						{
							navRecordRef = recordId.ALGetRecord(navSession);
							navRecordRef.Record.ALGet(recordId);
							text = "RecordFetched";
						}
						SetRunning();
						RunCodeunit(runCodeunit, navRecordRef);
						text = "CodeunitExecuted";
						RemoveTaskScheduleFromTenant(navSession);
						text = "TaskScheduleRemoved";
						navSession.EndTransaction(commit: true);
					}
					catch (NavSessionTerminatedException ex2)
					{
						navSession.Diagnostics.SendExceptionTag("00000M5", Category.TaskScheduling, ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Session Terminated, Task Scheduling State " + text));
						HandleSessionTermination(ex2, navSession);
						return;
					}
					catch (Exception exception2)
					{
						LogExceptionMessage(navSession, exception2, text, failureCodeunit != 0);
						text = "TaskScheduleFailed";
						navSession.EndTransaction(commit: false);
						if (failureCodeunit != 0)
						{
							navSession.BeginTransaction();
							try
							{
								RunCodeunit(failureCodeunit, navRecordRef);
								text = "FailureCodeunitExecuted";
								RemoveTaskScheduleFromTenant(navSession);
								text = "FailureTaskScheduleRemoved";
								navSession.EndTransaction(commit: true);
							}
							catch (NavSessionTerminatedException ex3)
							{
								navSession.Diagnostics.SendExceptionTag("00000M6", Category.TaskScheduling, ex3, Verbosity.Error, DiagnosticParameter.SystemMetadata("FailureCodeunit Session Terminated, Task Scheduling State " + text));
								HandleSessionTermination(ex3, navSession);
								return;
							}
							catch (Exception exception3)
							{
								LogExceptionMessage(navSession, exception3, text, isSuppressedException: false);
								navSession.EndTransaction(commit: false);
								SetFailedAndRemove(navSession, setSessionState: true);
							}
						}
						else
						{
							navSession.BeginTransaction();
							RemoveTaskScheduleFromTenant(navSession);
							navSession.EndTransaction(commit: true);
						}
					}
					SetDone(throwOnError: true);
				}
				catch (Exception)
				{
					SetSessionCommentTaskFailed(navSession);
					throw;
				}
				finally
				{
					navRecordRef?.Dispose();
					NavCurrentThread.Session = null;
				}
			}
		}
	}

	protected override void RemoveTaskScheduleFromTenant(NavSession session)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000175 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000175, SecurityFiltering.Ignored);
			if (navRecord.ALGet(DataError.TrapError, new NavGuid(ScheduleInfo.Id)))
			{
				navRecord.ALDelete(DataError.ThrowError);
			}
		}
	}

	protected override bool ShouldDeleteTaskOnException()
	{
		return true;
	}

	private void RunCodeunit(int codeunitId, NavRecordRef recordRef)
	{
		using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("000004K", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "RunScheduledTask Codeunit={0}", codeunitId)), MonitorType.None, Category.TaskScheduling))
		{
			if (CustomRunCodeunit != null)
			{
				CustomRunCodeunit(codeunitId, recordRef);
			}
			else if (recordRef == null)
			{
				NavCodeunit.RunCodeunit(codeunitId);
			}
			else
			{
				NavCodeunit.RunCodeunit(codeunitId, recordRef.Record);
			}
		}
	}
}
