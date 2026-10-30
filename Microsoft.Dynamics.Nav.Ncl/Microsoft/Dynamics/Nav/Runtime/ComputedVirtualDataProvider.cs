using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class ComputedVirtualDataProvider : VirtualDataProvider
{
	protected abstract IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinFilterForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters);

	protected virtual int CountValuesWithinFilter(ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter)
	{
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(primaryKey.KeyFieldCount - 1);
		return GetValuesWithinFilterForKeyField(keyFieldByIndex, precedingKeyFieldValues, filter, SortOrder.Ascending, null).Count();
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		NavRecordId recordId = request.RecordId;
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		Tuple<INavFieldMetadata, FilterExpression>[] array = new Tuple<INavFieldMetadata, FilterExpression>[primaryKey.KeyFieldCount];
		for (int i = 0; i < array.Length; i++)
		{
			NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
			array[i] = new Tuple<INavFieldMetadata, FilterExpression>(keyFieldByIndex, FilterExpression.Equal(recordId.FieldValueByIndex(keyFieldByIndex, i), keyFieldByIndex.ExpressionContext));
		}
		FilterFieldDictionary filterFieldDictionary = new FilterFieldDictionary(array);
		FilterFieldDictionary filters = DataProviderHelper.MergeFilterFieldDictionaries(filterFieldDictionary, request.FiltersAndMarks.Filters);
		recordBuffer = InnerFind(primaryKey.DefinedSortingFields, filters, null, FindType.Normal).FirstOrDefault();
		if (recordBuffer == null)
		{
			return false;
		}
		if (request.FiltersAndMarks.MarkedRecords == null)
		{
			return true;
		}
		if (request.FiltersAndMarks.MarkedRecords.IsMarked(recordBuffer))
		{
			return true;
		}
		recordBuffer = null;
		return false;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		return InnerFind(request.SortingFields, request.FiltersAndMarks.Filters, request.FiltersAndMarks.MarkedRecords, request.FindType);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		if (!request.SortingFields.FieldList.Equals(MetaTable.PrimaryKey.DefinedKeyFields))
		{
			return FindFromPositionOnAlternateSortOrder(request);
		}
		FilterFieldDictionary[] source = DataProviderHelper.BuildPositioningFilterDictionariesFromRecordBuffer(request.StartingPosition, request.SortingFields, request.IncludeCurrent);
		return from positionDictionary in source
			let mergedDictionary = DataProviderHelper.MergeFilterFieldDictionaries(positionDictionary, request.FiltersAndMarks.Filters)
			from row in InnerFind(request.SortingFields, mergedDictionary, request.FiltersAndMarks.MarkedRecords, request.FindType)
			select row;
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		return InnerFind(MetaTable.PrimaryKey.DefinedSortingFields, request.FiltersAndMarks.Filters, request.FiltersAndMarks.MarkedRecords, FindType.Normal).Any();
	}

	public override int Count(DataProviderRequest request)
	{
		FiltersAndMarks filtersAndMarks = request.FiltersAndMarks;
		FilterFieldDictionary filters = filtersAndMarks.Filters;
		if (AnyFiltersOnNonPrimaryKeyFields(filters) || filtersAndMarks.MarkedRecords != null)
		{
			return InnerFind(MetaTable.PrimaryKey.DefinedSortingFields, filters, filtersAndMarks.MarkedRecords, FindType.Normal).Count();
		}
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		IEnumerable<ReadOnlyRecordBuffer> source = new ReadOnlyRecordBuffer[1];
		int num = 0;
		FilterExpression filter;
		while (true)
		{
			NCLMetaField keyField = primaryKey.GetKeyFieldByIndex(num);
			filter = filters.GetFilterOnField(keyField);
			if (num >= primaryKey.KeyFieldCount - 1)
			{
				break;
			}
			source = from previousKeyValue in source
				from newIntermediateValue in GetValuesWithinFilterForKeyField(keyField, previousKeyValue, filter, SortOrder.Ascending, FilterFieldDictionary.Empty)
				select newIntermediateValue;
			num++;
		}
		return source.Sum((ReadOnlyRecordBuffer previousKeyValue) => CountValuesWithinFilter(previousKeyValue, filter));
	}

	private IEnumerable<ReadOnlyRecordBuffer> InnerFind(SortingFieldList sortingFields, FilterFieldDictionary filters, MarkedRecords markedRecords, FindType findType)
	{
		FilterFieldDictionary filterFieldDictionary = filters ?? FilterFieldDictionary.Empty;
		if (markedRecords != null)
		{
			return FindFromMarks(sortingFields, filterFieldDictionary, markedRecords, findType);
		}
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		FilterFieldDictionary nonPrimaryKeyFilters = GetFiltersOnNonPrimaryKeyFields(filterFieldDictionary);
		IEnumerable<ReadOnlyRecordBuffer> enumerable = new ReadOnlyRecordBuffer[1];
		IEnumerable<INavSortingField> enumerable2 = (sortingFields.FieldList.Equals(primaryKey.DefinedKeyFields) ? sortingFields : primaryKey.DefinedSortingFields);
		foreach (INavSortingField item in enumerable2)
		{
			NCLMetaField keyField = (NCLMetaField)item.Field;
			FilterExpression filter = filterFieldDictionary.GetFilterOnField(keyField);
			SortOrder order = item.Order;
			enumerable = from previousKeyValue in enumerable
				from newIntermediateValue in GetValuesWithinFilterForKeyField(keyField, previousKeyValue, filter, order, nonPrimaryKeyFilters)
				select newIntermediateValue;
		}
		return ApplyOrdering(enumerable, sortingFields, findType);
	}

	private IEnumerable<ReadOnlyRecordBuffer> FindFromPositionOnAlternateSortOrder(PositionedFindProviderRequest request)
	{
		FilterExpression positionFilter = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(request.StartingPosition, request.SortingFields, request.IncludeCurrent);
		IEnumerable<ReadOnlyRecordBuffer> records = from rec in InnerFind(MetaTable.PrimaryKey.DefinedSortingFields, request.FiltersAndMarks.Filters, request.FiltersAndMarks.MarkedRecords, request.FindType)
			where rec.PassesFilter(positionFilter)
			select rec;
		return ApplyOrdering(records, request.SortingFields, request.FindType);
	}

	private IEnumerable<ReadOnlyRecordBuffer> FindFromMarks(SortingFieldList sortingFields, FilterFieldDictionary filters, MarkedRecords markedRecords, FindType findType)
	{
		IEnumerable<ReadOnlyRecordBuffer> records;
		switch (markedRecords.Kind)
		{
		case MarkedRecords.MarkedRecordsKind.SimpleListOfMarks:
		{
			IEnumerable<NavRecordId> source = ((sortingFields.IsDescendingOnAllFields && sortingFields.FieldList.Equals(MetaTable.PrimaryKey.DefinedKeyFields)) ? markedRecords.MarkedRecordIds.Reverse() : markedRecords.MarkedRecordIds);
			FiltersAndMarks filtersAndMarksWithoutMarks = new FiltersAndMarks(filters);
			records = from recordBuffer in source.Select((NavRecordId markedRecordId) =>
				{
					RecordIdProviderRequest request = new RecordIdProviderRequest(0, MetaTable, DataLockState.None, filtersAndMarksWithoutMarks, SecurityFiltering.Ignored, markedRecordId);
					ReadOnlyRecordBuffer recordBuffer;
					return (!TryGetByPrimaryKey(request, out recordBuffer)) ? null : recordBuffer;
				})
				where recordBuffer != null
				select recordBuffer;
			break;
		}
		case MarkedRecords.MarkedRecordsKind.SimpleListOfExclusions:
			filters = DataProviderHelper.MergeFilterFieldDictionaries(filters, markedRecords.AllMarksFilter);
			goto case MarkedRecords.MarkedRecordsKind.Complex;
		case MarkedRecords.MarkedRecordsKind.Complex:
			records = from buffer in InnerFind(sortingFields, filters, null, findType)
				where markedRecords.IsMarked(buffer)
				select buffer;
			break;
		default:
			throw new NotSupportedException();
		}
		return ApplyOrdering(records, sortingFields, findType);
	}

	private static FilterFieldDictionary GetFiltersOnNonPrimaryKeyFields(FilterFieldDictionary inputDictionary)
	{
		Tuple<INavFieldMetadata, FilterExpression>[] items = inputDictionary.Items;
		if (items.Length == 0)
		{
			return inputDictionary;
		}
		List<Tuple<INavFieldMetadata, FilterExpression>> list = null;
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
		{
			if ((tuple.Item1 == FilterFieldDictionary.MultipleFields || !((NCLMetaField)tuple.Item1).FieldIsPartOfPrimaryKey) && !tuple.Item2.IsNullOrConstantTrue())
			{
				(list ?? (list = new List<Tuple<INavFieldMetadata, FilterExpression>>())).Add(tuple);
			}
		}
		if (list != null)
		{
			return new FilterFieldDictionary(list);
		}
		return FilterFieldDictionary.Empty;
	}

	private static bool AnyFiltersOnNonPrimaryKeyFields(FilterFieldDictionary inputDictionary)
	{
		Tuple<INavFieldMetadata, FilterExpression>[] items = inputDictionary.Items;
		if (items.Length == 0)
		{
			return false;
		}
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
		{
			if (tuple.Item1 == FilterFieldDictionary.MultipleFields || !((NCLMetaField)tuple.Item1).FieldIsPartOfPrimaryKey || tuple.Item2.IsNullOrConstantTrue())
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerable<ReadOnlyRecordBuffer> ApplyOrdering(IEnumerable<ReadOnlyRecordBuffer> records, SortingFieldList sortingFields, FindType findType)
	{
		if (sortingFields.FieldList.Equals(MetaTable.PrimaryKey.DefinedKeyFields))
		{
			return records;
		}
		return records.OrderBySortingFields(sortingFields, findType);
	}

	protected static IEnumerable<ReadOnlyRecordBuffer> EnumerateOptionsInRange(NCLOptionMetadata fieldOptionMetadata, Range range, SortOrder sortOrder, Func<NavOption, ReadOnlyRecordBuffer> createRecordBuffer)
	{
		if (range.GetInclusiveIntegerBounds(0, fieldOptionMetadata.Count - 1, out var low, out var high))
		{
			for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				yield return createRecordBuffer(NavOption.Create(fieldOptionMetadata, i));
			}
		}
	}
}
