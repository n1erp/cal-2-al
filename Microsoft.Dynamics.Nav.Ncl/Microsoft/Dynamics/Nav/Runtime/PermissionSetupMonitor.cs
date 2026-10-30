using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PermissionSetupMonitor : TableChangeMonitor
{
	private static readonly int[] tableIds = new int[7] { 2000000053, 2000000005, 2000000004, 2000000120, 2000000006, 2000000166, 2000000165 };

	public PermissionSetupMonitor(NavDatabase database)
		: base(database, tableIds, watchMetadataChanges: true)
	{
	}

	internal void ResetPermissionSetup(bool clearLicenseInfo)
	{
		IncrementSetupVersion();
		Database.SecurityAndLicense.ClearPermissions(clearLicenseInfo);
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		NavTenant tenant = Database.Tenant;
		if ((tenant == null || !(tenantId == tenant.Id)) && !changedTables.Any(SystemTables.ApplicationDatabaseTables.Contains))
		{
			return;
		}
		if (changedTables.Any((int tableId) => tableId == 2000000004 || tableId == 2000000005 || tableId == 2000000165 || tableId == 2000000166))
		{
			foreach (NavTenant item in from tempTenant in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true)
				where tempTenant.Id != tenantId
				select tempTenant)
			{
				item.Database.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: false);
			}
		}
		ResetPermissionSetup(clearLicenseInfo: false);
	}
}
