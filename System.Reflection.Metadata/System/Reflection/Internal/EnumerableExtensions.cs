using System.Collections.Generic;
using System.Collections.Immutable;

namespace System.Reflection.Internal;

internal static class EnumerableExtensions
{
	public static T FirstOrDefault<T>(this ImmutableArray<T> collection, Func<T, bool> predicate)
	{
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				return item;
			}
		}
		return default;
	}

	public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		foreach (TSource item in source)
		{
			yield return selector(item);
		}
	}

	public static T Last<T>(this ImmutableArray<T>.Builder source)
	{
		return source[source.Count - 1];
	}

	public static IEnumerable<T> OrderBy<T>(this List<T> source, Comparison<T> comparison)
	{
		int[] array = new int[source.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = i;
		}
		Array.Sort(array, (int left, int right) =>
		{
			if (left == right)
			{
				return 0;
			}
			int num2 = comparison(source[left], source[right]);
			return (num2 == 0) ? (left - right) : num2;
		});
		int[] array2 = array;
		foreach (int index in array2)
		{
			yield return source[index];
		}
	}
}
