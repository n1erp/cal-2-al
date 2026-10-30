using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DictionaryCacheItem<TKey> : LRUCacheItem
{
	private DictionaryCacheBase<TKey> parentDictionary;

	private readonly DateTime created;

	internal abstract TKey Key { get; }

	internal DateTime Created => created;

	protected DictionaryCacheBase<TKey> ParentDictionary => parentDictionary;

	protected DictionaryCacheItem(int size)
		: this((NavDatabase)null, size)
	{
	}

	protected DictionaryCacheItem(NavDatabase originDatabase, int size)
		: this(originDatabase, DateTime.UtcNow, size)
	{
	}

	protected DictionaryCacheItem(NavDatabase originDatabase, DateTime created, int size)
		: base(originDatabase, size)
	{
		this.created = created;
	}

	internal void SetParentDictionary(DictionaryCacheBase<TKey> newParentDictionary)
	{
		parentDictionary = newParentDictionary;
	}

	internal void ClearParentDictionary()
	{
		parentDictionary = null;
	}

	internal virtual bool ShouldItemBeKeptInCache(TKey key)
	{
		return true;
	}

	internal virtual bool DoesCacheItemSatisfyCacheRequest(TKey key)
	{
		return true;
	}

	internal sealed override void ItemHasBeenRemovedByScavenging()
	{
		parentDictionary?.Remove(this);
	}
}
