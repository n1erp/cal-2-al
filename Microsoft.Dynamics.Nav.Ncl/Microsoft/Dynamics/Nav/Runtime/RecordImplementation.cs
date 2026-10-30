using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class RecordImplementation : TreeObject
{
	[Flags]
	private enum AutoCalcFieldsState
	{
		NotCalculated = 0,
		NonSmartSqlFlowFieldsCalculated = 1,
		SmartSqlFlowFieldsCalculated = 2,
		AllCalculated = NonSmartSqlFlowFieldsCalculated | SmartSqlFlowFieldsCalculated
	}

	private class ManagedRecordState : RecordState
	{
		private RecordImplementation recordImplementation;

		private MutableRecordBuffer backupRecordBuffer;

		private TableState backupTableState;

		private ResultSetEnumerator backupEnumerator;

		private int backupTableStateBumperToken;

		private bool backupEnumeratorReversed;

		public ManagedRecordState(RecordImplementation recordImplementation)
		{
			this.recordImplementation = recordImplementation;
		}

		public override void BackupTableAndRecordHandle()
		{
			if (backupRecordBuffer != null || backupTableState != null)
			{
				throw new RecordStateException();
			}
			BackupRecordHandle();
			backupTableState = recordImplementation.tableState.Clone();
		}

		public override void BackupRecordHandle()
		{
			if (backupRecordBuffer != null)
			{
				throw new RecordStateException();
			}
			backupRecordBuffer = recordImplementation.CurrentMutableRecordBufferOrDefault.Clone();
			backupTableStateBumperToken = recordImplementation.tableState.VersionBumperToken;
			if (recordImplementation.resultSetEnumerator != null)
			{
				backupEnumerator = recordImplementation.resultSetEnumerator.Clone();
				backupEnumeratorReversed = recordImplementation.resultSetReversed;
			}
		}

		public override void RevertBackupHandles()
		{
			backupTableState = null;
			backupRecordBuffer = null;
		}

		public override void RestoreRecord()
		{
			if (backupRecordBuffer != null)
			{
				recordImplementation.mutableRecordBuffer = backupRecordBuffer;
			}
			if (backupTableState != null && backupTableStateBumperToken != recordImplementation.tableState.VersionBumperToken)
			{
				recordImplementation.tableState.CopyFrom(backupTableState);
			}
			if (backupEnumerator != null && (backupTableState != null || backupTableStateBumperToken == recordImplementation.tableState.VersionBumperToken))
			{
				recordImplementation.resultSetEnumerator = backupEnumerator;
				recordImplementation.resultSetReversed = backupEnumeratorReversed;
			}
			else
			{
				recordImplementation.InvalidateCurrentResultSetEnumerator();
			}
			RevertBackupHandles();
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				RevertBackupHandles();
			}
			finally
			{
				recordImplementation = null;
			}
		}
	}

	private const bool DefaultAllowCachedResults = true;

	private DataAccess dataAccess;

	private NCLMetaTable metaTable;

	private TableState tableState;

	private bool initialized;

	private bool resultSetReversed;

	private AutoCalcFieldsState autoCalcFieldsState;

	private SecurityFiltering securityFiltering;

	private SecurityFiltering orginalFiltering;

	private FilterFieldDictionary validatedSecurityFilters;

	private MutableRecordBuffer mutableRecordBuffer;

	private ResultSetEnumerator resultSetEnumerator;

	private bool areOnlyKeyFieldsNeededForNextRow;

	private readonly Func<bool> areOnlyKeyFieldsNeededForNextRowDelegate;

	private readonly NavRecord parentRecord;

	private NavMediaPurger navMediaPurger;

	public NavMediaPurger MediaPurger => navMediaPurger ?? (navMediaPurger = new NavMediaPurger(this, metaTable));

	public DataAccess DataAccess => dataAccess;

	public int? SearchTimeout { get; set; }

	internal TableState TableState
	{
		[DebuggerStepThrough]
		get
		{
			return tableState;
		}
	}

	private NavSession Session => parentRecord.Session;

	private MutableRecordBuffer DefaultMutableRecordBuffer => new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTable, new NavValue[metaTable.FieldCount]));

	private MutableRecordBuffer CurrentMutableRecordBufferOrDefault => mutableRecordBuffer ?? DefaultMutableRecordBuffer;

	internal NavTableInstanceIdentifier InstanceIdentifier
	{
		get
		{
			if (!parentRecord.IsTemporary)
			{
				return new NavTableInstanceIdentifier(metaTable.TableId, isTemporary: false);
			}
			return new ManagedTemporaryNavTableInstanceIdentifier(metaTable.TableId, dataAccess);
		}
	}

	internal bool DataPerCompany => metaTable.DataPerCompany;

	internal bool IsOpen
	{
		get
		{
			if (!IsDisposed)
			{
				return initialized;
			}
			return false;
		}
	}

	public int[] CurrentSortingFieldIds => tableState.CurrentSortingFieldIds;

	public SecurityFiltering SecurityFiltering
	{
		get
		{
			return securityFiltering;
		}
		set
		{
			if (securityFiltering != value)
			{
				ChangeSecurityFiltering(value);
			}
		}
	}

	public RecordImplementation(NavRecord parentRecord)
		: base(parentRecord)
	{
		this.parentRecord = parentRecord;
		areOnlyKeyFieldsNeededForNextRowDelegate = AreOnlyKeyFieldsNeededForNextRow;
	}

	internal ReadOnlyRecordBuffer CopyOfCurrentReadOnlyRecordBuffer()
	{
		return new ReadOnlyRecordBuffer(CurrentMutableRecordBufferOrDefault.MetaApplicationObject, onlyCurrentKey: false, CurrentMutableRecordBufferOrDefault.ToArray());
	}

	private void EnsurePrimaryKeyFieldsSetOnBuffer(MutableRecordBuffer recordBuffer)
	{
		NCLMetaKey primaryKey = metaTable.PrimaryKey;
		for (int i = 0; i < primaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
			if (recordBuffer[keyFieldByIndex.FieldIndex] == null)
			{
				recordBuffer[keyFieldByIndex.FieldIndex] = keyFieldByIndex.InitValue;
			}
		}
	}

	internal bool ArePrimaryKeyFieldsSetToInitialValue()
	{
		MutableRecordBuffer currentMutableRecordBufferOrDefault = CurrentMutableRecordBufferOrDefault;
		NCLMetaKey primaryKey = metaTable.PrimaryKey;
		for (int i = 0; i < primaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
			if (currentMutableRecordBufferOrDefault[keyFieldByIndex.FieldIndex] != null && currentMutableRecordBufferOrDefault[keyFieldByIndex.FieldIndex] != keyFieldByIndex.InitValue)
			{
				return false;
			}
		}
		return true;
	}

	private MutableRecordBuffer EnsurePrimaryKeyFieldsSetOnlyInReadOnlyPartOfBuffer(MutableRecordBuffer inputBuffer, bool forModify)
	{
		NavValue[] array = null;
		NCLMetaKey primaryKey = metaTable.PrimaryKey;
		MutableRecordBuffer mutableRecordBuffer = null;
		for (int i = 0; i < primaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
			NavValue originalValue = inputBuffer.GetOriginalValue(keyFieldByIndex.FieldIndex);
			NavValue changedFieldValue = inputBuffer.GetChangedFieldValue(keyFieldByIndex.FieldIndex);
			if (originalValue != null && changedFieldValue != null && originalValue.Equals(changedFieldValue))
			{
				inputBuffer[keyFieldByIndex.FieldIndex] = null;
			}
			else if (originalValue == null || changedFieldValue != null)
			{
				if (array == null)
				{
					array = inputBuffer.ReadOnlyBuffer.ToArray();
					mutableRecordBuffer = inputBuffer.Clone();
				}
				array[keyFieldByIndex.FieldIndex] = changedFieldValue ?? keyFieldByIndex.InitValue;
				mutableRecordBuffer[keyFieldByIndex.FieldIndex] = null;
			}
		}
		if (array == null)
		{
			return inputBuffer;
		}
		if (forModify)
		{
			for (int j = 0; j < metaTable.FieldCount; j++)
			{
				NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(j);
				if (!fieldByIndex.FieldIsPartOfPrimaryKey)
				{
					array[j] = null;
				}
			}
		}
		return new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTable, array), mutableRecordBuffer, forModify ? mutableRecordBuffer.TransactionToken : 0);
	}

	internal void Initialize(bool isTemporary, NavRecord sharedTable, SecurityFiltering filtering)
	{
		if (sharedTable == null)
		{
			throw new ArgumentNullException("sharedTable");
		}
		RecordImplementation recordImplementation = sharedTable.RecordImplementation;
		if (isTemporary)
		{
			if (dataAccess != null)
			{
				TempTableDataProvider tempTableDataProvider = (TempTableDataProvider)dataAccess.DataProvider;
				if (tempTableDataProvider.SharedTablesUsageCount > 0)
				{
					tempTableDataProvider.SharedTablesUsageCount--;
				}
				else if (tempTableDataProvider != recordImplementation.dataAccess.DataProvider)
				{
					tempTableDataProvider.DecrementTotalRowsWithCount();
				}
			}
			dataAccess = recordImplementation.dataAccess;
			((TempTableDataProvider)dataAccess.DataProvider).SharedTablesUsageCount++;
			InitializeImpl(recordImplementation.TableState.CompanyNameToken, null, filtering);
		}
		else
		{
			InitializeImpl(-1, recordImplementation.TableState, filtering);
		}
	}

	internal void Initialize(string companyName, SecurityFiltering filtering)
	{
		int token;
		if (parentRecord.MetaTable.DataPerCompany)
		{
			GetCompanyNameToken(DataError.ThrowError, companyName, out token);
		}
		else
		{
			token = 0;
		}
		InitializeImpl(token, null, filtering);
	}

	private void InitializeImpl(int companyToken, TableState sharedTableState, SecurityFiltering filtering)
	{
		metaTable = parentRecord.MetaTable;
		navMediaPurger = new NavMediaPurger(this, metaTable);
		if (dataAccess == null)
		{
			dataAccess = parentRecord.Session.DataAccessSource.GetDataAccessForTable(metaTable, parentRecord.IsTemporary);
		}
		orginalFiltering = filtering;
		initialized = true;
		if (sharedTableState == null)
		{
			tableState = new TableState(metaTable, dataAccess);
			ResetRecord();
			tableState.CompanyNameToken = companyToken;
		}
		else
		{
			tableState = sharedTableState;
			SetSecurityFiltering(orginalFiltering);
		}
	}

	internal void CopyRecord(NavRecord fromRecord, CopyRecordOptions options)
	{
		RecordImplementation recordImplementation = fromRecord.RecordImplementation;
		if (parentRecord.IsTemporary && !fromRecord.IsTemporary && recordImplementation.mutableRecordBuffer != null)
		{
			recordImplementation.dataAccess.DataProvider.FlushBufferedWrites(recordImplementation.mutableRecordBuffer);
		}
		if ((options & CopyRecordOptions.ShareTableData) != 0)
		{
			Initialize(isTemporary: true, fromRecord, securityFiltering);
			CopyAutoCalcFields(fromRecord, options);
		}
		if ((options & CopyRecordOptions.CopyFiltersSortingMarked) != 0)
		{
			CopyFilters(fromRecord, CopyFilterOptions.CopyFilter | CopyFilterOptions.CopySorting | CopyFilterOptions.CopyMarkedList);
		}
		if (!fromRecord.IsTemporary && !parentRecord.IsTemporary)
		{
			bool flag = (options & CopyRecordOptions.CopyFiltersSortingMarked) != 0 && recordImplementation.resultSetEnumerator != null && dataAccess.DataProvider == recordImplementation.dataAccess.DataProvider;
			if (flag | (recordImplementation.resultSetEnumerator != null && dataAccess.DataProvider == recordImplementation.dataAccess.DataProvider && tableState.IsEquivalentTo(recordImplementation.tableState)))
			{
				CopyResultSetEnumerator(recordImplementation);
			}
			else
			{
				InvalidateCurrentResultSetEnumerator();
			}
		}
		mutableRecordBuffer = ((recordImplementation.mutableRecordBuffer == null) ? null : recordImplementation.mutableRecordBuffer.Clone());
		if (mutableRecordBuffer != null && recordImplementation.tableState.CompanyNameToken != tableState.CompanyNameToken)
		{
			ReadOnlyRecordBuffer readOnlyBuffer = new ReadOnlyRecordBuffer(metaTable, new NavValue[metaTable.FieldCount]);
			mutableRecordBuffer = new MutableRecordBuffer(readOnlyBuffer, mutableRecordBuffer.ToArray(), mutableRecordBuffer.TransactionToken);
		}
		if ((options & CopyRecordOptions.ShareTableData) == 0)
		{
			CopyAutoCalcFields(fromRecord, options);
		}
		if (fromRecord.IsTemporary || parentRecord.IsTemporary)
		{
			InvalidateCurrentResultSetEnumerator();
			if (mutableRecordBuffer != null)
			{
				mutableRecordBuffer.IsCopiedFromTemporary = fromRecord.IsTemporary;
			}
		}
	}

	private void CopyAutoCalcFields(NavRecord fromRecord, CopyRecordOptions options)
	{
		if (options == CopyRecordOptions.CopyFiltersSortingMarked)
		{
			NCLMetaField[] autoCalcFields = fromRecord.RecordImplementation.GetAutoCalcFields();
			SetAutoCalcFields(DataError.ThrowError, autoCalcFields, replace: true);
		}
	}

	private void CopyResultSetEnumerator(RecordImplementation fromRecordImplementation)
	{
		resultSetEnumerator = fromRecordImplementation.resultSetEnumerator.Clone();
		resultSetReversed = fromRecordImplementation.resultSetReversed;
	}

	internal void ClearRecord()
	{
		mutableRecordBuffer = null;
		ResetRecord();
	}

	internal void InitRecord()
	{
		if (mutableRecordBuffer == null)
		{
			return;
		}
		for (int i = 1; i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (!fieldByIndex.FieldIsPartOfPrimaryKey && !fieldByIndex.FieldIsUserDefinedTimestamp)
			{
				mutableRecordBuffer[i] = fieldByIndex.InitValue;
			}
		}
	}

	internal int[] InitRecordFromFilters(bool includeNonPrimaryKeyFields, bool includeIdenticalFilters)
	{
		ResetRecordBuffer();
		FilterFieldDictionary filterFieldDictionary = tableState.CombineFiltersFromAllGroups((FilterExpression a, FilterExpression b) => (!includeIdenticalFilters || a == null || b == null || a.ExpressionType != FilterExpressionType.Equal || b.ExpressionType != FilterExpressionType.Equal || !((UnaryFilterExpression)a).Value.Equals(((UnaryFilterExpression)b).Value)) ? null : a);
		if (filterFieldDictionary == null)
		{
			return new int[0];
		}
		List<int> list = new List<int>(filterFieldDictionary.Items.Length);
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		Tuple<INavFieldMetadata, FilterExpression>[] items = filterFieldDictionary.Items;
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
		{
			if (tuple.Item1 == FilterFieldDictionary.MultipleFields)
			{
				continue;
			}
			NCLMetaField nCLMetaField = (NCLMetaField)tuple.Item1;
			if ((includeNonPrimaryKeyFields || nCLMetaField.FieldIsPartOfPrimaryKey) && nCLMetaField.FieldClass == FieldClass.Normal && tuple.Item2 != null && tuple.Item2.ExpressionType == FilterExpressionType.Equal)
			{
				list.Add(nCLMetaField.FieldNo);
				NavOption navOption = ((UnaryFilterExpression)tuple.Item2).Value as NavOption;
				if (navOption == null || navOption.NavOptionMetadata.IsValidOrdinal(navOption.Value))
				{
					mutableRecordBuffer[nCLMetaField.ColumnIndex] = ((UnaryFilterExpression)tuple.Item2).Value;
				}
			}
		}
		return list.ToArray();
	}

	internal void ResetRecord()
	{
		ResetRecordInternal();
		SetSecurityFiltering(orginalFiltering);
	}

	internal void ResetRecordInternal()
	{
		InvalidateCurrentResultSetEnumerator();
		tableState.Reset();
	}

	internal bool GetTableAscending()
	{
		return tableState.SortingFields.DefinedSortingFields.IsAscendingOnAllFields;
	}

	internal void SetTableAscending(bool ascending)
	{
		tableState.DefaultSortOrder = ((!ascending) ? SortOrder.Descending : SortOrder.Ascending);
		SortOrder order = ((!ascending) ? SortOrder.Descending : SortOrder.Ascending);
		if (!tableState.SortingFields.DefinedSortingFields.IsSameOrderOnAllFields(order))
		{
			tableState.SortingFields = tableState.SortingFields.CreateNewSortingFieldsWithAllFieldsSetToOrder((!ascending) ? SortOrder.Descending : SortOrder.Ascending);
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal void SetFieldSortOrder(NCLMetaField fieldToSet, SortOrder newSortOrder)
	{
		if (!tableState.SortingFields.ActualSortingFields.TryGetValue(fieldToSet, out var sortOrder))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.SetFieldAscendingNotAllowedOnFieldThatIsNotPartOfCurrentSorting, fieldToSet.FieldName));
		}
		if (sortOrder != newSortOrder)
		{
			tableState.SortingFields = tableState.SortingFields.CreateNewSortingWithSingleFieldSetToOrder(fieldToSet, newSortOrder);
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal void SetFieldAscending(int fieldNo, bool ascending)
	{
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		SortOrder newSortOrder = ((!ascending) ? SortOrder.Descending : SortOrder.Ascending);
		SetFieldSortOrder(fieldByNo, newSortOrder);
	}

	internal bool GetFieldAscending(int fieldNo)
	{
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		if (!tableState.SortingFields.ActualSortingFields.TryGetValue(fieldByNo, out var sortOrder))
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.GetFieldAscendingNotAllowedOnFieldThatIsNotPartOfCurrentSorting, fieldByNo.FieldName));
		}
		return sortOrder == SortOrder.Ascending;
	}

	internal int GetCurrentKeyIndex()
	{
		return tableState.CurrentKeyIndex;
	}

	internal void SetCurrentKeyIndex(int keyIndex)
	{
		if (keyIndex > 0 && keyIndex <= metaTable.KeyCount)
		{
			NCLMetaKey keyByIndex = metaTable.GetKeyByIndex(keyIndex - 1);
			ObsolescenceGuard.ThrowIfObsoleted(keyByIndex);
			if (keyByIndex.KeyIsActive)
			{
				AssignSortingFieldsIfDifferent(keyByIndex);
				return;
			}
		}
		throw new NavCSideException(85132271, string.Format(CultureInfo.CurrentCulture, Lang.TableHasNoKeyAtIndex, metaTable.TableCaptions.GetValueOrDefault(), keyIndex));
	}

	internal bool SetCurrentKey(DataError errorLevel, NCLMetaField[] metaFields)
	{
		if (DataProviderHelper.TryFindBestKey(metaTable, metaFields, out var bestKey))
		{
			AssignSortingFieldsIfDifferent(bestKey);
			return true;
		}
		if (CheckIfFieldsAreValidForSorting(errorLevel, metaFields))
		{
			AssignCustomSortingFieldsIfDifferent(metaFields);
			return true;
		}
		return false;
	}

	private bool CheckIfFieldsAreValidForSorting(DataError errorLevel, NCLMetaField[] metaFields)
	{
		IEnumerable<NCLMetaField> enumerable = (dataAccess.DataProvider.SupportsAutoCalcFields ? metaFields.Where((NCLMetaField field) => field.FieldClass == FieldClass.FlowFilter || (field.FieldClass == FieldClass.FlowField && (field.CalculationFormula.NonstandardJoinCriteriaFields != null || field.CalculationFormula.ContainsNestedFlowFields || field.Parent.TableType == TableType.ExternalSQL)) || field.FieldNavType == NavType.BLOB || field.SourceTableField.Parent.IsVirtualTable || field.SourceTableField.Parent.IsAppTable != field.Parent.IsAppTable || field.SourceTableField.Parent.TableType != field.Parent.TableType) : metaFields.Where((NCLMetaField field) => field.FieldClass == FieldClass.FlowFilter || field.FieldClass == FieldClass.FlowField || field.FieldNavType == NavType.BLOB));
		if (enumerable.Any())
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw RecordImplementationHelper.GetCannotSortOnCertainFieldsException(metaFields[0].Parent, enumerable, Lang.CannotSortOnCertainFields);
			}
			return false;
		}
		if (!metaFields.Any((NCLMetaField field) => field.FieldActive && !field.FieldObsoleted))
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw RecordImplementationHelper.GetKeyNotFoundException(metaFields[0].Parent, metaFields);
			}
			return false;
		}
		return true;
	}

	private void AssignCustomSortingFieldsIfDifferent(NCLMetaField[] metaFields)
	{
		if (tableState.AssignCustomSortingFields(metaFields))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	private void AssignSortingFieldsIfDifferent(NCLMetaKey newKey)
	{
		if (newKey != tableState.Key)
		{
			SortingFields sortingFields = tableState.SortingFields;
			tableState.AssignKeyAndSortingFields(newKey, null);
			if (!tableState.SortingFields.ActualSortingFields.Equals(sortingFields.ActualSortingFields))
			{
				InvalidateCurrentResultSetEnumerator();
			}
		}
	}

	internal string GetCurrentKey()
	{
		return RecordImplementationHelper.GetSortingFieldsString(tableState.SortingFields.DefinedSortingFields);
	}

	internal bool IsEmpty()
	{
		try
		{
			FiltersAndMarks filtersAndMarks = tableState.FiltersAndMarks;
			if (validatedSecurityFilters == null)
			{
				if (Exists(filtersAndMarks, SecurityFiltering))
				{
					VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
					return false;
				}
				return true;
			}
			FiltersAndMarks filtersAndMarks2 = new FiltersAndMarks(DataProviderHelper.MergeFilterFieldDictionaries(filtersAndMarks.Filters, validatedSecurityFilters), filtersAndMarks.MarkedRecords);
			if (Exists(filtersAndMarks2, SecurityFiltering))
			{
				return false;
			}
			VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
			if (Exists(filtersAndMarks, SecurityFiltering.Ignored))
			{
				throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterViolated, metaTable.TableName));
			}
			return true;
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
	}

	private bool Exists(FiltersAndMarks filtersAndMarks, SecurityFiltering requestSecurityFiltering)
	{
		return dataAccess.Exists(new ExistsCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, filtersAndMarks, requestSecurityFiltering, forceNonLockingProviderRequest: false, tableState.SortingFields.ActualSortingFields));
	}

	internal int GetRecordCount()
	{
		try
		{
			FiltersAndMarks filtersAndMarks = tableState.FiltersAndMarks;
			int num = dataAccess.Count(new CountCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, filtersAndMarks, SecurityFiltering));
			if (num > 0 || (IsPermissionCheckNeeded(readingOnly: true) && !filtersAndMarks.IsEmpty))
			{
				VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
				if (num > 0 && validatedSecurityFilters != null)
				{
					FiltersAndMarks filtersAndMarks2 = new FiltersAndMarks(DataProviderHelper.MergeFilterFieldDictionaries(filtersAndMarks.Filters, validatedSecurityFilters), filtersAndMarks.MarkedRecords);
					int num2 = dataAccess.Count(new CountCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, filtersAndMarks2, SecurityFiltering.Ignored));
					if (num != num2)
					{
						throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterViolated, metaTable.TableName));
					}
				}
			}
			return num;
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
	}

	internal int GetRecordCountApprox()
	{
		return GetRecordCount();
	}

	internal int GetRelation(NCLMetaField metaField)
	{
		int num = EvaluateRelation(metaField);
		if (num == -1)
		{
			return 0;
		}
		return metaField.FieldRelations[num].SourceTableId;
	}

	internal string GetView(bool useCaptions)
	{
		return RecordImplementationHelper.GetTableViewString(tableState.SortingFields.DefinedSortingFields, tableState.CurrentFilterGroup, useCaptions);
	}

	internal void SetView(string view)
	{
		TableViewResolver tableViewResolver = new TableViewResolver(metaTable, view);
		TableViewParser.ParseTableView(view, tableViewResolver);
		if (tableViewResolver.SortingFields != null)
		{
			SetCurrentKey(DataError.ThrowError, tableViewResolver.SortingFields.ToArray());
		}
		else
		{
			AssignSortingFieldsIfDifferent(metaTable.PrimaryKey);
		}
		if (tableViewResolver.SortOrder != null)
		{
			if (tableViewResolver.SortOrder.Count == 1)
			{
				SetTableAscending(tableViewResolver.SortOrder[0] == SortOrder.Ascending);
			}
			if (tableViewResolver.SortOrder.Count > 1)
			{
				if (tableViewResolver.SortOrder.Count != tableState.SortingFields.DefinedSortingFields.Count)
				{
					throw new NavCSideException(Lang.InvalidSortingSpecificationInTableView);
				}
				for (int i = 0; i < tableState.SortingFields.DefinedSortingFields.Count; i++)
				{
					NCLMetaField nCLMetaField = (NCLMetaField)tableState.SortingFields.DefinedSortingFields[i];
					SetFieldAscending(nCLMetaField.FieldNo, tableViewResolver.SortOrder[i] == SortOrder.Ascending);
				}
			}
		}
		InternalSetFilters(tableViewResolver.Filters, clearExistingFilters: true);
	}

	internal NavTableView GetTableView()
	{
		return tableState.GetNavTableView();
	}

	internal void LockTable(bool wait, bool versionCheck)
	{
		if (!wait | versionCheck)
		{
			throw new NavCSideException(23397458, string.Format(CultureInfo.CurrentCulture, Lang.NoWaitNotSupportedError, parentRecord.MetaTable.TableName));
		}
		dataAccess.LockTable(metaTable, tableState.CompanyNameToken);
	}

	internal bool ChangeCompany(DataError errorLevel, string companyName)
	{
		if (!parentRecord.MetaTable.DataPerCompany)
		{
			return true;
		}
		if (!GetCompanyNameToken(errorLevel, companyName, out var token))
		{
			return false;
		}
		if (token != tableState.CompanyNameToken)
		{
			tableState.CompanyNameToken = token;
			InvalidateCurrentResultSetEnumerator();
		}
		return true;
	}

	private bool GetCompanyNameToken(DataError errorLevel, string companyName, out int token)
	{
		if (string.IsNullOrEmpty(companyName))
		{
			token = Session.Company.CompanyNameToken;
		}
		else
		{
			if (!Session.ValidateUserHasAccessToCompany(companyName))
			{
				if (errorLevel == DataError.TrapError)
				{
					token = -1;
					return false;
				}
				if (Session.GetAllCompanies(useSecurityFilter: false).Contains(companyName))
				{
					throw new NavCSideDataException(18023729, string.Format(CultureInfo.CurrentCulture, Lang.UserDoesNotHaveAccessToCompany, companyName));
				}
				throw new NavCSideDataException(85132272, string.Format(CultureInfo.CurrentCulture, Lang.CompanyNameDoesNotExist, companyName));
			}
			token = Session.Database.GetCompanyNameToken(companyName);
		}
		return true;
	}

	internal string GetActiveCompany()
	{
		return Session.Database.GetCompanyNameFromToken(tableState.CompanyNameToken);
	}

	internal string GetPosition(bool useCaptions)
	{
		FilterFieldDictionary primaryKeyFields = new FilterFieldDictionary(metaTable.PrimaryKey.DefinedKeyFields.Zip(CurrentPrimaryKeyValues(), (INavFieldMetadata field, NavValue value) => Tuple.Create(field, (FilterExpression)FilterExpression.Equal(value, field.ExpressionContext))));
		return RecordImplementationHelper.GetPositionString(primaryKeyFields, useCaptions);
	}

	internal void SetPosition(string position)
	{
		TablePositionResolver tablePositionResolver = new TablePositionResolver(metaTable, position);
		TableViewParser.ParseTableFilters(position, tablePositionResolver);
		IReadOnlyList<Tuple<INavFieldMetadata, FilterExpression>> filters = tablePositionResolver.Filters;
		FieldList definedKeyFields = metaTable.PrimaryKey.DefinedKeyFields;
		if (filters.Count != definedKeyFields.Count)
		{
			throw RecordImplementationHelper.GetInvalidPositionStringException(position, metaTable);
		}
		MutableRecordBuffer currentMutableRecordBufferOrDefault = CurrentMutableRecordBufferOrDefault;
		currentMutableRecordBufferOrDefault.MarkAllFieldsAsChanged(includePrimaryKeyFields: false, checkMetadataForDefaults: false);
		for (int i = 0; i < definedKeyFields.Count; i++)
		{
			Tuple<INavFieldMetadata, FilterExpression> tuple = filters[i];
			if (tuple.Item1 != definedKeyFields[i])
			{
				throw RecordImplementationHelper.GetInvalidPositionStringException(position, metaTable);
			}
			NavValue value = ((UnaryFilterExpression)tuple.Item2).Value;
			currentMutableRecordBufferOrDefault[tuple.Item1.ColumnIndex] = value;
		}
		mutableRecordBuffer = currentMutableRecordBufferOrDefault;
		InvalidateCurrentResultSetEnumerator();
	}

	internal void CopyFilter(NCLMetaField fromField, NavRecord toRecord, NCLMetaField toField)
	{
		RecordImplementation recordImplementation = toRecord.RecordImplementation;
		if (toField.FieldNavType != fromField.FieldNavType)
		{
			throw new NavCSideException(23004238, string.Format(CultureInfo.CurrentCulture, Lang.NavConstFilterNotCompatible, toField.FieldNavType, fromField.FieldNavType, toField.FieldName, fromField.FieldName, fromField.Parent.TableName));
		}
		FilterExpression expression;
		if (tableState.TryGetFilterOnFieldInCurrentGroup(fromField, out var filterExpression))
		{
			try
			{
				expression = filterExpression.TransferToDifferentContext(toField.ExpressionContext);
			}
			catch (NavBaseException innerException)
			{
				throw RecordImplementationHelper.GetInvalidFilterException(parentRecord, toField, filterExpression.ToString(), innerException);
			}
		}
		else
		{
			expression = null;
		}
		if (recordImplementation.tableState.SetFilterOnCurrentGroup(toField, expression, resultSetEnumerator != null))
		{
			recordImplementation.InvalidateCurrentResultSetEnumerator();
		}
	}

	internal void CopyFiltersSortingMarkList(NavRecord fromRecord)
	{
		CopyFilters(fromRecord, CopyFilterOptions.CopyFilter | CopyFilterOptions.CopySorting | CopyFilterOptions.CopyMarkedList);
	}

	internal void CopyFilters(NavRecord fromRecord)
	{
		CopyFilters(fromRecord, CopyFilterOptions.CopyFilter);
	}

	internal void CopyFilters(NavRecord fromRecord, CopyFilterOptions options)
	{
		InvalidateCurrentResultSetEnumerator();
		tableState.CopyFrom(fromRecord.RecordImplementation.tableState, options);
	}

	internal bool HasFilter()
	{
		return tableState.HasFilterInCurrentGroup;
	}

	internal void SetFilterFromCurrentKey()
	{
		MutableRecordBuffer currentBuffer = CurrentMutableRecordBufferOrDefault;
		FieldList fieldList = tableState.SortingFields.DefinedSortingFields.FieldList;
		IEnumerable<INavFieldMetadata> enumerable;
		if (!metaTable.PrimaryKey.DefinedKeyFields.AreAllFieldsContainedInOtherAssumingSameParent(fieldList, out var fieldsNotContained))
		{
			enumerable = fieldList.Concat(fieldsNotContained);
		}
		else
		{
			IEnumerable<INavFieldMetadata> enumerable2 = fieldList;
			enumerable = enumerable2;
		}
		IEnumerable<INavFieldMetadata> source = enumerable;
		IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> fieldFilters = source.Select((INavFieldMetadata f) => Tuple.Create(f, (FilterExpression)FilterExpression.Equal(currentBuffer[f.ColumnIndex] ?? f.InitValue, f.ExpressionContext)));
		InternalSetFilters(fieldFilters, clearExistingFilters: false);
	}

	internal void SetPermissionFilter()
	{
		SetSecurityFiltering(SecurityFiltering.Filtered);
	}

	internal string GetFilters()
	{
		return RecordImplementationHelper.GetNonViewFilterString(tableState.CurrentFilterGroup, tableState.MarkedRecordsOnly);
	}

	internal string GetUrlFilters()
	{
		return RecordImplementationHelper.GetUrlFilterString(tableState.CurrentFilterGroup);
	}

	internal NavFilterGroup[] GetCurrentFilters()
	{
		return tableState.GetNavFilterGroups();
	}

	internal bool HasReadPermission(PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		if (parentRecord.IsTemporary)
		{
			return true;
		}
		NavApplicationObjectBase securableObject = parentRecord ?? Session.CurrentMethodScope.ApplicationObject;
		if (!GetPermissionSet().HasPermissions(securableObject, PermissionMask.Read, Session.LimitedUserPermissionChecker, null, PermissionMask.None, countAsLimitedUserAllowedTable: true, validationCategory))
		{
			return false;
		}
		switch (securityFiltering)
		{
		case SecurityFiltering.Filtered:
		case SecurityFiltering.Ignored:
			return true;
		case SecurityFiltering.Validated:
		case SecurityFiltering.Disallowed:
			return GetSecurityFilters(SecurityFilterType.Read) == FilterFieldDictionary.Empty;
		default:
			return false;
		}
	}

	public void MaximizeTablePermissions()
	{
		parentRecord.Session.MaximizePermissions(new int[1] { parentRecord.ObjectId.ObjectNumber });
	}

	internal bool HasWritePermission(PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		if (parentRecord.IsTemporary)
		{
			return true;
		}
		if (DataAccess.IsReadOnly)
		{
			return false;
		}
		NavApplicationObjectBase securableObject = parentRecord ?? Session.CurrentMethodScope.ApplicationObject;
		if (!GetPermissionSet().HasPermissions(securableObject, PermissionMask.Insert | PermissionMask.Modify | PermissionMask.Delete, Session.LimitedUserPermissionChecker, null, PermissionMask.None, countAsLimitedUserAllowedTable: false, validationCategory))
		{
			return false;
		}
		switch (securityFiltering)
		{
		case SecurityFiltering.Ignored:
			return true;
		case SecurityFiltering.Validated:
		case SecurityFiltering.Filtered:
		case SecurityFiltering.Disallowed:
			if (GetSecurityFilters(SecurityFilterType.Insert) != FilterFieldDictionary.Empty)
			{
				return false;
			}
			if (GetSecurityFilters(SecurityFilterType.Modify) != FilterFieldDictionary.Empty)
			{
				return false;
			}
			return GetSecurityFilters(SecurityFilterType.Delete) == FilterFieldDictionary.Empty;
		default:
			return false;
		}
	}

	public bool RequiresSecurityFiltersValidation(SecurityFilterType securityFilterType)
	{
		if (!(GetSecurityFilters(SecurityFilterType.Read) != FilterFieldDictionary.Empty))
		{
			return GetSecurityFilters(securityFilterType) != FilterFieldDictionary.Empty;
		}
		return true;
	}

	private void ChangeSecurityFiltering(SecurityFiltering filtering)
	{
		if (filtering != securityFiltering)
		{
			switch (securityFiltering)
			{
			case SecurityFiltering.Filtered:
				RemoveSecurityFilters();
				break;
			case SecurityFiltering.Validated:
				validatedSecurityFilters = null;
				break;
			}
			SetSecurityFiltering(filtering);
		}
	}

	private void SetSecurityFiltering(SecurityFiltering filtering)
	{
		securityFiltering = filtering;
		InvalidateCurrentResultSetEnumerator();
		switch (securityFiltering)
		{
		case SecurityFiltering.Filtered:
		{
			FilterFieldDictionary securityFilters = GetSecurityFilters(SecurityFilterType.Read);
			if (securityFilters == FilterFieldDictionary.Empty)
			{
				break;
			}
			int filterGroupNumber = tableState.FilterGroupNumber;
			tableState.FilterGroupNumber = 6;
			try
			{
				tableState.SetFiltersOnCurrentGroup(securityFilters.Items, clearExistingFilters: true, checkChanged: false);
				break;
			}
			finally
			{
				tableState.FilterGroupNumber = filterGroupNumber;
			}
		}
		case SecurityFiltering.Validated:
		{
			FilterFieldDictionary securityFilters = GetSecurityFilters(SecurityFilterType.Read);
			if (securityFilters != FilterFieldDictionary.Empty)
			{
				validatedSecurityFilters = securityFilters;
			}
			break;
		}
		case SecurityFiltering.Disallowed:
			if (GetSecurityFilters(SecurityFilterType.Read) != FilterFieldDictionary.Empty || GetSecurityFilters(SecurityFilterType.Insert) != FilterFieldDictionary.Empty || GetSecurityFilters(SecurityFilterType.Modify) != FilterFieldDictionary.Empty || GetSecurityFilters(SecurityFilterType.Delete) != FilterFieldDictionary.Empty)
			{
				throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterNotAllowed, parentRecord.ALTableName));
			}
			break;
		case SecurityFiltering.Ignored:
			break;
		}
	}

	private IPermissionSet GetPermissionSet()
	{
		IPermissionSet permissionSet = DataAccess.DataProvider.GetPermissionSet();
		if (permissionSet != null)
		{
			return permissionSet;
		}
		return Session.GetPermissionSet(Session.CurrentMethodScope.ApplicationObject, tableState.CompanyNameToken, new ApplicationObjectId(ObjectType.TableData, metaTable.ObjectId));
	}

	private void VerifySecurityFiltersOnRecord(IRecordBuffer recordBuffer, FilterFieldDictionary filters, bool checkAgainstOriginalAndModified = false, bool bothShouldPass = false)
	{
		if (!(filters == null) && IsPermissionCheckNeeded(readingOnly: true))
		{
			bool flag = recordBuffer.PassesFieldFilters(filters, includeFlowFields: false, checkAgainstOriginalAndModified, bothShouldPass);
			if (flag)
			{
				flag = VerifySecurityFiltersForFlowFields(recordBuffer, filters);
			}
			if (!flag)
			{
				throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterViolated, metaTable.TableName));
			}
		}
	}

	private bool VerifySecurityFiltersForFlowFields(IRecordBuffer recordBuffer, FilterFieldDictionary filters)
	{
		if (!filters.AnyFiltersOnFlowFields)
		{
			return true;
		}
		bool flag = filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> item) => item.Item1.FieldClass == FieldClass.FlowField && recordBuffer[item.Item1.ColumnIndex] != null).All((Tuple<INavFieldMetadata, FilterExpression> item) => item.Item2.Evaluate(recordBuffer[item.Item1.ColumnIndex]));
		if (flag)
		{
			Tuple<INavFieldMetadata, FilterExpression>[] array = filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> item) => item.Item1.FieldClass == FieldClass.FlowField && recordBuffer[item.Item1.ColumnIndex] == null).ToArray();
			if (array.Any())
			{
				FieldDictionary<NavValue> fieldDictionary = FlowFieldsHelper.CalcFields(Session, Session.Company.CompanyNameToken, recordBuffer, FiltersAndMarks.Empty, array.Select((Tuple<INavFieldMetadata, FilterExpression> tuple2) => (NCLMetaField)tuple2.Item1).ToArray(), onlyFieldsSourcedFromVirtualTables: false, securityFiltering);
				Tuple<INavFieldMetadata, FilterExpression>[] array2 = array;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in array2)
				{
					NavValue input = fieldDictionary[tuple.Item1];
					flag = tuple.Item2.Evaluate(input);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	private void VerifyPermissions(PermissionMask permissionMask, bool checkForEmptyTable)
	{
		if (!IsPermissionCheckNeeded(permissionMask == PermissionMask.Read))
		{
			return;
		}
		IPermissionSet permissionSet = GetPermissionSet();
		if (checkForEmptyTable)
		{
			permissionSet.VerifyPermissions(parentRecord ?? Session.CurrentMethodScope.ApplicationObject, permissionMask, Session.LimitedUserPermissionChecker, (int id) => !dataAccess.Exists(new ExistsCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, FiltersAndMarks.Empty, SecurityFiltering.Ignored, forceNonLockingProviderRequest: true)));
		}
		else
		{
			permissionSet.VerifyPermissions(parentRecord ?? Session.CurrentMethodScope.ApplicationObject, permissionMask, Session.LimitedUserPermissionChecker);
		}
	}

	private void VerifyPermissionsCalculatedFields(IEnumerable<NCLMetaField> fields)
	{
		if (!IsPermissionCheckNeeded(readingOnly: true))
		{
			return;
		}
		HashSet<NCLMetaTable> nonVirtualTables = new HashSet<NCLMetaTable>();
		if (metaTable.HasFlowFieldsSourcedFromVirtualTables)
		{
			foreach (NCLMetaField item in fields.Where((NCLMetaField field) => field.FieldNclType != NavNclType.NavBlob && !field.SourceTableField.Parent.IsVirtualTable))
			{
				nonVirtualTables.Add(item.SourceTableField.Parent);
			}
		}
		else
		{
			foreach (NCLMetaField field in fields)
			{
				nonVirtualTables.Add(field.SourceTableField.Parent);
			}
		}
		if (nonVirtualTables.Count == 0)
		{
			return;
		}
		IPermissionSet permissionSet = Session.GetPermissionSet(Session.CurrentMethodScope.ApplicationObject, tableState.CompanyNameToken, nonVirtualTables.Select((NCLMetaTable t) => new ApplicationObjectId(ObjectType.TableData, t.ObjectId)));
		permissionSet.VerifyPermissions(Session.CurrentMethodScope.ApplicationObject, PermissionMask.Read, Session.LimitedUserPermissionChecker, (int id) => !dataAccess.Exists(new ExistsCacheRequest(tableState.CompanyNameToken, nonVirtualTables.First((NCLMetaTable t) => t.ObjectId == id), cacheable: true, FiltersAndMarks.Empty, SecurityFiltering.Ignored, forceNonLockingProviderRequest: true)), PermissionMask.IndirectRead);
	}

	private void PassesSecurityFilters(MutableRecordBuffer recordBuffer, SecurityFilterType securityFilterType)
	{
		FilterFieldDictionary filterFieldDictionary;
		switch (securityFiltering)
		{
		case SecurityFiltering.Validated:
		case SecurityFiltering.Filtered:
			filterFieldDictionary = GetSecurityFilters(securityFilterType);
			break;
		case SecurityFiltering.Ignored:
			filterFieldDictionary = FilterFieldDictionary.Empty;
			break;
		case SecurityFiltering.Disallowed:
			filterFieldDictionary = GetSecurityFilters(securityFilterType);
			if (filterFieldDictionary != FilterFieldDictionary.Empty)
			{
				throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterNotAllowed, parentRecord.ALTableName));
			}
			break;
		default:
			filterFieldDictionary = FilterFieldDictionary.Empty;
			break;
		}
		bool flag = securityFilterType == SecurityFilterType.Modify;
		VerifySecurityFiltersOnRecord(recordBuffer, filterFieldDictionary, flag, flag);
	}

	private FilterFieldDictionary GetSecurityFilters(SecurityFilterType securityFilterType)
	{
		if (!IsPermissionCheckNeeded(readingOnly: false))
		{
			return FilterFieldDictionary.Empty;
		}
		return Session.GetSecurityFilters(tableState.CompanyNameToken, parentRecord.TableID, securityFilterType, Session.CurrentMethodScope.ApplicationObject, parentRecord);
	}

	private void VerifySecurityFilters(MutableRecordBuffer recordBuffer, SecurityFilterType securityFilterType)
	{
		if (!parentRecord.IsTemporary)
		{
			PassesSecurityFilters(recordBuffer, securityFilterType);
		}
	}

	private void VerifyNoRecordsOutsideValidatedSecurityFilter(FilterFieldDictionary securityFilters)
	{
		if (!(securityFilters == null) && !(securityFilters == FilterFieldDictionary.Empty) && !parentRecord.IsTemporary)
		{
			FiltersAndMarks filtersAndMarks = new FiltersAndMarks(tableState.FiltersAndMarks.Filters.AndNegatedFilters(securityFilters), tableState.FiltersAndMarks.MarkedRecords);
			ExistsCacheRequest request = new ExistsCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, filtersAndMarks, SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
			if (dataAccess.Exists(request))
			{
				throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterViolated, metaTable.TableName));
			}
		}
	}

	private bool IsPermissionCheckNeeded(bool readingOnly)
	{
		if (parentRecord.IsTemporary || parentRecord.GetNCLMetaTable().IsVirtualTable || parentRecord.GetNCLMetaTable().TableType == TableType.Query)
		{
			return false;
		}
		if (readingOnly)
		{
			int objectId = metaTable.ObjectId;
			if (objectId == 2000000001 || objectId == 2000000053 || objectId == 2000000071)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	internal int GetFilterGroup()
	{
		return tableState.FilterGroupNumber;
	}

	internal void SetFilterGroup(int filterGroup)
	{
		tableState.FilterGroupNumber = filterGroup;
	}

	internal void SetRecordTimestamp(long timestamp)
	{
		if (mutableRecordBuffer == null)
		{
			mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		}
		mutableRecordBuffer[0] = NavBigInteger.Create(timestamp);
	}

	internal NavRecordId GetRecordId()
	{
		return CurrentMutableRecordBufferOrDefault.GetRecordId();
	}

	internal long GetRecordTimestamp()
	{
		return (CurrentMutableRecordBufferOrDefault.Timestamp ?? NavBigInteger.Default).ToInt64();
	}

	internal void RemoveAllowedFilters()
	{
		if (tableState.ClearFilterGroup(-1))
		{
			InvalidateCurrentResultSetEnumerator();
		}
		if (tableState.ClearFilterGroup(0))
		{
			InvalidateCurrentResultSetEnumerator();
		}
		if (tableState.ClearFilterGroup(5))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	private void RemoveSecurityFilters()
	{
		if (tableState.ClearFilterGroup(6))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal int EvaluateRelation(NCLMetaField field)
	{
		if (field.FieldRelations == null)
		{
			return -1;
		}
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		List<NCLMetaField> list = new List<NCLMetaField>();
		for (int i = 0; i < field.FieldRelations.Count; i++)
		{
			NCLMetaFieldRelation nCLMetaFieldRelation = field.FieldRelations[i];
			FilterExpression concatenatedFilterNormalFields = nCLMetaFieldRelation.ExpressionDictionary.ConcatenatedFilterNormalFields;
			FilterExpression concatenatedFilterFlowFields = nCLMetaFieldRelation.ExpressionDictionary.ConcatenatedFilterFlowFields;
			if (concatenatedFilterNormalFields == null && concatenatedFilterFlowFields == null)
			{
				return i;
			}
			if (mutableRecordBuffer.PassesFilter(concatenatedFilterNormalFields))
			{
				if (concatenatedFilterFlowFields == null)
				{
					return i;
				}
				IEnumerable<NCLMetaField> first = from condition in nCLMetaFieldRelation.Conditions
					where condition.SourceField.FieldClass == FieldClass.FlowField
					select condition.SourceField;
				NCLMetaField[] array = first.Except(list).ToArray();
				if (array.Length != 0)
				{
					list.AddRange(array);
					CalcFields(DataError.ThrowError, array);
				}
				if (mutableRecordBuffer.PassesFilter(concatenatedFilterFlowFields))
				{
					return i;
				}
			}
		}
		return -1;
	}

	private bool CheckRecordIdValidity(DataError errorLevel, ref NavRecordId recordId)
	{
		if (recordId.TableNo != metaTable.TableId)
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			string arg = string.Empty;
			try
			{
				NCLMetaTable metaTableById = Session.NCLMetadata.GetMetaTableById(recordId.TableNo);
				arg = metaTableById.TableCaptions.GetValueOrDefault();
			}
			catch (NavMetadataNotFoundException)
			{
			}
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.IncompatibleRecordId, arg, metaTable.TableCaptions.GetValueOrDefault()));
		}
		if (recordId.FieldCount < metaTable.PrimaryKey.KeyFieldCount)
		{
			List<NavValue> list = new List<NavValue>(RecordImplementationHelper.RecordIdToValueArray(errorLevel, metaTable, recordId));
			for (int i = recordId.FieldCount; i < metaTable.PrimaryKey.KeyFieldCount; i++)
			{
				list.Add(metaTable.PrimaryKey.KeyFieldsList[i].InitValue);
			}
			recordId = NavRecordId.CreateFromValues(metaTable.TableId, list.ToArray());
		}
		return true;
	}

	internal void SetPrimaryKeyFromRecordId(NavRecordId recordId)
	{
		CheckRecordIdValidity(DataError.ThrowError, ref recordId);
		ReadOnlyCollection<NCLMetaField> keyFieldsList = metaTable.PrimaryKey.KeyFieldsList;
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		mutableRecordBuffer.MarkAllFieldsAsChanged(includePrimaryKeyFields: false, checkMetadataForDefaults: false);
		for (int i = 0; i < keyFieldsList.Count; i++)
		{
			NCLMetaField nCLMetaField = keyFieldsList[i];
			mutableRecordBuffer[nCLMetaField.ColumnIndex] = recordId.FieldValueByIndex(nCLMetaField, i);
		}
		InvalidateCurrentResultSetEnumerator();
	}

	internal void TransferFields(NavRecord fromRecord, bool initPrimaryKeyFields)
	{
		RecordImplementation recordImplementation = fromRecord.RecordImplementation;
		if (metaTable != fromRecord.MetaTable)
		{
			TransferFieldsFromDifferentTable(fromRecord, initPrimaryKeyFields);
			return;
		}
		recordImplementation.mutableRecordBuffer = recordImplementation.CurrentMutableRecordBufferOrDefault;
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		if (initPrimaryKeyFields)
		{
			recordImplementation.dataAccess.DataProvider.FlushBufferedWrites(recordImplementation.mutableRecordBuffer);
			mutableRecordBuffer = new MutableRecordBuffer(mutableRecordBuffer.ReadOnlyBuffer, mutableRecordBuffer, recordImplementation.mutableRecordBuffer.TransactionToken);
		}
		for (int i = ((!initPrimaryKeyFields) ? 1 : 0); i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (!initPrimaryKeyFields && fieldByIndex.FieldIsPartOfPrimaryKey)
			{
				continue;
			}
			NavValue navValue = recordImplementation.mutableRecordBuffer[i];
			if (navValue != null)
			{
				if (fieldByIndex.FieldNclType == NavNclType.NavBlob)
				{
					NavBLOB navBLOB = (NavBLOB)navValue;
					NavBLOB navBLOB2 = new NavBLOB();
					NavBLOB navBLOB3 = (NavBLOB)mutableRecordBuffer[i];
					if (navBLOB3 != null && NavBLOB.CompareBlobs(navBLOB, navBLOB3) == 1)
					{
						navBLOB3.IsDirty = true;
						continue;
					}
					navBLOB2.AssignFromStream(navBLOB.GetNewStream());
					navBLOB2.IsDirty = true;
					navValue = navBLOB2;
				}
				mutableRecordBuffer[i] = navValue;
			}
			else
			{
				mutableRecordBuffer[i] = fieldByIndex.InitValue;
			}
		}
		InvalidateCurrentResultSetEnumerator();
	}

	private void TransferFieldsFromDifferentTable(NavRecord fromRecord, bool initPrimaryKeyFields)
	{
		NCLMetaField[] allFields = parentRecord.MetaTable.AllFields;
		foreach (NCLMetaField nCLMetaField in allFields)
		{
			if ((nCLMetaField.FieldIsPartOfPrimaryKey && !initPrimaryKeyFields) || nCLMetaField.FieldIsTimestamp)
			{
				continue;
			}
			int fieldIndexByNoNoException = fromRecord.MetaTable.GetFieldIndexByNoNoException(nCLMetaField.FieldNo);
			if (fieldIndexByNoNoException >= 0)
			{
				NCLMetaField fieldByIndex = fromRecord.MetaTable.GetFieldByIndex(fieldIndexByNoNoException);
				if (nCLMetaField.FieldClass == FieldClass.Normal && fieldByIndex.FieldClass == FieldClass.Normal)
				{
					NavValue newValue = RecordImplementationHelper.TransferFieldValue(nCLMetaField, fieldByIndex, fromRecord.GetFieldValue(fieldByIndex));
					parentRecord.SetFieldValue(nCLMetaField, newValue);
				}
			}
		}
	}

	internal void ValidateNonFlowField(NCLMetaField field, bool isUserInput)
	{
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		NavValue navValue = mutableRecordBuffer[field.FieldIndex];
		if (isUserInput && field.FieldNclType == NavNclType.NavDate)
		{
			if (navValue == null)
			{
				return;
			}
			Session.License.EvaluateDateLicenseFilter((NavDate)navValue);
		}
		if (navValue == null || navValue.IsZeroOrEmpty || !field.ValidateRelations)
		{
			return;
		}
		int num = EvaluateRelation(field);
		if (num == -1)
		{
			return;
		}
		NCLMetaFieldRelation nCLMetaFieldRelation = field.FieldRelations[num];
		NCLMetaTable sourceTable = nCLMetaFieldRelation.SourceTableFilters.SourceTable;
		ObsolescenceGuard.ThrowIfObsoleted(sourceTable);
		NCLMetaField nCLMetaField = ((nCLMetaFieldRelation.SourceFieldId == 0) ? sourceTable.PrimaryKey.GetKeyFieldByIndex(0) : sourceTable.GetFieldByNo(nCLMetaFieldRelation.SourceFieldId));
		ObsolescenceGuard.ThrowIfObsoleted(nCLMetaField);
		NavValue value = NavValue.CreateNavValueFromObject(nCLMetaField, navValue);
		UnaryFilterExpression item = FilterExpression.Equal(value, nCLMetaField.ExpressionContext);
		FilterFieldDictionary filterFieldDictionary = DataProviderHelper.MergeFilterFieldDictionaries(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(nCLMetaField, item)), FlowFieldsHelper.GetFilterFromMetaFilterCollection(Session, tableState.CompanyNameToken, mutableRecordBuffer, tableState.FiltersAndMarks, SecurityFiltering, nCLMetaFieldRelation.SourceTableFilters), dataAccess.GlobalFilters.GetGlobalFilters(sourceTable.TableId));
		int companyToken = ((sourceTable.DataPerCompany || metaTable.DataPerCompany) ? (metaTable.DataPerCompany ? tableState.CompanyNameToken : Session.Company.CompanyNameToken) : 0);
		ExistsCacheRequest request = new ExistsCacheRequest(companyToken, sourceTable, cacheable: true, new FiltersAndMarks(filterFieldDictionary), SecurityFiltering, forceNonLockingProviderRequest: false);
		bool flag;
		try
		{
			DataAccess dataAccessForTable = Session.DataAccessSource.GetDataAccessForTable(sourceTable);
			flag = dataAccessForTable.Exists(request);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, mutableRecordBuffer);
			throw;
		}
		if (flag)
		{
			return;
		}
		int errorCode = 18023495;
		if (filterFieldDictionary.Items.Length == 0)
		{
			errorCode = 18023438;
		}
		else if (filterFieldDictionary.AreAllFieldsContainedInListAssumingSameParent(sourceTable.PrimaryKey.DefinedKeyFields) && filterFieldDictionary.Items.Count() == sourceTable.PrimaryKey.DefinedSortingFields.Count())
		{
			errorCode = ((sourceTable.PrimaryKey.DefinedSortingFields.Count != 1) ? 85132272 : 18023402);
		}
		throw NavCSideValidateTableRelationException.CreateRelatedRecordNotFound(errorCode, parentRecord.ALFieldCaption(field.FieldNo), field.Parent.TableCaptionSafe, navValue, sourceTable.TableCaptionSafe);
	}

	internal void FieldError(NCLMetaField field, string message, string diagnosticsMessage)
	{
		throw RecordImplementationHelper.GetFieldError(parentRecord, field, CurrentMutableRecordBufferOrDefault, message, diagnosticsMessage);
	}

	internal void SetDataItemLink(NavRecord fromRecord, RecordDataItemLink recordDataItemLink)
	{
		FilterFieldDictionary filterFieldDictionary = recordDataItemLink.GetFilterFieldDictionary();
		if (filterFieldDictionary == null)
		{
			return;
		}
		RecordImplementation recordImplementation = fromRecord.RecordImplementation;
		int filterGroupNumber = tableState.FilterGroupNumber;
		try
		{
			tableState.FilterGroupNumber = 4;
			Tuple<INavFieldMetadata, FilterExpression>[] items = filterFieldDictionary.Items;
			foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)tuple.Item1;
				NCLMetaField nCLMetaField2 = (NCLMetaField)((FieldEqualsFieldFilterExpression)tuple.Item2).FieldToCompareTo;
				if (nCLMetaField2.FieldClass == FieldClass.FlowFilter)
				{
					if (recordImplementation.tableState.TryGetFilterOnFieldInCurrentGroup(nCLMetaField2, out var filterExpression))
					{
						filterExpression = filterExpression.TransferToDifferentContext(nCLMetaField.ExpressionContext);
					}
					SetFieldFilter(nCLMetaField, filterExpression);
				}
				else
				{
					NavValue value = RecordImplementationHelper.TransferFieldValue(nCLMetaField, nCLMetaField2, fromRecord.GetFieldValue(nCLMetaField2));
					SetFieldFilter(nCLMetaField, FilterExpression.Equal(value, nCLMetaField.ExpressionContext));
				}
			}
		}
		finally
		{
			TableState.FilterGroupNumber = filterGroupNumber;
		}
	}

	internal string Format()
	{
		StringBuilder sb = new StringBuilder();
		MutableRecordBuffer recordBuffer = CurrentMutableRecordBufferOrDefault;
		sb.Join("\t", metaTable.Fields.Skip(1), (NCLMetaField f) =>
		{
			sb.Append(recordBuffer[f.FieldIndex] ?? f.InitValue);
		});
		return sb.ToString();
	}

	internal void ClearMarks()
	{
		if (tableState.ClearMarks())
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal bool GetMarkedOnly()
	{
		return tableState.MarkedRecordsOnly;
	}

	internal void SetMarkedOnly(bool markedOnly)
	{
		if (tableState.MarkedRecordsOnly != markedOnly)
		{
			InvalidateCurrentResultSetEnumerator();
		}
		tableState.MarkedRecordsOnly = markedOnly;
	}

	internal bool IsMarked()
	{
		return tableState.IsMarked(CurrentMutableRecordBufferOrDefault);
	}

	internal void SetMarked(bool mark)
	{
		if (tableState.SetMarkedState(CurrentMutableRecordBufferOrDefault, mark))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	public void ResetMarks(byte[][] marked, bool allExceptThese)
	{
		tableState.ResetMarks((marked == null) ? new NavRecordId[0] : Array.ConvertAll(marked, (byte[] bytes) => new NavRecordId(bytes)), allExceptThese);
		InvalidateCurrentResultSetEnumerator();
	}

	public byte[][] GetMarks()
	{
		return tableState.MarkedRecordIds.Select((NavRecordId id) => id.GetBytes()).ToArray();
	}

	internal bool GetRecord(DataError errorLevel, NavValue[] values)
	{
		NavValue[] array = null;
		ReadOnlyCollection<NCLMetaField> keyFieldsList = metaTable.PrimaryKey.KeyFieldsList;
		if (keyFieldsList.Count > values.Length)
		{
			array = new NavValue[keyFieldsList.Count];
			Array.Copy(values, array, Math.Min(array.Length, values.Length));
		}
		for (int i = 0; i < keyFieldsList.Count; i++)
		{
			NCLMetaField nCLMetaField = keyFieldsList[i];
			if (i >= values.Length)
			{
				array[i] = nCLMetaField.InitValue;
			}
			else if (values[i].NclType != nCLMetaField.FieldNclType)
			{
				if (array == null)
				{
					array = new NavValue[keyFieldsList.Count];
					Array.Copy(values, array, Math.Min(array.Length, values.Length));
				}
				array[i] = NavValue.CreateNavValueFromObject(nCLMetaField, values[i]);
			}
			else
			{
				RecordImplementationHelper.CheckForValidStringLength(nCLMetaField, values[i]);
			}
		}
		if (array == null)
		{
			array = values;
		}
		FiltersAndMarks filtersAndMarks = ((securityFiltering == SecurityFiltering.Filtered) ? new FiltersAndMarks(GetSecurityFilters(SecurityFilterType.Read)) : FiltersAndMarks.Empty);
		return FindRecordWithoutCheckingValues(errorLevel, array, filtersAndMarks, useRecord: true, cacheOnly: false, calcAutoCalcFields: true);
	}

	internal bool GetRecord(DataError errorLevel, NavRecordId recordId, bool copyRecord, bool useFilters = false, bool calcAutoCalcFields = true)
	{
		if (!CheckRecordIdValidity(errorLevel, ref recordId))
		{
			return false;
		}
		FiltersAndMarks filtersAndMarks = FiltersAndMarks.Empty;
		if (useFilters)
		{
			filtersAndMarks = tableState.FiltersAndMarks;
		}
		else if (securityFiltering == SecurityFiltering.Filtered)
		{
			filtersAndMarks = new FiltersAndMarks(GetSecurityFilters(SecurityFilterType.Read));
		}
		return FindRecordWithoutCheckingValues(errorLevel, RecordImplementationHelper.RecordIdToValueArray(errorLevel, metaTable, recordId), filtersAndMarks, copyRecord, cacheOnly: false, calcAutoCalcFields);
	}

	private bool FindRecordWithoutCheckingValues(DataError errorLevel, NavValue[] primaryKeyValues, FiltersAndMarks filtersAndMarks, bool useRecord, bool cacheOnly, bool calcAutoCalcFields)
	{
		RecordIdCacheRequest request = new RecordIdCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, filtersAndMarks, securityFiltering, NavRecordId.CreateFromValues(parentRecord.TableID, primaryKeyValues), cacheOnly);
		try
		{
			if (dataAccess.TryGetByPrimaryKey(request, out var recordBuffer))
			{
				VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
				VerifySecurityFiltersOnRecord(recordBuffer, validatedSecurityFilters);
				if (useRecord)
				{
					InvalidateCurrentResultSetEnumerator();
					mutableRecordBuffer = recordBuffer;
				}
				if (calcAutoCalcFields)
				{
					CalcAutoCalcFields(calculateAll: true);
				}
				return true;
			}
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
		if (!filtersAndMarks.IsEmpty)
		{
			VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
		}
		else
		{
			NavApplicationObjectBase navApplicationObjectBase = parentRecord ?? Session.CurrentMethodScope.ApplicationObject;
			NavServerEventSource.Log.WritePermissionUncheckedEvent(NavEnvironment.ServerInstanceName, NavCurrentThread.Session?.Tenant?.Id, (NavCurrentThread.Session != null) ? NavCurrentThread.Session.Id : (-1), 0, metaTable.TableId, 1, (int)navApplicationObjectBase.ObjectId.ObjectType, navApplicationObjectBase.ObjectId.ObjectNumber);
		}
		if (errorLevel == DataError.TrapError)
		{
			return false;
		}
		throw RecordImplementationHelper.GetRecordNotFoundException(metaTable, primaryKeyValues);
	}

	internal bool FindRecord(DataError errorLevel, string searchMethod)
	{
		autoCalcFieldsState = AutoCalcFieldsState.NotCalculated;
		bool flag = FindRecordImp(errorLevel, searchMethod);
		if (flag && (autoCalcFieldsState & AutoCalcFieldsState.NonSmartSqlFlowFieldsCalculated) == 0)
		{
			CalcAutoCalcFields(calculateAll: false);
		}
		return flag;
	}

	private void CalcAutoCalcFields(bool calculateAll)
	{
		NCLMetaField[] array;
		if (!dataAccess.DataProvider.SupportsAutoCalcFields | calculateAll)
		{
			array = ((tableState.AutoCalcFields == null) ? new NCLMetaField[0] : tableState.AutoCalcFields.AsNclMetaFields().ToArray());
			NCLMetaField[] array2 = (from sortingField in tableState.SortingFields.DefinedSortingFields
				where sortingField.Field.NclType == NavNclType.NavBlob || sortingField.Field.FieldClass == FieldClass.FlowField
				select (NCLMetaField)sortingField.Field).ToArray();
			if (array2.Length != 0)
			{
				array = array.Union(array2).ToArray();
			}
			if (array.Length == 0)
			{
				autoCalcFieldsState = AutoCalcFieldsState.AllCalculated;
				return;
			}
		}
		else
		{
			if (tableState.AutoCalcFields == null)
			{
				autoCalcFieldsState = AutoCalcFieldsState.AllCalculated;
				return;
			}
			IEnumerable<NCLMetaField> source = tableState.AutoCalcFields.Select((INavFieldMetadata field) => (NCLMetaField)field);
			source = ((!metaTable.HasFlowFieldsSourcedFromVirtualTables) ? source.Where((NCLMetaField field) => field.FieldNclType == NavNclType.NavBlob) : source.Where((NCLMetaField field) => field.FieldNclType == NavNclType.NavBlob || (field.FieldClass == FieldClass.FlowField && (field.SourceTableField.Parent.IsVirtualTable || metaTable.IsAppTable != field.SourceTableField.Parent.IsAppTable))));
			array = source.ToArray();
		}
		CalcFields(DataError.ThrowError, array, onlyFieldsSourcedFromVirtualTables: false);
		if (calculateAll)
		{
			autoCalcFieldsState = AutoCalcFieldsState.AllCalculated;
		}
		else
		{
			autoCalcFieldsState = AutoCalcFieldsState.NonSmartSqlFlowFieldsCalculated;
		}
	}

	private bool FindRecordImp(DataError errorLevel, string searchMethod)
	{
		RecordImplementationHelper.ValidateSearchMethod(searchMethod);
		for (int i = 0; i < searchMethod.Length; i++)
		{
			switch (searchMethod[i])
			{
			case '-':
				return IssueFindRequest(errorLevel, reversed: false, FindType.Normal, useCache: true);
			case '+':
				return IssueFindRequest(errorLevel, reversed: true, FindType.Normal, useCache: true);
			case '=':
			{
				int num = i + 1;
				if (num < searchMethod.Length)
				{
					i = num;
					switch (searchMethod[i])
					{
					case '>':
						if (FindEquals(DataError.TrapError, cacheOnly: true))
						{
							return true;
						}
						if (IssueFindFromPositionRequest(reverseDirection: false, includeCurrent: true))
						{
							return true;
						}
						break;
					case '<':
						if (FindEquals(DataError.TrapError, cacheOnly: true))
						{
							return true;
						}
						if (IssueFindFromPositionRequest(reverseDirection: true, includeCurrent: true))
						{
							return true;
						}
						break;
					}
				}
				else if (FindEquals(errorLevel, cacheOnly: false))
				{
					return true;
				}
				break;
			}
			case '>':
				if (NextRecord(1) != 0)
				{
					return true;
				}
				break;
			case '<':
				if (NextRecord(-1) != 0)
				{
					return true;
				}
				break;
			default:
				throw new InvalidOperationException();
			}
		}
		if (errorLevel == DataError.ThrowError)
		{
			throw RecordImplementationHelper.GetNoResultsInFindFromPositionException(metaTable, searchMethod, CurrentPrimaryKeyValues());
		}
		return false;
	}

	private bool FindEquals(DataError errorLevel, bool cacheOnly)
	{
		autoCalcFieldsState = AutoCalcFieldsState.NotCalculated;
		bool flag = FindEqualsImp(errorLevel, cacheOnly);
		if (flag && autoCalcFieldsState != AutoCalcFieldsState.AllCalculated)
		{
			CalcAutoCalcFields(calculateAll: true);
		}
		return flag;
	}

	private bool FindEqualsImp(DataError errorLevel, bool cacheOnly)
	{
		if (mutableRecordBuffer == null)
		{
			mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		}
		if (resultSetEnumerator != null && resultSetEnumerator.IsCurrentResultValid)
		{
			mutableRecordBuffer = new MutableRecordBuffer(mutableRecordBuffer.ReadOnlyBuffer, mutableRecordBuffer.TransactionToken);
			return true;
		}
		if (tableState.CurrentKeyIndex == 1)
		{
			return FindRecordWithoutCheckingValues(errorLevel, CurrentPrimaryKeyValues(), tableState.FiltersAndMarks, useRecord: true, cacheOnly, calcAutoCalcFields: true);
		}
		MutableRecordBuffer originalBuffer = mutableRecordBuffer;
		bool flag = FindRecordWithoutCheckingValues(DataError.TrapError, CurrentPrimaryKeyValues(), tableState.FiltersAndMarks, useRecord: true, cacheOnly, calcAutoCalcFields: true);
		if (flag)
		{
			if (tableState.SortingFields.DefinedSortingFields.Any((INavSortingField field) => !((NCLMetaField)field.Field).FieldIsPartOfPrimaryKey && !RecordImplementationHelper.NavValuesEqual(mutableRecordBuffer[field.Field.ColumnIndex], originalBuffer[field.Field.ColumnIndex], dataAccess.DataProvider is SqlTableDataProvider)))
			{
				flag = false;
			}
			if (!flag)
			{
				mutableRecordBuffer = originalBuffer;
			}
		}
		if (!flag && errorLevel == DataError.ThrowError)
		{
			throw RecordImplementationHelper.GetRecordNotFoundException(metaTable, tableState.SortingFields.ActualSortingFields.Select((INavSortingField f) => Tuple.Create((NCLMetaField)f.Field, mutableRecordBuffer[f.Field.ColumnIndex])));
		}
		return flag;
	}

	private NavValue[] CurrentPrimaryKeyValues()
	{
		return CurrentMutableRecordBufferOrDefault.GetPrimaryKeyValues();
	}

	private bool IssueFindRequest(DataError errorLevel, bool reversed, FindType findType, bool useCache)
	{
		resultSetReversed = reversed;
		FindCacheRequest request = new FindCacheRequest(tableState.CompanyNameToken, metaTable, useCache, tableState.FiltersAndMarks, null, SecurityFiltering, tableState.AutoCalcFields, reversed ? tableState.SortingFields.ActualSortingFields.Reversed : tableState.SortingFields.ActualSortingFields, findType, 0, 0, SearchTimeout);
		try
		{
			resultSetEnumerator = dataAccess.Find(request, areOnlyKeyFieldsNeededForNextRowDelegate);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
		if (Advance(1, moveBackwards: false, automaticFindFromPositionOnInvalid: false, out var resultSetValid) == 0)
		{
			InvalidateCurrentResultSetEnumerator();
			if (!resultSetValid)
			{
				return IssueFindRequest(errorLevel, reversed, findType, useCache: false);
			}
			if (!tableState.FiltersAndMarks.IsEmpty)
			{
				VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
			}
			if (errorLevel == DataError.ThrowError)
			{
				throw RecordImplementationHelper.GetNoResultsInFindException(metaTable, tableState.CurrentFilterGroup);
			}
			return false;
		}
		VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
		return true;
	}

	private bool IssueFindFromPositionRequest(bool reverseDirection, bool includeCurrent, int setSize = 0)
	{
		resultSetReversed = reverseDirection;
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		EnsurePrimaryKeyFieldsSetOnBuffer(mutableRecordBuffer);
		PositionedFindCacheRequest request = new PositionedFindCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, tableState.FiltersAndMarks, null, SecurityFiltering, tableState.AutoCalcFields, reverseDirection ? tableState.SortingFields.ActualSortingFields.Reversed : tableState.SortingFields.ActualSortingFields, FindType.Normal, mutableRecordBuffer, includeCurrent, setSize, 0, SearchTimeout);
		try
		{
			resultSetEnumerator = dataAccess.FindFromPosition(request, areOnlyKeyFieldsNeededForNextRowDelegate);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, mutableRecordBuffer);
			throw;
		}
		if (Advance(1, moveBackwards: false, automaticFindFromPositionOnInvalid: true, out var _) == 0)
		{
			InvalidateCurrentResultSetEnumerator();
			return false;
		}
		VerifyPermissions(PermissionMask.Read, checkForEmptyTable: true);
		return true;
	}

	internal bool FindFirstRecord(DataError errorLevel)
	{
		bool flag = IssueFindRequest(errorLevel, reversed: false, FindType.FirstOnly, useCache: true);
		if (flag)
		{
			CalcAutoCalcFields(calculateAll: false);
		}
		return flag;
	}

	internal bool FindLastRecord(DataError errorLevel)
	{
		bool flag = IssueFindRequest(errorLevel, reversed: true, FindType.FirstOnly, useCache: true);
		if (flag)
		{
			CalcAutoCalcFields(calculateAll: false);
		}
		return flag;
	}

	internal bool FindRecordSet(DataError errorLevel, bool forUpdate)
	{
		if (forUpdate)
		{
			LockTable(wait: true, versionCheck: false);
		}
		bool flag = IssueFindRequest(errorLevel, reversed: false, FindType.All, useCache: true);
		if (flag)
		{
			CalcAutoCalcFields(calculateAll: false);
		}
		return flag;
	}

	private bool AreOnlyKeyFieldsNeededForNextRow()
	{
		return areOnlyKeyFieldsNeededForNextRow;
	}

	internal int NextRecord(int steps, int setSize = 0)
	{
		bool flag = steps < 0;
		int num = Advance(flag ? (-steps) : steps, flag ^ resultSetReversed, automaticFindFromPositionOnInvalid: true, out var _, setSize);
		int num2 = (flag ? (-num) : num);
		if (num2 != 0)
		{
			CalcAutoCalcFields(calculateAll: false);
		}
		return num2;
	}

	private int Advance(int steps, bool moveBackwards, bool automaticFindFromPositionOnInvalid, out bool resultSetValid, int setSize = 0)
	{
		resultSetValid = true;
		bool flag = areOnlyKeyFieldsNeededForNextRow;
		try
		{
			if (steps > 1)
			{
				areOnlyKeyFieldsNeededForNextRow = true;
			}
			for (int i = 0; i < steps; i++)
			{
				if (i == steps - 1 && steps > 1)
				{
					areOnlyKeyFieldsNeededForNextRow = false;
				}
				if (i == 1)
				{
					automaticFindFromPositionOnInvalid = true;
				}
				if (resultSetEnumerator == null)
				{
					if (!CreateEnumeratorAndMoveFromCurrentPosition(resultSetReversed ^ moveBackwards, setSize))
					{
						return i;
					}
					moveBackwards = false;
					continue;
				}
				bool flag2 = true;
				ResultSetMoveResult resultSetMoveResult;
				MutableRecordBuffer recordBuffer;
				try
				{
					resultSetMoveResult = (moveBackwards ? resultSetEnumerator.TryMovePrevious(out recordBuffer) : resultSetEnumerator.TryMoveNext(out recordBuffer));
					flag2 = false;
				}
				catch (NavCSideException exception)
				{
					MapException(exception, CurrentMutableRecordBufferOrDefault);
					throw;
				}
				finally
				{
					if (flag2)
					{
						InvalidateCurrentResultSetEnumerator();
					}
				}
				switch (resultSetMoveResult)
				{
				case ResultSetMoveResult.Ok:
					if (IsPermissionCheckNeeded(readingOnly: true))
					{
						VerifySecurityFiltersOnRecord(recordBuffer, validatedSecurityFilters);
					}
					mutableRecordBuffer = recordBuffer;
					break;
				case ResultSetMoveResult.NoMoreResults:
					return i;
				case ResultSetMoveResult.ResultSetNoLongerValid:
					if (automaticFindFromPositionOnInvalid)
					{
						if (!CreateEnumeratorAndMoveFromCurrentPosition(resultSetReversed ^ moveBackwards))
						{
							return i;
						}
						moveBackwards = false;
						break;
					}
					resultSetValid = false;
					return i;
				default:
					throw new NotSupportedException();
				}
			}
			return steps;
		}
		finally
		{
			areOnlyKeyFieldsNeededForNextRow = flag;
			if (mutableRecordBuffer != null && mutableRecordBuffer.ReadOnlyBuffer.OnlyCurrentKey && !areOnlyKeyFieldsNeededForNextRow)
			{
				GetRecord(DataError.ThrowError, mutableRecordBuffer.GetRecordId(), copyRecord: true);
			}
		}
	}

	private bool CreateEnumeratorAndMoveFromCurrentPosition(bool reverseDirection, int setSize = 0)
	{
		if (!IssueFindFromPositionRequest(reverseDirection, includeCurrent: false, setSize))
		{
			InvalidateCurrentResultSetEnumerator();
			return false;
		}
		return true;
	}

	internal bool InsertRecord(DataError errorLevel)
	{
		VerifyPermissions(PermissionMask.Insert, checkForEmptyTable: false);
		MutableRecordBuffer currentMutableRecordBufferOrDefault = CurrentMutableRecordBufferOrDefault;
		VerifySecurityFilters(currentMutableRecordBufferOrDefault, SecurityFilterType.Insert);
		currentMutableRecordBufferOrDefault.MarkAllFieldsAsChanged(includePrimaryKeyFields: true, checkMetadataForDefaults: true);
		InsertResult insertResult;
		MutableRecordBuffer newRecordBuffer;
		try
		{
			bool allowBuffering = errorLevel != DataError.TrapError;
			insertResult = dataAccess.Insert(tableState.CompanyNameToken, currentMutableRecordBufferOrDefault, allowBuffering, out newRecordBuffer);
		}
		catch (NavCSideDuplicateKeyException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		catch (NavCSideException exception)
		{
			MapException(exception, currentMutableRecordBufferOrDefault);
			throw;
		}
		catch (NavExternalDataProviderException lastException) when (errorLevel == DataError.TrapError)
		{
			Session.LastException = lastException;
			return false;
		}
		switch (insertResult)
		{
		case InsertResult.Success:
			mutableRecordBuffer = newRecordBuffer;
			return true;
		case InsertResult.RecordAlreadyExists:
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw RecordImplementationHelper.GetDuplicatePrimaryKeyException(metaTable, CurrentPrimaryKeyValues());
		default:
			throw new NotSupportedException();
		}
	}

	private void MapException(NavCSideException exception, IRecordBuffer recordBuffer, bool makeNewExceptionRemappable = true)
	{
		RecordImplementationHelper.MapException(exception, Session.Database, tableState.CompanyNameToken, metaTable, recordBuffer, makeNewExceptionRemappable);
	}

	internal bool ModifyRecord(DataError errorLevel)
	{
		VerifyPermissions(PermissionMask.Modify, checkForEmptyTable: false);
		this.mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		VerifySecurityFilters(this.mutableRecordBuffer, SecurityFilterType.Modify);
		MutableRecordBuffer mutableRecordBuffer = EnsurePrimaryKeyFieldsSetOnlyInReadOnlyPartOfBuffer(CurrentMutableRecordBufferOrDefault, forModify: true);
		if (!parentRecord.IsTemporary && !this.mutableRecordBuffer.IsCopiedFromTemporary)
		{
			mutableRecordBuffer.OptimizeChanges();
		}
		MediaPurger.MediaPurgeOnModifyList(this.mutableRecordBuffer);
		ModifyResult modifyResult;
		MutableRecordBuffer newRecordBuffer;
		try
		{
			modifyResult = dataAccess.Modify(tableState.CompanyNameToken, mutableRecordBuffer, ref resultSetEnumerator, out newRecordBuffer);
		}
		catch (NavCSideDuplicateKeyException ex) when (errorLevel == DataError.TrapError && !ex.UniqueIndexViolation)
		{
			return false;
		}
		catch (NavCSideException exception)
		{
			MapException(exception, mutableRecordBuffer);
			throw;
		}
		catch (NavExternalDataProviderException lastException) when (errorLevel == DataError.TrapError)
		{
			Session.LastException = lastException;
			return false;
		}
		switch (modifyResult)
		{
		case ModifyResult.Success:
			this.mutableRecordBuffer = newRecordBuffer;
			return true;
		case ModifyResult.RecordNotFound:
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw RecordImplementationHelper.GetRecordNotFoundException(metaTable, CurrentPrimaryKeyValues());
		case ModifyResult.RecordAlreadyExists:
			throw new NavCSideException(Lang.DuplicateKeyErrorDuringModify);
		default:
			throw new NotSupportedException();
		}
	}

	internal bool DeleteRecord(DataError errorLevel)
	{
		VerifyPermissions(PermissionMask.Delete, checkForEmptyTable: false);
		MediaPurger.DiscoverMediaLinks();
		MutableRecordBuffer recordBuffer = EnsurePrimaryKeyFieldsSetOnlyInReadOnlyPartOfBuffer(CurrentMutableRecordBufferOrDefault, forModify: false);
		VerifySecurityFilters(recordBuffer, SecurityFilterType.Delete);
		DeleteResult deleteResult;
		try
		{
			deleteResult = dataAccess.Delete(tableState.CompanyNameToken, recordBuffer, ref resultSetEnumerator);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, recordBuffer);
			throw;
		}
		switch (deleteResult)
		{
		case DeleteResult.Success:
			return true;
		case DeleteResult.RecordNotFound:
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw RecordImplementationHelper.GetRecordNotFoundException(metaTable, CurrentPrimaryKeyValues(), Session.Tenant?.Id);
		default:
			throw new NotSupportedException();
		}
	}

	internal bool RenameRecord(DataError errorLevel, NavRecord renamedRecord)
	{
		VerifyPermissions(PermissionMask.Modify, checkForEmptyTable: false);
		MutableRecordBuffer mutableRecordBuffer = EnsurePrimaryKeyFieldsSetOnlyInReadOnlyPartOfBuffer(CurrentMutableRecordBufferOrDefault, forModify: false);
		if (mutableRecordBuffer == this.mutableRecordBuffer)
		{
			mutableRecordBuffer = mutableRecordBuffer.Clone();
		}
		MutableRecordBuffer mutableRecordBuffer2 = renamedRecord.RecordImplementation.mutableRecordBuffer;
		VerifySecurityFilters(mutableRecordBuffer2, SecurityFilterType.Modify);
		for (int i = 0; i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (fieldByIndex.FieldIsPartOfPrimaryKey)
			{
				mutableRecordBuffer[i] = mutableRecordBuffer2[i];
			}
			else if (fieldByIndex.FieldClass == FieldClass.Normal)
			{
				NavValue changedFieldValue = mutableRecordBuffer2.GetChangedFieldValue(i);
				if (changedFieldValue != null)
				{
					mutableRecordBuffer[i] = changedFieldValue;
				}
			}
		}
		ResultSetEnumerator resultSetEnumerator = null;
		ModifyResult modifyResult;
		MutableRecordBuffer newRecordBuffer;
		try
		{
			modifyResult = dataAccess.Modify(tableState.CompanyNameToken, mutableRecordBuffer, ref resultSetEnumerator, out newRecordBuffer);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, mutableRecordBuffer);
			throw;
		}
		catch (NavExternalDataProviderException lastException) when (errorLevel == DataError.TrapError)
		{
			Session.LastException = lastException;
			return false;
		}
		switch (modifyResult)
		{
		case ModifyResult.Success:
			this.mutableRecordBuffer = newRecordBuffer;
			return true;
		case ModifyResult.RecordNotFound:
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw RecordImplementationHelper.GetRecordNotFoundException(metaTable, CurrentPrimaryKeyValues());
		case ModifyResult.RecordAlreadyExists:
			throw RecordImplementationHelper.GetRecordAlreadyExistsException(metaTable, mutableRecordBuffer.GetPrimaryKeyValues());
		default:
			throw new NotSupportedException();
		}
	}

	internal void DeleteAllRecords()
	{
		VerifyPermissions(PermissionMask.Delete, checkForEmptyTable: true);
		FilterFieldDictionary securityFilters = ((SecurityFiltering == SecurityFiltering.Ignored) ? FilterFieldDictionary.Empty : GetSecurityFilters(SecurityFilterType.Delete));
		if (SecurityFiltering == SecurityFiltering.Validated || SecurityFiltering == SecurityFiltering.Filtered)
		{
			VerifyNoRecordsOutsideValidatedSecurityFilter(securityFilters);
		}
		try
		{
			dataAccess.DeleteAll(tableState.CompanyNameToken, metaTable, tableState.FiltersAndMarks, SecurityFiltering);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
	}

	internal void ModifyAllRecords(NCLMetaField metaField, NavValue value)
	{
		if (metaField.FieldIsPartOfPrimaryKey)
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotModifyAllOnPrimaryKey, parentRecord.ALFieldCaption(metaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
		}
		ModifyOrRenameAllRecords(new FieldDictionary<NavValue>(new Tuple<INavFieldMetadata, NavValue>(metaField, value)));
	}

	internal void ModifyOrRenameAllRecords(FieldDictionary<NavValue> newFieldValues)
	{
		VerifyPermissions(PermissionMask.Modify, checkForEmptyTable: true);
		FilterFieldDictionary securityFilters = ((SecurityFiltering == SecurityFiltering.Ignored) ? FilterFieldDictionary.Empty : GetSecurityFilters(SecurityFilterType.Modify));
		if (SecurityFiltering == SecurityFiltering.Validated || SecurityFiltering == SecurityFiltering.Filtered)
		{
			VerifyNoRecordsOutsideValidatedSecurityFilter(securityFilters);
		}
		try
		{
			dataAccess.ModifyAll(tableState.CompanyNameToken, metaTable, tableState.FiltersAndMarks, SecurityFiltering, newFieldValues);
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
	}

	internal bool CalcFields(DataError errorLevel, NCLMetaField[] fields)
	{
		return CalcFields(errorLevel, fields, onlyFieldsSourcedFromVirtualTables: false);
	}

	private bool CalcFields(DataError errorLevel, NCLMetaField[] fields, bool onlyFieldsSourcedFromVirtualTables)
	{
		if (fields == null || fields.Length == 0)
		{
			return true;
		}
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		EnsurePrimaryKeyFieldsSetOnBuffer(mutableRecordBuffer);
		List<NCLMetaField> list = new List<NCLMetaField>(fields.Length);
		List<NCLMetaField> list2 = new List<NCLMetaField>(fields.Length);
		foreach (NCLMetaField item in fields.Where((NCLMetaField field) => field.FieldActive))
		{
			if (item.FieldNclType == NavNclType.NavBlob)
			{
				GetWriteableBlobOnFieldAndEnsureInMutablePartOfBuffer(item);
				list.Add(item);
				continue;
			}
			if (item.FieldClass != FieldClass.FlowField)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.MustBeAFlowField, parentRecord.ALFieldCaption(item.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
			}
			if (item.CalculationFormula == NCLMetaCalculationFormula.EmptyFormula)
			{
				throw new NavCSideException(18023430, string.Format(CultureInfo.CurrentCulture, Lang.MustDefineFormula, parentRecord.ALFieldCaption(item.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
			}
			list2.Add(item);
		}
		try
		{
			VerifyPermissionsCalculatedFields(fields);
			if (list2.Count > 0)
			{
				FieldDictionary<NavValue> fieldDictionary = FlowFieldsHelper.CalcFields(Session, tableState.CompanyNameToken, CurrentMutableRecordBufferOrDefault, tableState.FiltersAndMarks, list2.ToArray(), onlyFieldsSourcedFromVirtualTables, securityFiltering);
				Tuple<INavFieldMetadata, NavValue>[] items = fieldDictionary.Items;
				foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
				{
					mutableRecordBuffer[((NCLMetaField)tuple.Item1).FieldIndex] = tuple.Item2;
				}
			}
			if (list.Count > 0)
			{
				int companyNameToken = tableState.CompanyNameToken;
				NCLMetaTable metaApplicationObject = metaTable;
				MutableRecordBuffer currentMutableRecordBufferOrDefault = CurrentMutableRecordBufferOrDefault;
				INavFieldMetadata[] fields2 = list.ToArray();
				GetBlobContentCacheRequest request = new GetBlobContentCacheRequest(companyNameToken, metaApplicationObject, cacheable: true, currentMutableRecordBufferOrDefault, new FieldList(fields2));
				Tuple<INavFieldMetadata, NavValue>[] items2 = dataAccess.GetBlobContent(request).Items;
				foreach (Tuple<INavFieldMetadata, NavValue> tuple2 in items2)
				{
					mutableRecordBuffer[((NCLMetaField)tuple2.Item1).FieldIndex] = tuple2.Item2;
				}
			}
			return true;
		}
		catch (NavCSideException ex)
		{
			if (errorLevel == DataError.TrapError && (ex.ErrorCode == 18023674 || ex.ErrorNumber == 1005))
			{
				return false;
			}
			MapException(ex, mutableRecordBuffer);
			throw;
		}
	}

	internal bool SetAutoCalcFields(DataError errorLevel, NCLMetaField[] calcFields, bool replace)
	{
		if (calcFields == null || calcFields.Length == 0)
		{
			if (tableState.AutoCalcFields != null)
			{
				tableState.AutoCalcFields = null;
				InvalidateCurrentResultSetEnumerator();
			}
			return true;
		}
		INavFieldMetadata[] array2;
		if (tableState.AutoCalcFields == null)
		{
			INavFieldMetadata[] array = calcFields.Distinct().ToArray();
			array2 = array;
		}
		else if (replace)
		{
			HashSet<NCLMetaField> distinctCalcFields = new HashSet<NCLMetaField>(calcFields);
			if (IsCurrentResultSetEnumeratorValid() && tableState.AutoCalcFields.Count == distinctCalcFields.Count && tableState.AutoCalcFields.FirstOrDefault((INavFieldMetadata f) => !distinctCalcFields.Contains(f)) == null)
			{
				return true;
			}
			INavFieldMetadata[] array = distinctCalcFields.ToArray();
			array2 = array;
		}
		else
		{
			array2 = tableState.AutoCalcFields.Union(calcFields).ToArray();
			if (array2.Length == tableState.AutoCalcFields.Count)
			{
				return true;
			}
		}
		foreach (NCLMetaField nCLMetaField in calcFields)
		{
			if (nCLMetaField.FieldClass != FieldClass.FlowField && nCLMetaField.FieldNclType != NavNclType.NavBlob)
			{
				if (errorLevel == DataError.ThrowError)
				{
					throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.MustBeAFlowField, parentRecord.ALFieldCaption(nCLMetaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
				}
				return false;
			}
			if (nCLMetaField.FieldClass == FieldClass.FlowField && nCLMetaField.CalculationFormula == NCLMetaCalculationFormula.EmptyFormula)
			{
				throw new NavCSideException(18023430, string.Format(CultureInfo.CurrentCulture, Lang.MustDefineFormula, parentRecord.ALFieldCaption(nCLMetaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
			}
		}
		tableState.AutoCalcFields = new FieldList(array2);
		InvalidateCurrentResultSetEnumerator();
		return true;
	}

	internal NCLMetaField[] GetAutoCalcFields()
	{
		if (tableState.AutoCalcFields != null)
		{
			return tableState.AutoCalcFields.AsNclMetaFields().ToArray();
		}
		return null;
	}

	internal bool CalcSums(NCLMetaField[] fields)
	{
		foreach (NCLMetaField nCLMetaField in fields)
		{
			if (!nCLMetaField.CanBeAggregated)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CanOnlySumNumericFields, parentRecord.ALFieldCaption(nCLMetaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
			}
		}
		CalcSumsCacheRequest request = new CalcSumsCacheRequest(tableState.CompanyNameToken, metaTable, cacheable: true, tableState.FiltersAndMarks, SecurityFiltering, new FieldList(fields));
		try
		{
			VerifyPermissionsCalculatedFields(fields);
			if (SecurityFiltering == SecurityFiltering.Validated)
			{
				VerifyNoRecordsOutsideValidatedSecurityFilter(validatedSecurityFilters);
			}
			FieldDictionary<NavValue> fieldDictionary = dataAccess.CalcSums(request);
			mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
			Tuple<INavFieldMetadata, NavValue>[] items = fieldDictionary.Items;
			foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
			{
				mutableRecordBuffer[tuple.Item1.ColumnIndex] = tuple.Item2;
			}
			return true;
		}
		catch (NavCSideException exception)
		{
			MapException(exception, CurrentMutableRecordBufferOrDefault);
			throw;
		}
	}

	internal int[] PrepareSubTableFromFlowField(NCLMetaField field, NavRecord subTableRecord)
	{
		NCLMetaCalculationFormula calculationFormula = field.CalculationFormula;
		FilterFieldDictionary filterFieldDictionary = DataProviderHelper.MergeFilterFieldDictionaries(FlowFieldsHelper.GetFilterFromMetaFilterCollection(Session, tableState.CompanyNameToken, CurrentMutableRecordBufferOrDefault, tableState.FiltersAndMarks, SecurityFiltering, calculationFormula.Filters), Session.DataAccessSource.GlobalFilters.GetGlobalFilters(subTableRecord.TableID));
		Tuple<INavFieldMetadata, FilterExpression>[] items = filterFieldDictionary.Items;
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
		{
			NCLMetaField metaField = (NCLMetaField)tuple.Item1;
			FilterExpression item = tuple.Item2;
			try
			{
				RangeList rangeList = item.ToRangeList();
				NavValue navValue = rangeList.Ranges.Aggregate(null, (NavValue foundVal, Range range) =>
				{
					NavValue navValue2 = foundVal;
					if (navValue2 == null)
					{
						if (!range.IsLowIncluded || range.IsLowIsMinimum)
						{
							if (!range.IsHighIncluded || range.IsHighMaximum)
							{
								return (NavValue)null;
							}
							return range.HighValue;
						}
						navValue2 = range.LowValue;
					}
					return navValue2;
				});
				if (navValue != null)
				{
					subTableRecord.RecordImplementation.SetFieldFilter(metaField, item);
					subTableRecord.SetFieldValue(metaField, navValue);
					continue;
				}
			}
			catch (CannotConvertWildcardIntoRangeException)
			{
			}
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ExpressionMustHaveInclusiveRange, item));
		}
		return filterFieldDictionary.Items.Select((Tuple<INavFieldMetadata, FilterExpression> t) => ((NCLMetaField)t.Item1).FieldNo).ToArray();
	}

	internal void SetFieldRange(NCLMetaField metaField, NavValue fromValue, NavValue toValue)
	{
		if (fromValue != null)
		{
			RecordImplementationHelper.CheckForValidStringLength(metaField, fromValue);
		}
		if (toValue != null)
		{
			RecordImplementationHelper.CheckForValidStringLength(metaField, toValue);
		}
		fromValue = ReplaceIfQuotedEmptyString(metaField, fromValue);
		toValue = ReplaceIfQuotedEmptyString(metaField, toValue);
		fromValue = ReplaceIfOptionValuesDifferOnMetadata(metaField, fromValue);
		toValue = ReplaceIfOptionValuesDifferOnMetadata(metaField, toValue);
		FilterExpression expression;
		if (toValue != null)
		{
			expression = FilterExpression.RangeBetweenInclusive(fromValue, toValue, metaField.ExpressionContext);
		}
		else
		{
			expression = ((fromValue == null) ? null : FilterExpression.Equal(fromValue, metaField.ExpressionContext));
		}
		if (tableState.SetFilterOnCurrentGroup(metaField, expression, resultSetEnumerator != null))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	private static NavValue ReplaceIfQuotedEmptyString(NCLMetaField metaField, NavValue value)
	{
		if (value == null || !metaField.IsStringType)
		{
			return value;
		}
		string text = value.ToString();
		if (text == "''")
		{
			return NavValue.CreateNavValueFromObject(metaField, string.Empty);
		}
		return value;
	}

	private static NavValue ReplaceIfOptionValuesDifferOnMetadata(NCLMetaField metaField, NavValue value)
	{
		if (value == null || value.NclType != NavNclType.NavOption || value.CompareTypeAndMetadata(metaField))
		{
			return value;
		}
		return NavValue.CreateNavValueFromObject(metaField, value);
	}

	internal void SetFieldFilter(NCLMetaField metaField, string expression, params NavValue[] values)
	{
		try
		{
			FilterExpression filterExpression = FilterExpressionParser.Parse(expression, metaField.ExpressionContext, values);
			SetFieldFilter(metaField, filterExpression);
		}
		catch (NavInvalidFilterExpressionException innerException)
		{
			throw RecordImplementationHelper.GetInvalidFilterException(parentRecord, metaField, expression, innerException);
		}
	}

	public void SetFieldFilter(NCLMetaField metaField, FilterExpression filterExpression)
	{
		if (filterExpression.IsNullOrConstantTrue())
		{
			filterExpression = null;
		}
		if (tableState.SetFilterOnCurrentGroup(metaField, filterExpression, resultSetEnumerator != null))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal bool TrySetFieldFilter(NCLMetaField metaField, string expression, params NavValue[] values)
	{
		if (!FilterExpressionParser.TryParse(expression, metaField.ExpressionContext, out var filterExpression, values))
		{
			return false;
		}
		SetFieldFilter(metaField, filterExpression);
		return true;
	}

	public void AddFiltersToFilterGroup(FilterFieldDictionary filters)
	{
		InternalSetFilters(filters.Items, clearExistingFilters: false);
	}

	private void InternalSetFilters(IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> fieldFilters, bool clearExistingFilters)
	{
		if (tableState.SetFiltersOnCurrentGroup(fieldFilters, clearExistingFilters, mutableRecordBuffer != null))
		{
			InvalidateCurrentResultSetEnumerator();
		}
	}

	internal string GetFieldFilter(NCLMetaField metaField)
	{
		if (tableState.TryGetFilterOnFieldInCurrentGroup(metaField, out var filterExpression))
		{
			return filterExpression.ToString();
		}
		return string.Empty;
	}

	internal NavValue GetFieldRangeMin(NCLMetaField metaField)
	{
		Range singleRange = GetSingleRange(metaField, out var expression);
		if (singleRange.IsLowIsMinimum)
		{
			throw new NavCSideFilterException(string.Format(CultureInfo.CurrentCulture, Lang.GetRangeMinNoMin, expression, parentRecord.ALFieldCaption(metaField.FieldNo), metaField.Parent.TableCaptions.GetValueOrDefault()));
		}
		return singleRange.LowValue;
	}

	internal NavValue GetFieldRangeMax(NCLMetaField metaField)
	{
		Range singleRange = GetSingleRange(metaField, out var expression);
		if (singleRange.IsHighMaximum)
		{
			throw new NavCSideFilterException(string.Format(CultureInfo.CurrentCulture, Lang.GetRangeMaxNoMax, expression, parentRecord.ALFieldCaption(metaField.FieldNo), metaField.Parent.TableCaptions.GetValueOrDefault()));
		}
		return singleRange.HighValue;
	}

	internal bool RecordCompliesWithFilters()
	{
		return CurrentMutableRecordBufferOrDefault.PassesFieldFilters(tableState.AllFiltersDictionary);
	}

	private Range GetSingleRange(NCLMetaField field, out FilterExpression expression)
	{
		if (!tableState.AllFiltersDictionary.TryGetValue(field, out expression))
		{
			throw new NavCSideFilterException(18023435, string.Format(CultureInfo.CurrentCulture, Lang.MissingFilter, parentRecord.ALFieldCaption(field.FieldNo), field.Parent.TableCaptions.GetValueOrDefault()));
		}
		try
		{
			RangeList rangeList = expression.ToRangeList();
			if (rangeList.IsEmpty)
			{
				throw NavCSideFilterException.CreateForNoPossibleValues(18023434, CultureInfo.CurrentCulture, parentRecord.ALFieldCaption(field.FieldNo), field.Parent.TableCaptions.GetValueOrDefault(), expression.ToString());
			}
			if (rangeList.Ranges.Count > 1)
			{
				throw NavCSideFilterException.CreateForNoSingleRange(18023434, CultureInfo.CurrentCulture, parentRecord.ALFieldCaption(field.FieldNo), field.Parent.TableCaptions.GetValueOrDefault(), expression.ToString());
			}
			return rangeList.Ranges[0];
		}
		catch (CannotConvertWildcardIntoRangeException)
		{
			return Range.CreateFull();
		}
	}

	internal void StoreFieldValue(NCLMetaField field, NavValue value)
	{
		if (field.FieldNclType == NavNclType.NavDecimal)
		{
			NavDecimalHelper.ValidateDecimalRange(value.ToDecimal());
		}
		if (mutableRecordBuffer == null)
		{
			mutableRecordBuffer = DefaultMutableRecordBuffer;
		}
		if (resultSetEnumerator != null)
		{
			if ((field.FieldIsPartOfPrimaryKey || tableState.SortingFields.ActualSortingFields.Contains(field)) && !(mutableRecordBuffer[field.FieldIndex] ?? field.InitValue).Equals(value))
			{
				InvalidateCurrentResultSetEnumerator();
				if (field.FieldIsPartOfPrimaryKey)
				{
					mutableRecordBuffer.MarkAllFieldsAsChanged(includePrimaryKeyFields: false, checkMetadataForDefaults: false);
				}
			}
		}
		else if (field.FieldIsPartOfPrimaryKey && !(mutableRecordBuffer[field.FieldIndex] ?? field.InitValue).Equals(value))
		{
			mutableRecordBuffer.MarkAllFieldsAsChanged(includePrimaryKeyFields: false, checkMetadataForDefaults: false);
		}
		mutableRecordBuffer[field.FieldIndex] = value;
	}

	internal NavValue GetFieldValue(NCLMetaField field)
	{
		if (field.FieldNclType == NavNclType.NavBlob)
		{
			return GetWriteableBlobOnFieldAndEnsureInMutablePartOfBuffer(field);
		}
		if (mutableRecordBuffer != null)
		{
			if (field.FieldIsTimestamp)
			{
				dataAccess.DataProvider.FlushBufferedWrites(mutableRecordBuffer);
			}
			NavValue navValue = mutableRecordBuffer[field.FieldIndex];
			if (navValue != null)
			{
				return navValue;
			}
		}
		return field.InitValue;
	}

	private NavValue GetWriteableBlobOnFieldAndEnsureInMutablePartOfBuffer(NCLMetaField field)
	{
		mutableRecordBuffer = CurrentMutableRecordBufferOrDefault;
		NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer.GetChangedFieldValue(field.FieldIndex);
		if (navBLOB == null)
		{
			NavBLOB navBLOB2 = (NavBLOB)mutableRecordBuffer.GetOriginalValue(field.FieldIndex);
			navBLOB = ((navBLOB2 != null) ? new NavBLOB(navBLOB2.GetLength()) : NavBLOB.Default());
			mutableRecordBuffer[field.FieldIndex] = navBLOB;
		}
		return navBLOB;
	}

	private void ResetRecordBuffer()
	{
		mutableRecordBuffer = DefaultMutableRecordBuffer;
		InvalidateCurrentResultSetEnumerator();
	}

	public void InitFromRecordId(NavRecordId recordId)
	{
		mutableRecordBuffer = null;
		SetPrimaryKeyFromRecordId(recordId);
	}

	public bool TableEquals(RecordImplementation other)
	{
		if (other != null)
		{
			return tableState == other.tableState;
		}
		return false;
	}

	public RecordState CreateRecordState()
	{
		return new ManagedRecordState(this);
	}

	internal int AddLink(string url, string description)
	{
		return RecordLink.AddLink(parentRecord, url, description);
	}

	internal void DeleteLink(int linkId)
	{
		RecordLink.DeleteLink(parentRecord, linkId);
	}

	internal void CopyLinks(NavRecord fromRecord)
	{
		RecordLink.CopyLinks(fromRecord, parentRecord);
	}

	internal bool HasLinks()
	{
		return RecordLink.HasLinks(parentRecord);
	}

	internal void MoveLinks(NavRecord sourceRecord)
	{
		RecordLink.MoveLinks(sourceRecord, parentRecord);
	}

	internal void DeleteLinks()
	{
		RecordLink.DeleteLinks(parentRecord);
	}

	public bool TableHasLinks()
	{
		return RecordLink.TableHasLinks(parentRecord, metaTable, GetActiveCompany());
	}

	private void InvalidateCurrentResultSetEnumerator()
	{
		resultSetEnumerator = null;
		resultSetReversed = false;
	}

	public void ClearResultSetMarksAndFilters()
	{
		InvalidateCurrentResultSetEnumerator();
		tableState.ClearMarks();
		tableState.ClearAllFilters();
	}

	public bool IsCurrentResultSetEnumeratorValid()
	{
		return resultSetEnumerator != null;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || IsDisposed)
			{
				return;
			}
			if (dataAccess != null && parentRecord.IsTemporary)
			{
				((TempTableDataProvider)dataAccess.DataProvider).DecrementUsageCountOrReportRows();
			}
			if (mutableRecordBuffer != null && metaTable.BlobFieldCount > 0)
			{
				NCLMetaField[] blobFields = metaTable.BlobFields;
				foreach (NCLMetaField nCLMetaField in blobFields)
				{
					((NavBLOB)mutableRecordBuffer.GetChangedFieldValue(nCLMetaField.FieldIndex))?.Dispose();
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
