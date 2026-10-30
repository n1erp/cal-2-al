using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class TransactionalDataCacheBase
{
	public virtual bool IsReadOnly => false;

	public abstract CacheLookupResult TryGetByPrimaryKey(RecordIdCacheRequest request, out ReadOnlyRecordBuffer readOnlyRecordBuffer, out DataLockState lockState, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> updateCache);

	public abstract bool TryFind(FindCacheRequest request, bool fromPosition, out ResultSet resultSet, out int transactionBumperToken, out DataLockState lockState, out int globalTableVersion, out int overallTableVersion, out Action<ResultSet> updateCache);

	public abstract bool TryGetExists(ExistsCacheRequest request, out bool exists, out DataLockState lockState, out int transactionBumperToken, out Action<bool> updateCache);

	public abstract bool TryGetCount(CountCacheRequest request, out int count, out DataLockState lockState, out int transactionBumperToken, out Action<int> updateCache);

	public abstract bool TryCalculateFields(DataCacheRequest request, out FieldDictionary<NavValue> result, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache);

	public abstract bool TryGetBlobContent(GetBlobContentCacheRequest request, out FieldDictionary<NavValue> values, out DataLockState lockState, out Action<FieldDictionary<NavValue>> updateCache);

	public abstract void OnBeforeInsert(int companyToken, MutableRecordBuffer recordBuffer, out int transactionBumperToken, out Action<ReadOnlyRecordBuffer> onAfterInsert);

	public abstract void OnBeforeModify(int companyToken, NCLMetaTable metaTable, MutableRecordBuffer inputRecordBuffer, out int transactionBumperToken, out bool recordWasReadWithLockInsideThisTransaction, out Action<MutableRecordBuffer, ReadOnlyRecordBuffer> onAfterModify);

	public abstract void OnBeforeDelete(int companyToken, MutableRecordBuffer recordBuffer, out bool recordWasReadWithLockInsideThisTransaction, out Action onAfterDelete);

	public abstract void OnBeforeDeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, out Action onAfterDeleteAll);

	public abstract void OnBeforeModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, FieldDictionary<NavValue> newFieldValues, out Action onAfterModifyAll);

	public abstract void LockTable(NCLMetaTable table, int companyToken);

	public abstract DataLockState GetLockState(int tableId, int companyToken);

	public abstract void ClearCache(bool clearGlobal);
}
