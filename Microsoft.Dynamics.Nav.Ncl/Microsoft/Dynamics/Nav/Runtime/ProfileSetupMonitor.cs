namespace Microsoft.Dynamics.Nav.Runtime;

internal class ProfileSetupMonitor : TableChangeMonitor
{
	private static readonly int[] tableIds = new int[2] { 2000000074, 2000000072 };

	public ProfileSetupMonitor(NavDatabase database)
		: base(database, tableIds, watchMetadataChanges: false)
	{
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		NavTenant tenant = Database.Tenant;
		if (tenant != null && tenantId == tenant.Id)
		{
			NavEnvironment.Instance.NavProfilePageMetadataCacheV1.RemoveAll();
		}
	}
}
