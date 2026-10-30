using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class RangeBasedComputedDataProvider : ComputedVirtualDataProvider
{
	protected abstract IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters);

	protected virtual int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		NCLMetaKey primaryKey = MetaTable.PrimaryKey;
		NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(primaryKey.KeyFieldCount - 1);
		return GetValuesWithinRangeForKeyField(keyFieldByIndex, precedingKeyFieldValues, range, SortOrder.Ascending, null).Count();
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinFilterForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		RangeList rangeList = filter.ToRangeList();
		IList<Range> list2;
		if (sortOrder != SortOrder.Ascending)
		{
			IList<Range> list = rangeList.Ranges.Reverse().ToList();
			list2 = list;
		}
		else
		{
			IList<Range> list = rangeList.Ranges;
			list2 = list;
		}
		IList<Range> source = list2;
		return from range in source
			from newIntermediateValue in GetValuesWithinRangeForKeyField(keyField, precedingKeyFieldValues, range, sortOrder, nonPrimaryKeyFilters)
			select newIntermediateValue;
	}

	protected override int CountValuesWithinFilter(ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter)
	{
		RangeList rangeList = filter.ToRangeList();
		return rangeList.Ranges.Sum((Range range) => CountValuesWithinRange(precedingKeyFieldValues, range));
	}
}
