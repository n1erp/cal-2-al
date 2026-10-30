using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;

internal class DeleteCompanySystemTask : SystemTask<DeleteCompanySystemTask.DeleteCompanySystemTaskParameters>
{
	internal class DeleteCompanySystemTaskParameters : SystemTaskParameters
	{
		public string CompanyName { get; set; }

		internal override string GetNonPersonalDataParametersForTelemetry()
		{
			return GetSerializedAsJson();
		}
	}

	public override void Run(NavSession systemSession)
	{
		string companyName = Parameters.CompanyName;
		systemSession.Diagnostics.SendTraceTag("00001IP", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} starting system task to delete {1}.", "DeleteCompanySystemTask", companyName)));
		NavSqlTransientErrorHelper.RetryOperationWithTransientError(() =>
		{
			DeleteCompanyData(systemSession.Tenant, companyName, commitIndividualDeleteOperations: true);
		}, 30, 10, "Run");
		systemSession.Diagnostics.SendTraceTag("00001IQ", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} end system task to delete {1}.", "DeleteCompanySystemTask", companyName)));
	}

	public static void DeleteCompanyData(NavTenant tenant, string companyName, bool commitIndividualDeleteOperations)
	{
		NavDatabase database = tenant.Database;
		int timeout = Convert.ToInt32(ServerUserSettings.Instance.IncrementalCompanyDeleteLockTimeout.Value.TotalMilliseconds);
		int valueOrDefault = database.TenantProperties.TenantLocalId.GetValueOrDefault();
		CompanyId? companyIdWhenCompanyIsReadyToDelete = GetCompanyIdWhenCompanyIsReadyToDelete(tenant, companyName);
		if (!companyIdWhenCompanyIsReadyToDelete.HasValue)
		{
			return;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		string text = valueOrDefault.ToString();
		CompanyId? companyId = companyIdWhenCompanyIsReadyToDelete;
		using (NavSqlDatabaseLock navSqlDatabaseLock = NavSqlDatabaseLock.LockResource(navSqlConnectionScope, "DeleteCompanyData_" + text + "_" + companyId.ToString(), timeout))
		{
			if (!navSqlDatabaseLock.IsLockAcquired())
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.NavNCLCompanyStillDeletingLockTimeout, companyName));
			}
			if (!IsCompanyReadyToDelete(tenant, companyName))
			{
				return;
			}
			tenant.Diagnostics.SendTraceTag("00001IR", Category.Operations, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Starting data deletion of {companyName}."));
			int companyNameToken = database.GetCompanyNameToken(companyName);
			database.DeleteCompanyData(companyNameToken, commitIndividualDeleteOperations);
			tenant.Diagnostics.SendTraceTag("00001IS", Category.Operations, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Done deleting company data for {companyName}."));
			database.TenantCompanies.Delete(companyName);
			tenant.Diagnostics.SendTraceTag("00001IT", Category.Operations, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Done deleting tenant company state for {companyName}."));
		}
		if (commitIndividualDeleteOperations)
		{
			navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Commit);
		}
	}

	private static CompanyId? GetCompanyIdWhenCompanyIsReadyToDelete(NavTenant tenant, string companyName)
	{
		try
		{
			CompanyState state = tenant.Database.TenantCompanies.GetState(companyName);
			if (state != CompanyState.Deleting)
			{
				tenant.Diagnostics.SendTraceTag("00001IU", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} was scheduled to delete {1} but it was in state {2}.", "DeleteCompanySystemTask", companyName, state)));
				return null;
			}
			return tenant.Database.TenantCompanies.GetCompanyId(companyName);
		}
		catch (NavCompanyNotFoundException)
		{
			tenant.Diagnostics.SendTraceTag("00001IV", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant(FormattableStringFactory.Create("{0} was scheduled to delete {1} but it was already deleted from tenant company table.", "DeleteCompanySystemTask", companyName)));
		}
		return null;
	}

	private static bool IsCompanyReadyToDelete(NavTenant tenant, string companyName)
	{
		return GetCompanyIdWhenCompanyIsReadyToDelete(tenant, companyName).HasValue;
	}

	public override bool ShouldRetry()
	{
		return true;
	}
}
