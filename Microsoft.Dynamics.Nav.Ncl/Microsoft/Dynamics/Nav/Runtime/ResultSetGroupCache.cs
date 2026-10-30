using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSetGroupCache : DictionaryCache<DataCacheRequest, ResultSetGroupCacheItem>
{
	private readonly DataCacheDatabaseState databaseState;

	private readonly DataCacheTableState tableState;

	private static int globalCachePromotionsCount;

	private static int globalCachePromotionsRejected;

	private static int nonTransactionalCachePromotionsCount;

	internal DataCacheDatabaseState DatabaseState => databaseState;

	internal DataCacheTableState TableState => tableState;

	internal ResultSetGroupCache(DataCacheDatabaseState databaseState, int transactionBumperToken, DataCacheTableState tableState)
		: base(databaseState.LRUCache, databaseState.Database, transactionBumperToken, (IEqualityComparer<DataCacheRequest>)DataCacheRequest.NonInheritedEqualityComparer.Instance)
	{
		this.databaseState = databaseState;
		this.tableState = tableState;
	}

	internal override void Clear()
	{
		Dictionary<DataCacheRequest, ResultSetGroupCacheItem> dictionary = ClearAndReturnPreviousInnerDictionary();
		if (dictionary == null || dictionary.Count == 0)
		{
			return;
		}
		foreach (ResultSetGroupCacheItem value in dictionary.Values)
		{
			value?.ResultSetCache.Clear();
		}
		dictionary.Clear();
	}

	public void Clear(MutableRecordBuffer recordBufferCausingWrite)
	{
		if (recordBufferCausingWrite == null || Cache.Count > 5)
		{
			Clear();
			return;
		}
		List<ResultSetGroupCacheItem> list = null;
		int count;
		lock (LockObject)
		{
			count = Cache.Count;
			foreach (ResultSetGroupCacheItem value in Cache.Values)
			{
				if (recordBufferCausingWrite.PassesFieldFiltersAndMarks(value.Key.FiltersAndMarks, includeFlowFields: false, checkAgainstOriginalAndModified: true))
				{
					(list ?? (list = new List<ResultSetGroupCacheItem>(count))).Add(value);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		if (list.Count == count)
		{
			Clear();
			return;
		}
		foreach (ResultSetGroupCacheItem item in list)
		{
			Remove(item);
		}
	}

	internal ResultSetGroupCachPromotionResult TryPromoteResultSetsFromPrivateCacheToNonTransactionalOrGlobalCache(ResultSetGroupCache nonTransactionalResultSetGroupCache, Dictionary<int, int> changedTablesVersions, int overallGlobalTableVersionToken)
	{
		if (!ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value)
		{
			return ResultSetGroupCachPromotionResult.Disabled;
		}
		Dictionary<DataCacheRequest, ResultSetGroupCacheItem> dictionary = ClearAndReturnPreviousInnerDictionary();
		if (dictionary != null && dictionary.Count != 0)
		{
			ResultSetGroupCache resultSetGroupCache = tableState.GlobalTableState.ResultSetGroupCache;
			foreach (KeyValuePair<DataCacheRequest, ResultSetGroupCacheItem> item in dictionary)
			{
				ResultSetGroupCacheItem value = item.Value;
				value.PromoteExistsCountIntoGivenCache(resultSetGroupCache);
				Dictionary<DataCacheRequest, DataCacheRequestItem> dictionary2 = value.ResultSetCache.ClearAndReturnPreviousInnerDictionary();
				foreach (KeyValuePair<DataCacheRequest, DataCacheRequestItem> item2 in dictionary2)
				{
					DataCacheRequestItem value2 = item2.Value;
					if (!(value2 is ResultSetCacheItem resultSetCacheItem))
					{
						PromoteResultSetGroupCacheItemAndResultSetCacheItemToOtherCache(resultSetGroupCache, value, value2);
						continue;
					}
					if (resultSetCacheItem.ResultSet.IsEnumerated)
					{
						PromoteResultSetGroupCacheItemAndResultSetCacheItemToOtherCache(resultSetGroupCache, value, value2);
						continue;
					}
					resultSetCacheItem.ResultSet.InvalidateDataProviderEnumerator();
					if (changedTablesVersions.TryGetValue(resultSetCacheItem.ResultSet.FindRequest.MetaApplicationObject.ObjectId, out var value3))
					{
						resultSetCacheItem.ResultSet.OverallTableVersionToken = overallGlobalTableVersionToken;
						resultSetCacheItem.ResultSet.TableVersionBumberToken = value3;
					}
					if (resultSetCacheItem.ResultSet.CanPromote)
					{
						PromoteResultSetGroupCacheItemAndResultSetCacheItemToOtherCache(nonTransactionalResultSetGroupCache, value, value2);
					}
					else
					{
						globalCachePromotionsRejected++;
					}
				}
			}
		}
		return ResultSetGroupCachPromotionResult.Successful;
	}

	internal void PromoteItemFromNonTransactionalCacheToGlobalCache(ResultSetGroupCacheItem resultSetGroupCacheItem, ResultSetCacheItem resultSetCacheItem)
	{
		if (tableState.IsNonTransactionalNonGlobal && ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value)
		{
			resultSetGroupCacheItem.ResultSetCache.Remove(resultSetCacheItem);
			ResultSetGroupCache resultSetGroupCache = tableState.GlobalTableState.ResultSetGroupCache;
			PromoteResultSetGroupCacheItemAndResultSetCacheItemToOtherCache(resultSetGroupCache, resultSetGroupCacheItem, resultSetCacheItem);
		}
	}

	protected internal override ResultSetGroupCacheItem CreateItem(DataCacheRequest key)
	{
		return new ResultSetGroupCacheItem(this, key);
	}

	[Conditional("Debug")]
	private static void ResultSetGroupCacheItemMustContainResultSetCacheItem(ResultSetGroupCacheItem resultSetGroupCacheItem, ResultSetCacheItem resultSetCacheItem)
	{
		resultSetGroupCacheItem.ResultSetCache.TryGetValueWhileKeepingLRUPosition(resultSetCacheItem.Key, out var _);
	}

	private static void PromoteResultSetGroupCacheItemAndResultSetCacheItemToOtherCache(ResultSetGroupCache otherCache, ResultSetGroupCacheItem resultSetGroupCacheItem, DataCacheRequestItem resultSetCacheItem)
	{
		if (!otherCache.GetValueOrCreateWhileHoldingLock(resultSetCacheItem.Key, out var result) || result != resultSetGroupCacheItem)
		{
			if (otherCache.TableState.IsGlobal)
			{
				globalCachePromotionsCount++;
			}
			else
			{
				nonTransactionalCachePromotionsCount++;
			}
			result.ResultSetCache.AddOrReplace(resultSetCacheItem);
			if (!FeatureSwitches.Instance.DisableCacheMemoryFix)
			{
				resultSetCacheItem.PromoteToGroupCacheItem(result);
			}
		}
	}
}
