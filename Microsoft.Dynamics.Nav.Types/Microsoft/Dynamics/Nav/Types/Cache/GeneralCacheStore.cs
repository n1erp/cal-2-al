using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types.Cache;

internal sealed class GeneralCacheStore<TKey, TValue> : IDisposable
{
	private static TimerCallback scavengeTimerCallback = ScavengeCallback;

	private TimeSpan scavengeInterval;

	private Timer scavengeTimer;

	private ReaderWriterLock cacheStoreLock;

	private Dictionary<TKey, CacheEntry<TValue>> cacheStore;

	private long lastAccessed;

	private int maxCacheSize;

	private int defaultScavengeSize;

	private bool disposed;

	private const int DefaultWaitTime = 10000;

	private CacheMode cacheMode;

	private bool ScavengeNeeded
	{
		get
		{
			if (cacheStore.Count >= maxCacheSize)
			{
				return cacheMode == CacheMode.AccessBasedScavenger;
			}
			return false;
		}
	}

	private DateTime LastAccessedUtc => new DateTime(Interlocked.Increment(ref lastAccessed), DateTimeKind.Utc);

	public int Count => cacheStore.Count;

	internal GeneralCacheStore(int maxCacheSize, int defaultScavengeSize, CacheMode cacheMode)
	{
		cacheStore = new Dictionary<TKey, CacheEntry<TValue>>(defaultScavengeSize);
		cacheStoreLock = new ReaderWriterLock();
		lastAccessed = DateTime.UtcNow.Ticks;
		this.cacheMode = cacheMode;
		this.maxCacheSize = maxCacheSize;
		this.defaultScavengeSize = defaultScavengeSize;
	}

	internal GeneralCacheStore(int maxCacheSize, int defaultScavengeSize, TimeSpan scavengeInterval, CacheMode cacheMode)
		: this(maxCacheSize, defaultScavengeSize, cacheMode)
	{
		if (cacheMode == CacheMode.TimerBasedScavenger)
		{
			this.scavengeInterval = scavengeInterval;
		}
	}

	internal bool TryGetItem(TKey key, out long version, out TValue item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		version = 0L;
		item = default;
		cacheStoreLock.AcquireReaderLock(10000);
		try
		{
			if (cacheStore.TryGetValue(key, out var value))
			{
				if (value != null && cacheMode == CacheMode.TimerBasedScavenger && IsExpired(value))
				{
					return false;
				}
				if (cacheMode == CacheMode.AccessBasedScavenger)
				{
					value.ScavengeTime = LastAccessedUtc;
				}
				version = value.Version;
				item = value.Item;
				return true;
			}
		}
		finally
		{
			if (cacheStoreLock.IsReaderLockHeld)
			{
				cacheStoreLock.ReleaseReaderLock();
			}
		}
		return false;
	}

	internal bool TryRemoveItem(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		cacheStoreLock.AcquireWriterLock(10000);
		bool result = false;
		try
		{
			if (ScavengeNeeded)
			{
				ScavengeOldItems();
			}
			if (cacheStore.TryGetValue(key, out var value))
			{
				IExpirableItem expirableItem = value;
				bool num = expirableItem != null && !IsExpired(expirableItem);
				if (expirableItem != null)
				{
					cacheStore.Remove(key);
					CancelTimerIfNeeded();
				}
				result = num;
			}
		}
		finally
		{
			if (cacheStoreLock.IsWriterLockHeld)
			{
				cacheStoreLock.ReleaseWriterLock();
			}
		}
		return result;
	}

	internal void AddItem(TKey key, long version, TValue item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		cacheStoreLock.AcquireWriterLock(10000);
		try
		{
			if (ScavengeNeeded)
			{
				ScavengeOldItems();
			}
			if (!cacheStore.TryGetValue(key, out var value))
			{
				cacheStore.Add(key, new CacheEntry<TValue>(item, version, LastAccessedUtc));
				return;
			}
			value.Item = item;
			value.Version = version;
			value.ScavengeTime = LastAccessedUtc;
		}
		finally
		{
			if (cacheStoreLock.IsWriterLockHeld)
			{
				cacheStoreLock.ReleaseWriterLock();
			}
		}
	}

	internal bool TryAddItem(TKey key, long version, TValue item, bool replaceExistingEntry)
	{
		return TryAddItem(key, new CacheEntry<TValue>(item, version, LastAccessedUtc), replaceExistingEntry);
	}

