namespace Microsoft.Dynamics.Nav.Runtime.Authentication;

internal class UserTableChangeMonitor : TableChangeMonitor
{
	private static readonly int[] tableIds = new int[2] { 2000000120, 2000000121 };

	public UserTableChangeMonitor(NavDatabase navDatabase)
		: base(navDatabase, tableIds, watchMetadataChanges: false)
	{
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		NavTenant tenant = Database.Tenant;
		if (tenant == null || !(tenant.Id != tenantId))
		{
			Database.NavUsers.ExpireCache();
		}
	}
}
