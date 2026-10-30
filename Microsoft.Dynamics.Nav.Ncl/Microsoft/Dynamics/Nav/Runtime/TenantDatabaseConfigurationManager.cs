using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class TenantDatabaseConfigurationManager
{
	private readonly NavDatabase applicationDatabase;

	private readonly TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage;

	public IList<TenantDatabaseConfiguration> AllTenantDatabaseConfigurations
	{
		get
		{
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(applicationDatabase);
			return tenantDatabaseConfigurationStorage.SelectTenantDatabases(conScope);
		}
	}

	public TenantDatabaseConfigurationManager(NavDatabase applicationDatabase)
	{
		if (applicationDatabase == null)
		{
			throw new ArgumentNullException("applicationDatabase");
		}
		this.applicationDatabase = applicationDatabase;
		tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(this.applicationDatabase);
	}

	public TenantDatabaseConfiguration AddTenantDatabaseConfiguration(TenantDatabaseConfiguration tenantDatabaseConfiguration)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(applicationDatabase, updateOrlockingActivity: true);
		return tenantDatabaseConfigurationStorage.InsertTenantDatabase(conScope, tenantDatabaseConfiguration);
	}

	public int TenantDatabaseCount()
	{
		return tenantDatabaseConfigurationStorage.TenantDatabaseCount;
	}

	public TenantDatabaseConfiguration GetTenantDatabaseConfiguration(string tenantDatabaseId)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(applicationDatabase);
		return tenantDatabaseConfigurationStorage.GetTenantDatabase(conScope, tenantDatabaseId);
	}

	public bool RemoveTenantDatabaseConfiguration(string tenantDatabaseId)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(applicationDatabase, updateOrlockingActivity: true);
		return tenantDatabaseConfigurationStorage.DeleteTenantDatabase(conScope, tenantDatabaseId);
	}

	public void FillDatabaseInformation(NavTenantSettings tenantSettings)
	{
		tenantDatabaseConfigurationStorage.FillDatabaseInformation(tenantSettings);
	}
}
