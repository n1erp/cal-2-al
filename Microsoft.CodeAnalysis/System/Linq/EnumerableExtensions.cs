using System.Collections.Generic;

namespace System.Linq;

internal static class EnumerableExtensions
{
	public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
	{
		if (first == second)
		{
			return true;
		}
		if (first == null || second == null)
		{
			return false;
		}
		using (IEnumerator<T> enumerator = first.GetEnumerator())
		{
			using IEnumerator<T> enumerator2 = second.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!enumerator2.MoveNext() || !comparer(enumerator.Current, enumerator2.Current))
				{
					return false;
				}
			}
			if (enumerator2.MoveNext())
			{
				return false;
			}
		}
		return true;
	}
}
