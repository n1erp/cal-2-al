using System;

namespace Microsoft.Dynamics.Nav.Types.Cache;

public class TimerCache<TKey, TValue> : IDisposable
{
	private GeneralCacheStore<TKey, TValue> cache;

	private static TimeSpan defaultExpirationTime = TimeSpan.FromMinutes(1.0);

	private const int DefaultCacheSize = 1000;

	private bool disposed;

	public TimerCache(TimeSpan scavengeInterval)
	{
		cache = new GeneralCacheStore<TKey, TValue>(1000, 1000, scavengeInterval, CacheMode.TimerBasedScavenger);
	}

	public TimerCache()
		: this(defaultExpirationTime)
	{
	}

	protected bool TryRemoveItem(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return cache.TryRemoveItem(key);
	}

	protected void Clear()
	{
		cache.Clear();
	}

	protected bool TryGetItem(TKey key, long version, out TValue item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (cache.TryGetItem(key, out var version2, out item) && version == version2)
		{
			return true;
		}
		item = default;
		return false;
	}

	protected bool TryAddItem(TKey key, long version, TValue item, DateTime expirationTime, bool replaceExistingEntry)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return cache.TryAddItem(key, version, item, expirationTime, replaceExistingEntry);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			if (cache != null)
			{
				cache.Dispose();
				cache = null;
			}
			disposed = true;
		}
	}
}
