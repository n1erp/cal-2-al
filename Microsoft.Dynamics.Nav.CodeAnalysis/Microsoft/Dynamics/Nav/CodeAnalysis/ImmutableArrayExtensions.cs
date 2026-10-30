using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ImmutableArrayExtensions
{
	internal static ImmutableArray<T> ToImmutableArrayOrEmpty<T>(this IEnumerable<T> items)
	{
		if (items == null)
		{
			return ImmutableArray<T>.Empty;
		}
		return ImmutableArray.CreateRange(items);
	}

	public static ImmutableArray<TResult> AsImmutable<TInput, TResult>(this IEnumerable<TInput> items) where TResult : TInput
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TInput item in items)
		{
			instance.Add((TResult)(object)item);
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> AsImmutableOrEmpty<TInput, TResult>(this TInput[] items, Func<TInput, TResult> creator)
	{
		if (items == null || items.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		TResult[] array = new TResult[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i] = creator(items[i]);
		}
		return ImmutableArray.Create(array);
	}

	public static ImmutableArray<TResult> AsImmutableOrEmpty<TInput, TResult>(this TInput[] items, Func<TInput, TResult> creator, Func<TInput, bool> predicate)
	{
		if (items == null || items.Length == 0)
		{
			return ImmutableArray<TResult>.Empty;
		}
		int length = 0;
		TResult[] array = new TResult[items.Length];
		foreach (TInput arg in items)
		{
			if (predicate(arg))
			{
				array[length++] = creator(arg);
			}
		}
		return ImmutableArray.Create(array, 0, length);
	}

	public static ImmutableArray<T> AsImmutable<T>(this T[] items)
	{
		DebugAssertHelper.Assert(items != null);
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

	public static ImmutableArray<TResult> SelectAsArray<TInput, TResult>(this ImmutableArray<TInput> items, Func<TInput, TResult> map)
	{
		return ImmutableArray.CreateRange(items, map);
	}

	public static ImmutableArray<TResult> SelectAsArray<TInput, TArg, TResult>(this ImmutableArray<TInput> items, Func<TInput, TArg, TResult> map, TArg arg)
	{
		return ImmutableArray.CreateRange(items, map, arg);
	}

	public static ImmutableArray<TResult> SelectAsArray<TInput, TArg, TResult>(this ImmutableArray<TInput> items, Func<TInput, int, TArg, TResult> map, TArg arg)
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

	public static ImmutableArray<TResult> SelectManyAsArray<TInput, TResult>(this ImmutableArray<TInput> items, Func<TInput, IEnumerable<TResult>> map) where TInput : TResult
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(items.Length);
		for (int i = 0; i < items.Length; i++)
		{
			foreach (TResult item in map(items[i]))
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> SelectManyAsArray<TInput, TResult, TIntermediary>(this ImmutableArray<TInput> items, Func<TInput, IEnumerable<TIntermediary>> map) where TResult : TIntermediary
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(items.Length);
		for (int i = 0; i < items.Length; i++)
		{
			foreach (TIntermediary item in map(items[i]))
			{
				instance.Add((TResult)(object)item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> WhereAsCastArray<TInput, TResult>(this IEnumerable<TInput> items, Func<TInput, bool> predicate) where TResult : TInput
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TInput item in items)
		{
			if (predicate(item))
			{
				instance.Add((TResult)(object)item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> WhereAndSelectAsCastArray<TInput, TIntermediary, TResult>(this ImmutableArray<TInput> items, Func<TInput, bool> predicate, Func<TIntermediary, TResult> map) where TIntermediary : TInput
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		foreach (TInput item in items)
		{
			if (predicate(item))
			{
				instance.Add(map((TIntermediary)(object)item));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<TResult> WhereAsCastArray<TResult, TInput>(this ImmutableArray<TInput> array, Func<TInput, bool> predicate) where TResult : TInput
	{
		ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance();
		for (int i = 0; i < array.Length; i++)
		{
			TInput val = array[i];
			if (predicate(val))
			{
				instance.Add((TResult)(object)val);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static ImmutableArray<T> WhereAsArray<T>(this ImmutableArray<T> array, Func<T, bool> predicate)
	{
		DebugAssertHelper.Assert(!array.IsDefault);
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
					DebugAssertHelper.Assert(i > 0);
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<T>.GetInstance();
					}
					arrayBuilder.Add(val);
				}
				continue;
			}
			if (flag)
			{
				flag2 = false;
				continue;
			}
			DebugAssertHelper.Assert(i > 0);
			if (flag2)
			{
				DebugAssertHelper.Assert(arrayBuilder == null);
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
			DebugAssertHelper.Assert(!flag2);
			DebugAssertHelper.Assert(!flag);
			return arrayBuilder.ToImmutableAndFree();
		}
		if (flag2)
		{
			return array;
		}
		DebugAssertHelper.Assert(flag);
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
		DebugAssertHelper.Assert(!array.IsDefault);
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
		for (int i = 0; i < diagnostics.Length; i++)
		{
			if (diagnostics[i].Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool HasAnyErrorsInSourceFile<T>(this ImmutableArray<T> diagnostics, string filePath) where T : Diagnostic
	{
		for (int i = 0; i < diagnostics.Length; i++)
		{
			Diagnostic diagnostic = diagnostics[i];
			if (diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location != null && diagnostic.Location.SourceTree != null && filePath.CreateUriFilePath().Equals(diagnostic.Location.SourceTree.FilePath.CreateUriFilePath()))
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

	internal static IReadOnlyList<T> ToImmutableReadOnlyListOrEmpty<T>(this IEnumerable<T> items)
	{
		if (items is ImmutableArray<T> { IsDefault: false })
		{
			return (IReadOnlyList<T>)items;
		}
		return items.ToImmutableArrayOrEmpty();
	}

	internal static void IterateOverManyDuplicatesBy<T, U, V>(this ImmutableArray<T> items, Func<T, ImmutableArray<U>> selectMany, Func<U, V> groupBy, Action<U, V> action)
	{
		PooledDictionary<V, U> instance = PooledDictionary<V, U>.GetInstance();
		try
		{
			foreach (T item in items)
			{
				foreach (U item2 in selectMany(item))
				{
					V val = groupBy(item2);
					if (instance.TryGetValue(val, out var value))
					{
						action(item2, val);
						if (value != null)
						{
							action(value, val);
							instance[val] = default;
						}
					}
					else
					{
						instance.Add(val, item2);
					}
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}
}
