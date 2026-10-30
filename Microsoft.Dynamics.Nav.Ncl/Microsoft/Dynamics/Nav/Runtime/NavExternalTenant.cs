using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavExternalTenant : NavTenant
{
	private readonly ExternalSqlTableConnectionSettings externalSqlTableConnectionSettings;

	private readonly NavDatabaseType databaseType;

	public override NavTenantType TenantType => NavTenantType.External;

	public NavExternalTenant(NavDatabaseServer databaseServer, string databaseName, ExternalSqlTableConnectionSettings externalSqlTableConnectionSettings, NavDatabaseType databaseType = NavDatabaseType.External)
		: base(new NavTenantSettings("nav-systemapplication", databaseName, databaseServer.ServerName, databaseServer.UserName, databaseServer.Password, null, allowAppDatabaseWrite: true), databaseServer)
	{
		this.externalSqlTableConnectionSettings = externalSqlTableConnectionSettings;
		this.databaseType = databaseType;
	}

	public NavExternalTenant(TenantDatabaseConfiguration config, NavDatabaseType databaseType = NavDatabaseType.External)
		: this(NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(config.DatabaseServer, config.DatabaseUserName, config.DatabasePassword), config.DatabaseName, new ExternalSqlTableConnectionSettings(config.DatabaseName, config.DatabaseServer, config.DatabaseName, config.DatabaseUserName, config.DatabasePassword, 0), databaseType)
	{
	}

	protected override NavDatabase CreateDatabase()
	{
		return new NavDatabase(this, DatabaseServer, TenantSettings.DatabaseName, databaseType, externalSqlTableConnectionSettings);
	}
}
