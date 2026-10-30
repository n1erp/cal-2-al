using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSetGroupCacheItem : DictionaryCacheItem<DataCacheRequest>
{
	private struct ExistsCountInfo(DateTime createdDateTime, int data)
	{
		private static readonly DateTime unspecifiedDateTime;

		private readonly int data = data;

		private readonly DateTime createdDateTime = createdDateTime;

		public bool IsKnown
		{
			[DebuggerStepThrough]
			get
			{
				return createdDateTime != unspecifiedDateTime;
			}
		}

		public DateTime CreatedDateTime
		{
			[DebuggerStepThrough]
			get
			{
				return createdDateTime;
			}
		}

		public int Data
		{
			[DebuggerStepThrough]
			get
			{
				return data;
			}
		}
	}

	private ResultSetGroupCache parent;

	private DataCacheRequest request;

	private ResultSetCache resultSetCache;

	private ExistsCountInfo existsInfo;

	private ExistsCountInfo countInfo;

	internal override DataCacheRequest Key => request;

	internal ResultSetCache ResultSetCache
	{
		get
		{
			if (resultSetCache == null)
			{
				resultSetCache = new ResultSetCache(this, parent.LRUCache, parent.TransactionBumperToken);
			}
			return resultSetCache;
		}
	}

	internal ResultSetGroupCache Parent => parent;

	internal ResultSetGroupCacheItem(ResultSetGroupCache parent, DataCacheRequest request)
		: base(parent.OriginDatabase, ApproximateByteSize(request))
	{
		if (parent.OriginDatabase == null)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.ValueNullOrEmpty, "parent.OriginDatabase"));
		}
		this.parent = parent;
		this.request = request;
	}

	internal unsafe static int ApproximateByteSize(DataCacheRequest request)
	{
		return request.ApproximateByteSize + 2 * sizeof(ExistsCountInfo) + SizableObject.GetApproximateByteSize(8) + 24 + SizableObject.GetApproximateByteSize(7) + 24;
	}

	internal static NavDatabase GetNavDatabase(ResultSetGroupCacheItem self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		return self.OriginDatabase;
	}

	internal void SetResultExist(bool exist, DateTime createdDateTime)
	{
		SetCountAndExistsInternal(Convert.ToInt32(exist), -1, createdDateTime);
	}

	internal void SetResultCount(int count, DateTime createdDateTime)
	{
		SetCountAndExistsInternal(Convert.ToInt32(count > 0), count, createdDateTime);
	}

	internal void ResultSetEnumerated(ResultSetCacheItem resultSetCacheItem)
	{
		SetExistsCount(resultSetCacheItem);
		Parent.PromoteItemFromNonTransactionalCacheToGlobalCache(this, resultSetCacheItem);
	}

	internal bool TryGetExistsCount(DataCacheRequest key, DateTime minimumRequiredCacheTime, out DataCacheRequestItem result)
	{
		if (key is ExistsCacheRequest existsCacheRequest)
		{
			if (existsInfo.IsKnown && existsInfo.CreatedDateTime >= minimumRequiredCacheTime)
			{
				result = new ExistsCacheItem(this, existsCacheRequest, Convert.ToBoolean(existsInfo.Data), existsInfo.CreatedDateTime);
				MoveToFrontOfLRU();
			}
			else
			{
				result = null;
			}
			return true;
		}
		if (key is CountCacheRequest countCacheRequest)
		{
			if (countInfo.IsKnown && countInfo.CreatedDateTime >= minimumRequiredCacheTime)
			{
				result = new CountCacheItem(this, countCacheRequest, countInfo.Data, countInfo.CreatedDateTime);
				MoveToFrontOfLRU();
			}
			else
			{
				result = null;
			}
			return true;
		}
		result = null;
		return false;
	}

	internal bool TryAddExistsCount(DataCacheRequestItem item)
	{
		if (item is ExistsCacheItem existsCacheItem)
		{
			SetResultExist(existsCacheItem.Exists, existsCacheItem.Created);
			MoveToFrontOfLRU();
			return true;
		}
		if (item is CountCacheItem countCacheItem)
		{
			SetResultCount(countCacheItem.Count, countCacheItem.Created);
			MoveToFrontOfLRU();
			return true;
		}
		return false;
	}

	internal void PromoteExistsCountIntoGivenCache(ResultSetGroupCache globalResultSetGroupCache)
	{
		if (existsInfo.IsKnown || countInfo.IsKnown)
		{
			globalResultSetGroupCache.GetValueOrCreateWhileHoldingLock(Key, out var result);
			if (existsInfo.IsKnown)
			{
				result.SetResultExist(Convert.ToBoolean(existsInfo.Data), existsInfo.CreatedDateTime);
			}
			if (countInfo.IsKnown)
			{
				result.SetResultCount(countInfo.Data, countInfo.CreatedDateTime);
			}
		}
	}

	internal void SetExistsCount(ResultSetCacheItem resultSetCacheItem)
	{
		FindCacheRequest findRequest = resultSetCacheItem.ResultSet.FindRequest;
		if (findRequest.FindType != FindType.FirstOnly && findRequest.TopNumberOfRowsToReturn == 0 && !findRequest.IsFromPosition && resultSetCacheItem.ResultSet.IsEnumerated)
		{
			SetResultCount(resultSetCacheItem.ResultSet.Count, resultSetCacheItem.Created);
		}
		else if (resultSetCacheItem.ResultSet.Count > 0)
		{
			SetResultExist(exist: true, resultSetCacheItem.Created);
		}
	}

	public void ClearExistsCount()
	{
		existsInfo = default;
		countInfo = default;
	}

	private void SetCountAndExistsInternal(int exists, int count, DateTime createdDateTime)
	{
		bool flag = UpdateExistsCountInfo(ref existsInfo, exists, createdDateTime);
		if ((flag | UpdateExistsCountInfo(ref countInfo, count, createdDateTime)) && Parent.TableState.IsNonTransactionalNonGlobal && ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value)
		{
			PromoteExistsCountIntoGivenCache(Parent.TableState.GlobalTableState.ResultSetGroupCache);
		}
	}

	private static bool UpdateExistsCountInfo(ref ExistsCountInfo existsCountInfo, int data, DateTime createdDateTime)
	{
		if (data >= 0 && (!existsCountInfo.IsKnown || createdDateTime > existsCountInfo.CreatedDateTime))
		{
			existsCountInfo = new ExistsCountInfo(createdDateTime, data);
			return true;
		}
		return false;
	}

	private void MoveToFrontOfLRU()
	{
		LRUCache?.MoveToFront(this);
	}
}
