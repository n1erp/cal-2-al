using System;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class CachingIdentityFactory<TKey, TValue> : CachingBase<CachingIdentityFactory<TKey, TValue>.Entry> where TKey : class
{
	internal struct Entry
	{
		internal TKey Key { get; set; }

		internal TValue Value { get; set; }
	}

	private readonly Func<TKey, TValue> valueFactory;

	private readonly ObjectPool<CachingIdentityFactory<TKey, TValue>> objectPool;

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory)
		: base(size)
	{
		this.valueFactory = valueFactory;
	}

	public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory, ObjectPool<CachingIdentityFactory<TKey, TValue>> objectPoolParam)
		: this(size, valueFactory)
	{
		objectPool = objectPoolParam;
	}

	public void Add(TKey key, TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & Mask;
		Entries[num].Key = key;
		Entries[num].Value = value;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = RuntimeHelpers.GetHashCode(key) & Mask;
		Entry[] entries = Entries;
		if (entries[num].Key == key)
		{
			value = entries[num].Value;
			return true;
		}
		value = default;
		return false;
	}

	public TValue GetOrMakeValue(TKey key)
	{
		int num = RuntimeHelpers.GetHashCode(key) & Mask;
		Entry[] entries = Entries;
		if (entries[num].Key == key)
		{
			return entries[num].Value;
		}
		TValue val = valueFactory(key);
		entries[num].Key = key;
		entries[num].Value = val;
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
		objectPool?.Free(this);
	}
}
