using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SharedPoolExtensions
{
	private const int Threshold = 512;

	public static PooledObject<StringBuilder> GetPooledObject(this ObjectPool<StringBuilder> pool)
	{
		return PooledObject<StringBuilder>.Create(pool);
	}

	public static PooledObject<Stack<TItem>> GetPooledObject<TItem>(this ObjectPool<Stack<TItem>> pool)
	{
		return PooledObject<Stack<TItem>>.Create(pool);
	}

	public static PooledObject<Queue<TItem>> GetPooledObject<TItem>(this ObjectPool<Queue<TItem>> pool)
	{
		return PooledObject<Queue<TItem>>.Create(pool);
	}

	public static PooledObject<HashSet<TItem>> GetPooledObject<TItem>(this ObjectPool<HashSet<TItem>> pool)
	{
		return PooledObject<HashSet<TItem>>.Create(pool);
	}

	public static PooledObject<Dictionary<TKey, TValue>> GetPooledObject<TKey, TValue>(this ObjectPool<Dictionary<TKey, TValue>> pool)
	{
		return PooledObject<Dictionary<TKey, TValue>>.Create(pool);
	}

	public static PooledObject<List<TItem>> GetPooledObject<TItem>(this ObjectPool<List<TItem>> pool)
	{
		return PooledObject<List<TItem>>.Create(pool);
	}

	public static PooledObject<T> GetPooledObject<T>(this ObjectPool<T> pool) where T : class
	{
		return new PooledObject<T>(pool, (ObjectPool<T> p) => p.Allocate(), (ObjectPool<T> p, T o) =>
		{
			p.Free(o);
		});
	}

	public static StringBuilder AllocateAndClear(this ObjectPool<StringBuilder> pool)
	{
		StringBuilder stringBuilder = pool.Allocate();
		stringBuilder.Clear();
		return stringBuilder;
	}

	public static Stack<T> AllocateAndClear<T>(this ObjectPool<Stack<T>> pool)
	{
		Stack<T> stack = pool.Allocate();
		stack.Clear();
		return stack;
	}

	public static Queue<T> AllocateAndClear<T>(this ObjectPool<Queue<T>> pool)
	{
		Queue<T> queue = pool.Allocate();
		queue.Clear();
		return queue;
	}

	public static HashSet<T> AllocateAndClear<T>(this ObjectPool<HashSet<T>> pool)
	{
		HashSet<T> hashSet = pool.Allocate();
		hashSet.Clear();
		return hashSet;
	}

	public static Dictionary<TKey, TValue> AllocateAndClear<TKey, TValue>(this ObjectPool<Dictionary<TKey, TValue>> pool)
	{
		Dictionary<TKey, TValue> dictionary = pool.Allocate();
		dictionary.Clear();
		return dictionary;
	}

	public static List<T> AllocateAndClear<T>(this ObjectPool<List<T>> pool)
	{
		List<T> list = pool.Allocate();
		list.Clear();
		return list;
	}

	public static void ClearAndFree(this ObjectPool<StringBuilder> pool, StringBuilder sb)
	{
		if (sb != null)
		{
			sb.Clear();
			if (sb.Capacity > 512)
			{
				sb.Capacity = 512;
			}
			pool.Free(sb);
		}
	}

	public static void ClearAndFree<T>(this ObjectPool<HashSet<T>> pool, HashSet<T> set)
	{
		if (set != null)
		{
			int count = set.Count;
			set.Clear();
			if (count > 512)
			{
				set.TrimExcess();
			}
			pool.Free(set);
		}
	}

	public static void ClearAndFree<T>(this ObjectPool<Stack<T>> pool, Stack<T> set)
	{
		if (set != null)
		{
			int count = set.Count;
			set.Clear();
			if (count > 512)
			{
				set.TrimExcess();
			}
			pool.Free(set);
		}
	}

	public static void ClearAndFree<T>(this ObjectPool<Queue<T>> pool, Queue<T> set)
	{
		if (set != null)
		{
			int count = set.Count;
			set.Clear();
			if (count > 512)
			{
				set.TrimExcess();
			}
			pool.Free(set);
		}
	}

	public static void ClearAndFree<TKey, TValue>(this ObjectPool<Dictionary<TKey, TValue>> pool, Dictionary<TKey, TValue> map)
	{
		if (map != null && map.Count <= 512)
		{
			map.Clear();
			pool.Free(map);
		}
	}

	public static void ClearAndFree<T>(this ObjectPool<List<T>> pool, List<T> list)
	{
		if (list != null)
		{
			list.Clear();
			if (list.Capacity > 512)
			{
				list.Capacity = 512;
			}
			pool.Free(list);
		}
	}
}
