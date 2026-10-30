using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSetCacheItem : DataCacheRequestItem
{
	private const int LRUCacheSizeUpdateFrequency = 10;

	private readonly int sizeIncrement;

	private ResultSet resultSet;

	private int resultSetCount;

	internal ResultSet ResultSet => resultSet;

	internal ResultSetCacheItem(ResultSetGroupCacheItem groupParent, ResultSet resultSet, DateTime created)
		: base(groupParent, resultSet.FindRequest, created, StartingDataSize(resultSet))
	{
		if (resultSet == null)
		{
			throw new ArgumentNullException("resultSet");
		}
		sizeIncrement = (int)Size * 2;
		this.resultSet = resultSet;
		this.resultSet.OwnerCacheItem = this;
	}

	private static int StartingDataSize(ResultSet resultSet)
	{
		int approximateRowByteSize = ((NCLMetaTable)resultSet.FindRequest.MetaApplicationObject).ApproximateRowByteSize;
		if (resultSet.FindRequest.FindType != FindType.FirstOnly)
		{
			return approximateRowByteSize * 5;
		}
		return approximateRowByteSize;
	}

	internal override bool ShouldItemBeKeptInCache(DataCacheRequest key)
	{
		if (resultSet.IsCacheable && (!GroupParent.Parent.TableState.IsNonTransactionalNonGlobal || !resultSet.GlobalTableVersionTokenChanged(GroupParent.Parent.TableState.DatabaseState)))
		{
			return !resultSet.FindRequest.MetaApplicationObject.Stale;
		}
		return false;
	}

	internal override bool DoesCacheItemSatisfyCacheRequest(DataCacheRequest key)
	{
		if (resultSet.FindRequest.FindType == FindType.FirstOnly)
		{
			return ((FindCacheRequest)key).FindType == FindType.FirstOnly;
		}
		return true;
	}

	internal void IncrementResultSetCount(int incrementAmount)
	{
		GroupParent.SetResultExist(exist: true, Created);
		resultSetCount += incrementAmount;
		if (IsLinked && (incrementAmount > 10 || resultSetCount % 10 == 0))
		{
			LRUCache?.AddSize(this, sizeIncrement);
		}
	}

	internal void ResultSetEnumerated()
	{
		GroupParent.ResultSetEnumerated(this);
	}
}
