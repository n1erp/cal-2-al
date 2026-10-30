using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class HashSetExtensions
{
	internal static bool IsNullOrEmpty<T>(this HashSet<T> hashSet)
	{
		if (hashSet != null)
		{
			return hashSet.Count == 0;
		}
		return true;
	}

	internal static IEnumerable<T> Intersect<T>(this ISet<T> first, ISet<T> second)
	{
		if (second.Count > first.Count)
		{
			ISet<T> set = first;
			first = second;
			second = set;
		}
		foreach (T item in second)
		{
			if (first.Contains(item))
			{
				yield return item;
			}
		}
	}
}
