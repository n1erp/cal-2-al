namespace Microsoft.Dynamics.Nav.Types.Cache;

public class GlobalVersionCache<TKey, TBaseItem>
{
	public delegate TItem ObtainItem<TItem>();

	private MemoryCache<TKey, TBaseItem> cache;

	private long version;

	private bool useCache = true;

	public long Version => version;

	public bool UseCache => useCache;

	public GlobalVersionCache(int maxCacheSize, int scavengeSize)
	{
		if (scavengeSize > 0)
		{
			cache = new MemoryCache<TKey, TBaseItem>(maxCacheSize, scavengeSize);
		}
		else
		{
			useCache = false;
		}
	}

	public void UpdateVersion(long newVersion)
	{
		lock (this)
		{
			version = newVersion;
			if (useCache)
			{
				cache.Clear();
			}
		}
	}

	public void Remove(TKey key)
	{
		if (useCache)
		{
			cache.RemoveFromCache(key);
		}
	}

	public TItem GetItem<TItem>(TKey key, ObtainItem<TItem> obtainItem) where TItem : TBaseItem
	{
		TItem val;
		if (useCache)
		{
			if (cache.TryGetValue(key, Version, out var item))
			{
				val = (TItem)(object)item;
			}
			else
			{
				long num = Version;
				val = obtainItem();
				lock (this)
				{
					if (num == Version)
					{
						cache.Store(key, Version, (TBaseItem)(object)val);
					}
				}
			}
		}
		else
		{
			val = obtainItem();
		}
		return val;
	}
}
