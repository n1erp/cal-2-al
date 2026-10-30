using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TransactionalDataCache : TransactionalDataCacheBase, ITreeObject, IDisposable
{
	private enum WriteType
	{
		Insert,
		Modify,
		Delete
	}

	private readonly bool cacheEnabled;

	private readonly bool isReadOnly;

	private readonly NavSession session;

	private readonly NavDatabase database;

	private readonly TransactionManager transactionManager;

	private readonly DataAccessTableVersionTokens tableVersionTokens;

	private readonly List<DataCacheSessionState> sessionStates = new List<DataCacheSessionState>();

	private int lastUsedTransactionBumperToken;

	private readonly SystemTableTriggers systemTableTriggers;

	private static readonly NavMetric3Dimensions primaryKeyCacheHitMetric = new NavMetric3Dimensions("PrimaryKeyCacheHit", "Tenant", "TableId", "Result");

	private static readonly NavMetric3Dimensions resultSetCacheHitMetric = new NavMetric3Dimensions("ResultSetCacheHit", "Tenant", "TableId", "Result");

	private TreeHandler tree;

	public override bool IsReadOnly => isReadOnly;

	public TreeHandler Tree => tree;

	public TransactionalDataCache(NavSession session, NavDatabase database, TransactionManager transactionManager, DataAccessTableVersionTokens tableVersionTokens, bool cacheEnabled, bool isReadOnly)
	{
		tree = TreeHandler.CreateTreeHandler(session, this);
		this.cacheEnabled = cacheEnabled;
		this.isReadOnly = isReadOnly;
		this.session = session;
		this.database = database;
		this.transactionManager = transactionManager;
		this.tableVersionTokens = tableVersionTokens;
		systemTableTriggers = new SystemTableTriggers(this.session);
		this.transactionManager.WriteTransactionEnding += (object sender, EndTransactionEventArgs e) =>
		{
			systemTableTriggers.OnTransactionEnding(e.Commit);
		};
		this.transactionManager.WriteTransactionEnded += (object sender, EndTransactionEventArgs e) =>
		{
			OndTransactionEnded(e.Commit);
		};
		this.transactionManager.WriteTransactionEnded += (object sender, EndTransactionEventArgs e) =>
		{
			systemTableTriggers.OnTransactionEnded(e.Commit);
		};
	}

	public override CacheLookupResult TryGetByPrimaryKey(RecordIdCacheRequest request, out ReadOnlyRecordBuffer readOnlyRecordBuffer, out DataLockState lockState, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> updateCache)
	{
		DataCacheSessionState sessionState = GetSessionState(request.CompanyToken);
		if (!cacheEnabled || !request.Cacheable || !IsDataCacheableForObject(request.MetaApplicationObject))
		{
			lockState = sessionState.GetCurrentLockState(request.MetaApplicationObject.ObjectId, transactionManager.CurrentTransactionType);
			transactionBumperToken = GetTransactionBumberTokenForLockState(sessionState, lockState);
			if (!request.CacheOnly)
			{
				transactionManager.EnsureReadTransactionStarted();
			}
			readOnlyRecordBuffer = null;
			updateCache = null;
			return CacheLookupResult.Unknown;
		}
		PrimaryKeyCache primaryKeyCache = sessionState.GetPrimaryKeyCache(request.MetaApplicationObject.ObjectId, transactionManager.CurrentTransactionType, out lockState, out var _);
		transactionBumperToken = GetTransactionBumberTokenForLockState(sessionState, lockState);
		bool flag = primaryKeyCache.TryGetValue(request.RecordId, tableVersionTokens.MinimumRequiredCacheTimestamp, out var result);
		UpdatePerformanceCountersWithPrimaryKeyCacheRequestResult(flag);
		if (!FeatureSwitches.Instance.DisableCacheMetrics)
		{
			primaryKeyCacheHitMetric.LogValue(1L, NavCurrentThread.Session?.Tenant?.Id, request.MetaApplicationObject.ObjectId.ToString(CultureInfo.InvariantCulture), flag ? "Hit" : "Miss");
		}
		if (flag)
		{
			readOnlyRecordBuffer = result.ReadOnlyRecordBuffer;
			if (readOnlyRecordBuffer == null)
			{
				updateCache = null;
				return CacheLookupResult.DoesNotExist;
			}
			bool flag2 = readOnlyRecordBuffer.MetaApplicationObject != null && readOnlyRecordBuffer.MetaApplicationObject.Stale;
			bool flag3 = readOnlyRecordBuffer.MetaApplicationObject != null && readOnlyRecordBuffer.MetaApplicationObject != request.MetaApplicationObject;
			if (!flag2 && !flag3)
			{
				updateCache = null;
				return CacheLookupResult.Exists;
			}
		}
		if (!request.CacheOnly)
		{
			transactionManager.EnsureReadTransactionStarted();
		}
		readOnlyRecordBuffer = null;
		updateCache = CreatePrimaryKeyUpdateFunction(request, primaryKeyCache);
		return CacheLookupResult.Unknown;
	}

	private static Action<ReadOnlyRecordBuffer> CreatePrimaryKeyUpdateFunction(RecordIdCacheRequest request, PrimaryKeyCache primaryKeyCache)
	{
		DateTime cacheCreationTime = DateTime.UtcNow;
		return (ReadOnlyRecordBuffer recordBuffer) =>
		{
			PrimaryKeyCacheItem item = ((recordBuffer != null) ? new PrimaryKeyCacheItem(primaryKeyCache.OriginDatabase, recordBuffer, cacheCreationTime) : new PrimaryKeyCacheItem(primaryKeyCache.OriginDatabase, request.RecordId, cacheCreationTime));
			primaryKeyCache.AddOrReplace(item);
		};
	}

	public override bool TryFind(FindCacheRequest request, bool fromPosition, out ResultSet resultSet, out int transactionBumperToken, out DataLockState lockState, out int globalTableVersion, out int overallTableVersion, out Action<ResultSet> updateCache)
	{
		if (!CanUseResultSetCache(request, ServerUserSettings.Instance.UseResultSetCache.Value))
		{
			transactionManager.EnsureReadTransactionStarted();
			lockState = GetLockState(request, out transactionBumperToken);
			resultSet = null;
			updateCache = null;
			globalTableVersion = GetGlobalTableVersion(request, null, out overallTableVersion);
			return false;
		}
		ResultSetCacheFindResult resultSetCacheFindResult = TryGetResultSetCacheItemLockStateAndTransactionBumberToken<ResultSetCacheItem>(request, out lockState, out transactionBumperToken, out var foundItem, out var resultSetCache);
		globalTableVersion = GetGlobalTableVersion(request, resultSetCache, out overallTableVersion);
		if (resultSetCacheFindResult != ResultSetCacheFindResult.NotFound)
		{
			resultSet = foundItem.ResultSet;
			updateCache = null;
			return true;
		}
		transactionManager.EnsureReadTransactionStarted();
		resultSet = null;
		updateCache = CreateUpdateResultSetCacheFunction(lockState, resultSetCache, foundItem);
		return false;
	}

	private static Action<ResultSet> CreateUpdateResultSetCacheFunction(DataLockState lockState, ResultSetCache resultSetCache, ResultSetCacheItem foundItem)
	{
		DateTime possibleCacheCreationTime = DateTime.UtcNow;
		return (ResultSet resultSet) =>
		{
			ResultSetCacheItem item = new ResultSetCacheItem(resultSetCache.GroupParent, resultSet, possibleCacheCreationTime);
			ResultSetCacheAddOrReplaceItem(resultSetCache, item, CanAddItemOrItMustBeAddOrReplacedInResultSetCache(lockState, foundItem));
		};
	}

	public override bool TryGetExists(ExistsCacheRequest request, out bool exists, out DataLockState lockState, out int transactionBumperToken, out Action<bool> updateCache)
	{
		if (!CanUseResultSetCache(request, ServerUserSettings.Instance.UseResultSetCache.Value))
		{
			transactionManager.EnsureReadTransactionStarted();
			updateCache = null;
			lockState = GetLockState(request, out transactionBumperToken);
			exists = false;
			return false;
		}
		if (TryGetResultSetCacheItemLockStateAndTransactionBumberToken<ExistsCacheItem>(request, out lockState, out transactionBumperToken, out var foundItem, out var resultSetCache) != ResultSetCacheFindResult.NotFound)
		{
			exists = foundItem.Exists;
			updateCache = null;
			return true;
		}
		transactionManager.EnsureReadTransactionStarted();
		exists = false;
		updateCache = CreateUpdateExistsCacheFunction(request, lockState, resultSetCache, foundItem);
		return false;
	}

	private static Action<bool> CreateUpdateExistsCacheFunction(ExistsCacheRequest request, DataLockState lockState, ResultSetCache resultSetCache, ExistsCacheItem originalExistsCacheItem)
	{
		DateTime cacheCreationTime = DateTime.UtcNow;
		return (bool exists) =>
		{
			ExistsCacheItem item = new ExistsCacheItem(resultSetCache.GroupParent, request, exists, cacheCreationTime);
			ResultSetCacheAddOrReplaceItem(resultSetCache, item, CanAddItemOrItMustBeAddOrReplacedInResultSetCache(lockState, originalExistsCacheItem));
		};
	}

	public override bool TryGetCount(CountCacheRequest request, out int count, out DataLockState lockState, out int transactionBumperToken, out Action<int> updateCache)
	{
		if (!CanUseResultSetCache(request, ServerUserSettings.Instance.UseResultSetCache.Value))
		{
			transactionManager.EnsureReadTransactionStarted();
			updateCache = null;
			lockState = GetLockState(request, out transactionBumperToken);
			count = 0;
			return false;
		}
		if (TryGetResultSetCacheItemLockStateAndTransactionBumberToken<CountCacheItem>(request, out lockState, out transactionBumperToken, out var foundItem, out var resultSetCache) != ResultSetCacheFindResult.NotFound)
		{
			count = foundItem.Count;
			updateCache = null;
			return true;
		}
		transactionManager.EnsureReadTransactionStarted();
		count = 0;
		updateCache = CreateUpateCountCacheFunction(request, lockState, resultSetCache, foundItem);
		return false;
	}

	private static Action<int> CreateUpateCountCacheFunction(CountCacheRequest request, DataLockState lockState, ResultSetCache resultSetCache, CountCacheItem originalCountCacheItem)
	{
		DateTime possibleCacheCreationTime = DateTime.UtcNow;
		return (int count) =>
		{
			CountCacheItem item = new CountCacheItem(resultSetCache.GroupParent, request, count, possibleCacheCreationTime);
			ResultSetCacheAddOrReplaceItem(resultSetCache, item, CanAddItemOrItMustBeAddOrReplacedInResultSetCache(lockState, originalCountCacheItem));
		};
	}

	public override bool TryCalculateFields(DataCacheRequest request, out FieldDictionary<NavValue> result, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache)
	{
		if (!CanUseResultSetCache(request, ServerUserSettings.Instance.UseCalculatedFieldsCache.Value))
		{
			transactionManager.EnsureReadTransactionStarted();
			lockState = GetLockState(request.MetaApplicationObject.ApplicationObjectId.ObjectNumber, request.CompanyToken);
			updateCache = null;
			result = null;
			return false;
		}
		if (TryGetResultSetCacheItemLockStateAndTransactionBumberToken<CalcFieldsCacheItem>(request, out lockState, out var _, out var foundItem, out var resultSetCache) != ResultSetCacheFindResult.NotFound)
		{
			result = foundItem.CalculatedValues;
			updateCache = null;
			return true;
		}
		transactionManager.EnsureReadTransactionStarted();
		result = null;
		updateCache = CreateUpdateCalculatedFieldsCacheFunction(request, lockState, resultSetCache, foundItem);
		return false;
	}

	private static Action<FieldDictionary<NavValue>> CreateUpdateCalculatedFieldsCacheFunction(DataCacheRequest request, DataLockState lockState, ResultSetCache resultSetCache, CalcFieldsCacheItem originalCacheItem)
	{
		DateTime possibleCacheCreationTime = DateTime.UtcNow;
		return (FieldDictionary<NavValue> newValues) =>
		{
			CalcFieldsCacheItem item = new CalcFieldsCacheItem(resultSetCache.GroupParent, request, newValues, possibleCacheCreationTime);
			ResultSetCacheAddOrReplaceItem(resultSetCache, item, CanAddItemOrItMustBeAddOrReplacedInResultSetCache(lockState, originalCacheItem));
		};
	}

	public override bool TryGetBlobContent(GetBlobContentCacheRequest request, out FieldDictionary<NavValue> values, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache)
	{
		transactionManager.EnsureReadTransactionStarted();
		values = null;
		lockState = GetLockState(request.MetaApplicationObject.ObjectId, request.CompanyToken);
		updateCache = null;
		return false;
	}

	public override void OnBeforeInsert(int companyToken, MutableRecordBuffer recordBuffer, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> onAfterInsert)
	{
		ThrowIfReadOnly(WriteType.Insert, recordBuffer.MetaApplicationObject);
		systemTableTriggers.OnBeforeInsert(recordBuffer);
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		LockTable(recordBuffer.MetaTable, sessionState);
		transactionBumperToken = sessionState.SessionTransactionBumperToken;
		onAfterInsert = CreateOnAfterInsertFunction(recordBuffer, sessionState);
	}

	private Action<ReadOnlyRecordBuffer> CreateOnAfterInsertFunction(MutableRecordBuffer recordBuffer, DataCacheSessionState sessionState)
	{
		return (ReadOnlyRecordBuffer outputRecordBuffer) =>
		{
			systemTableTriggers.OnAfterInsert(recordBuffer);
			if (IsDataCacheableForObject(recordBuffer.MetaApplicationObject))
			{
				int objectId = recordBuffer.MetaApplicationObject.ObjectId;
				if (cacheEnabled)
				{
					PrimaryKeyCacheItem item = new PrimaryKeyCacheItem(sessionState.DatabaseState.Database, outputRecordBuffer);
					sessionState.RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(objectId, item);
					sessionState.ClearNonGlobalCachesByTableId(objectId, CacheClear.ClearResultSetCache, recordBuffer);
					sessionState.EnsureTableWillBeClearedInGlobalCacheOnCommit(objectId, CacheClear.ClearResultSetCache);
				}
				sessionState.EnsureGlobalTableStateWillBeUpdatedOnCommmit(objectId);
			}
		};
	}

	public override void OnBeforeDelete(int companyToken, MutableRecordBuffer recordBuffer, out bool recordWasReadWithLockInsideThisTransaction, out Action onAfterDelete)
	{
		ThrowIfReadOnly(WriteType.Delete, recordBuffer.MetaApplicationObject);
		systemTableTriggers.OnBeforeDelete(recordBuffer);
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		recordWasReadWithLockInsideThisTransaction = LockTableAndReturnIfRecordWasReadWithCorrectLock(sessionState, recordBuffer);
		onAfterDelete = CreateOnAfterDeleteFunction(recordBuffer, sessionState);
	}

	private Action CreateOnAfterDeleteFunction(MutableRecordBuffer recordBuffer, DataCacheSessionState sessionState)
	{
		return () =>
		{
			systemTableTriggers.OnAfterDelete(recordBuffer);
			if (IsDataCacheableForObject(recordBuffer.MetaApplicationObject))
			{
				int tableId = recordBuffer.MetaTable.TableId;
				if (cacheEnabled)
				{
					NavRecordId recordId = recordBuffer.ReadOnlyBuffer.GetRecordId();
					PrimaryKeyCache primaryKeyCache = sessionState.GetPrimaryKeyCache(tableId, transactionManager.CurrentTransactionType, out var _, out var _);
					primaryKeyCache.AddOrReplace(new PrimaryKeyCacheItem(primaryKeyCache.OriginDatabase, recordId));
				}
				sessionState.ClearNonGlobalCachesByTableId(tableId, CacheClear.ClearResultSetCache);
				sessionState.EnsureTableWillBeClearedInGlobalCacheOnCommit(tableId, CacheClear.ClearResultSetCache);
				sessionState.EnsureGlobalTableStateWillBeUpdatedOnCommmit(recordBuffer.MetaTable.TableId);
			}
		};
	}

	public override void OnBeforeDeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, out Action onAfterDeleteAll)
	{
		ThrowIfReadOnly(WriteType.Delete, metaTable);
		systemTableTriggers.OnBeforeDeleteAll(metaTable, filtersAndMarks);
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		LockTable(metaTable, sessionState);
		onAfterDeleteAll = CreateOnAfterDeleteOrModifyAllFunction(metaTable, sessionState);
	}

	public override void OnBeforeModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, FieldDictionary<NavValue> newFieldValues, out Action onAfterModifyAll)
	{
		ThrowIfReadOnly(WriteType.Modify, metaTable);
		if (systemTableTriggers != null)
		{
			systemTableTriggers.OnBeforeModifyAll(metaTable, companyToken, filtersAndMarks, newFieldValues);
		}
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		LockTable(metaTable, sessionState);
		onAfterModifyAll = (IsDataCacheableForObject(metaTable) ? CreateOnAfterDeleteOrModifyAllFunction(metaTable, sessionState) : null);
	}

	private Action CreateOnAfterDeleteOrModifyAllFunction(NCLMetaTable metaTable, DataCacheSessionState sessionState)
	{
		return () =>
		{
			if (cacheEnabled)
			{
				sessionState.ClearNonGlobalCachesByTableId(metaTable.ObjectId, CacheClear.ClearAll);
				sessionState.EnsureTableWillBeClearedInGlobalCacheOnCommit(metaTable.ObjectId, CacheClear.ClearAll);
			}
			sessionState.EnsureGlobalTableStateWillBeUpdatedOnCommmit(metaTable.ObjectId);
		};
	}

	public override void OnBeforeModify(int companyToken, NCLMetaTable metaTable, MutableRecordBuffer inputRecordBuffer, out int transactionBumperToken, out bool recordWasReadWithLockInsideThisTransaction, out Action<MutableRecordBuffer, ReadOnlyRecordBuffer> onAfterModify)
	{
		ThrowIfReadOnly(WriteType.Modify, metaTable);
		systemTableTriggers.OnBeforeModify(inputRecordBuffer, companyToken);
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		recordWasReadWithLockInsideThisTransaction = LockTableAndReturnIfRecordWasReadWithCorrectLock(sessionState, inputRecordBuffer);
		transactionBumperToken = sessionState.SessionTransactionBumperToken;
		onAfterModify = (IsDataCacheableForObject(metaTable) ? CreateOnAfterModifyFunction(metaTable, sessionState) : null);
	}

	private Action<MutableRecordBuffer, ReadOnlyRecordBuffer> CreateOnAfterModifyFunction(NCLMetaTable metaTable, DataCacheSessionState sessionState)
	{
		return (MutableRecordBuffer inputRecordBuffer, ReadOnlyRecordBuffer outputRecordBuffer) =>
		{
			systemTableTriggers.OnAfterModify(inputRecordBuffer, outputRecordBuffer);
			int tableId = metaTable.TableId;
			if (cacheEnabled)
			{
				NavRecordId recordId = inputRecordBuffer.ReadOnlyBuffer.GetRecordId();
				sessionState.RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(tableId, recordId);
				NavRecordId recordId2 = outputRecordBuffer.GetRecordId();
				sessionState.RemoveFromPrivatePrimaryKeyCacheNowAndGlobalOnCommit(tableId, recordId2);
				sessionState.ClearNonGlobalCachesByTableId(tableId, CacheClear.ClearResultSetCache, inputRecordBuffer);
				sessionState.EnsureTableWillBeClearedInGlobalCacheOnCommit(tableId, CacheClear.ClearResultSetCache);
			}
			sessionState.EnsureGlobalTableStateWillBeUpdatedOnCommmit(tableId);
		};
	}

	public override void LockTable(NCLMetaTable table, int companyToken)
	{
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		LockTable(table, sessionState);
	}

	public override DataLockState GetLockState(int tableId, int companyToken)
	{
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		return sessionState.GetCurrentLockState(tableId, transactionManager.CurrentTransactionType);
	}

	public override void ClearCache(bool clearGlobal)
	{
		for (int i = 0; i < sessionStates.Count; i++)
		{
			sessionStates[i]?.ClearNonGlobalCaches(clearGlobal);
		}
		if (clearGlobal)
		{
			database.DatabaseState.ClearGlobalCache();
		}
	}

	private int GetGlobalTableVersion(int tableId, int companyToken, out int overallTableVersion)
	{
		DataCacheSessionState sessionState = GetSessionState(companyToken);
		DataCacheTableState globalTableState = sessionState.GetGlobalTableState(tableId);
		overallTableVersion = sessionState.DatabaseState.OverallGlobalTableVersionToken;
		return globalTableState.GlobalTableVersionToken;
	}

	private bool LockTable(NCLMetaTable table, DataCacheSessionState sessionState)
	{
		bool flag = sessionState.LockTable(table.TableId, ref lastUsedTransactionBumperToken);
		if (flag)
		{
			tableVersionTokens.IncrementUpdateBumperToken(sessionState.CompanyToken, table.TableId);
		}
		transactionManager.EnsureWriteTransactionStarted();
		return flag;
	}

	private DataLockState GetLockState(DataCacheRequest request, out int transactionBumperToken)
	{
		DataCacheSessionState sessionState = GetSessionState(request.CompanyToken);
		DataLockState dataLockState = ((request.MetaApplicationObject is NCLMetaTable nCLMetaTable) ? sessionState.GetCurrentLockState(nCLMetaTable.TableId, transactionManager.CurrentTransactionType) : DataLockState.None);
		transactionBumperToken = GetTransactionBumberTokenForLockState(sessionState, dataLockState);
		return dataLockState;
	}

	private void OndTransactionEnded(bool commit)
	{
		try
		{
			for (int i = 0; i < sessionStates.Count; i++)
			{
				sessionStates[i]?.InternalEndTransaction(commit);
			}
		}
		finally
		{
			ClearCache(clearGlobal: false);
		}
	}

	private DataCacheSessionState GetSessionState(int companyToken)
	{
		while (companyToken >= sessionStates.Count)
		{
			sessionStates.Add(null);
		}
		DataCacheSessionState dataCacheSessionState = sessionStates[companyToken];
		if (dataCacheSessionState == null)
		{
			dataCacheSessionState = new DataCacheSessionState(database, session, companyToken, isReadOnly);
			sessionStates[companyToken] = dataCacheSessionState;
		}
		return dataCacheSessionState;
	}

	private static int GetTransactionBumberTokenForLockState(DataCacheSessionState sessionState, DataLockState lockState)
	{
		if (lockState != DataLockState.None)
		{
			return sessionState.SessionTransactionBumperToken;
		}
		return 0;
	}

	private static void ResultSetCacheAddOrReplaceItem(ResultSetCache resultSetCache, DataCacheRequestItem item, bool canAdd)
	{
		if (!canAdd || item.Key.MetaApplicationObject.Stale)
		{
			resultSetCache.AddOrReplace(item);
		}
		else
		{
			resultSetCache.Add(item);
		}
	}

	private static bool CanAddItemOrItMustBeAddOrReplacedInResultSetCache(DataLockState lockState, DataCacheRequestItem foundInCache)
	{
		if (ServerUserSettings.Instance.ResultSetGroupCacheUsesGlobalCache.Value && lockState == DataLockState.None)
		{
			return false;
		}
		return foundInCache == null;
	}

	private int GetGlobalTableVersion(DataCacheRequest request, ResultSetCache resultSetCache, out int overallTableVersion)
	{
		if (resultSetCache != null)
		{
			overallTableVersion = resultSetCache.OverallGlobalTableVersionToken;
			return resultSetCache.GlobalTableVersionToken;
		}
		return GetGlobalTableVersion(request.MetaApplicationObject.ObjectId, request.CompanyToken, out overallTableVersion);
	}

	private bool CanUseResultSetCache(DataCacheRequest request, bool allowed)
	{
		if (allowed && cacheEnabled && IsDataCacheableForObject(request.MetaApplicationObject))
		{
			return request.CanUseResultSetCache();
		}
		return false;
	}

	private bool LockTableAndReturnIfRecordWasReadWithCorrectLock(DataCacheSessionState sessionState, MutableRecordBuffer recordBuffer)
	{
		if (LockTable(recordBuffer.MetaTable, sessionState))
		{
			return false;
		}
		if (recordBuffer.TransactionToken == 0 || recordBuffer.TransactionToken != sessionState.SessionTransactionBumperToken)
		{
			return false;
		}
		return true;
	}

	private static bool IsDataCacheableForObject(NCLMetaApplicationObject applicationObject)
	{
		if (!(applicationObject is NCLMetaTable nCLMetaTable))
		{
			return false;
		}
		if (nCLMetaTable.IsAppArchiveTable)
		{
			return false;
		}
		switch (nCLMetaTable.ObjectId)
		{
		case 2000000001:
		case 2000000053:
		case 2000000071:
		case 2000000076:
		case 2000000110:
		case 2000000150:
		case 2000000151:
		case 2000000152:
		case 2000000153:
		case 2000000155:
		case 2000000156:
		case 2000000160:
		case 2000000161:
		case 2000000162:
		case 2000000163:
		case 2000000168:
		case 2000000169:
		case 2000000176:
		case 2000000177:
		case 2000000187:
		case 2000000200:
			return false;
		default:
			return !nCLMetaTable.IsExternalDataSource;
		}
	}

	private static void UpdatePerformanceCountersWithPrimaryKeyCacheRequestResult(bool foundInCache)
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPrimaryKeyCacheRequestResult(foundInCache);
	}

	private static void UpdatePerformanceCountersWithResultSetCacheRequestResult(bool foundInCache, bool isForCalculatedField)
	{
		if (isForCalculatedField)
		{
			NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithCalculatedFieldsRequestResult(foundInCache);
		}
		else
		{
			NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithResultSetCacheRequestResult(foundInCache);
		}
	}

	private ResultSetCacheFindResult TryGetResultSetCacheItemLockStateAndTransactionBumberToken<T>(DataCacheRequest request, out DataLockState lockState, out int transactionBumperToken, out T foundItem, out ResultSetCache resultSetCache) where T : DataCacheRequestItem
	{
		DataCacheSessionState sessionState = GetSessionState(request.CompanyToken);
		ResultSetCacheFindResult resultSetCacheFindResult = sessionState.TryGetResultSetCacheItemAndLockState<T>(request, transactionManager.CurrentTransactionType, tableVersionTokens.MinimumRequiredCacheTimestamp, out lockState, out foundItem, out resultSetCache);
		UpdatePerformanceCountersWithResultSetCacheRequestResult(resultSetCacheFindResult != ResultSetCacheFindResult.NotFound, typeof(T) == typeof(CalcFieldsCacheItem));
		if (!FeatureSwitches.Instance.DisableCacheMetrics)
		{
			resultSetCacheHitMetric.LogValue(1L, NavCurrentThread.Session?.Tenant?.Id, request.MetaApplicationObject.ObjectId.ToString(CultureInfo.InvariantCulture), (resultSetCacheFindResult == ResultSetCacheFindResult.NotFound) ? "Miss" : "Hit");
		}
		transactionBumperToken = GetTransactionBumberTokenForLockState(sessionState, lockState);
		return resultSetCacheFindResult;
	}

	private void ThrowIfReadOnly(WriteType writeType, NCLMetaApplicationObject metaApplicationObject)
	{
		if (isReadOnly)
		{
			ThrowCannotWriteException(writeType, metaApplicationObject);
		}
	}

	private void ThrowCannotWriteException(WriteType writeType, NCLMetaApplicationObject metaApplicationObject)
	{
		string objectNameForErrorMessage = session.GetObjectNameForErrorMessage(metaApplicationObject.ApplicationObjectId);
		switch (writeType)
		{
		case WriteType.Insert:
			throw new NavInsertDeniedPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.CannotInsertApplicationTableRecords, objectNameForErrorMessage));
		case WriteType.Modify:
			throw new NavModifyDeniedPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.CannotModifyApplicationTableRecords, objectNameForErrorMessage));
		case WriteType.Delete:
			throw new NavDeleteDeniedPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.CannotDeleteApplicationTableRecords, objectNameForErrorMessage));
		}
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
			for (int i = 0; i < sessionStates.Count; i++)
			{
				sessionStates[i]?.ClearNonGlobalCaches(clearNonTransactionalCache: true);
			}
		}
	}
}
