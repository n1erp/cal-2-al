using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlCollationSensitiveCommandCacheKey : NavSqlCommandCacheKey
{
	private readonly SqlSortingProperties sqlSortingProperties;

	public SqlSortingProperties SqlSortingProperties => sqlSortingProperties;

	public NavSqlCollationSensitiveCommandCacheKey(CachedCommandType commandType, NavDatabase database, int hashCode)
		: base(commandType, database.DatabaseName, HashCodeHelper.CombineHashCodes(database.SqlSortingProperties.Collation.GetHashCode(), hashCode), database.IsSharedTenantSchema)
	{
		sqlSortingProperties = database.SqlSortingProperties;
	}

	public override bool Equals(object obj)
	{
		if (!base.Equals(obj))
		{
			return false;
		}
		NavSqlCollationSensitiveCommandCacheKey navSqlCollationSensitiveCommandCacheKey = (NavSqlCollationSensitiveCommandCacheKey)obj;
		if (navSqlCollationSensitiveCommandCacheKey != null)
		{
			return sqlSortingProperties == navSqlCollationSensitiveCommandCacheKey.sqlSortingProperties;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
