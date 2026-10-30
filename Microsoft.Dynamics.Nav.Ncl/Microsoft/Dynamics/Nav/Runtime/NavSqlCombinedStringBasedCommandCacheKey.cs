using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlCombinedStringBasedCommandCacheKey : NavSqlCommandCacheKey
{
	private readonly string[] keys;

	public NavSqlCombinedStringBasedCommandCacheKey(NavDatabase database, params string[] keys)
		: base(CachedCommandType.StringBased, database.DatabaseName, GetHashCode(keys), database.IsSharedTenantSchema)
	{
		this.keys = keys;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NavSqlCombinedStringBasedCommandCacheKey navSqlCombinedStringBasedCommandCacheKey))
		{
			return false;
		}
		if (base.Equals(obj))
		{
			return AreEqual(keys, navSqlCombinedStringBasedCommandCacheKey.keys);
		}
		return false;
	}

	private static int GetHashCode(string[] values)
	{
		if (values.Length == 0)
		{
			return 0;
		}
		int num = values[0].GetHashCode();
		for (int i = 1; i < values.Length; i++)
		{
			num = HashCodeHelper.CombineHashCodes(num, values[i].GetHashCode());
		}
		return num;
	}

	private static bool AreEqual(string[] array1, string[] array2)
	{
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!string.Equals(array1[i], array2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
