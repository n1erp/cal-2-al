using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDatabaseServer : IDisposable
{
	private static readonly string[] localhostAliases = new string[2] { "LOCALHOST", "." };

	private readonly NavSqlConnectionPool sqlConnectionPool;

	public string ServerName => sqlConnectionPool.ServerName;

	public string NormalizedServerName => sqlConnectionPool.NormalizedServerName;

	public string UserName => sqlConnectionPool.UserName;

	public ProtectedDatabasePassword Password => sqlConnectionPool.Password;

	public NavSqlConnectionPool SqlConnectionPool => sqlConnectionPool;

	public IList<NavTenant> TenantsOnServer => (from t in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true)
		where this == t.Database.DatabaseServer
		select t).ToList().AsReadOnly();

	public IList<NavDatabase> DatabasesOnServer => TenantsOnServer.Select((NavTenant tenant) => tenant.Database).ToList().AsReadOnly();

	internal NavDatabaseServer(string serverName, string databaseUserName, ProtectedDatabasePassword databasePassword)
	{
		sqlConnectionPool = new NavSqlConnectionPool(serverName, databaseUserName, databasePassword);
	}

	internal NavDatabaseServer(NavSqlConnectionPool sqlConnectionPool)
	{
		this.sqlConnectionPool = sqlConnectionPool;
	}

	public static string NormalizeServerName(string serverName)
	{
		serverName = serverName.ToUpperInvariant();
		if (localhostAliases.Any((string localHostAlias) => localHostAlias == serverName))
		{
			serverName = localhostAliases[0];
		}
		return serverName;
	}

	public void Dispose()
	{
		sqlConnectionPool.Dispose();
	}
}
