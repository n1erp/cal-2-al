using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class DataProviderHelper
{
	public static FilterExpressionType ReverseFilterExpressionType(FilterExpressionType typeToBeNegated)
	{
		return typeToBeNegated switch
		{
			FilterExpressionType.GreaterThan => FilterExpressionType.LessThan, 
			FilterExpressionType.GreaterThanOrEqual => FilterExpressionType.LessThanOrEqual, 
			FilterExpressionType.LessThan => FilterExpressionType.GreaterThan, 
			FilterExpressionType.LessThanOrEqual => FilterExpressionType.GreaterThanOrEqual, 
			_ => throw new ArgumentException("Can not negate this filterExpression type"), 
		};
	}

	internal static FilterExpressionType GetFilterExpressionType(SortOrder sortOrder, bool includeCurrent)
	{
		if (sortOrder == SortOrder.Ascending)
		{
			return includeCurrent ? FilterExpressionType.GreaterThanOrEqual : FilterExpressionType.GreaterThan;
		}
		return includeCurrent ? FilterExpressionType.LessThanOrEqual : FilterExpressionType.LessThan;
	}

	private static List<List<UnaryFilterExpression>> BuildPositioningFilterListFromRecordBuffer(IRecordBuffer record, SortingFieldList sortingFields, bool includeCurrent)
	{
		int count = sortingFields.Count;
		List<List<UnaryFilterExpression>> list = new List<List<UnaryFilterExpression>>(count);
		for (int i = 0; i < count; i++)
		{
			int num = count - i;
			List<UnaryFilterExpression> list2 = new List<UnaryFilterExpression>(num);
			for (int j = 1; j <= num; j++)
			{
				INavSortingField navSortingField = sortingFields[j - 1];
				FilterExpressionType filterExpressionType = ((j == num) ? GetFilterExpressionType(navSortingField.Order, includeCurrent && j == count) : FilterExpressionType.Equal);
				UnaryFilterExpression item = new UnaryFilterExpression(filterExpressionType, record[navSortingField.Field.ColumnIndex] ?? navSortingField.Field.InitValue, navSortingField.Field.ExpressionContext);
				list2.Add(item);
			}
			list.Add(list2);
		}
		return list;
	}

	internal static FilterFieldDictionary[] BuildPositioningFilterDictionariesFromRecordBuffer(MutableRecordBuffer record, SortingFieldList sortingFields, bool includeCurrent)
	{
		List<List<UnaryFilterExpression>> list = BuildPositioningFilterListFromRecordBuffer(record, sortingFields, includeCurrent);
		FilterFieldDictionary[] array = new FilterFieldDictionary[list.Count];
		for (int i = 0; i < array.Length; i++)
		{
			List<UnaryFilterExpression> list2 = list[i];
			Tuple<INavFieldMetadata, FilterExpression>[] array2 = new Tuple<INavFieldMetadata, FilterExpression>[list2.Count];
			for (int j = 0; j < list2.Count; j++)
			{
				array2[j] = new Tuple<INavFieldMetadata, FilterExpression>(sortingFields[j].Field, list2[j]);
			}
			array[i] = new FilterFieldDictionary(array2);
		}
		return array;
	}

	internal static FilterExpression BuildPositioningFilterFromRecordBuffer(IRecordBuffer record, SortingFieldList sortingFields, bool includeCurrent)
	{
		List<List<UnaryFilterExpression>> list = BuildPositioningFilterListFromRecordBuffer(record, sortingFields, includeCurrent);
		FilterExpression filterExpression = null;
		for (int i = 0; i < list.Count; i++)
		{
			List<UnaryFilterExpression> list2 = list[i];
			FilterExpression filterExpression2 = null;
			for (int j = 0; j < list2.Count; j++)
			{
				UnaryFilterExpression right = list2[j];
				filterExpression2 = FilterExpression.And(filterExpression2, right);
			}
			filterExpression = ((filterExpression == null) ? filterExpression2 : FilterExpression.Or(filterExpression, filterExpression2));
		}
		return filterExpression;
	}

	internal static FilterExpression BuildOptimizedCompletePositioningFilter(IRecordBuffer record, SortingFieldList sortingFields, bool includeCurrent, FiltersAndMarks filtersAndMarks)
	{
		if (filtersAndMarks.Filters.IsNullOrEmpty() || !AnyFiltersOnSortingFields(sortingFields, filtersAndMarks, out var sortingFieldFiltersList))
		{
			return FilterExpression.And(BuildPositioningFilterFromRecordBuffer(record, sortingFields, includeCurrent), filtersAndMarks.GetCombinedFilter());
		}
		List<List<UnaryFilterExpression>> list = BuildPositioningFilterListFromRecordBuffer(record, sortingFields, includeCurrent);
		FilterExpression filterExpression = null;
		Dictionary<Tuple<FilterExpressionType, int>, FilterExpression> dictionary = new Dictionary<Tuple<FilterExpressionType, int>, FilterExpression>();
		for (int i = 0; i < list.Count; i++)
		{
			List<UnaryFilterExpression> list2 = list[i];
			FilterExpression filterExpression2 = null;
			for (int j = 0; j < list2.Count; j++)
			{
				UnaryFilterExpression unaryFilterExpression = list2[j];
				FilterExpression value = unaryFilterExpression;
				FilterExpression filterExpression3 = sortingFieldFiltersList[j];
				if (filterExpression3 != null)
				{
					Tuple<FilterExpressionType, int> key = Tuple.Create(unaryFilterExpression.ExpressionType, unaryFilterExpression.ExpressionContext.Metadata.ColumnIndex);
					if (!dictionary.TryGetValue(key, out value))
					{
						value = (dictionary[key] = FilterExpression.And(unaryFilterExpression, filterExpression3).Simplify());
					}
				}
				if (value.IsConstantFalse())
				{
					filterExpression2 = value;
					break;
				}
				filterExpression2 = FilterExpression.And(filterExpression2, unaryFilterExpression);
			}
			if (!filterExpression2.IsConstantFalse())
			{
				filterExpression = ((filterExpression == null) ? filterExpression2 : FilterExpression.Or(filterExpression, filterExpression2));
			}
		}
		if (filterExpression == null || filterExpression.IsConstantFalse())
		{
			return BooleanConstantFilterExpression.Get(value: false);
		}
		return FilterExpression.And(filtersAndMarks.GetCombinedFilter(), filterExpression);
	}

	internal static UnaryFilterExpression CreateSingleUpperBoundedUnaryFilterExpressionForOnlyMaxLimitMetadataSetting(RangeList rangeList, FilterExpressionContext newContext)
	{
		if (rangeList.Ranges.Count != 1)
		{
			return null;
		}
		Range range = rangeList.Ranges[0];
		if (range.IsHighExcluded || range.HighValue == null)
		{
			return null;
		}
		return FilterExpression.LessThanOrEqual(range.HighValue, newContext);
	}

	private static bool AnyFiltersOnSortingFields(SortingFieldList sortingFields, FiltersAndMarks filtersAndMarks, out List<FilterExpression> sortingFieldFiltersList)
	{
		bool result = false;
		sortingFieldFiltersList = new List<FilterExpression>(sortingFields.Count);
		for (int i = 0; i < sortingFields.Count; i++)
		{
			INavFieldMetadata field = sortingFields[i].Field;
			if (filtersAndMarks.Filters.TryGetValue(field, out var value))
			{
				sortingFieldFiltersList.Add(value);
				result = true;
			}
			else
			{
				sortingFieldFiltersList.Add(null);
			}
		}
		return result;
	}

	public static FilterFieldDictionary MergeFilterFieldDictionaries(params FilterFieldDictionary[] filterFieldDictionaries)
	{
		FilterFieldDictionary filterFieldDictionary = null;
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = null;
		foreach (FilterFieldDictionary filterFieldDictionary2 in filterFieldDictionaries)
		{
			if (filterFieldDictionary2.IsNullOrEmpty())
			{
				continue;
			}
			if (filterFieldDictionary == null)
			{
				filterFieldDictionary = filterFieldDictionary2;
				continue;
			}
			if (dictionary == null)
			{
				dictionary = new Dictionary<INavFieldMetadata, FilterExpression>();
				Tuple<INavFieldMetadata, FilterExpression>[] items = filterFieldDictionary.Items;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
				{
					dictionary.Add(tuple.Item1, tuple.Item2);
				}
			}
			Tuple<INavFieldMetadata, FilterExpression>[] items2 = filterFieldDictionary2.Items;
			foreach (Tuple<INavFieldMetadata, FilterExpression> tuple2 in items2)
			{
				INavFieldMetadata item = tuple2.Item1;
				FilterExpression filterExpression = tuple2.Item2;
				if (dictionary.TryGetValue(item, out var value))
				{
					dictionary.Remove(item);
					filterExpression = FilterExpression.And(filterExpression, value);
				}
				dictionary.Add(item, filterExpression);
			}
		}
		if (dictionary != null)
		{
			return new FilterFieldDictionary(dictionary);
		}
		return filterFieldDictionary;
	}

	public static bool TryFindBestKey(NCLMetaTable metaTable, NCLMetaField[] desiredFields, out NCLMetaKey bestKey)
	{
		int num = desiredFields.Count((NCLMetaField t) => t.FieldActive);
		if (num == 0)
		{
			bestKey = null;
			return false;
		}
		NCLMetaKey[] keys = metaTable.Keys;
		foreach (NCLMetaKey nCLMetaKey in keys)
		{
			if (!nCLMetaKey.KeyIsActive || nCLMetaKey.KeyIsObsoleted || num > nCLMetaKey.SortingFieldsWithPrimaryKeyFields.Count)
			{
				continue;
			}
			bool flag = true;
			int num3 = 0;
			int num4 = 0;
			for (; num3 < desiredFields.Length; num3++)
			{
				NCLMetaField nCLMetaField = desiredFields[num3];
				if (nCLMetaField.FieldActive)
				{
					NCLMetaField nCLMetaField2 = (NCLMetaField)nCLMetaKey.SortingFieldsWithPrimaryKeyFields[num4];
					if (nCLMetaField != nCLMetaField2)
					{
						flag = false;
						break;
					}
					num4++;
				}
			}
			if (flag)
			{
				bestKey = nCLMetaKey;
				return true;
			}
		}
		bestKey = null;
		return false;
	}
}
