using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataAccess
{
	private readonly NavSession session;

	private readonly DataProvider provider;

	private readonly GlobalFilters globalFilters;

	private readonly DataAccessTableVersionTokens tableVersionTokens;

	private readonly TransactionalDataCacheBase transactionalDataCache;

	private readonly TimestampLog timestampLog = new TimestampLog();

	public GlobalFilters GlobalFilters => globalFilters;

	public DataAccessTableVersionTokens TableVersionTokens => tableVersionTokens;

	public DataProvider DataProvider => provider;

	public bool IsReadOnly => transactionalDataCache.IsReadOnly;

	internal DataAccess(NavSession session, DataProvider provider, GlobalFilters globalFilters, DataAccessTableVersionTokens tableVersionTokens, TransactionalDataCacheBase transactionalDataCache)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (globalFilters == null)
		{
			throw new ArgumentNullException("globalFilters");
		}
		if (tableVersionTokens == null)
		{
			throw new ArgumentNullException("tableVersionTokens");
		}
		this.session = session;
		this.provider = provider;
		this.globalFilters = globalFilters;
		this.tableVersionTokens = tableVersionTokens;
		this.transactionalDataCache = transactionalDataCache;
	}

	public bool TryGetByPrimaryKey(RecordIdCacheRequest request, out MutableRecordBuffer recordBuffer)
	{
		return InternalTryGetByPrimaryKey(request, out recordBuffer) == CacheLookupResult.Exists;
	}

	private CacheLookupResult InternalTryGetByPrimaryKey(RecordIdCacheRequest request, out MutableRecordBuffer recordBuffer)
	{
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			recordBuffer = null;
			return CacheLookupResult.DoesNotExist;
		}
		CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		CacheLookupResult cacheLookupResult = transactionalDataCache.TryGetByPrimaryKey(request, out var readOnlyRecordBuffer, out var lockState, out var transactionBumperToken, out var updateCache);
		switch (cacheLookupResult)
		{
		case CacheLookupResult.DoesNotExist:
			recordBuffer = null;
			return cacheLookupResult;
		case CacheLookupResult.Unknown:
		{
			if (request.CacheOnly)
			{
				recordBuffer = null;
				return cacheLookupResult;
			}
			RecordIdProviderRequest request2 = new RecordIdProviderRequest(request.CompanyToken, request.MetaApplicationObject, lockState, FiltersAndMarks.Empty, request.FlowFieldSecurityFiltering, request.RecordId);
			bool flag = provider.TryGetByPrimaryKey(request2, out readOnlyRecordBuffer);
			updateCache?.Invoke(readOnlyRecordBuffer);
			if (!flag)
			{
				recordBuffer = null;
				return CacheLookupResult.DoesNotExist;
			}
			break;
		}
		}
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks) && !FlowFieldsHelper.RecordIsWithinFilteredFlowFields(session, request.CompanyToken, readOnlyRecordBuffer, request.FiltersAndMarks, request.FlowFieldSecurityFiltering))
		{
			recordBuffer = null;
			return CacheLookupResult.DoesNotExist;
		}
		if (!readOnlyRecordBuffer.PassesFieldFiltersAndMarks(request.FiltersAndMarks, includeFlowFields: false))
		{
			recordBuffer = null;
			return CacheLookupResult.DoesNotExist;
		}
		recordBuffer = new MutableRecordBuffer(readOnlyRecordBuffer, transactionBumperToken);
		return CacheLookupResult.Exists;
	}

	public ResultSetEnumerator Find(FindCacheRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		return InnerFind(request, fromPosition: false, onlyCurrentKeyNeededForNextRow);
	}

	public ResultSetEnumerator FindFromPosition(PositionedFindCacheRequest request, Func<bool> onlyPrimaryKeyNeededForNextRow = null)
	{
		return InnerFind(request, fromPosition: true, onlyPrimaryKeyNeededForNextRow);
	}

	public bool Exists(ExistsCacheRequest request)
	{
		if (request.FiltersAndMarks.MarkedRecords != null && request.FiltersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			return MarkedRecordsHelper.ExistsWithManyMarks(this, request);
		}
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			return false;
		}
		CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		if (transactionalDataCache.TryGetExists(request, out var exists, out var lockState, out var _, out var updateCache))
		{
			return exists;
		}
		if (updateCache != null && TryHandleAsPrimaryKeyLookup(request, cacheOnly: true, out var recordBuffer))
		{
			return recordBuffer != null;
		}
		if (request.ForceNonLockingProviderRequest && lockState != DataLockState.None)
		{
			lockState = DataLockState.None;
			updateCache = null;
		}
		ExistsProviderRequest request2 = new ExistsProviderRequest(request.CompanyToken, request.MetaApplicationObject, lockState, request.FiltersAndMarks, null, request.FlowFieldSecurityFiltering, request.SortingFields);
		exists = provider.Exists(request2);
		updateCache?.Invoke(exists);
		return exists;
	}

	public int Count(CountCacheRequest request)
	{
		if (request.FiltersAndMarks.MarkedRecords != null && request.FiltersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			return MarkedRecordsHelper.CountWithManyMarks(this, request);
		}
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			return 0;
		}
		CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		if (transactionalDataCache.TryGetCount(request, out var count, out var lockState, out var _, out var updateCache))
		{
			return count;
		}
		if (updateCache != null && TryHandleAsPrimaryKeyLookup(request, cacheOnly: true, out var recordBuffer))
		{
			if (recordBuffer != null)
			{
				return 1;
			}
			return 0;
		}
		DataProviderRequest request2 = new DataProviderRequest(request.CompanyToken, request.MetaApplicationObject, lockState, request.FiltersAndMarks, null, request.FlowFieldSecurityFiltering);
		count = provider.Count(request2);
		updateCache?.Invoke(count);
		return count;
	}

	private static MutableRecordBuffer CreateNewBufferFromOutputBufferTransferBlobValuesFromOldRecord(MutableRecordBuffer oldRecord, ReadOnlyRecordBuffer outputBuffer, int transactionBumperToken)
	{
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(outputBuffer, transactionBumperToken);
		if (oldRecord.MetaTable.BlobFieldCount > 0)
		{
			NCLMetaField[] blobFields = oldRecord.MetaTable.BlobFields;
			foreach (NCLMetaField nCLMetaField in blobFields)
			{
				NavBLOB navBLOB = (NavBLOB)oldRecord.GetChangedFieldValue(nCLMetaField.FieldIndex);
				if (navBLOB != null)
				{
					mutableRecordBuffer[nCLMetaField.FieldIndex] = navBLOB;
					navBLOB.IsDirty = false;
				}
			}
		}
		return mutableRecordBuffer;
	}

	public FieldDictionary<NavValue> CalcNumeric(CalcNumericCacheRequest request)
	{
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			return new FieldDictionary<NavValue>(request.FieldsToCalculate.Select((INavFieldMetadata f) => Tuple.Create(f, f.InitValue)));
		}
		return CalculateFields(request, (DataLockState lockState) => new CalcNumericProviderRequest(request.CompanyToken, request.MetaApplicationObject, request.FiltersAndMarks, request.FlowFieldSecurityFiltering, request.RecordBuffer, request.FieldsToCalculate, lockState), provider.CalcNumeric);
	}

	public FieldDictionary<NavValue> CalcMinMax(CalcMinMaxCacheRequest request)
	{
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			return new FieldDictionary<NavValue>(request.FieldsToCalculate.Select((INavFieldMetadata f) => Tuple.Create(f, f.InitValue)));
		}
		return CalculateFields(request, (DataLockState lockState) => new CalcMinMaxProviderRequest(request.CompanyToken, request.MetaApplicationObject, request.FiltersAndMarks, request.FlowFieldSecurityFiltering, request.RecordBuffer, request.FieldsToCalculate, lockState), provider.CalcMinMax);
	}

	public FieldDictionary<NavValue> GetBlobContent(GetBlobContentCacheRequest request)
	{
		CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		if (transactionalDataCache.TryGetBlobContent(request, out var values, out var lockState, out var updateCache))
		{
			return values;
		}
		GetBlobContentProviderRequest request2 = new GetBlobContentProviderRequest(request.CompanyToken, request.MetaApplicationObject, request.RecordBuffer, request.BlobsToGet, lockState);
		values = provider.GetBlobContent(request2);
		updateCache?.Invoke(values);
		return values;
	}

	public FieldDictionary<NavValue> CalcSums(CalcSumsCacheRequest request)
	{
		if (request.FiltersAndMarks.MarkedRecords != null && request.FiltersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			return MarkedRecordsHelper.CalcSumsWithManyMarks(this, request);
		}
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			return new FieldDictionary<NavValue>(request.FieldsToCalculate.Select((INavFieldMetadata f) => Tuple.Create(f, f.InitValue)));
		}
		return CalculateFields(request, (DataLockState lockState) => new CalcSumsProviderRequest(request.CompanyToken, request.MetaApplicationObject, lockState, request.FiltersAndMarks, request.FlowFieldSecurityFiltering, request.FieldsToCalculate), provider.CalcSums);
	}

	public ModifyResult Modify(int companyToken, MutableRecordBuffer inputRecordBuffer, ref ResultSetEnumerator resultSetEnumerator, out MutableRecordBuffer newRecordBuffer)
	{
		NCLMetaTable metaTable = inputRecordBuffer.MetaTable;
		ValidateCompanyOpenOrNotPerCompany(metaTable, companyToken);
		CheckCloudStateReadyReplicationAllowOperation(inputRecordBuffer.MetaApplicationObject, companyToken);
		transactionalDataCache.OnBeforeModify(companyToken, metaTable, inputRecordBuffer, out var transactionBumperToken, out var recordWasReadWithLockInsideThisTransaction, out var onAfterModify);
		bool useCache = onAfterModify != null;
		ModifyResult modifyResult = PerformModify(companyToken, recordWasReadWithLockInsideThisTransaction, useCache, ref inputRecordBuffer, out var outputRecordBuffer);
		switch (modifyResult)
		{
		case ModifyResult.RecordStale:
			throw GetStaleRecordException(companyToken, inputRecordBuffer);
		default:
			newRecordBuffer = null;
			return modifyResult;
		case ModifyResult.Success:
			newRecordBuffer = CreateNewBufferFromOutputBufferTransferBlobValuesFromOldRecord(inputRecordBuffer, outputRecordBuffer, transactionBumperToken);
			if (newRecordBuffer[0].Equals(inputRecordBuffer[0]))
			{
				return modifyResult;
			}
			timestampLog.AddTimestamp(outputRecordBuffer.Timestamp.ToInt64());
			resultSetEnumerator = IncrementBumperTokenWithoutInvalidatingEnumerator(companyToken, metaTable, resultSetEnumerator, newRecordBuffer.ReadOnlyBuffer);
			onAfterModify?.Invoke(inputRecordBuffer, outputRecordBuffer);
			return modifyResult;
		}
	}

	private ResultSetEnumerator IncrementBumperTokenWithoutInvalidatingEnumerator(int companyToken, NCLMetaTable metaTable, ResultSetEnumerator resultSetEnumerator, ReadOnlyRecordBuffer recordBuffer)
	{
		bool flag = resultSetEnumerator?.IsStillValid ?? false;
		tableVersionTokens.IncrementUpdateBumperToken(companyToken, metaTable.TableId);
		if (flag)
		{
			resultSetEnumerator = resultSetEnumerator.UpdateCurrentRowAndClone(recordBuffer, tableVersionTokens.CreateTableVersionStillValidFunction(companyToken, metaTable.TableId, globalFilters, dynamic: true));
		}
		return resultSetEnumerator;
	}

	private ModifyResult PerformModify(int companyToken, bool recordWasReadWithLockInsideThisTransaction, bool useCache, ref MutableRecordBuffer inputRecordBuffer, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		ModifyResult modifyResult;
		if (recordWasReadWithLockInsideThisTransaction)
		{
			if (inputRecordBuffer.IsCopiedFromTemporary)
			{
				modifyResult = ModifyResult.RecordStale;
				outputRecordBuffer = null;
			}
			else
			{
				modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: true, checkThatRecordExists: false, out outputRecordBuffer);
			}
			if (modifyResult == ModifyResult.RecordStale)
			{
				if (TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, useCache, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var recordBuffer))
				{
					inputRecordBuffer = MutableRecordBuffer.ComputeChangeset(inputRecordBuffer, recordBuffer.ReadOnlyBuffer);
					modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: false, checkThatRecordExists: false, out outputRecordBuffer);
				}
				else
				{
					outputRecordBuffer = null;
					modifyResult = ModifyResult.RecordNotFound;
				}
			}
		}
		else if (inputRecordBuffer.Timestamp == null || inputRecordBuffer.Timestamp.ToInt64() == 0L)
		{
			if (TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, useCache, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var recordBuffer2))
			{
				inputRecordBuffer = MutableRecordBuffer.ComputeChangeset(inputRecordBuffer, recordBuffer2.ReadOnlyBuffer);
				modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: false, checkThatRecordExists: false, out outputRecordBuffer);
			}
			else
			{
				outputRecordBuffer = null;
				modifyResult = ModifyResult.RecordNotFound;
			}
		}
		else if (inputRecordBuffer.ReadOnlyBuffer.Timestamp == null)
		{
			if (TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, useCache, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var recordBuffer3))
			{
				if ((long)recordBuffer3.Timestamp > (long)inputRecordBuffer.Timestamp)
				{
					outputRecordBuffer = null;
					modifyResult = ModifyResult.RecordStale;
				}
				else
				{
					inputRecordBuffer = MutableRecordBuffer.ComputeChangeset(inputRecordBuffer, recordBuffer3.ReadOnlyBuffer);
					modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: false, checkThatRecordExists: false, out outputRecordBuffer);
				}
			}
			else
			{
				outputRecordBuffer = null;
				modifyResult = ModifyResult.RecordNotFound;
			}
		}
		else
		{
			if (inputRecordBuffer.IsCopiedFromTemporary)
			{
				modifyResult = ModifyResult.RecordStale;
				outputRecordBuffer = null;
			}
			else
			{
				modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: true, checkThatRecordExists: true, out outputRecordBuffer);
			}
			if (modifyResult == ModifyResult.RecordStale)
			{
				if (TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, useCache, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var recordBuffer4))
				{
					if (recordBuffer4.Timestamp.ToInt64() > inputRecordBuffer.Timestamp.ToInt64())
					{
						outputRecordBuffer = null;
						modifyResult = ModifyResult.RecordStale;
					}
					else
					{
						inputRecordBuffer = MutableRecordBuffer.ComputeChangeset(inputRecordBuffer, recordBuffer4.ReadOnlyBuffer);
						modifyResult = IssueModify(companyToken, inputRecordBuffer, withTimestampCheck: false, checkThatRecordExists: false, out outputRecordBuffer);
					}
				}
				else
				{
					outputRecordBuffer = null;
					modifyResult = ModifyResult.RecordNotFound;
				}
			}
		}
		return modifyResult;
	}

	private ModifyResult IssueModify(int companyToken, MutableRecordBuffer inputRecordBuffer, bool withTimestampCheck, bool checkThatRecordExists, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		ModifyResult modifyResult = provider.Modify(companyToken, inputRecordBuffer, !withTimestampCheck, out outputRecordBuffer);
		if (((modifyResult == ModifyResult.RecordStale) & withTimestampCheck & checkThatRecordExists) && inputRecordBuffer[0] != null && !inputRecordBuffer[0].IsZeroOrEmpty && !TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, cacheable: true, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var _))
		{
			outputRecordBuffer = null;
			return ModifyResult.RecordNotFound;
		}
		return modifyResult;
	}

	[Conditional("DEBUG")]
	private static void ValidateModifyBuffer(MutableRecordBuffer inputRecordBuffer, NCLMetaTable metaTable)
	{
		if (inputRecordBuffer.ReadOnlyBuffer[0] == null)
		{
			return;
		}
		int num = 0;
		for (int i = 1; i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (fieldByIndex.FieldClass == FieldClass.Normal)
			{
				num |= ((!fieldByIndex.FieldIsPartOfPrimaryKey) ? ((inputRecordBuffer.GetChangedFieldValue(i) == null) ? 8 : 4) : ((inputRecordBuffer.GetChangedFieldValue(i) != null) ? 1 : 2));
			}
		}
		if ((num & 1) == 0 || (num & 2) == 0)
		{
			return;
		}
		throw new InvalidOperationException("When doing a rename, all primary key fields must be part of the changeset.");
	}

	public void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		if (filtersAndMarks.MarkedRecords != null && filtersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			MarkedRecordsHelper.ModifyAllWithManyMarks(this, companyToken, metaTable, filtersAndMarks, flowFieldSecurityFiltering, newFieldValues);
		}
		else if (!filtersAndMarks.SetIsGuaranteedEmpty)
		{
			ValidateCompanyOpenOrNotPerCompany(metaTable, companyToken);
			CheckCloudStateReadyReplicationAllowOperation(metaTable, companyToken);
			transactionalDataCache.OnBeforeModifyAll(companyToken, metaTable, filtersAndMarks, newFieldValues, out var onAfterModifyAll);
			provider.ModifyAll(companyToken, metaTable, filtersAndMarks, flowFieldSecurityFiltering, newFieldValues);
			tableVersionTokens.IncrementUpdateBumperToken(companyToken, metaTable.ObjectId);
			onAfterModifyAll?.Invoke();
		}
	}

	public DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, ref ResultSetEnumerator resultSetEnumerator)
	{
		ValidateCompanyOpenOrNotPerCompany(recordBuffer.MetaApplicationObject, companyToken);
		CheckCloudStateReadyReplicationAllowOperation(recordBuffer.MetaApplicationObject, companyToken);
		transactionalDataCache.OnBeforeDelete(companyToken, recordBuffer, out var recordWasReadWithLockInsideThisTransaction, out var onAfterDelete);
		DeleteResult deleteResult = provider.Delete(companyToken, recordBuffer, recordWasReadWithLockInsideThisTransaction);
		switch (deleteResult)
		{
		case DeleteResult.RecordStale:
			throw GetStaleRecordException(companyToken, recordBuffer);
		default:
			return deleteResult;
		case DeleteResult.Success:
			resultSetEnumerator = IncrementBumperTokenWithoutInvalidatingEnumerator(companyToken, recordBuffer.MetaTable, resultSetEnumerator, null);
			onAfterDelete?.Invoke();
			return deleteResult;
		}
	}

	public void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		if (filtersAndMarks.MarkedRecords != null && filtersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			MarkedRecordsHelper.DeleteAllWithManyMarks(this, companyToken, metaTable, filtersAndMarks, securityFiltering);
		}
		else if (!filtersAndMarks.SetIsGuaranteedEmpty)
		{
			ValidateCompanyOpenOrNotPerCompany(metaTable, companyToken);
			CheckCloudStateReadyReplicationAllowOperation(metaTable, companyToken);
			transactionalDataCache.OnBeforeDeleteAll(companyToken, metaTable, filtersAndMarks, out var onAfterDeleteAll);
			provider.DeleteAll(companyToken, metaTable, filtersAndMarks, securityFiltering);
			tableVersionTokens.IncrementUpdateBumperToken(companyToken, metaTable.ObjectId);
			onAfterDeleteAll?.Invoke();
		}
	}

	public InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out MutableRecordBuffer newRecordBuffer)
	{
		ValidateCompanyOpenOrNotPerCompany(recordBuffer.MetaApplicationObject, companyToken);
		CheckCloudStateReadyReplicationAllowOperation(recordBuffer.MetaApplicationObject, companyToken);
		transactionalDataCache.OnBeforeInsert(companyToken, recordBuffer, out var transactionBumperToken, out var onAfterInsert);
		InsertResult insertResult = provider.Insert(companyToken, recordBuffer, allowBuffering, out var outputRecordBuffer);
		if (insertResult != InsertResult.Success)
		{
			newRecordBuffer = null;
			return insertResult;
		}
		newRecordBuffer = CreateNewBufferFromOutputBufferTransferBlobValuesFromOldRecord(recordBuffer, outputRecordBuffer, transactionBumperToken);
		onAfterInsert?.Invoke(outputRecordBuffer);
		tableVersionTokens.IncrementUpdateBumperToken(companyToken, recordBuffer.MetaApplicationObject.ObjectId);
		return insertResult;
	}

	public void ClearCache(bool clearGlobal)
	{
		transactionalDataCache.ClearCache(clearGlobal);
	}

	public void LockTable(NCLMetaTable table, int companyToken)
	{
		transactionalDataCache.LockTable(table, companyToken);
	}

	public bool IsTableLocked(int tableId, int companyToken)
	{
		return transactionalDataCache.GetLockState(tableId, companyToken) != DataLockState.None;
	}

	public DataLockState GetTableLockState(int tableId, int companyToken)
	{
		return transactionalDataCache.GetLockState(tableId, companyToken);
	}

	private ResultSetEnumerator InnerFind(FindCacheRequest request, bool fromPosition, Func<bool> onlyCurrentKeyNeededForNextRow)
	{
		if (request.FiltersAndMarks.MarkedRecords != null && request.FiltersAndMarks.MarkedRecords.IsCompleteExpressionLarge)
		{
			return MarkedRecordsHelper.FindWithManyMarks(this, request, fromPosition);
		}
		ValidateCompanyOpenOrNotPerCompany(request.MetaApplicationObject, request.CompanyToken);
		if (!request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		}
		int globalTableVersion = 0;
		int overallTableVersion = 0;
		ResultSet resultSet = null;
		Action<ResultSet> updateCache = null;
		IEnumerable<ReadOnlyRecordBuffer> enumerable = ReadOnlyRecordBuffer.EmptyArray;
		bool flag = request.AutoCalcFields != null && request.AutoCalcFields.Count > 0 && provider.SupportsAutoCalcFields;
		int transactionBumperToken;
		MutableRecordBuffer recordBuffer;
		DataLockState lockState;
		if (request.FiltersAndMarks.SetIsGuaranteedEmpty)
		{
			transactionBumperToken = 0;
		}
		else if (!flag && TryHandleAsPrimaryKeyLookup(request, cacheOnly: false, out recordBuffer))
		{
			if (recordBuffer == null)
			{
				transactionBumperToken = 0;
			}
			else
			{
				transactionBumperToken = recordBuffer.TransactionToken;
				enumerable = new ReadOnlyRecordBuffer[1] { recordBuffer.ReadOnlyBuffer };
			}
		}
		else if (!transactionalDataCache.TryFind(request, fromPosition, out resultSet, out transactionBumperToken, out lockState, out globalTableVersion, out overallTableVersion, out updateCache))
		{
			if (fromPosition)
			{
				PositionedFindCacheRequest positionedFindCacheRequest = (PositionedFindCacheRequest)request;
				PositionedFindProviderRequest request2 = new PositionedFindProviderRequest(positionedFindCacheRequest.CompanyToken, positionedFindCacheRequest.MetaApplicationObject, lockState, positionedFindCacheRequest.FiltersAndMarks, positionedFindCacheRequest.GlobalAndSecurityFilters, positionedFindCacheRequest.FlowFieldSecurityFiltering, positionedFindCacheRequest.AutoCalcFields, positionedFindCacheRequest.SortingFields, positionedFindCacheRequest.FindType, positionedFindCacheRequest.StartingPosition, positionedFindCacheRequest.IncludeCurrent, positionedFindCacheRequest.TopNumberOfRowsToReturn, positionedFindCacheRequest.SkipNumberOfRows, positionedFindCacheRequest.Timeout);
				enumerable = provider.FindFromPosition(request2, onlyCurrentKeyNeededForNextRow);
			}
			else
			{
				FindProviderRequest request3 = new FindProviderRequest(request.CompanyToken, request.MetaApplicationObject, lockState, request.FiltersAndMarks, request.GlobalAndSecurityFilters, request.FlowFieldSecurityFiltering, request.AutoCalcFields, request.SortingFields, request.FindType, request.TopNumberOfRowsToReturn, request.SkipNumberOfRows, request.Timeout);
				enumerable = provider.Find(request3, onlyCurrentKeyNeededForNextRow);
			}
		}
		if (resultSet == null)
		{
			resultSet = new ResultSet(enumerable.GetEnumerator(), request, overallTableVersion, globalTableVersion, provider.ShouldResultSetBufferRows);
		}
		updateCache?.Invoke(resultSet);
		bool dynamic = request.MetaApplicationObject.ApplicationObjectType == ObjectType.Table;
		Func<bool> isStillValidFunc = tableVersionTokens.CreateTableVersionStillValidFunction(request.CompanyToken, request.MetaApplicationObject.ObjectId, globalFilters, dynamic);
		return new ResultSetEnumerator(resultSet, session, this, transactionBumperToken, isStillValidFunc);
	}

	private void ValidateCompanyOpenOrNotPerCompany(NCLMetaApplicationObject requestMetaApplicationObject, int companyToken)
	{
		if (session.IsCompanyOpen || companyToken > 0)
		{
			return;
		}
		NCLMetaTable nCLMetaTable = requestMetaApplicationObject as NCLMetaTable;
		if (nCLMetaTable != null)
		{
			if (!nCLMetaTable.DataPerCompany)
			{
				return;
			}
		}
		else
		{
			NCLMetaQuery nCLMetaQuery = (NCLMetaQuery)requestMetaApplicationObject;
			NCLMetaQueryDataItem nCLMetaQueryDataItem = nCLMetaQuery.QueryDefinition.DataItems.FirstOrDefault((NCLMetaQueryDataItem di) => !di.MetaTable.DataPerCompany);
			if (nCLMetaQueryDataItem == null)
			{
				return;
			}
			nCLMetaTable = nCLMetaQueryDataItem.MetaTable;
		}
		throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotAccessTableWithNoCompanyOpen, nCLMetaTable.TableCaptions.GetValueOrDefault()));
	}

	private Exception GetStaleRecordException(int companyToken, MutableRecordBuffer inputRecordBuffer)
	{
		bool flag = TryGetByPrimaryKey(new RecordIdCacheRequest(companyToken, inputRecordBuffer.MetaApplicationObject, cacheable: true, FiltersAndMarks.Empty, SecurityFiltering.Ignored, inputRecordBuffer.ReadOnlyBuffer.GetRecordId(), cacheOnly: false), out var recordBuffer);
		long timestamp = recordBuffer.Timestamp.ToInt64();
		NavValue[] primaryKeyValues = inputRecordBuffer.MetaTable.PrimaryKey.KeyFieldsList.Select((NCLMetaField f) => inputRecordBuffer.GetOriginalValue(f.ColumnIndex)).ToArray();
		return ((!flag) ? TimestampLog.ContainsResult.DoesNotContain : timestampLog.ContainsTimestamp(timestamp)) switch
		{
			TimestampLog.ContainsResult.Contains => RecordImplementationHelper.GetRecordStaleModifiedByThisUserException(inputRecordBuffer.MetaTable, primaryKeyValues), 
			TimestampLog.ContainsResult.DoesNotContain => RecordImplementationHelper.GetRecordStaleModifiedByAnotherUserException(inputRecordBuffer.MetaTable, primaryKeyValues), 
			TimestampLog.ContainsResult.UnknownTooOld => RecordImplementationHelper.GetRecordStaleUnknownReasonException(inputRecordBuffer.MetaTable, primaryKeyValues), 
			_ => throw new NotSupportedException(), 
		};
	}

	private FieldDictionary<NavValue> CalculateFields<T, U>(T request, Func<DataLockState, U> createProviderRequest, Func<U, FieldDictionary<NavValue>> providerRequest) where T : DataCacheRequest where U : DataProviderRequest
	{
		CheckCloudStateReadyReplicationAllowOperation(request.MetaApplicationObject, request.CompanyToken);
		if (transactionalDataCache.TryCalculateFields(request, out var result, out var lockState, out var updateCache))
		{
			return result;
		}
		result = providerRequest(createProviderRequest(lockState));
		updateCache?.Invoke(result);
		return result;
	}

	private bool TryHandleAsPrimaryKeyLookup(DataCacheRequest request, bool cacheOnly, out MutableRecordBuffer recordBuffer)
	{
		recordBuffer = null;
		FilterFieldDictionary filters = request.FiltersAndMarks.Filters;
		if (filters.IsNullOrEmpty())
		{
			return false;
		}
		if (!(request.MetaApplicationObject is NCLMetaTable nCLMetaTable))
		{
			return false;
		}
		if (filters.Items.Length < nCLMetaTable.PrimaryKey.KeyFieldCount)
		{
			return false;
		}
		if (!nCLMetaTable.PrimaryKey.DefinedKeyFields.AreAllFieldsContainedInOtherAssumingSameParent(filters))
		{
			return false;
		}
		int keyFieldCount = nCLMetaTable.PrimaryKey.KeyFieldCount;
		int num = 0;
		for (int i = 0; i < filters.Items.Length; i++)
		{
			if (num >= keyFieldCount)
			{
				break;
			}
			Tuple<INavFieldMetadata, FilterExpression> tuple = filters.Items[i];
			if (tuple.Item1 != FilterFieldDictionary.MultipleFields && ((NCLMetaField)tuple.Item1).FieldIsPartOfPrimaryKey)
			{
				num++;
				if (tuple.Item2.ExpressionType != FilterExpressionType.Equal)
				{
					return false;
				}
			}
		}
		NavRecordId navRecordId = NavRecordId.CreateFromValues(nCLMetaTable.TableId, nCLMetaTable.PrimaryKey.KeyFieldsList.Select((NCLMetaField f) => ((UnaryFilterExpression)filters[f]).Value).ToArray());
		PositionedFindCacheRequest positionedFindCacheRequest = request as PositionedFindCacheRequest;
		bool flag = false;
		FilterExpression filter = null;
		if (positionedFindCacheRequest != null && positionedFindCacheRequest.StartingPosition != null)
		{
			filter = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(positionedFindCacheRequest.StartingPosition, positionedFindCacheRequest.SortingFields, positionedFindCacheRequest.IncludeCurrent);
			if (!positionedFindCacheRequest.SortingFields.Equals(nCLMetaTable.PrimaryKey.DefinedSortingFields))
			{
				flag = true;
				if (positionedFindCacheRequest.SortingFields.Any((INavSortingField field) => field.Field.FieldClass != FieldClass.Normal))
				{
					return false;
				}
			}
			else
			{
				NavValue[] array = new NavValue[nCLMetaTable.FieldCount];
				for (int num2 = 0; num2 < nCLMetaTable.PrimaryKey.KeyFieldCount; num2++)
				{
					NCLMetaField keyFieldByIndex = nCLMetaTable.PrimaryKey.GetKeyFieldByIndex(num2);
					array[keyFieldByIndex.ColumnIndex] = navRecordId.FieldValueByIndex(keyFieldByIndex, num2);
				}
				if (!new ReadOnlyRecordBuffer(nCLMetaTable, array).PassesFilter(filter))
				{
					recordBuffer = null;
					return true;
				}
			}
		}
		RecordIdCacheRequest request2 = new RecordIdCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, request.FiltersAndMarks, request.FlowFieldSecurityFiltering, navRecordId, cacheOnly);
		switch (InternalTryGetByPrimaryKey(request2, out recordBuffer))
		{
		case CacheLookupResult.Unknown:
			return false;
		case CacheLookupResult.Exists:
			if (flag && !recordBuffer.PassesFilter(filter))
			{
				recordBuffer = null;
				return false;
			}
			return true;
		case CacheLookupResult.DoesNotExist:
			return true;
		default:
			throw new NotSupportedException();
		}
	}

	private void CheckCloudStateReadyReplicationAllowOperation(NCLMetaApplicationObject requestObject, int companyToken)
	{
		if (!session.IsIntelligentCloudDataAccessBlockingEnabled)
		{
			return;
		}
		NavTenant tenant = session.Tenant;
		if (tenant == null)
		{
			return;
		}
		NavDatabase database = tenant.Database;
		if (database.DatabaseType != NavDatabaseType.Tenant || !TenantIsInProperStateForIntelligentCloud() || !tenant.IsIntelligentCloudReplicationEnabled || !session.IsCompanyOpen)
		{
			return;
		}
		string companyName = database.GetCompanyNameFromToken(companyToken);
		List<int> tablesInMetaObject = GetTablesInMetaObject(requestObject);
		foreach (int item in tablesInMetaObject)
		{
			if (item != 2000000173)
			{
				NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(item);
				List<string> extensionsTablesFullSqlNames = GetExtensionsTablesFullSqlNames(metaTableById, database, companyToken);
				extensionsTablesFullSqlNames.Add(GetMainTableFullSqlName(metaTableById, database, companyToken));
				if (extensionsTablesFullSqlNames.Any((string tName) => tenant.CloudReadyReplicationStatusCache.IsTableStatusBlocked(tName, companyName)))
				{
					string intelligentCloudBlockedTable = Lang.IntelligentCloudBlockedTable;
					NavALException ex = new NavALException(string.Format(CultureInfo.InvariantCulture, intelligentCloudBlockedTable, NavGlobal.NCLMetadata.GetTableNameByTableId(item)));
					LogWriterHelper.LogEvent(EventLogEntryType.Error, ex.Message);
					DiagnosticsResolver.GetMostSpecificInstance()?.SendExceptionTag("000028C", Category.CloudReady, ex);
					throw new NavALException(string.Format(CultureInfo.CurrentCulture, Lang.IntelligentCloudBlockedTable, NavGlobal.NCLMetadata.GetTableNameByTableId(item)));
				}
			}
		}
	}

	private static string GetMainTableFullSqlName(NCLMetaTable tableMetadata, NavDatabase database, int companyToken)
	{
		StringBuilder output = new StringBuilder();
		output = NavSqlStatementHelper.AppendTableNameCompanyQualified(output, tableMetadata, database, companyToken, NavAppTableContext.Main);
		return output.ToString();
	}

	private static List<string> GetExtensionsTablesFullSqlNames(NCLMetaTable tableMetadata, NavDatabase database, int companyToken)
	{
		List<string> list = new List<string>();
		foreach (NavAppTableContext item in tableMetadata.DistinctCompanionTableOrigins.Select((NavAppOrigin o) => NavAppTableContext.Companion(o)))
		{
			StringBuilder output = new StringBuilder();
			output = NavSqlStatementHelper.AppendTableNameCompanyQualified(output, tableMetadata, database, companyToken, item);
			list.Add(output.ToString());
		}
		return list;
	}

	private bool TenantIsInProperStateForIntelligentCloud()
	{
		return session.Tenant.State == NavTenantState.Operational;
	}

	private static List<int> GetTablesInMetaObject(NCLMetaApplicationObject metadataObject)
	{
		List<int> list = new List<int>();
		NCLMetaTable nCLMetaTable = metadataObject as NCLMetaTable;
		NCLMetaQuery nCLMetaQuery = metadataObject as NCLMetaQuery;
		if (nCLMetaTable != null && nCLMetaTable.TableType == TableType.Normal)
		{
			list.Add(nCLMetaTable.TableId);
		}
		else if (nCLMetaTable != null && nCLMetaTable.TableType == TableType.Query)
		{
			nCLMetaQuery = NavGlobal.NCLMetadata.GetMetaQueryById(nCLMetaTable.ObjectId);
		}
		if (nCLMetaQuery != null)
		{
			foreach (MetaQueryDataItem item in nCLMetaQuery.MetaQuery?.DataItems)
			{
				list.Add(item.TableNo);
			}
		}
		return list;
	}
}
