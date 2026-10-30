using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSet : IDisposable
{
	internal const int NonGlobalTableVersionToken = 0;

	private static int maxBufferedResults = 1024;

	private readonly FindCacheRequest findRequest;

	private int overallTableVersionToken;

	private int tableVersionToken;

	private IEnumerator<ReadOnlyRecordBuffer> dataProviderEnumerator;

	private ReadOnlyRecordBuffer[] bufferedResults;

	private int indexFetched = -1;

	private int isEnumerated;

	private ResultSetCacheItem ownerCacheItem;

	private bool containsCurrentKeyOnlyRecords;

	public static int MaxBufferedResults
	{
		get
		{
			return maxBufferedResults;
		}
		set
		{
			maxBufferedResults = value;
		}
	}

	public FindCacheRequest FindRequest
	{
		[DebuggerStepThrough]
		get
		{
			return findRequest;
		}
	}

	internal bool IsEnumerated
	{
		[DebuggerStepThrough]
		get
		{
			return isEnumerated != 0;
		}
	}

	internal int Count => indexFetched + 1;

	public bool IsCacheable
	{
		get
		{
			if (bufferedResults != null)
			{
				return !containsCurrentKeyOnlyRecords;
			}
			return false;
		}
	}

	internal int OverallTableVersionToken
	{
		get
		{
			return overallTableVersionToken;
		}
		set
		{
			overallTableVersionToken = value;
		}
	}

	internal int TableVersionBumberToken
	{
		get
		{
			return tableVersionToken;
		}
		set
		{
			tableVersionToken = value;
		}
	}

	internal ResultSetCacheItem OwnerCacheItem
	{
		get
		{
			return ownerCacheItem;
		}
		set
		{
			ownerCacheItem = value;
		}
	}

	internal bool CanPromote
	{
		get
		{
			if (ownerCacheItem != null)
			{
				return ownerCacheItem.GroupParent.Parent.TableState.GlobalTableState.GlobalTableVersionToken == tableVersionToken;
			}
			return false;
		}
	}

	public ResultSet(IEnumerator<ReadOnlyRecordBuffer> dataProviderEnumerator, FindCacheRequest findRequest, int overallTableVersionToken, int tableVersionToken, bool bufferResults)
	{
		this.dataProviderEnumerator = dataProviderEnumerator;
		this.findRequest = findRequest;
		this.overallTableVersionToken = overallTableVersionToken;
		this.tableVersionToken = tableVersionToken;
		if (bufferResults)
		{
			bufferedResults = new ReadOnlyRecordBuffer[8];
		}
	}

	public void InvalidateDataProviderEnumerator()
	{
		dataProviderEnumerator = null;
	}

	private void MarkResultSetAsEnumerated()
	{
		if (IsEnumerated || Interlocked.Exchange(ref isEnumerated, 1) != 0)
		{
			return;
		}
		InvalidateDataProviderEnumerator();
		if (ownerCacheItem == null)
		{
			return;
		}
		if (CanPromote)
		{
			ownerCacheItem.ResultSetEnumerated();
			return;
		}
		ResultSetGroupCache parent = ownerCacheItem.GroupParent.Parent;
		if (parent.TableState.IsNonTransactionalNonGlobal)
		{
			ownerCacheItem.GroupParent.ResultSetCache.Remove(ownerCacheItem);
			ownerCacheItem = null;
		}
	}

	internal bool GlobalTableVersionTokenChanged(DataCacheDatabaseState databaseState)
	{
		int overallGlobalTableVersionToken = databaseState.OverallGlobalTableVersionToken;
		if (overallGlobalTableVersionToken == overallTableVersionToken)
		{
			return false;
		}
		DataCacheTableState globalTableState = databaseState.GetGlobalTableState(findRequest.CompanyToken, findRequest.MetaApplicationObject.ObjectId);
		if (globalTableState.GlobalTableVersionToken == tableVersionToken)
		{
			overallTableVersionToken = overallGlobalTableVersionToken;
			return false;
		}
		return true;
	}

	public ResultSetMoveResult TryGetAtIndex(int index, out ReadOnlyRecordBuffer recordBuffer)
	{
		recordBuffer = null;
		if (index > 0 && findRequest.FindType == FindType.FirstOnly)
		{
			return ResultSetMoveResult.ResultSetNoLongerValid;
		}
		ReadOnlyRecordBuffer[] array = bufferedResults;
		if (array == null)
		{
			return TryGetAtIndexWithoutBuffer(index, out recordBuffer);
		}
		while (index > indexFetched)
		{
			if (IsEnumerated)
			{
				return ResultSetMoveResult.NoMoreResults;
			}
			if (dataProviderEnumerator == null)
			{
				return ResultSetMoveResult.ResultSetNoLongerValid;
			}
			array = bufferedResults;
			if (index <= indexFetched)
			{
				continue;
			}
			if (array == null)
			{
				return ResultSetMoveResult.ResultSetNoLongerValid;
			}
			if (!dataProviderEnumerator.MoveNext())
			{
				MarkResultSetAsEnumerated();
				return ResultSetMoveResult.NoMoreResults;
			}
			IncrementResultSetCount(1);
			int num = indexFetched + 1;
			ReadOnlyRecordBuffer current = dataProviderEnumerator.Current;
			if (num >= array.Length)
			{
				if (num >= MaxBufferedResults)
				{
					recordBuffer = current;
					indexFetched = num;
					bufferedResults = null;
					IncrementResultSetCount(-array.Length);
					return ResultSetMoveResult.Ok;
				}
				if (current.OnlyCurrentKey)
				{
					containsCurrentKeyOnlyRecords = true;
				}
				ReadOnlyRecordBuffer[] array2 = new ReadOnlyRecordBuffer[array.Length * 2];
				Array.Copy(array, array2, array.Length);
				ReadOnlyRecordBuffer[] array3 = Interlocked.CompareExchange(ref bufferedResults, array2, array);
				if (array3 == null)
				{
					return ResultSetMoveResult.ResultSetNoLongerValid;
				}
				array = array2;
			}
			array[num] = current;
			indexFetched = num;
		}
		recordBuffer = array[index];
		if (recordBuffer == null)
		{
			return ResultSetMoveResult.ResultSetNoLongerValid;
		}
		if (findRequest.FindType == FindType.FirstOnly)
		{
			MarkResultSetAsEnumerated();
		}
		return ResultSetMoveResult.Ok;
	}

	private void IncrementResultSetCount(int incrementAmount)
	{
		if (ownerCacheItem != null)
		{
			ownerCacheItem.IncrementResultSetCount(incrementAmount);
		}
	}

	private ResultSetMoveResult TryGetAtIndexWithoutBuffer(int index, out ReadOnlyRecordBuffer recordBuffer)
	{
		recordBuffer = null;
		while (index > indexFetched)
		{
			if (IsEnumerated)
			{
				return ResultSetMoveResult.NoMoreResults;
			}
			if (dataProviderEnumerator == null)
			{
				return ResultSetMoveResult.ResultSetNoLongerValid;
			}
			if (index <= indexFetched)
			{
				continue;
			}
			if (!dataProviderEnumerator.MoveNext())
			{
				MarkResultSetAsEnumerated();
				return ResultSetMoveResult.NoMoreResults;
			}
			indexFetched++;
			if (indexFetched == index)
			{
				recordBuffer = dataProviderEnumerator.Current;
				if (findRequest.FindType == FindType.FirstOnly)
				{
					MarkResultSetAsEnumerated();
				}
				return ResultSetMoveResult.Ok;
			}
		}
		return ResultSetMoveResult.ResultSetNoLongerValid;
	}

	public bool TryUpdateAtIndex(int index, ReadOnlyRecordBuffer recordBuffer)
	{
		if (bufferedResults != null && index < bufferedResults.Length)
		{
			bufferedResults[index] = recordBuffer;
			return true;
		}
		return false;
	}

	public bool IsResultDeleted(int index)
	{
		if (bufferedResults != null && index < bufferedResults.Length)
		{
			return bufferedResults[index] == null;
		}
		return false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			bufferedResults = null;
		}
	}
}
