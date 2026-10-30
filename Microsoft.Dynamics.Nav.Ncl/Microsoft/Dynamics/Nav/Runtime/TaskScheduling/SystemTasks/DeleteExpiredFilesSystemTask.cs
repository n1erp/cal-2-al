using System;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;

internal class DeleteExpiredFilesSystemTask : SystemTask<DeleteExpiredFilesSystemTask.DeleteExpiredFilesSystemTaskParameters>
{
	internal class DeleteExpiredFilesSystemTaskParameters : SystemTaskParameters
	{
		internal override string GetNonPersonalDataParametersForTelemetry()
		{
			return GetSerializedAsJson();
		}
	}

	public override void Run(NavSession systemSession)
	{
		DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001T2", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} starting system task to delete expired files from {1}.", "DeleteExpiredFilesSystemTask", systemSession.Tenant.Id)));
		NavSqlTransientErrorHelper.RetryOperationWithTransientError(() =>
		{
			DeleteExpiredFiles(systemSession);
		}, 30, 10, "Run");
		DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001T3", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} end system task to delete expired files from {1}.", "DeleteExpiredFilesSystemTask", systemSession.Tenant.Id)));
	}

	public static void DeleteExpiredFiles(NavSession session)
	{
		if (IsExpiredFilesReadyToDelete(session))
		{
			NavMedia.CleanupExpiredFiles(session.Tenant);
		}
	}

	private static bool IsExpiredFilesReadyToDelete(NavSession session)
	{
		if (session.Tenant.TenantType != NavTenantType.Normal)
		{
			DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001T5", Category.TaskScheduling, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} The tenant type of {1} is {2} instead of Normal", "DeleteExpiredFilesSystemTask", session.Tenant.Id, session.Tenant.TenantType.ToString())));
			return false;
		}
		return true;
	}

	public override bool ShouldRetry()
	{
		return true;
	}
}
