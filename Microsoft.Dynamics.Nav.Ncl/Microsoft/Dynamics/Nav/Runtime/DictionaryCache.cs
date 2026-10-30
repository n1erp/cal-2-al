using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DictionaryCache<TKey, TValue> : DictionaryCacheBase<TKey> where TValue : DictionaryCacheItem<TKey>
{
	private readonly object lockObject = new object();

	private Dictionary<TKey, TValue> cache;

	private readonly IEqualityComparer<TKey> comparer;

	private readonly LRUCache lruCache;

	private readonly IReference<NavDatabase> originDatabase;

	private readonly int transactionBumperToken;

	private bool isStale;

	internal int TransactionBumperToken => transactionBumperToken;

	internal LRUCache LRUCache => lruCache;

	internal override NavDatabase OriginDatabase => originDatabase?.Target;

	internal bool IsStale => isStale;

	protected Dictionary<TKey, TValue> Cache
	{
		[DebuggerStepThrough]
		get
		{
			return cache;
		}
	}

	protected object LockObject
	{
		[DebuggerStepThrough]
		get
		{
			return lockObject;
		}
	}

	protected DictionaryCache(LRUCache lruCache, IEqualityComparer<TKey> comparer)
		: this(lruCache, (NavDatabase)null, 0, comparer)
	{
	}

	protected DictionaryCache(LRUCache lruCache, NavDatabase originDatabase, int transactionBumperToken)
		: this(lruCache, originDatabase, transactionBumperToken, (IEqualityComparer<TKey>)null)
	{
	}

	protected DictionaryCache(LRUCache lruCache, NavDatabase originDatabase, int transactionBumperToken, IEqualityComparer<TKey> comparer)
	{
		this.lruCache = lruCache;
		this.originDatabase = originDatabase?.GetReference();
		this.transactionBumperToken = transactionBumperToken;
		this.comparer = comparer;
		CreateNewDictionaryAndReturnPrevious();
	}

	internal void SetAsStale()
	{
		isStale = true;
	}

	internal virtual void Add(TValue item)
	{
		lock (lockObject)
		{
			item.SetParentDictionary(this);
			cache.Add(item.Key, item);
		}
		lruCache.Add(item);
	}

	public bool Remove(TKey key)
	{
		TValue value;
		lock (lockObject)
		{
			if (!cache.TryGetValue(key, out value))
			{
				return false;
			}
		}
		return Remove(value);
	}

	internal override bool Remove(DictionaryCacheItem<TKey> item)
	{
		lruCache.Remove(item);
		lock (lockObject)
		{
			item.ClearParentDictionary();
			return cache.Remove(item.Key);
		}
	}

	internal virtual void AddOrReplace(TValue item)
	{
		while (true)
		{
			TValue value;
			lock (lockObject)
			{
				if (!cache.TryGetValue(item.Key, out value))
				{
					item.SetParentDictionary(this);
					cache.Add(item.Key, item);
					break;
				}
			}
			Remove(value);
		}
		lruCache.Add(item);
	}

	internal void AddOrReplace(TValue item, Func<TValue, TValue> promoteAction)
	{
		if (item == null)
		{
			return;
		}
		if (promoteAction == null)
		{
			AddOrReplace(item);
			return;
		}
		TValue val = promoteAction(item);
		if (val != null)
		{
			AddOrReplace(val);
		}
	}

	internal virtual bool TryGetValue(TKey key, DateTime minimumRequiredCacheTime, out TValue result)
	{
		lock (lockObject)
		{
			if (!cache.TryGetValue(key, out result))
			{
				return false;
			}
			if (minimumRequiredCacheTime > result.Created)
			{
				return false;
			}
		}
		if (result.ShouldItemBeKeptInCache(key))
		{
			if (result.DoesCacheItemSatisfyCacheRequest(key))
			{
				lruCache.MoveToFront(result);
				return true;
			}
		}
		else
		{
			Remove(result);
			result = null;
		}
		return false;
	}

	internal bool GetValueOrCreateWhileHoldingLock(TKey key, out TValue result)
	{
		lock (lockObject)
		{
			if (cache.TryGetValue(key, out result))
			{
				return true;
			}
			result = CreateItem(key);
			result.SetParentDictionary(this);
			cache.Add(key, result);
		}
		lruCache.Add(result);
		return false;
	}

	protected internal virtual TValue CreateItem(TKey key)
	{
		throw new NotSupportedException();
	}

	internal bool TryGetValueWhileKeepingLRUPosition(TKey key, out TValue result)
	{
		lock (lockObject)
		{
			return cache.TryGetValue(key, out result);
		}
	}

	internal virtual void Clear()
	{
		ClearAndReturnPreviousInnerDictionary()?.Clear();
	}

	internal virtual void PromoteItemsFromOtherCacheIntoThisCache(DictionaryCache<TKey, TValue> otherCache, Func<TValue, TValue> promoteAction)
	{
		if (otherCache == null)
		{
			return;
		}
		Dictionary<TKey, TValue> dictionary = otherCache.ClearAndReturnPreviousInnerDictionary();
		if (dictionary == null || dictionary.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			TValue value = item.Value;
			AddOrReplace(value, promoteAction);
		}
	}

	internal Dictionary<TKey, TValue> ClearAndReturnPreviousInnerDictionary()
	{
		Dictionary<TKey, TValue> dictionary;
		lock (lockObject)
		{
			dictionary = CreateNewDictionaryAndReturnPrevious();
			if (dictionary.Count == 0)
			{
				return dictionary;
			}
			foreach (TValue value in dictionary.Values)
			{
				value?.ClearParentDictionary();
			}
		}
		lruCache.Remove(dictionary.Values);
		return dictionary;
	}

	private Dictionary<TKey, TValue> CreateNewDictionaryAndReturnPrevious()
	{
		lock (lockObject)
		{
			Dictionary<TKey, TValue> result = cache;
			cache = new Dictionary<TKey, TValue>(comparer);
			return result;
		}
	}
}
