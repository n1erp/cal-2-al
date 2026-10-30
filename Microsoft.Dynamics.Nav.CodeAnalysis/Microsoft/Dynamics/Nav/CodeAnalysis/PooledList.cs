using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledList<T> : List<T>
{
	private readonly ObjectPool<PooledList<T>> pool;

	private static readonly ObjectPool<PooledList<T>> poolInstance = CreatePool();

	private PooledList(ObjectPool<PooledList<T>> pool)
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

	public static ObjectPool<PooledList<T>> CreatePool()
	{
		ObjectPool<PooledList<T>> pool = null;
		pool = new ObjectPool<PooledList<T>>(() => new PooledList<T>(pool), 128);
		return pool;
	}

	public static PooledList<T> GetInstance()
	{
		PooledList<T> pooledList = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledList.Count == 0);
		return pooledList;
	}
}
