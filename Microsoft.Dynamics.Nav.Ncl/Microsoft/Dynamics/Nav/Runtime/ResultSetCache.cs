using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSetCache : DictionaryCache<DataCacheRequest, DataCacheRequestItem>
{
	private readonly ResultSetGroupCacheItem groupParent;

	public int OverallGlobalTableVersionToken => groupParent.Parent.TableState.GlobalTableState.DatabaseState.OverallGlobalTableVersionToken;

	public int GlobalTableVersionToken => groupParent.Parent.TableState.GlobalTableState.GlobalTableVersionToken;

	public ResultSetGroupCacheItem GroupParent => groupParent;

	public ResultSetCache(ResultSetGroupCacheItem groupParent, LRUCache lruCache, int transactionBumperToken)
		: base(lruCache, groupParent.OriginDatabase, transactionBumperToken)
	{
		this.groupParent = groupParent;
	}

	internal override void Add(DataCacheRequestItem item)
	{
		if (!groupParent.TryAddExistsCount(item))
		{
			base.Add(item);
		}
	}

	internal override void AddOrReplace(DataCacheRequestItem item)
	{
		if (!groupParent.TryAddExistsCount(item))
		{
			base.AddOrReplace(item);
		}
	}

	internal override bool TryGetValue(DataCacheRequest key, DateTime minimumRequiredCacheTime, out DataCacheRequestItem result)
	{
		if (groupParent.TryGetExistsCount(key, minimumRequiredCacheTime, out result))
		{
			return result != null;
		}
		return base.TryGetValue(key, minimumRequiredCacheTime, out result);
	}

	internal override void Clear()
	{
		groupParent.ClearExistsCount();
		base.Clear();
	}
}
