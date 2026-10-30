using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class PooledDictionary<K, V> : Dictionary<K, V>
{
	private readonly ObjectPool<PooledDictionary<K, V>> pool;

	private static readonly ObjectPool<PooledDictionary<K, V>> poolInstance = CreatePool();

	private PooledDictionary(ObjectPool<PooledDictionary<K, V>> pool)
	{
		this.pool = pool;
	}

	public ImmutableDictionary<K, V> ToImmutableDictionaryAndFree()
	{
		ImmutableDictionary<K, V> result = this.ToImmutableDictionary();
		Free();
		return result;
	}

	public void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}

	public static ObjectPool<PooledDictionary<K, V>> CreatePool()
	{
		ObjectPool<PooledDictionary<K, V>> pool = null;
		pool = new ObjectPool<PooledDictionary<K, V>>(() => new PooledDictionary<K, V>(pool), 128);
		return pool;
	}

	public static PooledDictionary<K, V> GetInstance()
	{
		PooledDictionary<K, V> pooledDictionary = poolInstance.Allocate();
		DebugAssertHelper.Assert(pooledDictionary.Count == 0);
		return pooledDictionary;
	}
}
