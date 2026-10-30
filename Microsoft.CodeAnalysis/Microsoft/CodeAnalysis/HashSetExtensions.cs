using System.Collections.Generic;

namespace Microsoft.CodeAnalysis;

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

	internal static bool InitializeAndAdd<T>(ref HashSet<T> hashSet, T item) where T : class
	{
		if (item == null)
		{
			return false;
		}
		if (hashSet == null)
		{
			hashSet = new HashSet<T>();
		}
		return hashSet.Add(item);
	}
}
