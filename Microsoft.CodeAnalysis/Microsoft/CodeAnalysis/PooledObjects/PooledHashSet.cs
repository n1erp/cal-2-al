using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.PooledObjects;

internal class PooledHashSet<T> : HashSet<T>
{
	private readonly ObjectPool<PooledHashSet<T>> _pool;

	private static readonly ObjectPool<PooledHashSet<T>> s_poolInstance = CreatePool();

	private PooledHashSet(ObjectPool<PooledHashSet<T>> pool)
	{
		_pool = pool;
	}

	public void Free()
	{
		Clear();
		_pool?.Free(this);
	}

	public static ObjectPool<PooledHashSet<T>> CreatePool()
	{
		ObjectPool<PooledHashSet<T>> pool = null;
		pool = new ObjectPool<PooledHashSet<T>>(() => new PooledHashSet<T>(pool), 128);
		return pool;
	}

	public static PooledHashSet<T> GetInstance()
	{
		return s_poolInstance.Allocate();
	}
}
