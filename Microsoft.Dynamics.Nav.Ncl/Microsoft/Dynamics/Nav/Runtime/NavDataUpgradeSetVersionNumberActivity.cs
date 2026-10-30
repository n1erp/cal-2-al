using System;
using System.Activities;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataUpgradeSetVersionNumberActivity : NativeActivity
{
	private string VersionNumber { get; }

	public NavDataUpgradeSetVersionNumberActivity(string versionNumber, string displayName = null)
	{
		if (displayName != null)
		{
			DisplayName = displayName;
		}
		VersionNumber = versionNumber;
	}

	protected override void Execute(NativeActivityContext context)
	{
		INavWorkflowTenantSessionManager extension = context.GetExtension<INavWorkflowTenantSessionManager>();
		if (extension == null)
		{
			CheckNotNull(extension, "sessionManager");
		}
		extension.RunActionInSystemSession((NavSession navSession) =>
		{
			navSession.Diagnostics.SendTraceTag("00000WX", Category.DataUpgrade, Verbosity.Normal, "Upgrading from {0} to {1} for tenant {2}", DiagnosticParameter.SystemMetadata(navSession.Database.TenantProperties.TenantDataVersion), DiagnosticParameter.SystemMetadata(VersionNumber), DiagnosticParameter.Oii(navSession.Tenant.Id));
			navSession.Database.TenantProperties.SetTenantDataVersion(VersionNumber);
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
