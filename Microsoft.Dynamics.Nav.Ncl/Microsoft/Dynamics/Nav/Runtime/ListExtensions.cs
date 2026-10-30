using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ListExtensions
{
	internal static int BinarySearch<T>(this IList<T> list, int index, int count, Func<T, int> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (list.Count - index < count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int num = index;
		int num2 = index + count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			T arg = list[num3];
			int num4 = comparer(arg);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 > 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	internal static int BinarySearch<T>(this IList<T> list, Func<T, int> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return list.BinarySearch(0, list.Count, comparer);
	}

	internal static int BinarySearch<T>(this IList<T> list, T item) where T : IComparable<T>
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		return list.BinarySearch(0, list.Count, ((IComparable<T>)item).CompareTo);
	}

	public static int IndexOf<T>(this IEnumerable<T> list, T value, IEqualityComparer<T> comparer)
	{
		int num = 0;
		foreach (T item in list)
		{
			if (comparer.Equals(item, value))
			{
				return num;
			}
			num++;
		}
		return -1;
	}
}
