using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class IntegerDataProvider : RangeBasedComputedDataProvider
{
	public static readonly IntegerDataProvider Instance = new IntegerDataProvider();

	private const int FirstRecordKey = -1000000000;

	private const int LastRecordKey = 1000000000;

	public override int TableId => 2000000026;

	private IntegerDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (range.GetInclusiveIntegerBounds(-1000000000, 1000000000, out var low, out var high))
		{
			for (int thisKey = ((sortOrder == SortOrder.Ascending) ? low : high); thisKey >= low && thisKey <= high; thisKey += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				yield return RecordFromInteger(thisKey);
			}
		}
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		if (range.GetInclusiveIntegerBounds(-1000000000, 1000000000, out var low, out var high))
		{
			return high - low + 1;
		}
		return 0;
	}

	private ReadOnlyRecordBuffer RecordFromInteger(int value)
	{
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(value));
	}
}
