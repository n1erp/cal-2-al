using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class VirtualAndTempTransactionalDataCache : TransactionalDataCacheBase
{
	public static readonly VirtualAndTempTransactionalDataCache Instance = new VirtualAndTempTransactionalDataCache();

	private VirtualAndTempTransactionalDataCache()
	{
	}

	public override CacheLookupResult TryGetByPrimaryKey(RecordIdCacheRequest request, out ReadOnlyRecordBuffer readOnlyRecordBuffer, out DataLockState lockState, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> updateCache)
	{
		readOnlyRecordBuffer = null;
		lockState = DataLockState.None;
		transactionBumperToken = 0;
		updateCache = null;
		return CacheLookupResult.Unknown;
	}

	public override bool TryFind(FindCacheRequest request, bool fromPosition, out ResultSet resultSet, out int transactionBumperToken, out DataLockState lockState, out int globalTableVersion, out int overallTableVersion, out Action<ResultSet> updateCache)
	{
		resultSet = null;
		transactionBumperToken = 0;
		lockState = DataLockState.None;
		updateCache = null;
		globalTableVersion = 0;
		overallTableVersion = 0;
		return false;
	}

	public override bool TryGetExists(ExistsCacheRequest request, out bool exists, out DataLockState lockState, out int transactionBumperToken, out Action<bool> updateCache)
	{
		exists = false;
		lockState = DataLockState.None;
		transactionBumperToken = 0;
		updateCache = null;
		return false;
	}

	public override bool TryGetCount(CountCacheRequest request, out int count, out DataLockState lockState, out int transactionBumperToken, out Action<int> updateCache)
	{
		count = 0;
		lockState = DataLockState.None;
		transactionBumperToken = 0;
		updateCache = null;
		return false;
	}

	public override bool TryCalculateFields(DataCacheRequest request, out FieldDictionary<NavValue> result, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache)
	{
		result = null;
		lockState = DataLockState.None;
		updateCache = null;
		return false;
	}

	public override bool TryGetBlobContent(GetBlobContentCacheRequest request, out FieldDictionary<NavValue> values, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache)
	{
		values = null;
		lockState = DataLockState.None;
		updateCache = null;
		return false;
	}

	public override void OnBeforeInsert(int companyToken, MutableRecordBuffer recordBuffer, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> onAfterInsert)
	{
		transactionBumperToken = 0;
		onAfterInsert = null;
	}

	public override void OnBeforeDelete(int companyToken, MutableRecordBuffer recordBuffer, out bool recordWasReadWithLockInsideThisTransaction, out Action onAfterDelete)
	{
		recordWasReadWithLockInsideThisTransaction = false;
		onAfterDelete = null;
	}

	public override void OnBeforeDeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, out Action onAfterDeleteAll)
	{
		onAfterDeleteAll = null;
	}

	public override void OnBeforeModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, FieldDictionary<NavValue> newFieldValues, out Action onAfterModifyAll)
	{
		onAfterModifyAll = null;
	}

	public override void OnBeforeModify(int companyToken, NCLMetaTable metaTable, MutableRecordBuffer inputRecordBuffer, out int transactionBumperToken, out bool recordWasReadWithLockInsideThisTransaction, out Action<MutableRecordBuffer, ReadOnlyRecordBuffer> onAfterModify)
	{
		transactionBumperToken = 0;
		recordWasReadWithLockInsideThisTransaction = false;
		onAfterModify = null;
	}

	public override void LockTable(NCLMetaTable table, int companyToken)
	{
	}

	public override DataLockState GetLockState(int tableId, int companyToken)
	{
		return DataLockState.None;
	}

	public override void ClearCache(bool clearGlobal)
	{
	}
}
