using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TempTableDataProvider : DataProvider
{
	private class RecordBufferEvaluatorVisitor : FilterExpressionVisitor<TempTableRecordBuffer, bool>
	{
		private readonly TempTableDataProvider dataProvider;

		private readonly FiltersAndMarks filtersAndMarks;

		private readonly int companyToken;

		public RecordBufferEvaluatorVisitor(int companyToken, TempTableDataProvider dataProvider, FiltersAndMarks filtersAndMarksAndMarks)
		{
			this.dataProvider = dataProvider;
			filtersAndMarks = filtersAndMarksAndMarks;
			this.companyToken = companyToken;
		}

		protected override bool VisitBinary(BinaryFilterExpression binaryExpression, TempTableRecordBuffer input)
		{
			switch (binaryExpression.ExpressionType)
			{
			case FilterExpressionType.And:
				if (Visit(binaryExpression.Left, input))
				{
					return Visit(binaryExpression.Right, input);
				}
				return false;
			case FilterExpressionType.Or:
				if (!Visit(binaryExpression.Left, input))
				{
					return Visit(binaryExpression.Right, input);
				}
				return true;
			default:
				throw new NotSupportedException();
			}
		}

		protected override bool VisitUnary(UnaryFilterExpression unaryExpression, TempTableRecordBuffer input)
		{
			return Evaluate(unaryExpression, unaryExpression.ExpressionContext, input);
		}

		protected override bool VisitWildcard(WildcardFilterExpression wildcardExpression, TempTableRecordBuffer input)
		{
			return Evaluate(wildcardExpression, wildcardExpression.ExpressionContext, input);
		}

		protected override bool VisitBooleanConstant(BooleanConstantFilterExpression booleanConstantExpression, TempTableRecordBuffer input)
		{
			return booleanConstantExpression.Value;
		}

		protected override bool VisitFieldEqualsField(FieldEqualsFieldFilterExpression fieldEqualsFieldExpression, TempTableRecordBuffer input)
		{
			throw new NotSupportedException();
		}

		private bool Evaluate(FilterExpression expression, FilterExpressionContext expressionContext, TempTableRecordBuffer input)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)expressionContext.Metadata;
			NavValue navValue = null;
			if (nCLMetaField.FieldClass == FieldClass.FlowField)
			{
				FieldDictionary<NavValue> fieldDictionary = FlowFieldsHelper.CalcFields(dataProvider.navSession, companyToken, input, filtersAndMarks, new NCLMetaField[1] { nCLMetaField }, onlyFieldsSourcedFromVirtualTables: false, SecurityFiltering.Ignored);
				if (fieldDictionary.Items.Length != 0)
				{
					navValue = fieldDictionary.Items[0].Item2;
				}
			}
			else
			{
				navValue = input[nCLMetaField.ColumnIndex];
			}
			return expression.Evaluate(navValue ?? nCLMetaField.InitValue);
		}
	}

	private class UniqueIndexItem
	{
		private bool hashCalculated;

		private int hash;

		public NavValue[] Values { get; private set; }

		public UniqueIndexItem(NavValue[] values)
		{
			Values = values ?? throw new ArgumentNullException("values");
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (!(obj is UniqueIndexItem uniqueIndexItem) || Values.Length != uniqueIndexItem.Values.Length || GetHashCode() != uniqueIndexItem.GetHashCode())
			{
				return false;
			}
			for (int i = 0; i < Values.Length; i++)
			{
				NavValue navValue = Values[i];
				NavValue navValue2 = uniqueIndexItem.Values[i];
				NavStringValue navStringValue = navValue as NavStringValue;
				if (navStringValue != null)
				{
					NavStringValue navStringValue2 = navValue2 as NavStringValue;
					if (navStringValue2 == null || !NavGlobal.AppDatabase.CollationAwareStringComparer.Equals(navStringValue.Value, navStringValue2.Value))
					{
						return false;
					}
				}
				else if (!navValue.Equals(navValue2))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			if (hashCalculated)
			{
				return hash;
			}
			NavValue[] values = Values;
			foreach (NavValue navValue in values)
			{
				NavStringValue navStringValue = navValue as NavStringValue;
				hash = HashCodeHelper.CombineHashCodes(hash, (navStringValue != null) ? NavGlobal.AppDatabase.CollationAwareStringComparer.GetHashCode(navStringValue) : navValue.GetHashCode());
			}
			hashCalculated = true;
			return hash;
		}
	}

	private readonly NavSession navSession;

	private readonly NCLMetaTable table;

	private Dictionary<SortingFieldList, AvlTree<TempTableRecordBuffer>> trees;

	private AvlTree<TempTableRecordBuffer> primaryTree;

	private Dictionary<NCLMetaKey, HashSet<UniqueIndexItem>> uniqueIndexes;

	private readonly SortingFieldList primaryKeySortingFields;

	private int sharedTableUsageCount;

	internal int SharedTablesUsageCount
	{
		get
		{
			return sharedTableUsageCount;
		}
		set
		{
			sharedTableUsageCount = value;
		}
	}

	private bool HasSecondaryTrees
	{
		get
		{
			if (trees != null)
			{
				return trees.Count > 1;
			}
			return false;
		}
	}

	public override bool ShouldResultSetBufferRows => false;

	public TempTableDataProvider(NavSession session, NCLMetaTable table)
	{
		navSession = session;
		this.table = table;
		primaryKeySortingFields = this.table.PrimaryKey.SortingFieldsWithPrimaryKeyFields;
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		if (TryGetValue(request.RecordId, out var buffer))
		{
			recordBuffer = buffer.ToReadOnlyBuffer();
			FilterExpression combinedExpressionFilter = GetCombinedExpressionFilter(request.FiltersAndMarks, null);
			if (combinedExpressionFilter != null)
			{
				RecordBufferEvaluatorVisitor visitor = new RecordBufferEvaluatorVisitor(request.CompanyToken, this, request.FiltersAndMarks);
				return IsRowWithinFilter(buffer, combinedExpressionFilter, visitor);
			}
			return true;
		}
		recordBuffer = null;
		return false;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = FindImplementation(request);
		if (request.FindType != FindType.FirstOnly)
		{
			return enumerable;
		}
		return enumerable.Take(1);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = FindByPositionImplementation(request);
		if (request.FindType != FindType.FirstOnly)
		{
			return enumerable;
		}
		return enumerable.Take(1);
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		return FindImplementation(MakeExistsFindRequest(request)).Any();
	}

	public override int Count(DataProviderRequest request)
	{
		if (TreeIsNullOrEmpty())
		{
			return 0;
		}
		if (request.FiltersAndMarks.IsEmpty || (request.FiltersAndMarks.GetCombinedFilter() == null && request.FiltersAndMarks.Filters.ConcatenatedFilterFlowFields == null))
		{
			return primaryTree.Count;
		}
		IEnumerable<TempTableRecordBuffer> enumerable = Filter(request.CompanyToken, request.FiltersAndMarks, null, primaryKeySortingFields, includeCurrent: false);
		if (enumerable is AvlTree<TempTableRecordBuffer> { Count: not -1 } avlTree)
		{
			return avlTree.Count;
		}
		return enumerable.Count();
	}

	public override FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override FieldDictionary<NavValue> CalcMinMax(CalcMinMaxProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[request.BlobsToGet.Count];
		for (int i = 0; i < request.BlobsToGet.Count; i++)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)request.BlobsToGet[i];
			if (nCLMetaField.FieldNclType != NavNclType.NavBlob)
			{
				throw new InvalidOperationException("This method can only get the content of blobs");
			}
			NavBLOB navBLOB = (NavBLOB)request.RecordBuffer[nCLMetaField.FieldIndex];
			if (TryGetValue(request.RecordBuffer.GetRecordId(), out var buffer))
			{
				NavBLOB navBLOB2 = (NavBLOB)buffer[nCLMetaField.FieldIndex];
				if (navBLOB == navBLOB2)
				{
					navBLOB = new NavBLOB(navBLOB2.GetLength());
				}
				navBLOB.AssignFromStream(navBLOB2.GetStream());
			}
			else
			{
				navBLOB.Clear();
			}
			array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, navBLOB);
		}
		return new FieldDictionary<NavValue>(array);
	}

	public override FieldDictionary<NavValue> CalcSums(CalcSumsProviderRequest request)
	{
		Decimal18[] array = new Decimal18[request.FieldsToCalculate.Count];
		foreach (TempTableRecordBuffer item in Filter(request.CompanyToken, request.FiltersAndMarks, null, primaryKeySortingFields, includeCurrent: false))
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += item[request.FieldsToCalculate[i].ColumnIndex].ToDecimal();
			}
		}
		return new FieldDictionary<NavValue>(request.FieldsToCalculate.Zip(array, (INavFieldMetadata f, Decimal18 s) =>
		{
			try
			{
				return new Tuple<INavFieldMetadata, NavValue>(f, NavValue.CreateNavValueFromObject(f, s));
			}
			catch (OverflowException)
			{
				throw new NavNCLCalculationOverflowException(string.Format(CultureInfo.CurrentCulture, Lang.FieldCalculationOverflow, f.DescriptiveName));
			}
		}));
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		outputRecordBuffer = null;
		EnsureTreeCreated();
		NavRecordId recordId = recordBuffer.GetRecordId();
		NavRecordId recordId2 = recordBuffer.ReadOnlyBuffer.GetRecordId();
		bool flag = !recordId2.Equals(recordId);
		List<(HashSet<UniqueIndexItem>, UniqueIndexItem, UniqueIndexItem)> list = UpdateUniqueIndexes((int idx) => recordBuffer.GetOriginalValue(idx), (int idx) => recordBuffer[idx]);
		bool flag2;
		TempTableRecordBuffer buffer;
		TempTableRecordBuffer tempTableRecordBuffer;
		if (flag)
		{
			TempTableRecordBuffer value = new TempTableRecordBuffer(table, recordBuffer.ReadOnlyBuffer.ToArray());
			flag2 = primaryTree.Remove(value, out buffer);
			tempTableRecordBuffer = new TempTableRecordBuffer(table, recordBuffer.ToArray());
		}
		else
		{
			flag2 = TryGetValue(recordId2, out buffer);
			tempTableRecordBuffer = buffer;
		}
		if (!flag2)
		{
			return ModifyResult.RecordNotFound;
		}
		ModifyResult modifyResult = ModifyAllTrees(recordBuffer, tempTableRecordBuffer, buffer, flag);
		if (modifyResult != ModifyResult.Success)
		{
			return modifyResult;
		}
		foreach (var item in list)
		{
			item.Item1.Remove(item.Item2);
			item.Item1.Add(item.Item3);
		}
		NavValue[] array = (NavValue[])tempTableRecordBuffer.Items;
		outputRecordBuffer = new ReadOnlyRecordBuffer(recordBuffer.MetaTable, (NavValue[])array.Clone());
		return ModifyResult.Success;
	}

	private List<(HashSet<UniqueIndexItem> Index, UniqueIndexItem OldValues, UniqueIndexItem NewValues)> UpdateUniqueIndexes(Func<int, NavValue> getOriginalValue, Func<int, NavValue> getNewValue)
	{
		List<(HashSet<UniqueIndexItem>, UniqueIndexItem, UniqueIndexItem)> list = new List<(HashSet<UniqueIndexItem>, UniqueIndexItem, UniqueIndexItem)>();
		foreach (KeyValuePair<NCLMetaKey, HashSet<UniqueIndexItem>> uniqueIndex in uniqueIndexes)
		{
			UniqueIndexItem uniqueIndexItem = new UniqueIndexItem(uniqueIndex.Key.KeyFieldsList.Select((NCLMetaField kf) => getOriginalValue(kf.FieldIndex)).ToArray());
			UniqueIndexItem uniqueIndexItem2 = new UniqueIndexItem(uniqueIndex.Key.KeyFieldsList.Select((NCLMetaField kf) => getNewValue(kf.FieldIndex) ?? getOriginalValue(kf.FieldIndex)).ToArray());
			if (!uniqueIndex.Value.Comparer.Equals(uniqueIndexItem, uniqueIndexItem2))
			{
				if (uniqueIndex.Value.Contains(uniqueIndexItem2))
				{
					throw RecordImplementationHelper.GetUniqueConstraintException(table, uniqueIndex.Key, uniqueIndexItem2.Values);
				}
				list.Add((uniqueIndex.Value, uniqueIndexItem, uniqueIndexItem2));
			}
		}
		return list;
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		if (TreeIsNullOrEmpty())
		{
			return;
		}
		List<AvlTree<TempTableRecordBuffer>> list = new List<AvlTree<TempTableRecordBuffer>>();
		if (HasSecondaryTrees)
		{
			foreach (KeyValuePair<SortingFieldList, AvlTree<TempTableRecordBuffer>> keyValue in trees)
			{
				if (primaryKeySortingFields != keyValue.Key && newFieldValues.Items.Any((Tuple<INavFieldMetadata, NavValue> f) => keyValue.Key.Contains(f.Item1)))
				{
					list.Add(keyValue.Value);
				}
			}
		}
		Dictionary<int, NavValue> coercedValues = new Dictionary<int, NavValue>();
		Tuple<INavFieldMetadata, NavValue>[] items = newFieldValues.Items;
		foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
		{
			coercedValues.Add(tuple.Item1.ColumnIndex, CalculateFieldValue(tuple.Item1, tuple.Item2));
		}
		TempTableRecordBuffer[] array = Filter(companyToken, filtersAndMarks, null, primaryKeySortingFields, includeCurrent: false).ToArray();
		foreach (TempTableRecordBuffer row in array)
		{
			List<(HashSet<UniqueIndexItem>, UniqueIndexItem, UniqueIndexItem)> list2 = UpdateUniqueIndexes((int idx) => row.GetOriginalValue(idx), (int idx) => (!coercedValues.ContainsKey(idx)) ? row.GetOriginalValue(idx) : coercedValues[idx]);
			foreach (var item in list2)
			{
				item.Item1.Remove(item.Item2);
				item.Item1.Add(item.Item3);
			}
			foreach (AvlTree<TempTableRecordBuffer> item2 in list)
			{
				item2.Remove(row, out var _);
			}
			foreach (KeyValuePair<int, NavValue> item3 in coercedValues)
			{
				row[item3.Key] = item3.Value;
			}
			foreach (AvlTree<TempTableRecordBuffer> item4 in list)
			{
				item4.Add(row);
			}
		}
	}

	private static NavValue CalculateFieldValue(INavFieldMetadata metaField, NavValue newValue)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (newValue == null)
		{
			throw new ArgumentNullException("newValue");
		}
		if (metaField.ColumnIndex == 0)
		{
			long value = newValue.ToInt64();
			return NavBigInteger.Create(value);
		}
		if (!newValue.CompareTypeAndMetadata(metaField))
		{
			newValue = NavValue.CreateNavValueFromObject(metaField, newValue);
		}
		if (metaField.SqlDataType == SqlDataType.Variant)
		{
			switch (metaField.NclType)
			{
			case NavNclType.NavCode:
				newValue = ((NavCode)newValue).UpdateWithSqlDataType(metaField.SqlDataType);
				break;
			case NavNclType.NavOemCode:
				newValue = ((NavOemCode)newValue).UpdateWithSqlDataType(metaField.SqlDataType);
				break;
			}
		}
		return newValue;
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		if (TreeIsNullOrEmpty())
		{
			return DeleteResult.RecordNotFound;
		}
		TempTableRecordBuffer value = new TempTableRecordBuffer(recordBuffer.MetaTable, recordBuffer.ToArray());
		if (!primaryTree.Remove(value, out var removedBuffer))
		{
			return DeleteResult.RecordNotFound;
		}
		RemoveRecordFromUniqueIndexes(removedBuffer);
		ForAllTreesDo((AvlTree<TempTableRecordBuffer> tree) =>
		{
			tree.Remove(removedBuffer, out var _);
		}, includePrimary: false);
		TempTableStatistics.Instance.ReportIncrementChange(recordBuffer.MetaApplicationObject.ObjectId, ((NCLMetaTable)recordBuffer.MetaApplicationObject).ApproximateRowByteSize, -1);
		return DeleteResult.Success;
	}

	private void RemoveRecordFromUniqueIndexes(TempTableRecordBuffer recordBuffer)
	{
		foreach (KeyValuePair<NCLMetaKey, HashSet<UniqueIndexItem>> uniqueIndex in uniqueIndexes)
		{
			UniqueIndexItem item = new UniqueIndexItem(uniqueIndex.Key.KeyFieldsList.Select((NCLMetaField kf) => recordBuffer[kf.FieldIndex]).ToArray());
			uniqueIndex.Value.Remove(item);
		}
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		if (TreeIsNullOrEmpty())
		{
			return;
		}
		int approximateRowByteSize = metaTable.ApproximateRowByteSize;
		if (filtersAndMarks.GetCombinedFilter() == null && filtersAndMarks.Filters.ConcatenatedFilterFlowFields == null)
		{
			TempTableStatistics.Instance.ReportIncrementChange(metaTable.ObjectId, approximateRowByteSize, -primaryTree.Count);
			ForAllTreesDo((AvlTree<TempTableRecordBuffer> tree) =>
			{
				tree.Clear();
			}, includePrimary: true);
			uniqueIndexes.ForEach((KeyValuePair<NCLMetaKey, HashSet<UniqueIndexItem>> idx) =>
			{
				idx.Value.Clear();
			});
			return;
		}
		TempTableRecordBuffer[] array = Filter(companyToken, filtersAndMarks, null, primaryKeySortingFields, includeCurrent: false).ToArray();
		TempTableRecordBuffer[] array2 = array;
		foreach (TempTableRecordBuffer rowToDelete in array2)
		{
			ForAllTreesDo((AvlTree<TempTableRecordBuffer> tree) =>
			{
				tree.Remove(rowToDelete, out var _);
			}, includePrimary: true);
			RemoveRecordFromUniqueIndexes(rowToDelete);
		}
		TempTableStatistics.Instance.ReportIncrementChange(metaTable.ObjectId, approximateRowByteSize, -array.Length);
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		outputRecordBuffer = null;
		EnsureTreeCreated();
		NavValue[] items = recordBuffer.ToArray();
		TempTableRecordBuffer value = new TempTableRecordBuffer(recordBuffer.MetaTable, items);
		List<(HashSet<UniqueIndexItem>, UniqueIndexItem)> list = new List<(HashSet<UniqueIndexItem>, UniqueIndexItem)>();
		foreach (KeyValuePair<NCLMetaKey, HashSet<UniqueIndexItem>> uniqueIndex in uniqueIndexes)
		{
			UniqueIndexItem uniqueIndexItem = new UniqueIndexItem(uniqueIndex.Key.KeyFieldsList.Select((NCLMetaField kf) => recordBuffer[kf.FieldIndex]).ToArray());
			if (uniqueIndex.Value.Contains(uniqueIndexItem))
			{
				throw RecordImplementationHelper.GetUniqueConstraintException(table, uniqueIndex.Key, uniqueIndexItem.Values);
			}
			list.Add((uniqueIndex.Value, uniqueIndexItem));
		}
		if (!primaryTree.Add(value))
		{
			return InsertResult.RecordAlreadyExists;
		}
		foreach (var (hashSet, item) in list)
		{
			hashSet.Add(item);
		}
		value.CloneBlobs(recordBuffer);
		if (HasSecondaryTrees)
		{
			ForAllTreesDo((AvlTree<TempTableRecordBuffer> tree) =>
			{
				tree.Add(value);
			}, includePrimary: false);
		}
		outputRecordBuffer = value.ToReadOnlyBuffer();
		TempTableStatistics.Instance.ReportIncrementChange(recordBuffer.MetaApplicationObject.ObjectId, ((NCLMetaTable)recordBuffer.MetaApplicationObject).ApproximateRowByteSize, 1);
		return InsertResult.Success;
	}

	public override void BeginTransaction(TransactionType transactionType)
	{
	}

	public override void EndTransaction(bool commit)
	{
	}

	internal void DecrementUsageCountOrReportRows()
	{
		if (sharedTableUsageCount > 0)
		{
			sharedTableUsageCount--;
		}
		else
		{
			DecrementTotalRowsWithCount();
		}
	}

	internal void DecrementTotalRowsWithCount()
	{
		if (primaryTree != null && sharedTableUsageCount == 0)
		{
			int count = primaryTree.Count;
			if (count > 0)
			{
				TempTableStatistics.Instance.ReportIncrementChange(table.TableId, table.ApproximateRowByteSize, -count);
			}
		}
	}

	private static bool IsRowWithinFilter(TempTableRecordBuffer row, FilterExpression combinedExpressionFilter, RecordBufferEvaluatorVisitor visitor)
	{
		return visitor.Visit(combinedExpressionFilter, row);
	}

	private static FilterExpression GetCombinedExpressionFilter(FiltersAndMarks filtersAndMarks, FilterExpression positionFilter)
	{
		return FilterExpression.And(positionFilter, filtersAndMarks.GetCombinedFilter(), (filtersAndMarks.Filters == null) ? null : filtersAndMarks.Filters.ConcatenatedFilterFlowFields);
	}

	private static FindProviderRequest MakeExistsFindRequest(ExistsProviderRequest filteredDataRequest)
	{
		SortingFieldList sortingFields = ((filteredDataRequest.SortingFields != null && ServerUserSettings.Instance.UseCurrentKeyForIsEmptyOnTempTables.Value) ? filteredDataRequest.SortingFields : ((NCLMetaTable)filteredDataRequest.MetaApplicationObject).PrimaryKey.SortingFieldsWithPrimaryKeyFields);
		return new FindProviderRequest(filteredDataRequest.CompanyToken, filteredDataRequest.MetaApplicationObject, filteredDataRequest.LockState, filteredDataRequest.FiltersAndMarks, null, filteredDataRequest.FlowFieldSecurityFiltering, FieldList.Empty, sortingFields, FindType.FirstOnly);
	}

	private AvlTree<TempTableRecordBuffer> EnsureTreeCreated(SortingFieldList sortingFieldsWithPrimaryKeyFields = null)
	{
		if (trees == null)
		{
			trees = new Dictionary<SortingFieldList, AvlTree<TempTableRecordBuffer>>();
			primaryTree = new AvlTree<TempTableRecordBuffer>(null, null, isLowerBoundDefined: false, isUpperBoundDefined: false, new RecordBufferComparer(primaryKeySortingFields));
			trees.Add(primaryKeySortingFields, primaryTree);
			uniqueIndexes = new Dictionary<NCLMetaKey, HashSet<UniqueIndexItem>>();
			foreach (NCLMetaKey item in table.Keys.Where((NCLMetaKey k) => k.Unique))
			{
				uniqueIndexes.Add(item, new HashSet<UniqueIndexItem>());
			}
		}
		if (sortingFieldsWithPrimaryKeyFields == null)
		{
			return primaryTree;
		}
		if (trees.TryGetValue(sortingFieldsWithPrimaryKeyFields, out var value))
		{
			return value;
		}
		value = new AvlTree<TempTableRecordBuffer>(null, null, isLowerBoundDefined: false, isUpperBoundDefined: false, new RecordBufferComparer(sortingFieldsWithPrimaryKeyFields));
		trees.Add(sortingFieldsWithPrimaryKeyFields, value);
		foreach (TempTableRecordBuffer item2 in primaryTree)
		{
			value.Add(item2);
		}
		return value;
	}

	private bool TreeIsNullOrEmpty()
	{
		if (primaryTree != null)
		{
			return primaryTree.Count == 0;
		}
		return true;
	}

	private void ForAllTreesDo(Action<AvlTree<TempTableRecordBuffer>> action, bool includePrimary)
	{
		if (trees.Count == 1)
		{
			if (includePrimary)
			{
				action(primaryTree);
			}
			return;
		}
		foreach (AvlTree<TempTableRecordBuffer> value in trees.Values)
		{
			if (includePrimary || value != primaryTree)
			{
				action(value);
			}
		}
	}

	[Conditional("DEBUG")]
	private void CheckForTreeErrors()
	{
		foreach (AvlTree<TempTableRecordBuffer> value in trees.Values)
		{
			_ = value;
			_ = primaryTree;
		}
	}

	private ModifyResult ModifyAllTrees(MutableRecordBuffer mutableRecordBuffer, TempTableRecordBuffer workTableBuffer, TempTableRecordBuffer storedTableBuffer, bool primaryKeyChanged)
	{
		List<AvlTree<TempTableRecordBuffer>> list = new List<AvlTree<TempTableRecordBuffer>>();
		if (HasSecondaryTrees)
		{
			ReadOnlyRecordBuffer bufferInDatabase = storedTableBuffer.ToReadOnlyBuffer();
			MutableRecordBuffer mutableRecordBuffer2 = MutableRecordBuffer.ComputeChangeset(mutableRecordBuffer, bufferInDatabase);
			foreach (KeyValuePair<SortingFieldList, AvlTree<TempTableRecordBuffer>> tree in trees)
			{
				bool flag = primaryKeyChanged;
				if (!primaryKeyChanged)
				{
					for (int i = 0; i < tree.Key.Count; i++)
					{
						if (mutableRecordBuffer2.GetChangedFieldValue(tree.Key[i].Field.ColumnIndex) != null)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					list.Add(tree.Value);
					if (!primaryKeyChanged || tree.Key != primaryKeySortingFields)
					{
						tree.Value.Remove(storedTableBuffer, out var _);
					}
				}
			}
		}
		else if (primaryKeyChanged)
		{
			list.Add(primaryTree);
		}
		for (int j = 0; j < workTableBuffer.FieldCount; j++)
		{
			NCLMetaField fieldByIndex = mutableRecordBuffer.MetaTable.GetFieldByIndex(j);
			if (fieldByIndex.FieldNclType == NavNclType.NavBlob)
			{
				NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer.GetChangedFieldValue(j);
				if (navBLOB != null && navBLOB.IsDirty)
				{
					workTableBuffer[j] = (navBLOB.IsZeroOrEmpty ? NavBLOB.Default() : new NavBLOB(navBLOB.GetBytes()));
				}
			}
			else
			{
				workTableBuffer[j] = mutableRecordBuffer[j];
			}
		}
		ModifyResult modifyResult = ModifyResult.Success;
		for (int k = 0; k < list.Count; k++)
		{
			AvlTree<TempTableRecordBuffer> avlTree = list[k];
			if (!avlTree.Add(workTableBuffer))
			{
				modifyResult = ModifyResult.RecordAlreadyExists;
				for (int num = k - 1; num >= 0; num--)
				{
					list[num].Remove(workTableBuffer, out var _);
				}
				break;
			}
		}
		if (modifyResult == ModifyResult.RecordAlreadyExists)
		{
			list.ForEach((AvlTree<TempTableRecordBuffer> t) =>
			{
				t.Add(storedTableBuffer);
			});
		}
		return modifyResult;
	}

	private IEnumerable<ReadOnlyRecordBuffer> FindImplementation(FindProviderRequest request)
	{
		PositionedFindProviderRequest request2 = new PositionedFindProviderRequest(request, null, includeCurrent: false);
		return FindByPositionImplementation(request2);
	}

	private int TryGetRanges(FilterFieldDictionary filters, SortingFieldList sortingFields, out TempTableRecordBuffer lowerRecord, out TempTableRecordBuffer upperRecord)
	{
		lowerRecord = null;
		upperRecord = null;
		NavValue[] array = new NavValue[table.FieldCount];
		NavValue[] array2 = new NavValue[table.FieldCount];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < sortingFields.Count; i++)
		{
			INavSortingField navSortingField = sortingFields[i];
			FilterExpression value = null;
			if (!filters.TryGetValue(navSortingField.Field, out value) || value == null)
			{
				break;
			}
			RangeList rangeList;
			try
			{
				rangeList = value.ToRangeList();
			}
			catch (CannotConvertWildcardIntoRangeException)
			{
				return 0;
			}
			if (rangeList.IsEmpty || rangeList.Ranges.Count > 1 || rangeList.IsFull)
			{
				break;
			}
			Range range = rangeList.Ranges[0];
			NavValue navValue = ((!range.IsHighMaximum && range.IsHighDefined) ? range.HighValue : null);
			NavValue navValue2 = ((!range.IsLowIsMinimum && range.IsLowDefined) ? range.LowValue : null);
			if (num == i)
			{
				NavValue navValue3 = ((navSortingField.Order == SortOrder.Ascending) ? navValue2 : navValue);
				if (navValue3 != null)
				{
					array[navSortingField.Field.ColumnIndex] = navValue3;
					num++;
				}
			}
			if (num2 == i)
			{
				NavValue navValue4 = ((navSortingField.Order == SortOrder.Ascending) ? navValue : navValue2);
				if (navValue4 != null)
				{
					array2[navSortingField.Field.ColumnIndex] = navValue4;
					num2++;
				}
			}
		}
		if (num2 > 0 && num2 == num)
		{
			lowerRecord = new TempTableRecordBuffer(table, array);
			upperRecord = new TempTableRecordBuffer(table, array2);
			return num2;
		}
		if (num > num2)
		{
			lowerRecord = new TempTableRecordBuffer(table, array);
			return num;
		}
		if (num2 > num)
		{
			upperRecord = new TempTableRecordBuffer(table, array2);
			return num2;
		}
		return 0;
	}

	private IEnumerable<TempTableRecordBuffer> Filter(int companyToken, FiltersAndMarks filtersAndMarks, MutableRecordBuffer startingPosition, SortingFieldList sortingFields, bool includeCurrent)
	{
		TempTableRecordBuffer lowerRecord = null;
		TempTableRecordBuffer upperRecord = null;
		if (sortingFields == null)
		{
			sortingFields = primaryKeySortingFields;
		}
		AvlTree<TempTableRecordBuffer> avlTree = EnsureTreeCreated(sortingFields);
		FilterExpression combinedExpressionFilter = GetCombinedExpressionFilter(filtersAndMarks, null);
		if (combinedExpressionFilter == null && startingPosition == null)
		{
			return avlTree;
		}
		int num = 0;
		if (startingPosition != null)
		{
			num = sortingFields.Count;
			lowerRecord = new TempTableRecordBuffer(startingPosition.MetaTable, startingPosition.ToArray());
			FilterExpression right = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(startingPosition, sortingFields, includeCurrent);
			combinedExpressionFilter = FilterExpression.And(combinedExpressionFilter, right);
		}
		else if (num == 0 && filtersAndMarks.Filters != null)
		{
			num = TryGetRanges(filtersAndMarks.Filters, sortingFields, out lowerRecord, out upperRecord);
		}
		RecordBufferEvaluatorVisitor filteringVisitor = new RecordBufferEvaluatorVisitor(companyToken, this, filtersAndMarks);
		if (num == 0)
		{
			return avlTree.Where((TempTableRecordBuffer row) => filteringVisitor.Visit(combinedExpressionFilter, row));
		}
		return FilterUsingRanges(avlTree, sortingFields, startingPosition, includeCurrent, upperRecord, lowerRecord, num, combinedExpressionFilter, filteringVisitor);
	}

	private IEnumerable<TempTableRecordBuffer> FilterUsingRanges(AvlTree<TempTableRecordBuffer> tree, SortingFieldList sortingFields, MutableRecordBuffer startingPosition, bool includeCurrent, TempTableRecordBuffer high, TempTableRecordBuffer low, int rangeCount, FilterExpression combinedExpressionFilter, RecordBufferEvaluatorVisitor filteringVisitor)
	{
		IComparer<TempTableRecordBuffer> comparer2;
		if (rangeCount != sortingFields.Count)
		{
			IComparer<TempTableRecordBuffer> comparer = new RecordBufferComparer(SortingFieldList.Create(sortingFields.Take(rangeCount)));
			comparer2 = comparer;
		}
		else
		{
			comparer2 = tree.Comparer;
		}
		IComparer<TempTableRecordBuffer> comparerParameter = comparer2;
		AvlTree<TempTableRecordBuffer> avlTree;
		if (low != null && high != null)
		{
			avlTree = tree.GetViewBetween(low, high, comparerParameter);
		}
		else
		{
			avlTree = ((low == null) ? tree.GetViewTo(high, comparerParameter) : tree.GetViewFrom(low, comparerParameter));
		}
		if (startingPosition == null)
		{
			return avlTree.Where((TempTableRecordBuffer row) => filteringVisitor.Visit(combinedExpressionFilter, row));
		}
		IEnumerable<TempTableRecordBuffer> source = avlTree;
		if (!includeCurrent)
		{
			RecordBufferComparer firstRowComparer = new RecordBufferComparer(sortingFields);
			TempTableRecordBuffer startingPositionBuffer = new TempTableRecordBuffer(table, startingPosition.ToArray());
			source = source.SkipWhile((TempTableRecordBuffer row, int index) => index == 0 && firstRowComparer.Compare(row, startingPositionBuffer) == 0);
		}
		return source.Where((TempTableRecordBuffer row) => filteringVisitor.Visit(combinedExpressionFilter, row));
	}

	private IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request)
	{
		if (TreeIsNullOrEmpty())
		{
			return new ReadOnlyRecordBuffer[0];
		}
		IEnumerable<TempTableRecordBuffer> source = Filter(request.CompanyToken, request.FiltersAndMarks, request.StartingPosition, request.SortingFields, request.IncludeCurrent);
		return source.Select((TempTableRecordBuffer row) => row.ToReadOnlyBuffer());
	}

	private bool TryGetValue(NavRecordId id, out TempTableRecordBuffer buffer)
	{
		buffer = null;
		if (TreeIsNullOrEmpty())
		{
			return false;
		}
		TempTableRecordBuffer key = RecordIdToBuffer(id);
		buffer = primaryTree.GetValue(key);
		return buffer != null;
	}

	private TempTableRecordBuffer RecordIdToBuffer(NavRecordId recordId)
	{
		NavValue[] array = new NavValue[table.FieldCount];
		for (int i = 0; i < table.PrimaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = table.PrimaryKey.GetKeyFieldByIndex(i);
			array[keyFieldByIndex.FieldIndex] = recordId.FieldValueByIndex(keyFieldByIndex, i);
		}
		return new TempTableRecordBuffer(table, array);
	}
}
