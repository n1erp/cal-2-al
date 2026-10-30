using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types.Cache;

public class MemoryCache<TKey, TValue> : IMemoryCache
{
	private static object syncObject = new object();

	private readonly GeneralCacheStore<TKey, TValue> cache;

	private const int MaxCacheSize = 1000;

	private const int DefaultScavengeSize = 100;

	private GeneralCacheStore<TKey, TValue> Cache => cache;

	public MemoryCache(int maxCacheSize, int defaultScavengeSize)
	{
		cache = new GeneralCacheStore<TKey, TValue>(maxCacheSize, defaultScavengeSize, CacheMode.AccessBasedScavenger);
	}

	public MemoryCache()
		: this(1000, 100)
	{
	}

	public void RemoveFromCache(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		Cache.Remove(key);
	}

	public void Clear()
	{
		Cache.Clear();
	}

	public TValue FindValue(TKey key, long version, Func<TValue> loadIfNotFound)
	{
		if (TryGetValue(key, version, out var item))
		{
			return item;
		}
		item = loadIfNotFound();
		if (item != null)
		{
			Store(key, version, item);
		}
		return item;
	}

	public bool TryGetValue(TKey key, long version, out TValue item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (Cache.TryGetItem(key, out var version2, out item) && version == version2)
		{
			return true;
		}
		item = default;
		return false;
	}

	public void Store(TKey metadataIdentifier, long version, TValue data)
	{
		if (metadataIdentifier == null)
		{
			throw new ArgumentNullException("metadataIdentifier");
		}
		Cache.TryAddItem(metadataIdentifier, version, data, replaceExistingEntry: true);
	}

	public ICollection<TKey> GetKeys()
	{
		return cache.GetKeys();
	}
}
