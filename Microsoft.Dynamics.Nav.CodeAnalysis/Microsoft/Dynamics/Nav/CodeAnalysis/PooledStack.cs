using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledStack<T> : Stack<T>
{
	private readonly ObjectPool<PooledStack<T>> pool;

	private static readonly ObjectPool<PooledStack<T>> poolInstance = CreatePool();

	private PooledStack(ObjectPool<PooledStack<T>> pool)
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

	public static ObjectPool<PooledStack<T>> CreatePool()
	{
		ObjectPool<PooledStack<T>> pool = null;
		pool = new ObjectPool<PooledStack<T>>(() => new PooledStack<T>(pool), 128);
		return pool;
	}

	public static PooledStack<T> GetInstance()
	{
		PooledStack<T> pooledStack = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledStack.Count == 0);
		return pooledStack;
	}
}
