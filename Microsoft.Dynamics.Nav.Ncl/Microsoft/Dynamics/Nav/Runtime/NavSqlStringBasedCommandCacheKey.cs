namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlStringBasedCommandCacheKey : NavSqlCommandCacheKey
{
	private readonly string key;

	public NavSqlStringBasedCommandCacheKey(string key, NavDatabase database)
		: base(CachedCommandType.StringBased, database.DatabaseName, key.GetHashCode(), database.IsSharedTenantSchema)
	{
		this.key = key;
	}

	public override bool Equals(object obj)
	{
		NavSqlStringBasedCommandCacheKey navSqlStringBasedCommandCacheKey = obj as NavSqlStringBasedCommandCacheKey;
		if (obj == null)
		{
			return false;
		}
		if (base.Equals(obj))
		{
			return string.Equals(key, navSqlStringBasedCommandCacheKey.key);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
