using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataCacheSessionState
{
	private class GlobalCacheAccessWrapper<T>
	{
		private readonly int companyId;

		private readonly Func<int, int, T> accessDelegate;

		private int lastGetGlobalCacheId = -1;

		private T lastGetGlobalCacheResult;

		internal GlobalCacheAccessWrapper(int companyId, Func<int, int, T> access)
		{
			this.companyId = companyId;
			accessDelegate = access;
		}

		internal void ResetLastKnownGlobalState()
		{
			lastGetGlobalCacheId = -1;
			lastGetGlobalCacheResult = default;
		}

		internal T GetGlobalCache(int tableId)
		{
			if (tableId == lastGetGlobalCacheId)
			{
				return lastGetGlobalCacheResult;
			}
			lastGetGlobalCacheId = tableId;
			lastGetGlobalCacheResult = accessDelegate(companyId, tableId);
			return lastGetGlobalCacheResult;
		}
	}

	private class TableStateWrapper
	{
		private readonly DataCacheDatabaseState databaseState;

		private readonly int companyToken;

		private readonly IDictionary<int, DataCacheTableState> tableStates;

		private readonly bool isPrivate;

		internal IDictionary<int, DataCacheTableState> TableStates => tableStates;

		internal TableStateWrapper(DataCacheDatabaseState databaseState, int companyToken)
			: this(databaseState, companyToken, isPrivate: false)
		{
		}

		internal TableStateWrapper(DataCacheDatabaseState databaseState, int companyToken, bool isPrivate)
		{
			this.databaseState = databaseState;
			this.companyToken = companyToken;
			this.isPrivate = isPrivate;
			if (this.isPrivate)
			{
				tableStates = new Dictionary<int, DataCacheTableState>(13);
			}
			else
			{
				tableStates = new ConcurrentDictionary<int, DataCacheTableState>();
			}
		}

		internal virtual DataCacheTableState CreateDataAccessTableState(int tableId)
		{
			DataCacheTableState dataCacheTableState = new DataCacheTableState(databaseState, companyToken, tableId, isGlobal: false);
			if (isPrivate)
			{
				tableStates.Add(tableId, dataCacheTableState);
				return dataCacheTableState;
			}
			return ((ConcurrentDictionary<int, DataCacheTableState>)tableStates).GetOrAdd(tableId, dataCacheTableState);
		}

		internal virtual DataCacheTableState GetTableState(int tableId)
		{
			tableStates.TryGetValue(tableId, out var value);
			return value;
		}

		internal virtual DataCacheTableState GetOrCreateTableState(int tableId)
		{
			DataCacheTableState tableState = GetTableState(tableId);
			if (tableState != null)
			{
				return tableState;
			}
			return CreateDataAccessTableState(tableId);
		}

		internal virtual void Clear()
		{
			foreach (KeyValuePair<int, DataCacheTableState> tableState in tableStates)
			{
				tableState.Value.Clear();
			}
			tableStates.Clear();
		}
	}

	private class PrivateTableStateWrapper : TableStateWrapper
	{
		private int lastGetTableStateId = -1;

		private DataCacheTableState lastGetTableStateResult;

		internal PrivateTableStateWrapper(DataCacheDatabaseState databaseState, int companyToken)
			: base(databaseState, companyToken, isPrivate: true)
		{
		}

		internal void ResetLastKnownState()
		{
			lastGetTableStateId = -1;
			lastGetTableStateResult = null;
		}

		internal override DataCacheTableState CreateDataAccessTableState(int tableId)
		{
			DataCacheTableState result = base.CreateDataAccessTableState(tableId);
			lastGetTableStateId = tableId;
			lastGetTableStateResult = result;
			return result;
		}

		internal override DataCacheTableState GetTableState(int tableId)
		{
			if (tableId != lastGetTableStateId)
			{
				lastGetTableStateId = tableId;
				TableStates.TryGetValue(tableId, out lastGetTableStateResult);
			}
			return lastGetTableStateResult;
		}

		internal override void Clear()
		{
			base.Clear();
			ResetLastKnownState();
		}
	}

	private const int ThresholdForFlushTableCache = 1000;

	private readonly DataCacheDatabaseState databaseState;

	private readonly int companyToken;

	private readonly bool isReadOnly;

	private readonly HashSet<int> changedTables = new HashSet<int>();

	private readonly Dictionary<int, HashSet<NavRecordId>> primaryKeyItemsToRemoveFromGlobalCacheOnCommit = new Dictionary<int, HashSet<NavRecordId>>();

	private readonly TableStateWrapper privateTableStates;

	private readonly TableStateWrapper nonTransactionalTableStates;

	private readonly GlobalCacheAccessWrapper<PrimaryKeyCache> globalPrimaryKeyCacheAccess;

	private readonly GlobalCacheAccessWrapper<ResultSetGroupCache> globalResultSetGroupCacheAccess;

	private readonly ResultSetGroupCache privateQueryGroupCache;

	private int lockedTablesBumperToken;

	private int lockedTablesCount;

	private int sessionTransactionBumperToken;

	internal DataCacheDatabaseState DatabaseState => databaseState;

	internal int CompanyToken => companyToken;

	internal int LockedTableCount => lockedTablesCount;

	internal int LockedTablesBumperToken => lockedTablesBumperToken;

	internal ResultSetGroupCache PrivateQueryGroupCache => privateQueryGroupCache;

	internal int SessionTransactionBumperToken => sessionTransactionBumperToken;

	internal DataCacheSessionState(NavDatabase database, NavSession session, int companyToken, bool isReadOnly)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.companyToken = companyToken;
		this.isReadOnly = isReadOnly;
		databaseState = database.DatabaseState;
		privateTableStates = new PrivateTableStateWrapper(databaseState, this.companyToken);
		nonTransactionalTableStates = new TableStateWrapper(databaseState, this.companyToken);
		globalPrimaryKeyCacheAccess = new GlobalCacheAccessWrapper<PrimaryKeyCache>(this.companyToken, databaseState.GlobalPrimaryKeyCache);
		globalResultSetGroupCacheAccess = new GlobalCacheAccessWrapper<ResultSetGroupCache>(this.companyToken, databaseState.GlobalResultSetGroupCache);
		privateQueryGroupCache = new ResultSetGroupCache(databaseState, 0, null);
		lockedTablesBumperToken = 1;
	}

	internal DataLockState GetCurrentLockState(int tableId, TransactionType transactionType)
	{
		GetPrivateTableStateWithAssociatedLockState(tableId, transactionType, out var lockState);
		return lockState;
	}

	internal DataCacheTableState GetGlobalTableState(int tableId)
	{
		ResultSetGroupCache globalCache = globalResultSetGroupCacheAccess.GetGlobalCache(tableId);
		return globalCache.TableState.GlobalTableState;
	}

	internal DataCacheTableState GetPrivateTableState(int tableId)
	{
		return privateTableStates.GetTableState(tableId);
	}

	internal DataCacheTableState GetNonTransactionalTableState(int tableId)
	{
		return nonTransactionalTableStates.GetTableState(tableId);
	}

	internal DataCacheTableState GetOrCreatePrivateTableState(int tableId)
	{
		return privateTableStates.GetOrCreateTableState(tableId);
	}

	internal DataCacheTableState GetOrCreateNonTransactionalTableState(int tableId)
	{
		return nonTransactionalTableStates.GetOrCreateTableState(tableId);
	}

	internal ResultSetCacheFindResult TryGetResultSetCacheItemAndLockState<T>(DataCacheRequest request, TransactionType transactionType, DateTime minimumRequiredCacheTimestamp, out DataLockState lockState, out T resultSetCacheItem, out ResultSetCache resultSetCache) where T : DataCacheRequestItem
	{
		lockState = DataLockState.None;
		resultSetCacheItem = null;
		resultSetCache = null;
		if (request.MetaApplicationObject.ApplicationObjectType == ObjectType.Query)
		{
			return ResultSetCacheFindResult.NotFound;
		}
		int objectId = request.MetaApplicationObject.ObjectId;
		DataCacheTableState privateTableStateWithAssociatedLockState = GetPrivateTableStateWithAssociatedLockState(objectId, transactionType, out lockState);
		if (privateTableStateWithAssociatedLockState == null && ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value)
		{
			return TryGetResultSetCacheItemFromGlobalOrNonTransactionalCache<T>(request, minimumRequiredCacheTimestamp, out resultSetCacheItem, out resultSetCache);
		}
		if (privateTableStateWithAssociatedLockState == null)
		{
			privateTableStateWithAssociatedLockState = CreatePrivateDataAccessTableState(objectId);
			privateTableStateWithAssociatedLockState.ResultSetGroupCache.GetValueOrCreateWhileHoldingLock(request, out var result);
			resultSetCache = result.ResultSetCache;
			return ResultSetCacheFindResult.NotFound;
		}
		if (!TryGetResultSetCacheItem<T>(privateTableStateWithAssociatedLockState.ResultSetGroupCache, request, minimumRequiredCacheTimestamp, out resultSetCacheItem, out resultSetCache))
		{
			return ResultSetCacheFindResult.NotFound;
		}
		return ResultSetCacheFindResult.PrivateCache;
	}

	internal bool LockTable(int tableId, ref int sessionTransactionBumperTokenRef)
	{
		DataCacheTableState orCreatePrivateTableState = GetOrCreatePrivateTableState(tableId);
		DataLockState dataLockState = (isReadOnly ? DataLockState.Shared : DataLockState.LockExclusive);
		bool flag = orCreatePrivateTableState.LockState != dataLockState;
		if (flag)
		{
			lockedTablesCount++;
			if (lockedTablesCount == 1)
			{
				sessionTransactionBumperToken = ++sessionTransactionBumperTokenRef;
			}
			lockedTablesBumperToken++;
			orCreatePrivateTableState.SetLockStateAndTransactionToken(dataLockState, sessionTransactionBumperToken);
		}
		return flag;
	}

	internal void ClearNonGlobalCaches(bool clearNonTransactionalCache)
	{
		if (privateQueryGroupCache != null)
		{
			privateQueryGroupCache.Clear();
		}
		privateTableStates.Clear();
		if (clearNonTransactionalCache)
		{
			nonTransactionalTableStates.Clear();
		}
	}

	internal void ClearNonGlobalCachesByTableId(int tableId, CacheClear cacheClear, MutableRecordBuffer recordBufferCausingWrite = null)
	{
		DataCacheTableState privateTableState = GetPrivateTableState(tableId);
		if (privateTableState != null && !privateTableState.IsGlobal)
		{
			privateTableState.Clear(cacheClear, recordBufferCausingWrite);
		}
		DataCacheTableState nonTransactionalTableState = GetNonTransactionalTableState(tableId);
		if (nonTransactionalTableState != null && !nonTransactionalTableState.IsGlobal)
		{
			nonTransactionalTableState.Clear(cacheClear, recordBufferCausingWrite);
		}
	}

	internal void RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(int tableId, DictionaryCacheItem<NavRecordId> item)
	{
		RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(tableId, item.Key);
	}

	internal void RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(int tableId, NavRecordId id)
	{
		DataCacheTableState privateTableState = GetPrivateTableState(tableId);
		if (privateTableState != null && !privateTableState.IsGlobal)
		{
			privateTableState.PrimaryKeyCache.Remove(id);
		}
		if (!primaryKeyItemsToRemoveFromGlobalCacheOnCommit.TryGetValue(tableId, out var value))
		{
			value = new HashSet<NavRecordId>();
			primaryKeyItemsToRemoveFromGlobalCacheOnCommit[tableId] = value;
		}
		if (value.Count < 1000)
		{
			value.Add(id);
		}
	}

	internal void EnsureTableWillBeClearedInGlobalCacheOnCommit(int tableId, CacheClear cacheClear)
	{
		DataCacheTableState privateTableState = GetPrivateTableState(tableId);
		if (privateTableState == null || privateTableState.LockState != DataLockState.LockExclusive)
		{
			throw new InvalidOperationException();
		}
		privateTableState.ClearGlobalCacheOnCommit |= cacheClear;
	}

	internal void EnsureGlobalTableStateWillBeUpdatedOnCommmit(int tableId)
	{
		changedTables.Add(tableId);
	}

	internal PrimaryKeyCache GetPrimaryKeyCache(int tableId, TransactionType transactionType, out DataLockState lockState, out bool isGlobal)
	{
		DataCacheTableState privateTableStateWithAssociatedLockState = GetPrivateTableStateWithAssociatedLockState(tableId, transactionType, out lockState);
		if (privateTableStateWithAssociatedLockState == null)
		{
			isGlobal = false;
			return globalPrimaryKeyCacheAccess.GetGlobalCache(tableId);
		}
		isGlobal = lockState == DataLockState.None;
		if (!isGlobal)
		{
			return privateTableStateWithAssociatedLockState.PrimaryKeyCache;
		}
		return privateTableStateWithAssociatedLockState.GlobalTableState.PrimaryKeyCache;
	}

	internal void InternalEndTransaction(bool commit)
	{
		try
		{
			if (!commit)
			{
				return;
			}
			foreach (KeyValuePair<int, HashSet<NavRecordId>> item in primaryKeyItemsToRemoveFromGlobalCacheOnCommit)
			{
				DataCacheTableState globalTableState = GetGlobalTableState(item.Key);
				if (globalTableState == null)
				{
					continue;
				}
				if (item.Value.Count == 1000)
				{
					globalTableState.PrimaryKeyCache.Clear();
					continue;
				}
				foreach (NavRecordId item2 in item.Value)
				{
					globalTableState.PrimaryKeyCache.Remove(item2);
				}
			}
			UpdateTableVersionTokens(out var changedTablesVersions, out var overallGlobalTableVersionToken);
			foreach (KeyValuePair<int, DataCacheTableState> tableState in privateTableStates.TableStates)
			{
				DataCacheTableState value = tableState.Value;
				if (value.LockState == DataLockState.None)
				{
					continue;
				}
				if (value.LockState == DataLockState.LockExclusive)
				{
					DataCacheTableState globalTableState2 = value.GlobalTableState;
					if ((value.ClearGlobalCacheOnCommit & CacheClear.ClearPrimaryKeyCache) == CacheClear.ClearPrimaryKeyCache)
					{
						globalTableState2.PrimaryKeyCache.Clear();
					}
					if ((value.ClearGlobalCacheOnCommit & CacheClear.ClearResultSetCache) == CacheClear.ClearResultSetCache && ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value)
					{
						globalTableState2.ResultSetGroupCache.Clear();
					}
					globalTableState2.PrimaryKeyCache.PromoteItemsFromOtherCacheIntoThisCache(value.PrimaryKeyCache, (PrimaryKeyCacheItem item) => item);
				}
				PromoteResultSetGroupCacheFromPrivateCacheToNonTransactionalOrGlobalCache(value, changedTablesVersions, overallGlobalTableVersionToken);
			}
		}
		finally
		{
			changedTables.Clear();
			primaryKeyItemsToRemoveFromGlobalCacheOnCommit.Clear();
			ResetLastKnownGlobalAndNonTransactionalTableState();
		}
	}

	private static bool TryGetResultSetCacheItem<T>(ResultSetGroupCache resultSetGroupCache, DataCacheRequest request, DateTime minimumRequiredCacheTimestamp, out T resultSetCacheItem, out ResultSetCache resultSetCache) where T : DictionaryCacheItem<DataCacheRequest>
	{
		resultSetCacheItem = null;
		resultSetCache = null;
		bool flag = false;
		if (resultSetGroupCache.GetValueOrCreateWhileHoldingLock(request, out var result))
		{
			flag = result.ResultSetCache.TryGetValue(request, minimumRequiredCacheTimestamp, out var result2);
			if (result2 != null)
			{
				resultSetCacheItem = result2 as T;
			}
		}
		resultSetCache = result.ResultSetCache;
		if (flag)
		{
			return resultSetCacheItem != null;
		}
		return false;
	}

	private void PromoteResultSetGroupCacheFromPrivateCacheToNonTransactionalOrGlobalCache(DataCacheTableState tableState, Dictionary<int, int> changedTablesVersions, int overallGlobalTableVersionToken)
	{
		DataCacheTableState orCreateTableState = nonTransactionalTableStates.GetOrCreateTableState(tableState.TableId);
		ResultSetGroupCache resultSetGroupCache = orCreateTableState.ResultSetGroupCache;
		tableState.ResultSetGroupCache.TryPromoteResultSetsFromPrivateCacheToNonTransactionalOrGlobalCache(resultSetGroupCache, changedTablesVersions, overallGlobalTableVersionToken);
	}

	private void ResetLastKnownGlobalAndNonTransactionalTableState()
	{
		globalPrimaryKeyCacheAccess.ResetLastKnownGlobalState();
		globalResultSetGroupCacheAccess.ResetLastKnownGlobalState();
	}

	private DataCacheTableState CreatePrivateDataAccessTableState(int tableId)
	{
		return privateTableStates.CreateDataAccessTableState(tableId);
	}

	private DataCacheTableState GetPrivateTableStateWithAssociatedLockState(int tableId, TransactionType transactionType, out DataLockState lockState)
	{
		DataCacheTableState dataCacheTableState = GetPrivateTableState(tableId);
		if (dataCacheTableState == null && (transactionType == TransactionType.Snapshot || transactionType == TransactionType.Update))
		{
			dataCacheTableState = CreatePrivateDataAccessTableState(tableId);
			dataCacheTableState.SetLockStateAndTransactionToken(DataLockState.Shared, sessionTransactionBumperToken);
		}
		lockState = dataCacheTableState?.LockState ?? DataLockState.None;
		return dataCacheTableState;
	}

	private ResultSetCacheFindResult TryGetResultSetCacheItemFromGlobalOrNonTransactionalCache<T>(DataCacheRequest request, DateTime minimumRequiredCacheTimestamp, out T resultSetCacheItem, out ResultSetCache resultSetCache) where T : DataCacheRequestItem
	{
		int objectId = request.MetaApplicationObject.ObjectId;
		ResultSetGroupCache globalCache = globalResultSetGroupCacheAccess.GetGlobalCache(objectId);
		if (TryGetResultSetCacheItem<T>(globalCache, request, minimumRequiredCacheTimestamp, out resultSetCacheItem, out resultSetCache))
		{
			return ResultSetCacheFindResult.GlobalCache;
		}
		if (request.ResultCanBeFoundInPrivateNonTransactionalCache)
		{
			ResultSetCache resultSetCache2 = resultSetCache;
			if (nonTransactionalTableStates.TableStates.TryGetValue(objectId, out var value))
			{
				if (!TryGetResultSetCacheItem<T>(value.ResultSetGroupCache, request, minimumRequiredCacheTimestamp, out resultSetCacheItem, out resultSetCache))
				{
					if (!request.ResultCanBeStoredInPrivateNonTransactionalCache)
					{
						resultSetCache = resultSetCache2;
					}
					return ResultSetCacheFindResult.NotFound;
				}
				return ResultSetCacheFindResult.NonTransactionalCache;
			}
			if (request.ResultCanBeStoredInPrivateNonTransactionalCache)
			{
				value = nonTransactionalTableStates.CreateDataAccessTableState(objectId);
				globalCache = value.ResultSetGroupCache;
				globalCache.GetValueOrCreateWhileHoldingLock(request, out var result);
				resultSetCache = result.ResultSetCache;
			}
		}
		return ResultSetCacheFindResult.NotFound;
	}

	private void UpdateTableVersionTokens(out Dictionary<int, int> changedTablesVersions, out int overallGlobalTableVersionToken)
	{
		changedTablesVersions = new Dictionary<int, int>(changedTables.Count);
		overallGlobalTableVersionToken = 0;
		if (changedTables.Count <= 0)
		{
			return;
		}
		foreach (int changedTable in changedTables)
		{
			DataCacheTableState globalTableState = GetGlobalTableState(changedTable);
			changedTablesVersions[changedTable] = globalTableState.IncrementGlobalTableVersionToken();
		}
		overallGlobalTableVersionToken = databaseState.IncrementOverallGlobalTableVersionToken();
	}
}
