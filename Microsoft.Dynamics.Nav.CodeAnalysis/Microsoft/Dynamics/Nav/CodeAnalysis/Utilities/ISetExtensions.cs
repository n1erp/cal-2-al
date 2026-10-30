using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ISetExtensions
{
	public static bool AddAll<T>(this ISet<T> set, IEnumerable<T> values)
	{
		bool flag = false;
		foreach (T value in values)
		{
			flag |= set.Add(value);
		}
		return flag;
	}

	public static bool RemoveAll<T>(this ISet<T> set, IEnumerable<T> values)
	{
		bool flag = false;
		foreach (T value in values)
		{
			flag |= set.Remove(value);
		}
		return flag;
	}
}
