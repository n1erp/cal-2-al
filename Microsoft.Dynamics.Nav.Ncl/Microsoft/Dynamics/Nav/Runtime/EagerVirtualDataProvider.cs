using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class EagerVirtualDataProvider : VirtualDataProvider
{
	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		NavRecordId recordId = request.RecordId;
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		bool isOrderedByPrimaryKeyAscending;
		foreach (ReadOnlyRecordBuffer allItem in GetAllItems(out isOrderedByPrimaryKeyAscending))
		{
			bool flag = true;
			for (int i = 0; i < primaryKey.KeyFieldCount; i++)
			{
				NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
				if (!recordId.FieldValueByIndex(keyFieldByIndex, i).Equals(allItem[keyFieldByIndex.ColumnIndex]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (allItem.PassesFieldFiltersAndMarks(request.FiltersAndMarks))
				{
					recordBuffer = allItem;
					return true;
				}
				break;
			}
		}
		recordBuffer = null;
		return false;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		IEnumerable<ReadOnlyRecordBuffer> records = from rb in GetAllItems(out var isOrderedByPrimaryKeyAscending)
			where rb.PassesFieldFiltersAndMarks(request.FiltersAndMarks)
			select rb;
		return ApplyOrdering(records, request.SortingFields, request.FindType, isOrderedByPrimaryKeyAscending);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		FilterExpression positionFilter = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(request.StartingPosition, request.SortingFields, request.IncludeCurrent);
		IEnumerable<ReadOnlyRecordBuffer> records = from rb in GetAllItems(out var isOrderedByPrimaryKeyAscending)
			where rb.PassesFieldFiltersAndMarks(request.FiltersAndMarks) && rb.PassesFilter(positionFilter)
			select rb;
		return ApplyOrdering(records, request.SortingFields, request.FindType, isOrderedByPrimaryKeyAscending);
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		return Count(request) > 0;
	}

	public override int Count(DataProviderRequest request)
	{
		FindProviderRequest request2 = new FindProviderRequest(0, request.MetaApplicationObject, request.LockState, request.FiltersAndMarks, request.GlobalAndSecurityFilters, request.FlowFieldSecurityFiltering, FieldList.Empty, MetaTable.PrimaryKey.DefinedSortingFields, FindType.Normal);
		return Find(request2).Count();
	}

	protected abstract IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending);

	private IEnumerable<ReadOnlyRecordBuffer> ApplyOrdering(IEnumerable<ReadOnlyRecordBuffer> records, SortingFieldList sortingFields, FindType findType, bool recordsOrderedByPrimaryKeyAscending)
	{
		if (recordsOrderedByPrimaryKeyAscending && sortingFields.Equals(MetaTable.PrimaryKey.DefinedSortingFields))
		{
			return records;
		}
		return records.OrderBySortingFields(sortingFields, findType);
	}
}
