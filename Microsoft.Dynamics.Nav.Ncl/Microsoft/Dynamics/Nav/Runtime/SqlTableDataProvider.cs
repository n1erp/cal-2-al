using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SqlTableDataProvider : DataProvider
{
	private readonly NavSession navSession;

	private readonly NavDatabase database;

	private const int MaxSqlParameteresPerExecute = 200;

	private const int MaxRowsPerInsert = 5;

	private NavSqlConnectionScope currentSqlConnectionScope;

	private static readonly NavBigInteger PlaceHolderTimestampForBufferedInsert = NavBigInteger.Create(long.MaxValue);

	private bool? tempLockTimeoutEnabled;

	private Dictionary<int, List<ReadOnlyRecordBuffer>> bufferedInserts;

	private int bufferCompanyToken;

	public override bool IsDataModifiedInSqlServer => true;

	public bool LockTimeoutEnabled
	{
		get
		{
			if (currentSqlConnectionScope != null)
			{
				return currentSqlConnectionScope.Connection.LockTimeoutEnabled;
			}
			if (tempLockTimeoutEnabled.HasValue)
			{
				return tempLockTimeoutEnabled.Value;
			}
			return true;
		}
		set
		{
			if (currentSqlConnectionScope != null)
			{
				currentSqlConnectionScope.Connection.SetLockTimeout(value);
			}
			else
			{
				tempLockTimeoutEnabled = value;
			}
		}
	}

	protected NavSession Session => navSession;

	public override bool SupportsAutoCalcFields => !ServerUserSettings.Instance.DisableSmartSql.Value;

	public SqlTableDataProvider(NavSession session, NavDatabase database)
	{
		DataProvider.ValidateNotNull(session, "connection");
		DataProvider.ValidateNotNull(database, "database");
		navSession = session;
		this.database = database;
	}

	public static int BufferedInsertRowCount(NCLMetaTable table)
	{
		return Math.Min(200 / table.SqlReadableFields.Count + 1, 5);
	}

	private static bool BufferedInsertsAllowed(NCLMetaTable table)
	{
		if (table.AutoIncrementField == null && table.BlobFieldCount == 0 && !table.IsAppArchiveTable)
		{
			return table.TableId != 2000000006;
		}
		return false;
	}

	private InsertResult AddBufferedRow(MutableRecordBuffer newRow, int companyToken, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		InsertResult result = InsertResult.Success;
		if (bufferedInserts == null)
		{
			bufferedInserts = new Dictionary<int, List<ReadOnlyRecordBuffer>>();
			bufferCompanyToken = companyToken;
		}
		else if (bufferCompanyToken != companyToken)
		{
			result = FlushBufferedRows();
			bufferCompanyToken = companyToken;
		}
		List<ReadOnlyRecordBuffer> value = null;
		if (!bufferedInserts.TryGetValue(newRow.MetaTable.TableId, out value))
		{
			value = new List<ReadOnlyRecordBuffer>();
			bufferedInserts.Add(newRow.MetaTable.TableId, value);
		}
		NavValue[] array = newRow.ToArray();
		array[newRow.MetaTable.TimestampField.FieldIndex] = PlaceHolderTimestampForBufferedInsert;
		outputRecordBuffer = new ReadOnlyRecordBuffer(newRow.MetaApplicationObject, array);
		value.Add(outputRecordBuffer);
		if (value.Count >= BufferedInsertRowCount(newRow.MetaTable))
		{
			result = FlushBufferedRows(newRow.MetaTable);
		}
		return result;
	}

	private InsertResult FlushBufferedRows(NCLMetaTable table)
	{
		if (bufferedInserts != null && bufferedInserts.Count > 0 && BufferedInsertsAllowed(table))
		{
			List<ReadOnlyRecordBuffer> value = null;
			if (bufferedInserts.TryGetValue(table.TableId, out value))
			{
				List<ReadOnlyRecordBuffer> recordSet = new List<ReadOnlyRecordBuffer>(BufferedInsertRowCount(table));
				DoFlush(table, value, recordSet);
				bufferedInserts.Remove(table.TableId);
			}
		}
		return InsertResult.Success;
	}

	private InsertResult FlushBufferedRows()
	{
		if (bufferedInserts != null && bufferedInserts.Count > 0)
		{
			foreach (List<ReadOnlyRecordBuffer> value in bufferedInserts.Values)
			{
				NCLMetaTable table = value[0].MetaApplicationObject as NCLMetaTable;
				List<ReadOnlyRecordBuffer> recordSet = new List<ReadOnlyRecordBuffer>(BufferedInsertRowCount(table));
				DoFlush(table, value, recordSet);
			}
			bufferedInserts.Clear();
		}
		return InsertResult.Success;
	}

	private void DoFlush(NCLMetaTable table, List<ReadOnlyRecordBuffer> bufferedRows, List<ReadOnlyRecordBuffer> recordSet)
	{
		int num = 0;
		NavBigInteger timestamp = null;
		foreach (ReadOnlyRecordBuffer bufferedRow in bufferedRows)
		{
			num++;
			recordSet.Add(bufferedRow);
			if (num % BufferedInsertRowCount(table) == 0)
			{
				timestamp = InsertRecordSet(table, recordSet);
			}
		}
		if (recordSet.Count > 0)
		{
			timestamp = InsertRecordSet(table, recordSet);
		}
		foreach (ReadOnlyRecordBuffer bufferedRow2 in bufferedRows)
		{
			bufferedRow2.SetTimestamp(timestamp);
		}
	}

	private NavBigInteger InsertRecordSet(NCLMetaTable table, List<ReadOnlyRecordBuffer> recordSet)
	{
		TraceAlCallStack();
		NavSqlDmlCommand.InsertRecordSet(database, table, bufferCompanyToken, recordSet, retrieveTimestamp: true, out var timestampValue);
		recordSet.Clear();
		return timestampValue;
	}

	private void ClearBufferedRows()
	{
		if (bufferedInserts != null && bufferedInserts.Count > 0)
		{
			bufferedInserts.Clear();
		}
	}

	private void RegisterTableDataChange(int companyToken, NCLMetaApplicationObject metaApplicationObject)
	{
		database.CacheSynchronization.RegisterTableDataChanged(metaApplicationObject.ObjectId, companyToken);
	}

	private static ReadOnlyRecordBuffer CreateOutputRecordBufferWithUpdatedTimestampAndIdentity(MutableRecordBuffer inputRecordBuffer, NavValue identityValue, NavValue timestampValue)
	{
		NCLMetaTable metaTable = inputRecordBuffer.MetaTable;
		NavValue[] array = inputRecordBuffer.ToArray();
		array[metaTable.TimestampField.FieldIndex] = timestampValue;
		if (metaTable.UserDefinedTimestampField != null && metaTable.SqlHasTimestamp)
		{
			array[metaTable.UserDefinedTimestampField.FieldIndex] = timestampValue;
		}
		if (metaTable.AutoIncrementField != null && identityValue != null)
		{
			array[metaTable.AutoIncrementField.FieldIndex] = identityValue;
		}
		if (metaTable.BlobFieldCount > 0)
		{
			NCLMetaField[] blobFields = metaTable.BlobFields;
			foreach (NCLMetaField nCLMetaField in blobFields)
			{
				NavBLOB navBLOB = (NavBLOB)array[nCLMetaField.FieldIndex];
				if (navBLOB != null)
				{
					array[nCLMetaField.FieldIndex] = new NavBLOB(navBLOB.GetLength());
				}
			}
		}
		return new ReadOnlyRecordBuffer(metaTable, array);
	}

	private void TraceAlCallStack()
	{
		navSession.SqlProfilerCommentProvider.WriteAlCallStackToSql(database);
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		DataProvider.ValidateNotNull(request, "request");
		DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows((NCLMetaTable)request.MetaApplicationObject);
		recordBuffer = NavSqlIsamQueryCommand.FindEqual(database, request);
		if (recordBuffer != null && FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks) && !FlowFieldsHelper.RecordIsWithinFilteredFlowFields(navSession, request.CompanyToken, recordBuffer, request.FiltersAndMarks, request.FlowFieldSecurityFiltering))
		{
			recordBuffer = null;
		}
		return recordBuffer != null;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		DataProvider.ValidateNotNull(request, "request");
		TraceAlCallStack();
		return FindFromPosition(new PositionedFindProviderRequest(request, null, includeCurrent: false), onlyCurrentKeyNeededForNextRow);
	}

	private IEnumerable<ReadOnlyRecordBuffer> TableFindFromPosition(NCLMetaTable table, PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow)
	{
		CheckIfAnySortingOnFlowFieldsWithValidatedOptionSecurityFiltering(table, request);
		Tuple<NCLMetaField, bool>[] fieldsToAutoCalculate = SqlTableDataProviderHelper.GetFieldsToAutoCalculate(request);
		if (table.TableType == TableType.Normal && fieldsToAutoCalculate != null && fieldsToAutoCalculate.Length != 0 && (request.FlowFieldSecurityFiltering != SecurityFiltering.Validated || navSession.Permissions.IsSuperForAllCompanies || navSession.Permissions.IsSuperInCurrentCompany(request.CompanyToken) || !navSession.Database.SecurityAndLicense.IsSecurityFiltersActive))
		{
			Tuple<NCLMetaField, bool>[] flowFieldsWithNonstandardJoinCriteria = null;
			Tuple<NCLMetaField, bool>[] nonCalculableFlowFields = null;
			Tuple<NCLMetaField, bool>[] flowFieldsBeingCalculatedByQuery = null;
			NCLMetaQueryDefinition queryDefinition = (ServerUserSettings.Instance.DisableSmartSql.Value ? null : SqlTableDataProviderHelper.BuildQueryDefinitionForAutoCalculationOfFlowFields(navSession, table, request.FiltersAndMarks, request.CompanyToken, request.LockState, request.SortingFields, fieldsToAutoCalculate, out flowFieldsWithNonstandardJoinCriteria, out nonCalculableFlowFields, out flowFieldsBeingCalculatedByQuery, request.FlowFieldSecurityFiltering == SecurityFiltering.Validated));
			if (queryDefinition != null)
			{
				FiltersAndMarks queryFilters = SqlTableDataProviderHelper.BuildQueryFiltersFromFilteredFlowFields(queryDefinition, flowFieldsBeingCalculatedByQuery, request.FiltersAndMarks.Filters);
				foreach (NCLMetaTable includedTable in queryDefinition.IncludedTables)
				{
					FlushBufferedRows(includedTable);
				}
				FindType findType = ((request.FindType != FindType.FirstOnly || flowFieldsWithNonstandardJoinCriteria == null) ? request.FindType : FindType.Normal);
				int num = request.CompanyToken;
				if (num == 0 && queryDefinition.IncludedTables.Any((NCLMetaTable includedTable) => includedTable.DataPerCompany))
				{
					num = Session.Company.CompanyNameToken;
				}
				IEnumerable<ReadOnlyRecordBuffer> staticResultSet = NavSqlQueryCommand.Query(queryDefinition, database, num, request.TopNumberOfRowsToReturn, request.SkipNumberOfRows, queryFilters, request.GlobalAndSecurityFilters, request.IncludeCurrent, findType, request.StartingPosition, request.FlowFieldSecurityFiltering, addExplicitJoinHints: false, request.Timeout);
				staticResultSet = new DynamicQueryResultSetCollection(queryDefinition, this, num, request.TopNumberOfRowsToReturn, request.SkipNumberOfRows, queryFilters, request.GlobalAndSecurityFilters, navSession, request.FlowFieldSecurityFiltering, staticResultSet).Select((ReadOnlyRecordBuffer recordBuffer) => SqlTableDataProviderHelper.CreateTableBasedRecordBufferFromQueryResult(queryDefinition.DataItems[0], recordBuffer));
				if (flowFieldsWithNonstandardJoinCriteria != null)
				{
					staticResultSet = from row in staticResultSet
						select SqlTableDataProviderHelper.ValidateFiltersAndValuesForAutoCalculatedFlowFields(navSession, request, row, flowFieldsWithNonstandardJoinCriteria, calculateDependingOnNonstandardJoinCriteriaCondition: true) into row
						where row != null
						select row;
				}
				if (nonCalculableFlowFields != null)
				{
					staticResultSet = from row in staticResultSet
						select SqlTableDataProviderHelper.ValidateFiltersAndValuesForAutoCalculatedFlowFields(navSession, request, row, nonCalculableFlowFields, calculateDependingOnNonstandardJoinCriteriaCondition: false) into row
						where row != null
						select row;
				}
				return staticResultSet;
			}
		}
		bool flag = FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks);
		bool flag2 = request.FindType == FindType.FirstOnly && !flag;
		FlushBufferedRows(table);
		IEnumerable<ReadOnlyRecordBuffer> enumerable;
		if (ServerUserSettings.Instance.UseQueryForFind.Value)
		{
			NCLMetaQueryDefinition queryDefinition2 = SqlTableDataProviderHelper.BuildSimpleTableBasedQueryDefinition(table, request.FiltersAndMarks, request.LockState, flag2, request.SortingFields);
			enumerable = from recordBuffer in NavSqlQueryCommand.Query(queryDefinition2, database, request.CompanyToken, request.TopNumberOfRowsToReturn, request.SkipNumberOfRows, null, null, request.IncludeCurrent, request.FindType, request.StartingPosition, SecurityFiltering.Ignored, addExplicitJoinHints: false, request.Timeout)
				select SqlTableDataProviderHelper.CreateTableBasedRecordBufferFromQueryResult(queryDefinition2.DataItems[0], recordBuffer);
		}
		else if (flag2)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindTop1(database, request, onlyCurrentKeyNeededForNextRow?.Invoke() ?? false);
			enumerable = ((readOnlyRecordBuffer != null) ? new ReadOnlyRecordBuffer[1] { readOnlyRecordBuffer } : new ReadOnlyRecordBuffer[0]);
		}
		else
		{
			enumerable = NavSqlIsamQueryCommand.FindAll(database, request, onlyCurrentKeyNeededForNextRow);
		}
		if (fieldsToAutoCalculate != null && fieldsToAutoCalculate.Length != 0)
		{
			enumerable = from row in enumerable
				select SqlTableDataProviderHelper.ValidateFiltersAndValuesForAutoCalculatedFlowFields(navSession, request, row, fieldsToAutoCalculate, calculateDependingOnNonstandardJoinCriteriaCondition: false) into row
				where row != null
				select row;
			if (request.FindType == FindType.FirstOnly)
			{
				enumerable = enumerable.Take(1);
			}
		}
		return enumerable;
	}

	private void CheckIfAnySortingOnFlowFieldsWithValidatedOptionSecurityFiltering(NCLMetaTable table, FindProviderRequest request)
	{
		if (request.SortingFields != null && request.FlowFieldSecurityFiltering == SecurityFiltering.Validated && !navSession.Permissions.IsSuperForAllCompanies && navSession.Database.SecurityAndLicense.IsSecurityFiltersActive)
		{
			IList<NCLMetaField> list = (from field in request.SortingFields
				select (NCLMetaField)field.Field into field
				where field.FieldClass == FieldClass.FlowField
				select field).ToList();
			if (list.Count > 0 && (!navSession.Permissions.IsSuperInCurrentCompany(request.CompanyToken) || !AreAllFlowFieldTablesPerCompany(request.SortingFields)))
			{
				throw RecordImplementationHelper.GetCannotSortOnCertainFieldsException(table, list, Lang.CannotSortOnFlowFieldsOnRecordsWithValidatedSecurityFilter);
			}
		}
	}

	private static bool AreAllFlowFieldTablesPerCompany(SortingFieldList sortingFields)
	{
		return sortingFields.OfType<NCLMetaField>().All((NCLMetaField f) => f.FieldClass == FieldClass.FlowField && f.CalculationFormula.SourceField.Parent.DataPerCompany);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		DataProvider.ValidateNotNull(request, "request");
		TraceAlCallStack();
		if (request.MetaApplicationObject is NCLMetaTable table)
		{
			return TableFindFromPosition(table, request, onlyCurrentKeyNeededForNextRow);
		}
		if (!(request.MetaApplicationObject is NCLMetaQuery nCLMetaQuery))
		{
			throw new ArgumentOutOfRangeException("request");
		}
		return GetQueryResult(nCLMetaQuery.QueryDefinition, request.CompanyToken, request.TopNumberOfRowsToReturn, request.SkipNumberOfRows, request.FiltersAndMarks, request.GlobalAndSecurityFilters, request.StartingPosition, request.FlowFieldSecurityFiltering, addExplicitJoinHints: true, request.IncludeCurrent, request.SortingFields);
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks))
		{
			return Find(new FindProviderRequest(request, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.Normal)).Any();
		}
		return NavSqlExistsCommand.Exists(database, nCLMetaTable, request.CompanyToken, request.FiltersAndMarks, request.LockState);
	}

	public override int Count(DataProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks))
		{
			return Find(new FindProviderRequest(request, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.Normal)).Count();
		}
		return NavSqlAggregateCommand.AggregateCount(database, nCLMetaTable, request.CompanyToken, request.FiltersAndMarks, request.LockState);
	}

	public override FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks))
		{
			return base.CalcNumeric(request);
		}
		return NavSqlAggregateCommand.AggregateFlowFields(database, nCLMetaTable, request.CompanyToken, request.FiltersAndMarks, request.FieldsToCalculate, request.LockState == DataLockState.LockExclusive);
	}

	public override FieldDictionary<NavValue> CalcMinMax(CalcMinMaxProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks))
		{
			return base.CalcMinMax(request);
		}
		return NavSqlAggregateCommand.AggregateFlowFields(database, nCLMetaTable, request.CompanyToken, request.FiltersAndMarks, request.FieldsToCalculate, request.LockState == DataLockState.LockExclusive);
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable table = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(table);
		return NavSqlBlobCommand.GetBlob(table, database, request.CompanyToken, request.BlobsToGet, request.RecordBuffer, request.LockState);
	}

	public override FieldDictionary<NavValue> CalcSums(CalcSumsProviderRequest request)
	{
		DataProvider.ValidateNotNull(request, "request");
		NCLMetaTable table = DataProvider.ValidateIsTable(request.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(table);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks))
		{
			return base.CalcSums(request);
		}
		return NavSqlAggregateCommand.AggregateNonFlowFields(database, table, request.CompanyToken, request.FiltersAndMarks, request.LockState, NCLMetaCalculationMethod.Sum, request.FieldsToCalculate);
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		DataProvider.ValidateNotNull(recordBuffer, "recordBuffer");
		DataProvider.ValidateIsTable(recordBuffer.MetaApplicationObject);
		RegisterTableDataChange(companyToken, recordBuffer.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(recordBuffer.MetaTable);
		ModifyResult result = NavSqlDmlCommand.Modify(database, companyToken, recordBuffer, recordHasBeenReadWithLockInsideThisTransaction, out var timestampValue, throwOnDuplicate: true);
		outputRecordBuffer = CreateOutputRecordBufferWithUpdatedTimestampAndIdentity(recordBuffer, null, timestampValue);
		return result;
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		DataProvider.ValidateNotNull(metaTable, "metaApplicationObject");
		DataProvider.ValidateNotNull(filtersAndMarks, "filterExpressions");
		DataProvider.ValidateNotNull(newFieldValues, "newFieldValues");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(metaTable);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		RegisterTableDataChange(companyToken, metaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(filtersAndMarks))
		{
			FindProviderRequest request = new FindProviderRequest(companyToken, metaTable, DataLockState.LockExclusive, filtersAndMarks, null, flowFieldSecurityFiltering, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.All);
			{
				foreach (ReadOnlyRecordBuffer item in Find(request))
				{
					MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(item);
					Tuple<INavFieldMetadata, NavValue>[] items = newFieldValues.Items;
					foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
					{
						mutableRecordBuffer[tuple.Item1.ColumnIndex] = tuple.Item2;
					}
					Modify(companyToken, mutableRecordBuffer, recordHasBeenReadWithLockInsideThisTransaction: true, out var _);
				}
				return;
			}
		}
		NavSqlDmlCommand.ModifyMultiple(database, nCLMetaTable, companyToken, newFieldValues, filtersAndMarks);
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		DataProvider.ValidateNotNull(recordBuffer, "recordBuffer");
		DataProvider.ValidateIsTable(recordBuffer.MetaApplicationObject);
		RegisterTableDataChange(companyToken, recordBuffer.MetaApplicationObject);
		TraceAlCallStack();
		FlushBufferedRows(recordBuffer.MetaTable);
		return NavSqlDmlCommand.Delete(database, recordBuffer.MetaTable, companyToken, recordBuffer, recordHasBeenReadWithLockInsideThisTransaction);
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		DataProvider.ValidateNotNull(metaTable, "metaApplicationObject");
		DataProvider.ValidateNotNull(filtersAndMarks, "filterExpressions");
		NCLMetaTable nCLMetaTable = DataProvider.ValidateIsTable(metaTable);
		TraceAlCallStack();
		FlushBufferedRows(nCLMetaTable);
		RegisterTableDataChange(companyToken, metaTable);
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(filtersAndMarks))
		{
			FindProviderRequest request = new FindProviderRequest(companyToken, metaTable, DataLockState.LockExclusive, filtersAndMarks, null, securityFiltering, FieldList.Empty, nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.All);
			{
				foreach (ReadOnlyRecordBuffer item in Find(request))
				{
					Delete(companyToken, new MutableRecordBuffer(item), recordHasBeenReadWithLockInsideThisTransaction: true);
				}
				return;
			}
		}
		NavSqlDmlCommand.DeleteMultiple(database, nCLMetaTable, companyToken, filtersAndMarks, NavAppTableContext.Main);
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		DataProvider.ValidateNotNull(recordBuffer, "recordBuffer");
		DataProvider.ValidateIsTable(recordBuffer.MetaApplicationObject);
		RegisterTableDataChange(companyToken, recordBuffer.MetaApplicationObject);
		if (BufferedInsertsAllowed(recordBuffer.MetaTable) && BufferedInsertRowCount(recordBuffer.MetaTable) > 1)
		{
			if (allowBuffering && ServerUserSettings.Instance.BufferedInsertEnabled.Value)
			{
				return AddBufferedRow(recordBuffer, companyToken, out outputRecordBuffer);
			}
			FlushBufferedRows(recordBuffer.MetaTable);
		}
		NavValue identityValue = null;
		TraceAlCallStack();
		if (database.HasSequencedAutoIncrementField(recordBuffer.MetaTable, NavAppTableContext.Main))
		{
			NCLMetaField autoIncrementField = recordBuffer.MetaTable.AutoIncrementField;
			if (autoIncrementField != null)
			{
				identityValue = recordBuffer[autoIncrementField.FieldIndex];
				CompanyId? companyId = database.GetCompanyId(recordBuffer.MetaTable, companyToken);
				if (companyId.HasValue)
				{
					database.CheckTableInSync(recordBuffer.MetaTable);
					if (identityValue != null && identityValue.ToInt64() != 0L)
					{
						database.SequenceProvider.UpdateSequenceCurrentValue(recordBuffer.MetaTable, autoIncrementField, companyId.Value, identityValue);
					}
					else
					{
						identityValue = database.SequenceProvider.GetNextAutoIncrementValue(recordBuffer.MetaTable, companyId.Value);
						recordBuffer[autoIncrementField.FieldIndex] = identityValue;
					}
				}
			}
		}
		InsertResult result = NavSqlDmlCommand.Insert(database, recordBuffer.MetaTable, companyToken, recordBuffer, ref identityValue, out var timestampValue, throwOnDuplicate: true);
		outputRecordBuffer = CreateOutputRecordBufferWithUpdatedTimestampAndIdentity(recordBuffer, identityValue, timestampValue);
		return result;
	}

	public override void BeginTransaction(TransactionType transactionType)
	{
		if (currentSqlConnectionScope != null)
		{
			throw new InvalidOperationException("A transaction has already been started");
		}
		currentSqlConnectionScope = NavSqlConnectionScope.Create(database, transactionType);
		if (tempLockTimeoutEnabled.HasValue)
		{
			currentSqlConnectionScope.Connection.SetLockTimeout(tempLockTimeoutEnabled.Value);
			tempLockTimeoutEnabled = null;
		}
	}

	public override void EndTransaction(bool commit)
	{
		if (currentSqlConnectionScope == null)
		{
			throw new InvalidOperationException("A transaction was not started");
		}
		try
		{
			if (commit)
			{
				commit = false;
				FlushBufferedRows();
				commit = true;
			}
		}
		finally
		{
			try
			{
				currentSqlConnectionScope.TransactionEnd((!commit) ? NavSqlEndTransactionType.Rollback : NavSqlEndTransactionType.Commit);
			}
			finally
			{
				currentSqlConnectionScope = null;
				ClearBufferedRows();
			}
		}
	}

	public override void FlushBufferedWrites(NCLMetaApplicationObject applicationObject)
	{
		NCLMetaTable table = DataProvider.ValidateIsTable(applicationObject);
		FlushBufferedRows(table);
	}

	public override void FlushBufferedWrites(MutableRecordBuffer recordBuffer)
	{
		if (recordBuffer.Timestamp == PlaceHolderTimestampForBufferedInsert)
		{
			FlushBufferedRows(recordBuffer.MetaTable);
		}
	}

	public override void FlushBufferedWrites()
	{
		FlushBufferedRows();
	}

	public override int GetQueryCount(NCLMetaQueryDefinition queryDefinition, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints = false)
	{
		if (queryDefinition == null)
		{
			throw new ArgumentOutOfRangeException("queryDefinition");
		}
		TraceAlCallStack();
		foreach (NCLMetaTable includedTable in queryDefinition.IncludedTables)
		{
			FlushBufferedRows(includedTable);
		}
		return NavSqlQueryCommand.QueryCount(queryDefinition, database, companyToken, topNumberOfRows, skipNumberOfRows, queryFilters, globalAndSecurityFilters, includeCurrent: false, FindType.All, startingPosition, securityFiltering, addExplicitJoinHints);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> GetQueryResult(NCLMetaQueryDefinition queryDefinition, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints = false, bool includeCurrent = false, SortingFieldList sortingFields = null)
	{
		if (queryDefinition == null)
		{
			throw new ArgumentOutOfRangeException("queryDefinition");
		}
		TraceAlCallStack();
		foreach (NCLMetaTable includedTable in queryDefinition.IncludedTables)
		{
			FlushBufferedRows(includedTable);
		}
		return NavSqlQueryCommand.Query(queryDefinition, database, companyToken, topNumberOfRows, skipNumberOfRows, queryFilters, globalAndSecurityFilters, includeCurrent, FindType.All, startingPosition, securityFiltering, addExplicitJoinHints, null, sortingFields);
	}
}
