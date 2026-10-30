namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTenantDatabaseCollection
{
	private readonly object syncLock = new object();

	private TenantDatabaseConfigurationManager tenantDatabaseConfigurationManager;

	public TenantDatabaseConfigurationManager Configuration
	{
		get
		{
			if (tenantDatabaseConfigurationManager == null)
			{
				lock (syncLock)
				{
					if (tenantDatabaseConfigurationManager == null)
					{
						tenantDatabaseConfigurationManager = new TenantDatabaseConfigurationManager(NavEnvironment.Instance.Tenants.SystemTenant.Database);
					}
				}
			}
			return tenantDatabaseConfigurationManager;
		}
	}
}
