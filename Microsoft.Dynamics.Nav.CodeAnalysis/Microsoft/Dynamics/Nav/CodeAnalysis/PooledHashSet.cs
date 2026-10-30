using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledHashSet<T> : HashSet<T>
{
	private readonly ObjectPool<PooledHashSet<T>> pool;

	private static readonly ObjectPool<PooledHashSet<T>> poolInstance = CreatePool();

	private PooledHashSet(ObjectPool<PooledHashSet<T>> pool)
	{
		this.pool = pool;
	}

	public void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}

	public static ObjectPool<PooledHashSet<T>> CreatePool()
	{
		ObjectPool<PooledHashSet<T>> pool = null;
		pool = new ObjectPool<PooledHashSet<T>>(() => new PooledHashSet<T>(pool), 128);
		return pool;
	}

	public static PooledHashSet<T> GetInstance()
	{
		PooledHashSet<T> pooledHashSet = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledHashSet.Count == 0);
		return pooledHashSet;
	}
}
