using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.PooledObjects;

internal class PooledDictionary<K, V> : Dictionary<K, V>
{
	private readonly ObjectPool<PooledDictionary<K, V>> _pool;

	private static readonly ObjectPool<PooledDictionary<K, V>> s_poolInstance = CreatePool();

	private PooledDictionary(ObjectPool<PooledDictionary<K, V>> pool)
	{
		_pool = pool;
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
		_pool?.Free(this);
	}

	public static ObjectPool<PooledDictionary<K, V>> CreatePool()
	{
		ObjectPool<PooledDictionary<K, V>> pool = null;
		pool = new ObjectPool<PooledDictionary<K, V>>(() => new PooledDictionary<K, V>(pool), 128);
		return pool;
	}

	public static PooledDictionary<K, V> GetInstance()
	{
		return s_poolInstance.Allocate();
	}
}
