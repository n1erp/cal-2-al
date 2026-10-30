using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class MarkedRecordsHelper
{
	internal static bool ExistsWithManyMarks(DataAccess dataAccess, ExistsCacheRequest request)
	{
		Partition(request.FiltersAndMarks, out var partition, out var partition2);
		if (!dataAccess.Exists(new ExistsCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition, request.FlowFieldSecurityFiltering, request.ForceNonLockingProviderRequest)))
		{
			return dataAccess.Exists(new ExistsCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition2, request.FlowFieldSecurityFiltering, request.ForceNonLockingProviderRequest));
		}
		return true;
	}

	internal static int CountWithManyMarks(DataAccess dataAccess, CountCacheRequest request)
	{
		Partition(request.FiltersAndMarks, out var partition, out var partition2);
		return dataAccess.Count(new CountCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition, request.FlowFieldSecurityFiltering)) + dataAccess.Count(new CountCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition2, request.FlowFieldSecurityFiltering));
	}

	internal static FieldDictionary<NavValue> CalcSumsWithManyMarks(DataAccess dataAccess, CalcSumsCacheRequest request)
	{
		Partition(request.FiltersAndMarks, out var partition, out var partition2);
		FieldDictionary<NavValue> fieldDictionary = dataAccess.CalcSums(new CalcSumsCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition, request.FlowFieldSecurityFiltering, request.FieldsToCalculate));
		FieldDictionary<NavValue> fieldDictionary2 = dataAccess.CalcSums(new CalcSumsCacheRequest(request.CompanyToken, request.MetaApplicationObject, request.Cacheable, partition2, request.FlowFieldSecurityFiltering, request.FieldsToCalculate));
		Dictionary<INavFieldMetadata, NavValue> dictionary = fieldDictionary.ToDictionary();
		Tuple<INavFieldMetadata, NavValue>[] items = fieldDictionary2.Items;
		foreach (Tuple<INavFieldMetadata, NavValue> tuple in items)
		{
			NavValue navValue = dictionary[tuple.Item1];
			NavValue item = tuple.Item2;
			Decimal18 @decimal = navValue.ToDecimal() + item.ToDecimal();
			NavValue value = NavValue.CreateNavValueFromObject(tuple.Item1, @decimal);
			dictionary[tuple.Item1] = value;
		}
		return new FieldDictionary<NavValue>(dictionary);
	}

	internal static void ModifyAllWithManyMarks(DataAccess dataAccess, int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		Partition(filtersAndMarks, out var partition, out var partition2);
		dataAccess.ModifyAll(companyToken, metaTable, partition, flowFieldSecurityFiltering, newFieldValues);
		dataAccess.ModifyAll(companyToken, metaTable, partition2, flowFieldSecurityFiltering, newFieldValues);
	}

	internal static void DeleteAllWithManyMarks(DataAccess dataAccess, int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		Partition(filtersAndMarks, out var partition, out var partition2);
		dataAccess.DeleteAll(companyToken, metaTable, partition, securityFiltering);
		dataAccess.DeleteAll(companyToken, metaTable, partition2, securityFiltering);
	}

	internal static ResultSetEnumerator FindWithManyMarks(DataAccess dataAccess, FindCacheRequest originalRequest, bool fromPosition)
	{
		Partition(originalRequest.FiltersAndMarks, out var partition, out var partition2);
		ResultSetEnumerator resultSetEnumerator = IssueFindWithMarksPartition(dataAccess, originalRequest, fromPosition, partition);
		ResultSetEnumerator partition2Enumerator = IssueFindWithMarksPartition(dataAccess, originalRequest, fromPosition, partition2);
		IEnumerable<ReadOnlyRecordBuffer> enumerable = MergeSequences(resultSetEnumerator, partition2Enumerator, originalRequest.SortingFields).ToList();
		if (originalRequest.FindType == FindType.FirstOnly)
		{
			enumerable = enumerable.Take(1);
		}
		ResultSet newResultSet = new ResultSet(enumerable.GetEnumerator(), originalRequest, 0, 0, resultSetEnumerator.ResultSet.IsCacheable);
		return resultSetEnumerator.CreateWithNewResultSet(newResultSet);
	}

	private static IEnumerable<ReadOnlyRecordBuffer> MergeSequences(ResultSetEnumerator partition1Enumerator, ResultSetEnumerator partition2Enumerator, SortingFieldList sortingFields)
	{
		ResultSetMoveResult partition2State = partition2Enumerator.TryMoveNext(out var partition2Buffer);
		MutableRecordBuffer partition1Buffer;
		while (partition1Enumerator.TryMoveNext(out partition1Buffer) == ResultSetMoveResult.Ok)
		{
			while (partition2State == ResultSetMoveResult.Ok && CompareRecordBuffers(partition1Buffer, partition2Buffer, sortingFields) > 0)
			{
				yield return partition2Buffer.ReadOnlyBuffer;
				partition2State = partition2Enumerator.TryMoveNext(out partition2Buffer);
			}
			yield return partition1Buffer.ReadOnlyBuffer;
		}
		while (partition2State == ResultSetMoveResult.Ok)
		{
			yield return partition2Buffer.ReadOnlyBuffer;
			partition2State = partition2Enumerator.TryMoveNext(out partition2Buffer);
		}
	}

	private static int CompareRecordBuffers(IRecordBuffer buffer1, IRecordBuffer buffer2, SortingFieldList sortingFields)
	{
		foreach (INavSortingField sortingField in sortingFields)
		{
			INavFieldMetadata field = sortingField.Field;
			SortOrder order = sortingField.Order;
			int num = field.ExpressionContext.Compare(buffer1[field.ColumnIndex], buffer2[field.ColumnIndex]);
			if (num != 0)
			{
				return (order == SortOrder.Ascending) ? num : (-num);
			}
		}
		return 0;
	}

	private static ResultSetEnumerator IssueFindWithMarksPartition(DataAccess dataAccess, FindCacheRequest originalRequest, bool fromPosition, FiltersAndMarks newFiltersAndMarks)
	{
		if (fromPosition)
		{
			PositionedFindCacheRequest positionedFindCacheRequest = (PositionedFindCacheRequest)originalRequest;
			PositionedFindCacheRequest request = new PositionedFindCacheRequest(positionedFindCacheRequest.CompanyToken, positionedFindCacheRequest.MetaApplicationObject, positionedFindCacheRequest.Cacheable, newFiltersAndMarks, null, originalRequest.FlowFieldSecurityFiltering, originalRequest.AutoCalcFields, positionedFindCacheRequest.SortingFields, positionedFindCacheRequest.FindType, positionedFindCacheRequest.StartingPosition, positionedFindCacheRequest.IncludeCurrent, positionedFindCacheRequest.TopNumberOfRowsToReturn, positionedFindCacheRequest.SkipNumberOfRows, positionedFindCacheRequest.Timeout);
			return dataAccess.FindFromPosition(request);
		}
		FindCacheRequest request2 = new FindCacheRequest(originalRequest.CompanyToken, originalRequest.MetaApplicationObject, originalRequest.Cacheable, newFiltersAndMarks, null, originalRequest.FlowFieldSecurityFiltering, originalRequest.AutoCalcFields, originalRequest.SortingFields, originalRequest.FindType, originalRequest.TopNumberOfRowsToReturn, originalRequest.SkipNumberOfRows, originalRequest.Timeout);
		return dataAccess.Find(request2);
	}

	private static void Partition(FiltersAndMarks input, out FiltersAndMarks partition1, out FiltersAndMarks partition2)
	{
		input.MarkedRecords.Partition(out var partition3, out var partition4);
		partition1 = new FiltersAndMarks(input.Filters, partition3);
		partition2 = new FiltersAndMarks(input.Filters, partition4);
	}
}
