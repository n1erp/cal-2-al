using System;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;

internal class RemoveTenantSystemTask : SystemTask<RemoveTenantSystemTask.RemoveTenantSystemTaskParameters>
{
	internal class RemoveTenantSystemTaskParameters : SystemTaskParameters
	{
		internal override string GetNonPersonalDataParametersForTelemetry()
		{
			return GetSerializedAsJson();
		}
	}

	public override void Run(NavSession systemSession)
	{
		DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001JV", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} starting system task to remove {1}.", "RemoveTenantSystemTask", systemSession.Tenant.Id)));
		NavSqlTransientErrorHelper.RetryOperationWithTransientError(() =>
		{
			RemoveTenant(systemSession);
		}, 30, 10, "Run");
		DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001JW", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} end system task to remove {1}.", "RemoveTenantSystemTask", systemSession.Tenant.Id)));
	}

	public static void RemoveTenant(NavSession session)
	{
		if (EnsureTenantIsReadyToRemove(session))
		{
			session.Tenant.Database.RemoveTenant(async: false);
		}
	}

	private static bool EnsureTenantIsReadyToRemove(NavSession session)
	{
		if (!session.Tenant.Database.IsSharedTenantSchema)
		{
			DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001JY", Category.TaskScheduling, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} was scheduled to remove tenant {1} but it was not running with shared schema.", "RemoveTenantSystemTask", session.Tenant.Id)));
			return false;
		}
		if (session.Tenant.Database.TenantProperties.TenantType != NavTenantType.SoftDeleted)
		{
			DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001JZ", Category.TaskScheduling, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} was scheduled to remove tenant {1} but it was not marked as soft deleted.", "RemoveTenantSystemTask", session.Tenant.Id)));
			return false;
		}
		return true;
	}

	public override bool ShouldRetry()
	{
		return true;
	}
}
