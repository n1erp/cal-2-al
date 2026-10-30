using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ArrayPool<T>
{
	private const int MaxPooledArraySize = 256;

	private static ObjectPool<T[]> pool = new ObjectPool<T[]>(() => new T[256]);

	public static T[] GetArray(int size)
	{
		if (size <= 256)
		{
			T[] array = pool.Allocate();
			Array.Clear(array, 0, array.Length);
			return array;
		}
		return new T[size];
	}

	public static void ReleaseArray(T[] array)
	{
		if (array.Length <= 256)
		{
			pool.Free(array);
		}
	}
}
