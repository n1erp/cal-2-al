using System;
using System.Globalization;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlCommandCacheKey
{
	private readonly CachedCommandType commandType;

	private readonly string databaseName;

	private readonly int hashCode;

	private readonly bool isSharedTenantSchema;

	public string DatabaseName => databaseName;

	public NavSqlCommandCacheKey(CachedCommandType commandType, string databaseName, int hashCode, bool isSharedTenantSchema)
	{
		this.commandType = commandType;
		this.databaseName = databaseName;
		this.hashCode = HashCodeHelper.CombineHashCodes((int)this.commandType, this.databaseName.GetHashCode(), hashCode, isSharedTenantSchema ? 1 : 0);
		this.isSharedTenantSchema = isSharedTenantSchema;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		if (GetHashCode() != obj.GetHashCode())
		{
			return false;
		}
		NavSqlCommandCacheKey navSqlCommandCacheKey = (NavSqlCommandCacheKey)obj;
		if (databaseName.Equals(navSqlCommandCacheKey.databaseName, StringComparison.OrdinalIgnoreCase) && commandType == navSqlCommandCacheKey.commandType)
		{
			return isSharedTenantSchema == navSqlCommandCacheKey.isSharedTenantSchema;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hashCode;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "HashCode {0} CommandType {1}", hashCode, commandType);
	}

	public static int GetCompanyIndexForCacheKey(NavDatabase database, int companyIndex)
	{
		if (database.IsSharedTenantSchema && !ServerUserSettings.Instance.DisableSharedSqlCommandCacheAcrossTenant.Value)
		{
			return int.MinValue;
		}
		return companyIndex;
	}
}
