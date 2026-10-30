using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types.Cache;

public abstract class CacheGroup : IDisposable
{
	private bool isDisposed;

	private List<IMemoryCache> MemoryCacheList { get; set; }

	public event EventHandler Cleared;

	public T ProvideCache<T>() where T : class, IMemoryCache
	{
		if (MemoryCacheList == null)
		{
			if (isDisposed)
			{
				return null;
			}
			MemoryCacheList = new List<IMemoryCache>();
			CreateMemoryCaches();
		}
		return MemoryCacheList.OfType<T>().FirstOrDefault();
	}

	protected virtual void CreateMemoryCaches()
	{
	}

	protected void AddToCacheList(IMemoryCache cache)
	{
		if (cache == null)
		{
			throw new ArgumentNullException("cache");
		}
		MemoryCacheList.Add(cache);
	}

	public void ClearAllFromCache()
	{
		if (MemoryCacheList != null)
		{
			foreach (IMemoryCache memoryCache in MemoryCacheList)
			{
				memoryCache.Clear();
			}
		}
		if (Cleared != null)
		{
			Cleared(this, EventArgs.Empty);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			if (disposing && MemoryCacheList != null)
			{
				ClearAllFromCache();
				MemoryCacheList = null;
			}
			isDisposed = true;
		}
	}
}
