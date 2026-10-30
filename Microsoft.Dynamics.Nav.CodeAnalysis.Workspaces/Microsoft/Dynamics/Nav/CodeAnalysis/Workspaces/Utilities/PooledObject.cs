using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal struct PooledObject<T> : IDisposable where T : class
{
	private readonly Action<ObjectPool<T>, T> releaser;

	private readonly ObjectPool<T> pool;

	public T Object { get; private set; }

	public PooledObject(ObjectPool<T> pool, Func<ObjectPool<T>, T> allocator, Action<ObjectPool<T>, T> releaser)
	{
		this = default;
		this.pool = pool;
		Object = allocator(pool);
		this.releaser = releaser;
	}

	public void Dispose()
	{
		if (Object != null)
		{
			releaser(pool, Object);
			Object = null;
		}
	}

	public static PooledObject<StringBuilder> Create(ObjectPool<StringBuilder> pool)
	{
		return new PooledObject<StringBuilder>(pool, Allocator, Releaser);
	}

	public static PooledObject<Stack<TItem>> Create<TItem>(ObjectPool<Stack<TItem>> pool)
	{
		return new PooledObject<Stack<TItem>>(pool, Allocator, Releaser);
	}

	public static PooledObject<Queue<TItem>> Create<TItem>(ObjectPool<Queue<TItem>> pool)
	{
		return new PooledObject<Queue<TItem>>(pool, Allocator, Releaser);
	}

	public static PooledObject<HashSet<TItem>> Create<TItem>(ObjectPool<HashSet<TItem>> pool)
	{
		return new PooledObject<HashSet<TItem>>(pool, Allocator, Releaser);
	}

	public static PooledObject<Dictionary<TKey, TValue>> Create<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool)
	{
		return new PooledObject<Dictionary<TKey, TValue>>(pool, Allocator, Releaser);
	}

	public static PooledObject<List<TItem>> Create<TItem>(ObjectPool<List<TItem>> pool)
	{
		return new PooledObject<List<TItem>>(pool, Allocator, Releaser);
	}

	private static StringBuilder Allocator(ObjectPool<StringBuilder> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser(ObjectPool<StringBuilder> pool, StringBuilder sb)
	{
		pool.ClearAndFree(sb);
	}

	private static Stack<TItem> Allocator<TItem>(ObjectPool<Stack<TItem>> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser<TItem>(ObjectPool<Stack<TItem>> pool, Stack<TItem> obj)
	{
		pool.ClearAndFree(obj);
	}

	private static Queue<TItem> Allocator<TItem>(ObjectPool<Queue<TItem>> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser<TItem>(ObjectPool<Queue<TItem>> pool, Queue<TItem> obj)
	{
		pool.ClearAndFree(obj);
	}

	private static HashSet<TItem> Allocator<TItem>(ObjectPool<HashSet<TItem>> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser<TItem>(ObjectPool<HashSet<TItem>> pool, HashSet<TItem> obj)
	{
		pool.ClearAndFree(obj);
	}

	private static Dictionary<TKey, TValue> Allocator<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool, Dictionary<TKey, TValue> obj)
	{
		pool.ClearAndFree(obj);
	}

	private static List<TItem> Allocator<TItem>(ObjectPool<List<TItem>> pool)
	{
		return pool.AllocateAndClear();
	}

	private static void Releaser<TItem>(ObjectPool<List<TItem>> pool, List<TItem> obj)
	{
		pool.ClearAndFree(obj);
	}
}
