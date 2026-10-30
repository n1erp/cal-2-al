using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class EnumerableExtensions
{
	private static class Functions<T>
	{
		public static readonly Func<T, T> Identity = (T t) => t;
	}

	private static class Comparisons<T> where T : IComparable<T>
	{
		private static readonly Comparison<T> CompareTo = (T t1, T t2) => t1.CompareTo(t2);

		public static readonly IComparer<T> Comparer = new ComparisonComparer<T>(CompareTo);
	}

	private class ComparisonComparer<T> : Comparer<T>
	{
		private readonly Comparison<T> compare;

		public ComparisonComparer(Comparison<T> compare)
		{
			this.compare = compare;
		}

		public override int Compare(T x, T y)
		{
			return compare(x, y);
		}
	}

	private static readonly Func<object, bool> NotNullTest = (object x) => x != null;

	public static ImmutableDictionary<K, V> ToImmutableDictionaryOrEmpty<K, V>(this IEnumerable<KeyValuePair<K, V>> items)
	{
		if (items == null)
		{
			return ImmutableDictionary.Create<K, V>();
		}
		return ImmutableDictionary.CreateRange(items);
	}

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
		return !source.Any();
	}

	public static bool HasMoreThan<T>(this IEnumerable<T> source, int noOfElements)
	{
		return source.Skip(noOfElements).Any();
	}

	public static T FirstIfSingle<T>(this IEnumerable<T> source)
	{
		if (source.HasMoreThan(1))
		{
			return default;
		}
		return source.FirstOrDefault();
	}

	public static bool HasMoreOrEqualThan<T>(this IEnumerable<T> source, int noOfElements)
	{
		if (noOfElements < 0)
		{
			throw new ArgumentOutOfRangeException("noOfElements");
		}
		if (noOfElements == 0)
		{
			return source.Any();
		}
		return source.Skip(noOfElements - 1).Any();
	}

	public static bool HasLessThan<T>(this IEnumerable<T> source, int noOfElements)
	{
		return !source.HasMoreOrEqualThan(noOfElements);
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

	public static bool IsEmpty<T, U>(this Dictionary<T, U> source)
	{
		return !source.Any();
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
	{
		if (source == null)
		{
			return SpecializedCollections.EmptyEnumerable<T>();
		}
		return source.Where((Func<T, bool>)NotNullTest);
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
		return source.OrderBy(new ComparisonComparer<T>(compare));
	}

	public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) where T : IComparable<T>
	{
		return source.OrderBy(Comparisons<T>.Comparer);
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

	public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
	{
		DebugAssertHelper.Assert(comparer != null);
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

	public static bool Contains<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
	{
		return sequence.Any(predicate);
	}

	public static bool Contains(this IEnumerable<string> sequence, string s)
	{
		return sequence.Any((string item) => item == s);
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IComparer<T> comparer)
	{
		return source.ThenBy(Functions<T>.Identity, comparer);
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Comparison<T> compare)
	{
		return source.ThenBy(new ComparisonComparer<T>(compare));
	}

	public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source) where T : IComparable<T>
	{
		return source.ThenBy(Comparisons<T>.Comparer);
	}

	public static IDictionary<TKey, TValue> ToDictionaryDistinct<TObject, TKey, TValue>(this IEnumerable<TObject> source, Func<TObject, TKey> keySelector, Func<TObject, TValue> valueSelector, IEqualityComparer<TKey> keyComparer = null)
	{
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(keyComparer);
		foreach (TObject item in source)
		{
			TValue value = valueSelector(item);
			TKey val = keySelector(item);
			if (val != null && !dictionary.ContainsKey(val))
			{
				dictionary[val] = value;
			}
		}
		return dictionary;
	}

	public static IDictionary<TKey, TValue> ToDictionaryDistinct<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
	{
		return source.ToDictionaryDistinct(keySelector, (TValue v) => v, keyComparer);
	}

	public static Dictionary<TKey, ImmutableArray<TValue>> ToDictionaryOfArrays<TValue, TKey>(this IEnumerable<TValue> source, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
	{
		ArrayBuilder<TValue> instance = ArrayBuilder<TValue>.GetInstance();
		instance.AddRange(source);
		Dictionary<TKey, ImmutableArray<TValue>> result = instance.ToDictionary(keySelector, keyComparer);
		instance.Free();
		return result;
	}
}
