using System;
using System.Activities;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Workflow;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeTransactionActivity : NativeActivity
{
	public bool ForceCreateNew { get; }

	public NavDataUpgradeTransactionActivity(string displayName = null, bool forceCreateNew = false)
	{
		if (displayName != null)
		{
			DisplayName = displayName;
		}
		ForceCreateNew = forceCreateNew;
	}

	protected override void Execute(NativeActivityContext context)
	{
		INavWorkflowTenantSessionManager sessionManager = context.GetExtension<INavWorkflowTenantSessionManager>();
		CheckNotNull(sessionManager, "sessionManager");
		INavDataUpgradeScope dataUpgradeTransaction = context.GetExtension<INavDataUpgradeScope>();
		CheckNotNull(dataUpgradeTransaction, "dataUpgradeTransaction");
		sessionManager.RunActionInSystemSession((NavSession navSession) =>
		{
			if (!ForceCreateNew && dataUpgradeTransaction.IsTransactionStarted)
			{
				navSession.Diagnostics.SendTraceTag("00000UP", Category.DataUpgrade, Verbosity.Normal, "Commit data upgrade root transaction, clientSessionId = {0}, clientActivityId = {1}, tenant = {2}.", DiagnosticParameter.SystemMetadata(sessionManager.ClientSessionId), DiagnosticParameter.SystemMetadata(sessionManager.ClientActivityId), DiagnosticParameter.Oii(navSession.Tenant.Id));
				dataUpgradeTransaction.CommitTransaction();
			}
			else
			{
				navSession.Diagnostics.SendTraceTag("00000UQ", Category.DataUpgrade, Verbosity.Normal, "Start data upgrade root transaction, clientSessionId = {0}, clientActivityId = {1}, tenant = {2}.", DiagnosticParameter.SystemMetadata(sessionManager.ClientSessionId), DiagnosticParameter.SystemMetadata(sessionManager.ClientActivityId), DiagnosticParameter.Oii(navSession.Tenant.Id));
				dataUpgradeTransaction.StartTransaction(navSession, ForceCreateNew);
			}
		});
	}

	private static void CheckNotNull(object value, string name)
	{
		if (value == null)
		{
			throw new ArgumentNullException(name);
		}
	}
}