	internal bool TryAddItem(TKey key, long version, TValue item, DateTime expirationTime, bool replaceExistingEntry)
	{
		return TryAddItem(key, new CacheEntry<TValue>(item, version, expirationTime), replaceExistingEntry);
	}

	private bool TryAddItem(TKey key, CacheEntry<TValue> item, bool replaceExistingEntry)
	{
		try
		{
			cacheStoreLock.AcquireWriterLock(10000);
			if (ScavengeNeeded)
			{
				ScavengeOldItems();
			}
			if (cacheStore.TryGetValue(key, out var value))
			{
				if (value != null && !IsExpired(value) && !replaceExistingEntry)
				{
					return false;
				}
				cacheStore.Remove(key);
			}
			cacheStore.Add(key, item);
			StartTimerIfNeeded();
			return true;
		}
		finally
		{
			if (cacheStoreLock.IsWriterLockHeld)
			{
				cacheStoreLock.ReleaseWriterLock();
			}
		}
	}

	internal void Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		cacheStoreLock.AcquireWriterLock(10000);
		try
		{
			cacheStore.Remove(key);
		}
		finally
		{
			if (cacheStoreLock.IsWriterLockHeld)
			{
				cacheStoreLock.ReleaseWriterLock();
			}
		}
	}

	internal ICollection<TKey> GetKeys()
	{
		List<TKey> list = new List<TKey>(cacheStore.Count);
		foreach (TKey key in cacheStore.Keys)
		{
			list.Add(key);
		}
		return list;
	}

	internal void Clear()
	{
		cacheStoreLock.AcquireWriterLock(10000);
		try
		{
			if (typeof(TValue).GetInterface("System.IDisposable") != null)
			{
				foreach (CacheEntry<TValue> value in cacheStore.Values)
				{
					((IDisposable)(object)value.Item).Dispose();
				}
			}
			cacheStore.Clear();
			CancelTimerIfNeeded();
		}
		finally
		{
			if (cacheStoreLock.IsWriterLockHeld)
			{
				cacheStoreLock.ReleaseWriterLock();
			}
		}
	}

	private void StartTimerIfNeeded()
	{
		if (cacheMode == CacheMode.TimerBasedScavenger && scavengeTimer == null)
		{
			scavengeTimer = new Timer(scavengeTimerCallback, this, scavengeInterval, new TimeSpan(-1L));
		}
	}

	private static void ScavengeCallback(object state)
	{
		GeneralCacheStore<TKey, TValue> generalCacheStore = (GeneralCacheStore<TKey, TValue>)state;
		generalCacheStore.cacheStoreLock.AcquireWriterLock(10000);
		try
		{
			if (generalCacheStore.scavengeTimer != null)
			{
				generalCacheStore.ScavengeOldItems();
				if (generalCacheStore.Count > 0 && generalCacheStore.scavengeTimer != null)
				{
					generalCacheStore.scavengeTimer.Change(generalCacheStore.scavengeInterval, new TimeSpan(-1L));
				}
			}
		}
		finally
		{
			if (generalCacheStore.cacheStoreLock.IsWriterLockHeld)
			{
				generalCacheStore.cacheStoreLock.ReleaseWriterLock();
			}
		}
	}

	private static bool IsExpired(IExpirableItem item)
	{
		return item.ScavengeTime <= DateTime.UtcNow;
	}

	private void ScavengeOldItems()
	{
		int num = 0;
		SortedList<DateTime, TKey> sortedList = new SortedList<DateTime, TKey>(cacheStore.Count);
		foreach (KeyValuePair<TKey, CacheEntry<TValue>> item in cacheStore)
		{
			IExpirableItem value = item.Value;
			if (cacheMode == CacheMode.TimerBasedScavenger)
			{
				if (IsExpired(value))
				{
					sortedList.Add(LastAccessedUtc, item.Key);
				}
			}
			else
			{
				sortedList.Add(value.ScavengeTime, item.Key);
			}
		}
		foreach (KeyValuePair<DateTime, TKey> item2 in sortedList)
		{
			if (num >= defaultScavengeSize)
			{
				break;
			}
			cacheStore.Remove(item2.Value);
			num++;
		}
	}

	private void CancelTimerIfNeeded()
	{
		if (Count == 0 && scavengeTimer != null)
		{
			scavengeTimer.Dispose();
			scavengeTimer = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			if (scavengeTimer != null)
			{
				scavengeTimer.Dispose();
				scavengeTimer = null;
			}
			disposed = true;
		}
	}
}
