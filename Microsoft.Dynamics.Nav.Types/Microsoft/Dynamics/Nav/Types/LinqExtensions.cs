using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

public static class LinqExtensions
{
	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		HashSet<TKey> distinctKeys = new HashSet<TKey>();
		return source.Where((TSource e) => distinctKeys.Add(keySelector(e)));
	}

	public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		foreach (TSource item in source)
		{
			action(item);
		}
	}
}
