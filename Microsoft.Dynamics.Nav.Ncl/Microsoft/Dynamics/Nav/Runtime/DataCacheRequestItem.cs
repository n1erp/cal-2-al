using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DataCacheRequestItem : DictionaryCacheItem<DataCacheRequest>
{
	private ResultSetGroupCacheItem groupParent;

	internal ResultSetGroupCacheItem GroupParent => groupParent;

	internal override DataCacheRequest Key { get; }

	internal DataCacheRequestItem(ResultSetGroupCacheItem groupParentParameter, DataCacheRequest dataCacheRequestParameter, DateTime created, int sizeInBytes)
		: this(groupParentParameter, ResultSetGroupCacheItem.GetNavDatabase(groupParentParameter), dataCacheRequestParameter, created, sizeInBytes)
	{
	}

	private DataCacheRequestItem(ResultSetGroupCacheItem groupParentParameter, NavDatabase originDatabase, DataCacheRequest dataCacheRequestParameter, DateTime created, int sizeInBytes)
		: base(originDatabase, created, sizeInBytes)
	{
		if (originDatabase == null)
		{
			throw new ArgumentNullException("originDatabase");
		}
		groupParent = groupParentParameter ?? throw new ArgumentNullException("groupParentParameter");
		Key = dataCacheRequestParameter ?? throw new ArgumentNullException("dataCacheRequestParameter");
	}

	internal override bool ShouldItemBeKeptInCache(DataCacheRequest key)
	{
		return !key.MetaApplicationObject.Stale;
	}

	internal void PromoteToGroupCacheItem(ResultSetGroupCacheItem newParent)
	{
		groupParent = newParent ?? throw new ArgumentNullException("newParent");
	}
}
