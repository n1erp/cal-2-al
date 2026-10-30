using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cci;

internal static class IteratorHelper
{
	public static bool EnumerableIsNotEmpty<T>(IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			return false;
		}
		if (enumerable is IList<T> list)
		{
			return list.Count != 0;
		}
		if (enumerable is IList list2)
		{
			return list2.Count != 0;
		}
		return enumerable.GetEnumerator().MoveNext();
	}

	public static bool EnumerableIsEmpty<T>(IEnumerable<T> enumerable)
	{
		return !EnumerableIsNotEmpty(enumerable);
	}

	public static uint EnumerableCount<T>(IEnumerable<T> enumerable)
	{
		if (enumerable == null)
		{
			return 0u;
		}
		if (enumerable is IList<T> list)
		{
			return (uint)list.Count;
		}
		if (enumerable is IList list2)
		{
			return (uint)list2.Count;
		}
		uint num = 0u;
		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
		}
		return num & 0x7FFFFFFF;
	}
}
