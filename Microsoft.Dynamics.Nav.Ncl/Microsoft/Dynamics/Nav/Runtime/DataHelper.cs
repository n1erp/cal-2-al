using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class DataHelper
{
	internal static bool PassesFieldFilters(this IRecordBuffer recordBuffer, FilterFieldDictionary filters, bool includeFlowFields = true, bool checkAgainstOriginalAndModified = false, bool bothShouldPass = false)
	{
		if (filters == null)
		{
			return true;
		}
		RecordBufferFilterExpressionEvaluatorInput evaluatorInput = new RecordBufferFilterExpressionEvaluatorInput(recordBuffer, checkAgainstOriginalAndModified, bothShouldPass);
		IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> source = filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> filterItem) => filterItem.Item1.FieldClass != FieldClass.FlowFilter);
		if (!includeFlowFields)
		{
			source = source.Where((Tuple<INavFieldMetadata, FilterExpression> filterItem) => filterItem.Item1.FieldClass != FieldClass.FlowField);
		}
		return source.All((Tuple<INavFieldMetadata, FilterExpression> filterItem) => (filterItem.Item1 == FilterFieldDictionary.MultipleFields) ? RecordBufferFilterExpressionEvaluator.Instance.Visit(filterItem.Item2, evaluatorInput) : RecordBufferFilterExpressionEvaluator.EvaluateFieldFilter(filterItem.Item1, filterItem.Item2, evaluatorInput));
	}

	internal static bool PassesFieldFiltersAndMarks(this IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, bool includeFlowFields = true, bool checkAgainstOriginalAndModified = false)
	{
		if (filtersAndMarks == null)
		{
			return true;
		}
		if (recordBuffer.PassesFieldFilters(filtersAndMarks.Filters, includeFlowFields, checkAgainstOriginalAndModified))
		{
			if (filtersAndMarks.MarkedRecords != null)
			{
				return filtersAndMarks.MarkedRecords.IsMarked(recordBuffer, includeFlowFields);
			}
			return true;
		}
		return false;
	}

	internal static bool PassesFilter(this IRecordBuffer recordBuffer, FilterExpression filter)
	{
		RecordBufferFilterExpressionEvaluatorInput input = new RecordBufferFilterExpressionEvaluatorInput(recordBuffer, checkAgainstOriginalAndModified: false, bothShouldPass: false);
		if (filter != null)
		{
			return RecordBufferFilterExpressionEvaluator.Instance.Visit(filter, input);
		}
		return true;
	}

	internal static NavValue[] GetPrimaryKeyValues(this IRecordBuffer recordBuffer)
	{
		NCLMetaTable nCLMetaTable = (NCLMetaTable)recordBuffer.MetaApplicationObject;
		int keyFieldCount = nCLMetaTable.PrimaryKey.KeyFieldCount;
		NavValue[] array = new NavValue[keyFieldCount];
		for (int i = 0; i < keyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = nCLMetaTable.PrimaryKey.GetKeyFieldByIndex(i);
			NavValue navValue = recordBuffer[keyFieldByIndex.FieldIndex];
			if (navValue != null)
			{
				array[i] = navValue;
			}
			else
			{
				array[i] = keyFieldByIndex.InitValue;
			}
		}
		return array;
	}

	internal static NavValue[] ToArray(this IRecordBuffer recordBuffer, bool requireNotNull)
	{
		NavValue[] array = recordBuffer.ToArray();
		if (requireNotNull)
		{
			NCLMetaTable nCLMetaTable = (NCLMetaTable)recordBuffer.MetaApplicationObject;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					array[i] = nCLMetaTable.GetFieldByIndex(i).InitValue;
				}
			}
		}
		return array;
	}

	internal static IEnumerable<ReadOnlyRecordBuffer> OrderBySortingFields(this IEnumerable<ReadOnlyRecordBuffer> records, SortingFieldList sortingFields, FindType findType)
	{
		if (findType == FindType.FirstOnly)
		{
			IComparer<IRecordBuffer> comparer = new RecordBufferComparer(sortingFields);
			ReadOnlyRecordBuffer readOnlyRecordBuffer = records.Aggregate(null, (ReadOnlyRecordBuffer accumulated, ReadOnlyRecordBuffer curr) => (accumulated != null && comparer.Compare(curr, accumulated) >= 0) ? accumulated : curr);
			if (readOnlyRecordBuffer != null)
			{
				return new ReadOnlyRecordBuffer[1] { readOnlyRecordBuffer };
			}
			return new ReadOnlyRecordBuffer[0];
		}
		return records.OrderBySortingFields(sortingFields);
	}

	internal static IEnumerable<T> OrderBySortingFields<T>(this IEnumerable<T> records, SortingFieldList sortingFields) where T : IRecordBuffer
	{
		RecordBufferComparer comparer = new RecordBufferComparer(sortingFields);
		return records.OrderBy((T r) => r, comparer);
	}

	internal static bool IsNullOrEmpty<T>(this FieldDictionary<T> fieldDictionary)
	{
		if (!(fieldDictionary == null))
		{
			return fieldDictionary.Items.Length == 0;
		}
		return true;
	}
}
