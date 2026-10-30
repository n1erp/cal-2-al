using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class LRUCache : LRUCacheItem
{
	private object lockObject = new object();

	private long maxSize;

	private int count;

	private ManualResetGate scavangeInProgress = new ManualResetGate();

	private int version;

	public int Count => count;

	public long MaxSize => maxSize;

	public bool WaitOnScavenge { get; }

	private long ShrinkedCacheSize => (maxSize >> 2) + (maxSize >> 1);

	public LRUCache(long maxSize)
		: base(null, 0L)
	{
		this.maxSize = maxSize;
		Next = this;
		Prev = this;
	}

	internal void Add(LRUCacheItem item)
	{
		bool flag;
		lock (lockObject)
		{
			if (item == null || item.IsLinked)
			{
				throw new InvalidOperationException("Can't add null or already linked values to LRU");
			}
			version++;
			item.LRUCache = this;
			item.Next = Next;
			item.Prev = this;
			Next.Prev = item;
			Next = item;
			Size += item.Size;
			count++;
			flag = Size > maxSize;
		}
		if (flag)
		{
			ScavengeCacheAsync();
		}
	}

	internal void MoveToFront(LRUCacheItem item)
	{
		if (item == null)
		{
			throw new InvalidOperationException("Can't move a null item to the front");
		}
		lock (lockObject)
		{
			if (item.IsLinked && Next != item)
			{
				version++;
				item.Next.Prev = item.Prev;
				item.Prev.Next = item.Next;
				item.Next = Next;
				item.Prev = this;
				Next.Prev = item;
				Next = item;
			}
		}
	}

	internal void Remove(LRUCacheItem item)
	{
		if (item == null)
		{
			throw new InvalidOperationException("Can't remove a null item from the LRUCache");
		}
		lock (lockObject)
		{
			if (item.IsLinked)
			{
				version++;
				InternalUnlink(item);
			}
		}
	}

	internal void Remove(IEnumerable<LRUCacheItem> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		lock (lockObject)
		{
			version++;
			foreach (LRUCacheItem item in items)
			{
				if (item.IsLinked)
				{
					InternalUnlink(item);
				}
			}
		}
	}

	private void InternalUnlink(LRUCacheItem itemToUnlink)
	{
		itemToUnlink.Next.Prev = itemToUnlink.Prev;
		itemToUnlink.Prev.Next = itemToUnlink.Next;
		itemToUnlink.Next = null;
		itemToUnlink.Prev = null;
		itemToUnlink.LRUCache = null;
		Size -= itemToUnlink.Size;
		count--;
	}

	internal void RemoveAllFromOrigin(NavDatabase originOfItemsToRemove)
	{
		if (originOfItemsToRemove == null)
		{
			throw new ArgumentNullException("originOfItemsToRemove");
		}
		lock (lockObject)
		{
			version++;
			LRUCacheItem lRUCacheItem = Next;
			while (lRUCacheItem != this)
			{
				if (lRUCacheItem.OriginDatabase != originOfItemsToRemove)
				{
					lRUCacheItem = lRUCacheItem.Next;
					continue;
				}
				LRUCacheItem itemToUnlink = lRUCacheItem;
				lRUCacheItem = lRUCacheItem.Next;
				InternalUnlink(itemToUnlink);
			}
		}
	}

	internal void AddSize(LRUCacheItem item, long sizeToAdd)
	{
		bool flag = false;
		lock (lockObject)
		{
			if (item.IsLinked)
			{
				if (item.LRUCache != this)
				{
					throw new InvalidOperationException("Can't change size on LRU item.  Item does not belong to this LRUCache");
				}
				Size += sizeToAdd;
				if (Size > maxSize)
				{
					flag = true;
				}
			}
			item.Size += sizeToAdd;
		}
		if (flag)
		{
			ScavengeCacheAsync();
		}
	}

	private void ScavengeCacheAsync()
	{
		if (scavangeInProgress.TryEnter())
		{
			Task task = NavTaskFactory.StartNewTask(() =>
			{
				ScavengeCache();
			}).ContinueWith((Task t) =>
			{
				scavangeInProgress.Reset();
			}, TaskContinuationOptions.ExecuteSynchronously);
			if (WaitOnScavenge)
			{
				task.WaitThrowingOriginalException();
			}
			else
			{
				task.LogOrFailOnException(failFast: true);
			}
		}
	}

	internal IEnumerable<LRUCacheItem> Items()
	{
		int expVersion = version;
		for (LRUCacheItem item = Next; item != this; item = item.Next)
		{
			if (version != expVersion)
			{
				throw new InvalidOperationException();
			}
			yield return item;
		}
	}

	internal override void ItemHasBeenRemovedByScavenging()
	{
		throw new InvalidOperationException();
	}

	internal int ScavengeCache(Func<LRUCacheItem, bool> cacheItemPredicate = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		long num = 0L;
		try
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001ZW", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Starting cache scavenging. Elements: {0}, Size: {1} bytes, Max Size: {2} bytes", count, Size, maxSize);
			List<LRUCacheItem> list = new List<LRUCacheItem>();
			lock (lockObject)
			{
				num = stopwatch.ElapsedMilliseconds;
				bool flag = cacheItemPredicate != null;
				if (Size <= maxSize && !flag)
				{
					num = stopwatch.ElapsedMilliseconds - num;
					return 0;
				}
				version++;
				LRUCacheItem lRUCacheItem = Prev;
				while (lRUCacheItem != this && ((Size > ShrinkedCacheSize) | flag))
				{
					if (!flag || cacheItemPredicate(lRUCacheItem))
					{
						LRUCacheItem lRUCacheItem2 = lRUCacheItem;
						lRUCacheItem = lRUCacheItem.Prev;
						Size -= lRUCacheItem2.Size;
						count--;
						if (flag)
						{
							lRUCacheItem2.Next.Prev = lRUCacheItem2.Prev;
							lRUCacheItem2.Prev.Next = lRUCacheItem2.Next;
						}
						lRUCacheItem2.LRUCache = null;
						lRUCacheItem2.Prev = null;
						lRUCacheItem2.Next = null;
						list.Add(lRUCacheItem2);
					}
					else
					{
						lRUCacheItem = lRUCacheItem.Prev;
					}
				}
				if (!flag)
				{
					lRUCacheItem.Next = this;
					Prev = lRUCacheItem;
				}
				num = stopwatch.ElapsedMilliseconds - num;
			}
			foreach (LRUCacheItem item in list)
			{
				item?.ItemHasBeenRemovedByScavenging();
			}
			return list.Count;
		}
		finally
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001ZW", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Done cache scavenging. Elements: {0}, Size: {1} bytes, Max Size: {2} bytes, Duration: {3} ms, Cache lock held: {4} ms", count, Size, maxSize, stopwatch.ElapsedMilliseconds, num);
		}
	}

	public void ServerUserSettingsChanged(object sender, SettingsChangedEventArgs eventArgs)
	{
		if (!(eventArgs.SettingName != "DataCacheSize") && maxSize != ServerUserSettings.Instance.DataCacheSize.CacheSize)
		{
			long num = MaxSize;
			maxSize = ServerUserSettings.Instance.DataCacheSize.CacheSize;
			if (num > maxSize)
			{
				ScavengeCacheAsync();
			}
		}
	}
}
