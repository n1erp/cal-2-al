namespace Microsoft.Dynamics.Nav.Runtime;

internal class EntitlementSetupMonitor : TableChangeMonitor
{
	private static readonly int[] tableIds = new int[3] { 2000000191, 2000000190, 2000000195 };

	public EntitlementSetupMonitor(NavDatabase database)
		: base(database, tableIds, watchMetadataChanges: true)
	{
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		IncrementSetupVersion();
		EntitlementPermissionSet.ClearCache();
		NavGlobal.SystemTenant.EntitlementProvider.PreloadMembershipEntitlements();
	}
}
