using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class WebServiceSetupMonitor : TableChangeMonitor
{
	private static readonly int[] TableIds = new int[3] { 2000000076, 2000000168, 2000000179 };

	public WebServiceSetupMonitor(NavDatabase database)
		: base(database, TableIds, watchMetadataChanges: true)
	{
	}

	internal void ResetWebServiceSetup()
	{
		IncrementSetupVersion();
	}

	protected override void ResetSetup(string tenantId, int[] changedTables)
	{
		NavTenant tenant = Database.Tenant;
		bool flag = tenant != null && tenantId == tenant.Id;
		bool flag2 = changedTables.Any(SystemTables.ApplicationDatabaseTables.Contains);
		if (!(flag | flag2))
		{
			return;
		}
		if (flag2)
		{
			foreach (NavTenant item in from tempTenant in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true)
				where tempTenant.Id != tenantId
				select tempTenant)
			{
				item.Database.WebServiceSetupMonitor.ResetWebServiceSetup();
			}
		}
		ResetWebServiceSetup();
	}
}
