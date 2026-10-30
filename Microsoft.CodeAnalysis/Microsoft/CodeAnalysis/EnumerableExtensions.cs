using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class EnumerableExtensions
{
	public static ImmutableDictionary<K, V> ToImmutableDictionaryOrEmpty<K, V>(this IEnumerable<KeyValuePair<K, V>> items)
	{
		if (items == null)
		{
			return ImmutableDictionary.Create<K, V>();
		}
		return ImmutableDictionary.CreateRange(items);
	}

	public static ImmutableDictionary<K, V> ToImmutableDictionaryOrEmpty<K, V>(this IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<K> keyComparer)
	{
		if (items == null)
		{
			return ImmutableDictionary.Create<K, V>(keyComparer);
		}
		return ImmutableDictionary.CreateRange(keyComparer, items);
	}

	internal static IList<IList<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> data)
	{
		return data.TransposeInternal().ToArray();
	}

	private static IEnumerable<IList<T>> TransposeInternal<T>(this IEnumerable<IEnumerable<T>> data)
	{
		List<IEnumerator<T>> enumerators = new List<IEnumerator<T>>();
		int width = 0;
		foreach (IEnumerable<T> datum in data)
		{
			enumerators.Add(datum.GetEnumerator());
			width++;
		}
		try
		{
			while (true)
			{
				T[] array = null;
				for (int i = 0; i < width; i++)
				{
					IEnumerator<T> enumerator2 = enumerators[i];
					if (enumerator2.MoveNext())
					{
						if (array == null)
						{
							array = new T[width];
						}
						array[i] = enumerator2.Current;
						continue;
					}
					yield break;
				}
				yield return array;
			}
		}
		finally
		{
			foreach (IEnumerator<T> item in enumerators)
			{
				item.Dispose();
			}
		}
	}

	internal static void AddAllValues<K, T>(this IDictionary<K, ImmutableArray<T>> data, ArrayBuilder<T> builder)
	{
		foreach (ImmutableArray<T> value in data.Values)
		{
			builder.AddRange(value);
		}
	}

	internal static Dictionary<K, ImmutableArray<T>> ToDictionary<K, T>(this IEnumerable<T> data, Func<T, K> keySelector, IEqualityComparer<K> comparer = null)
	{
		Dictionary<K, ImmutableArray<T>> dictionary = new Dictionary<K, ImmutableArray<T>>(comparer);
		foreach (IGrouping<K, T> item in data.GroupBy(keySelector, comparer))
		{
			ImmutableArray<T> value = item.AsImmutable();
			dictionary.Add(item.Key, value);
		}
		return dictionary;
	}

	internal static TSource AsSingleton<TSource>(this IEnumerable<TSource> source)
	{
		if (source == null)
		{
			return default;
		}
		if (source is IList<TSource> list)
		{
			if (list.Count != 1)
			{
				return default;
			}
			return list[0];
		}
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return default;
		}
		TSource current = enumerator.Current;
		if (enumerator.MoveNext())
		{
			return default;
		}
		return current;
	}
}
