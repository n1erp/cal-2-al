using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal static class EnumerableExtensions
{
	private static class Comparisons<T> where T : IComparable<T>
	{
		public static readonly Comparison<T> CompareTo = (T t1, T t2) => t1.CompareTo(t2);

		public static readonly IComparer<T> Comparer = Comparer<T>.Create(CompareTo);
	}

	private static readonly Func<object, bool> s_notNullTest = (object x) => x != null;

	public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (source is IList<T> list)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				action(list[i]);
			}
		}
		else
		{
			foreach (T item in source)
			{
				action(item);
			}
		}
		return source;
	}

	public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new ReadOnlyCollection<T>(source.ToList());
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, T value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.ConcatWorker(value);
	}

	private static IEnumerable<T> ConcatWorker<T>(this IEnumerable<T> source, T value)
	{
		foreach (T item in source)
		{
			yield return item;
		}
		yield return value;
	}

	public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		return source1.ToSet(comparer).SetEquals(source2);
	}

	public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		return source1.ToSet().SetEquals(source2);
	}

	public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new HashSet<T>(source, comparer);
	}

	public static ISet<T> ToSet<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return (source as ISet<T>) ?? new HashSet<T>(source);
	}

	public static T? FirstOrNullable<T>(this IEnumerable<T> source) where T : struct
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Cast<T?>().FirstOrDefault();
	}

	public static T? FirstOrNullable<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Cast<T?>().FirstOrDefault((T? v) => predicate(v.Value));
	}

	public static T? LastOrNullable<T>(this IEnumerable<T> source) where T : struct
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Cast<T?>().LastOrDefault();
	}

	public static bool IsSingle<T>(this IEnumerable<T> list)
	{
		using IEnumerator<T> enumerator = list.GetEnumerator();
		return enumerator.MoveNext() && !enumerator.MoveNext();
	}

	public static bool IsEmpty<T>(this IEnumerable<T> source)
	{
		if (source is IReadOnlyCollection<T> readOnlyCollection)
		{
			return readOnlyCollection.Count == 0;
		}
		if (source is ICollection<T> collection)
		{
			return collection.Count == 0;
		}
		if (source is ICollection collection2)
		{
			return collection2.Count == 0;
		}
		if (source is string text)
		{
			return text.Length == 0;
		}
		using (IEnumerator<T> enumerator = source.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return false;
			}
		}
		return true;
	}

	public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
	{
		return source.Count == 0;
	}

	public static bool IsEmpty<T>(this ICollection<T> source)
	{
		return source.Count == 0;
	}

	public static bool IsEmpty(this string source)
	{
		return source.Length == 0;
	}

	public static bool IsEmpty<T>(this T[] source)
	{
		return source.Length == 0;
	}

	public static bool IsEmpty<T>(this List<T> source)
	{
		return source.Count == 0;
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
	{
		if (source == null)
		{
			return SpecializedCollections.EmptyEnumerable<T>();
		}
		return source.Where((Func<T, bool>)s_notNullTest);
	}

	public static ImmutableArray<TResult> SelectAsArray<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
	{
		if (source == null)
		{
			return ImmutableArray<TResult>.Empty;
		}
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		instance.AddRange(source.Select(selector));
		return instance.ToImmutableAndFree();
	}

	public static bool All(this IEnumerable<bool> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		foreach (bool item in source)
		{
			if (!item)
			{
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> sequence)
	{
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		return sequence.SelectMany((IEnumerable<T> s) => s);
	}

	public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T> comparer)
	{
		return source.OrderBy(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare)
	{
		return source.OrderBy(Comparer<T>.Create(compare));
	}

	public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) where T : IComparable<T>
	{
		return source.OrderBy(Comparisons<T>.Comparer);
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IComparer<T> comparer)
	{
		return source.ThenBy(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Comparison<T> compare)
	{
		return source.ThenBy(Comparer<T>.Create(compare));
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source) where T : IComparable<T>
	{
		return source.ThenBy(Comparisons<T>.Comparer);
	}

	public static bool IsSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
	{
		using IEnumerator<T> enumerator = enumerable.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return true;
		}
		T current = enumerator.Current;
		while (enumerator.MoveNext())
		{
			if (comparer.Compare(current, enumerator.Current) > 0)
			{
				return false;
			}
			current = enumerator.Current;
		}
		return true;
	}

	public static bool Contains<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
	{
		return sequence.Any(predicate);
	}

	public static bool Contains(this IEnumerable<string> sequence, string s)
	{
		foreach (string item in sequence)
		{
			if (item == s)
			{
				return true;
			}
		}
		return false;
	}

	public static IComparer<T> ToComparer<T>(this Comparison<T> comparison)
	{
		return Comparer<T>.Create(comparison);
	}
}
