namespace Microsoft.Dynamics.Nav.Runtime;

internal class CompanyTableChangeMonitor : TableChangeMonitor
{
	private static readonly int[] TableIds = new int[1] { 2000000006 };

	public CompanyTableChangeMonitor(NavDatabase navDatabase)
		: base(navDatabase, TableIds, watchMetadataChanges: false)
	{
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		NavTenant tenant = Database.Tenant;
		if (tenant == null || !(tenant.Id != tenantId))
		{
			Database.ResetCompanyIdCache();
		}
	}
}
