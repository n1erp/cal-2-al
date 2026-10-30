using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class ImmutableArrayExtensions
{
	public static ImmutableArray<T> AsImmutable<T>(this IEnumerable<T> items)
	{
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutableOrEmpty<T>(this IEnumerable<T> items)
	{
		if (items == null)
		{
			return ImmutableArray<T>.Empty;
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutableOrNull<T>(this IEnumerable<T> items)
	{
		if (items == null)
		{
			return default;
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<T> AsImmutable<T>(this T[] items)
	{
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<T> AsImmutableOrNull<T>(this T[] items)
	{
		if (items == null)
		{
			return default;
		}
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<T> AsImmutableOrEmpty<T>(this T[] items)
	{
		if (items == null)
		{
			return ImmutableArray<T>.Empty;
		}
		return ImmutableArray.Create(items);
	}

	public static ImmutableArray<byte> ToImmutable(this MemoryStream stream)
	{
		return ImmutableArray.Create(stream.ToArray());
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TResult>(this ImmutableArray<TItem> items, Func<TItem, TResult> map)
	{
		return ImmutableArray.CreateRange(items, map);
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> items, Func<TItem, TArg, TResult> map, TArg arg)
	{
		return ImmutableArray.CreateRange(items, map, arg);
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TArg, TResult>(this ImmutableArray<TItem> items, Func<TItem, int, TArg, TResult> map, TArg arg)
	{
		switch (items.Length)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableArray.Create(map(items[0], 0, arg));
		case 2:
			return ImmutableArray.Create(map(items[0], 0, arg), map(items[1], 1, arg));
		case 3:
			return ImmutableArray.Create(map(items[0], 0, arg), map(items[1], 1, arg), map(items[2], 2, arg));
		case 4:
			return ImmutableArray.Create(map(items[0], 0, arg), map(items[1], 1, arg), map(items[2], 2, arg), map(items[3], 3, arg));
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(items.Length);
			for (int i = 0; i < items.Length; i++)
			{
				instance.Add(map(items[i], i, arg));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public static ImmutableArray<TResult> ZipAsArray<T1, T2, TResult>(this ImmutableArray<T1> self, ImmutableArray<T2> other, Func<T1, T2, TResult> map)
	{
		switch (self.Length)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableArray.Create(map(self[0], other[0]));
		case 2:
			return ImmutableArray.Create(map(self[0], other[0]), map(self[1], other[1]));
		case 3:
			return ImmutableArray.Create(map(self[0], other[0]), map(self[1], other[1]), map(self[2], other[2]));
		case 4:
			return ImmutableArray.Create(map(self[0], other[0]), map(self[1], other[1]), map(self[2], other[2]), map(self[3], other[3]));
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(self.Length);
			for (int i = 0; i < self.Length; i++)
			{
				instance.Add(map(self[i], other[i]));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public static ImmutableArray<T> WhereAsArray<T>(this ImmutableArray<T> array, Func<T, bool> predicate)
	{
		ArrayBuilder<T> arrayBuilder = null;
		bool flag = true;
		bool flag2 = true;
		int length = array.Length;
		for (int i = 0; i < length; i++)
		{
			T val = array[i];
			if (predicate(val))
			{
				flag = false;
				if (!flag2)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<T>.GetInstance();
					}
					arrayBuilder.Add(val);
				}
			}
			else if (flag)
			{
				flag2 = false;
			}
			else if (flag2)
			{
				flag2 = false;
				arrayBuilder = ArrayBuilder<T>.GetInstance();
				for (int j = 0; j < i; j++)
				{
					arrayBuilder.Add(array[j]);
				}
			}
		}
		if (arrayBuilder != null)
		{
			return arrayBuilder.ToImmutableAndFree();
		}
		if (flag2)
		{
			return array;
		}
		return ImmutableArray<T>.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ImmutableArray<TBase> Cast<TDerived, TBase>(this ImmutableArray<TDerived> items) where TDerived : class, TBase
	{
		return ImmutableArray<TBase>.CastUp(items);
	}

	public static bool SetEquals<T>(this ImmutableArray<T> array1, ImmutableArray<T> array2, IEqualityComparer<T> comparer)
	{
		if (array1.IsDefault)
		{
			return array2.IsDefault;
		}
		if (array2.IsDefault)
		{
			return false;
		}
		int length = array1.Length;
		int length2 = array2.Length;
		if (length == 0)
		{
			return length2 == 0;
		}
		if (length2 == 0)
		{
			return false;
		}
		if (length == 1 && length2 == 1)
		{
			T x = array1[0];
			T y = array2[0];
			return comparer.Equals(x, y);
		}
		HashSet<T> hashSet = new HashSet<T>(array1, comparer);
		HashSet<T> hashSet2 = new HashSet<T>(array2, comparer);
		return hashSet.SetEquals(hashSet2);
	}

	public static ImmutableArray<T> NullToEmpty<T>(this ImmutableArray<T> array)
	{
		if (!array.IsDefault)
		{
			return array;
		}
		return ImmutableArray<T>.Empty;
	}

	public static ImmutableArray<T> Distinct<T>(this ImmutableArray<T> array, IEqualityComparer<T> comparer = null)
	{
		if (array.Length < 2)
		{
			return array;
		}
		HashSet<T> hashSet = new HashSet<T>(comparer);
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		foreach (T item in array)
		{
			if (hashSet.Add(item))
			{
				instance.Add(item);
			}
		}
		ImmutableArray<T> result = ((instance.Count == array.Length) ? array : instance.ToImmutable());
		instance.Free();
		return result;
	}

	internal static bool HasAnyErrors<T>(this ImmutableArray<T> diagnostics) where T : Diagnostic
	{
		foreach (T item in diagnostics)
		{
			if (item.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	internal static ImmutableArray<T> DeOrder<T>(this ImmutableArray<T> array)
	{
		if (!array.IsDefault && array.Length >= 2)
		{
			T[] array2 = array.ToArray();
			int num = array2.Length - 1;
			T val = array2[0];
			array2[0] = array2[num];
			array2[num] = val;
			return array2.AsImmutable();
		}
		return array;
	}

	internal static ImmutableArray<TValue> Flatten<TKey, TValue>(this Dictionary<TKey, ImmutableArray<TValue>> dictionary, IComparer<TValue> comparer = null)
	{
		if (dictionary.Count == 0)
		{
			return ImmutableArray<TValue>.Empty;
		}
		ArrayBuilder<TValue> instance = ArrayBuilder<TValue>.GetInstance();
		foreach (KeyValuePair<TKey, ImmutableArray<TValue>> item in dictionary)
		{
			instance.AddRange(item.Value);
		}
		if (comparer != null && instance.Count > 1)
		{
			instance.Sort(comparer);
		}
		return instance.ToImmutableAndFree();
	}

	internal static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, ImmutableArray<T> second)
	{
		return first.AddRange(second);
	}

	internal static ImmutableArray<T> Concat<T>(this ImmutableArray<T> first, T second)
	{
		return first.Add(second);
	}

	internal static bool HasDuplicates<T>(this ImmutableArray<T> array, IEqualityComparer<T> comparer)
	{
		switch (array.Length)
		{
		case 0:
		case 1:
			return false;
		case 2:
			return comparer.Equals(array[0], array[1]);
		default:
		{
			HashSet<T> hashSet = new HashSet<T>(comparer);
			foreach (T item in array)
			{
				if (!hashSet.Add(item))
				{
					return true;
				}
			}
			return false;
		}
		}
	}

	public static int Count<T>(this ImmutableArray<T> items, Func<T, bool> predicate)
	{
		if (items.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			if (predicate(items[i]))
			{
				num++;
			}
		}
		return num;
	}

	internal static Dictionary<K, ImmutableArray<T>> ToDictionary<K, T>(this ImmutableArray<T> items, Func<T, K> keySelector, IEqualityComparer<K> comparer = null)
	{
		if (items.Length == 1)
		{
			Dictionary<K, ImmutableArray<T>> dictionary = new Dictionary<K, ImmutableArray<T>>(1, comparer);
			T val = items[0];
			dictionary.Add(keySelector(val), ImmutableArray.Create(val));
			return dictionary;
		}
		if (items.Length == 0)
		{
			return new Dictionary<K, ImmutableArray<T>>(comparer);
		}
		Dictionary<K, ArrayBuilder<T>> dictionary2 = new Dictionary<K, ArrayBuilder<T>>(items.Length, comparer);
		for (int i = 0; i < items.Length; i++)
		{
			T val2 = items[i];
			K key = keySelector(val2);
			if (!dictionary2.TryGetValue(key, out var value))
			{
				value = ArrayBuilder<T>.GetInstance();
				dictionary2.Add(key, value);
			}
			value.Add(val2);
		}
		Dictionary<K, ImmutableArray<T>> dictionary3 = new Dictionary<K, ImmutableArray<T>>(dictionary2.Count, comparer);
		foreach (KeyValuePair<K, ArrayBuilder<T>> item in dictionary2)
		{
			dictionary3.Add(item.Key, item.Value.ToImmutableAndFree());
		}
		return dictionary3;
	}
}
