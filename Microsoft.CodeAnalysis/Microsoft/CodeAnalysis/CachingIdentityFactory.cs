using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal class CachingIdentityFactory<TKey, TValue> : CachingBase<CachingIdentityFactory<TKey, TValue>.Entry> where TKey : class
{
	internal struct Entry
	{
		internal TKey key;

		internal TValue value;
	}

	private readonly Func<TKey, TValue> _valueFactory;

	private readonly ObjectPool<CachingIdentityFactory<TKey, TValue>> _pool;

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory)
		: base(size)
	{
		_valueFactory = valueFactory;
	}

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory, ObjectPool<CachingIdentityFactory<TKey, TValue>> pool)
		: this(size, valueFactory)
	{
		_pool = pool;
	}

	public void Add(TKey key, TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		entries[num].key = key;
		entries[num].value = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		Entry[] array = entries;
		if (array[num].key == key)
		{
			value = array[num].value;
			return true;
		}
		value = default;
		return false;
	}

	public TValue GetOrMakeValue(TKey key)
	{
		int num = RuntimeHelpers.GetHashCode(key) & mask;
		Entry[] array = entries;
		if (array[num].key == key)
		{
			return array[num].value;
		}
		TValue val = _valueFactory(key);
		array[num].key = key;
		array[num].value = val;
		return val;
	}

	public static ObjectPool<CachingIdentityFactory<TKey, TValue>> CreatePool(int size, Func<TKey, TValue> valueFactory)
	{
		ObjectPool<CachingIdentityFactory<TKey, TValue>> pool = null;
		pool = new ObjectPool<CachingIdentityFactory<TKey, TValue>>(() => new CachingIdentityFactory<TKey, TValue>(size, valueFactory, pool), Environment.ProcessorCount * 2);
		return pool;
	}

	public void Free()
	{
		_pool?.Free(this);
	}
}
