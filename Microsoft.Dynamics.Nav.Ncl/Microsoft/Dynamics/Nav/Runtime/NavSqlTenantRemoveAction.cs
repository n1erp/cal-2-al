using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlTenantRemoveAction
{
	private readonly NavDatabase tenantDatabase;

	public NavSqlTenantRemoveAction(NavDatabase database)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		if (database.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("Given database is not a tenant database");
		}
		tenantDatabase = database;
	}

	public string MarkTenantAsSoftDeleted()
	{
		if (tenantDatabase.TenantProperties.TenantType != NavTenantType.Normal)
		{
			throw new NavTenantRemoveException(string.Format(CultureInfo.InvariantCulture, "Cannot remove tenant {0} because it is not a Normal type tenant.", tenantDatabase.Tenant.Id));
		}
		return NavSqlTransientErrorHelper.RetryOperationWithTransientError(() =>
		{
			using (NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update))
			{
				return tenantDatabase.TenantProperties.MarkAsSoftRemoved();
			}
		}, 30, 10, "MarkTenantAsSoftDeleted");
	}
}
