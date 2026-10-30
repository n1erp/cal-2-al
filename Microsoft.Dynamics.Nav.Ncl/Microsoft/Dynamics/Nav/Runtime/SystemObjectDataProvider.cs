using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SystemObjectDataProvider : MetadataDataProvider
{
	private static RangeList objectTypeRangeList;

	private static RangeList tableObjectNumberRangeList;

	private RangeList ObjectTypeRangeList
	{
		get
		{
			if (objectTypeRangeList == null)
			{
				NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(TableId, -1, requireCompiled: false);
				NCLMetaField fieldByNo = metaTableById.GetFieldByNo(1);
				objectTypeRangeList = RangeList.Union(RangeList.Create(Range.CreateForSingleValue(NavOption.Create(fieldByNo.FieldOptionMetadata, 1), fieldByNo.ExpressionContext)), RangeList.Create(Range.CreateForSingleValue(NavOption.Create(fieldByNo.FieldOptionMetadata, 10), fieldByNo.ExpressionContext)));
			}
			return objectTypeRangeList;
		}
	}

	private RangeList SystemTableObjectNumberRangeList
	{
		get
		{
			if (tableObjectNumberRangeList == null)
			{
				NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(TableId, -1, requireCompiled: false);
				NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
				tableObjectNumberRangeList = RangeList.Create(Range.CreateToMaximum(NavInteger.Create(2000000001), lowIsInclusive: true, fieldByNo.ExpressionContext));
			}
			return tableObjectNumberRangeList;
		}
	}

	public override int TableId => 2000000029;

	public SystemObjectDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
			return from objectType in GetObjectTypesWithinRange(range, ObjectTypeRangeList, sortOrder)
				select new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, NavOption.Create(keyField.FieldOptionMetadata, (int)objectType));
		case 3:
		{
			NavOption objectTypeOption = (NavOption)precedingKeyFieldValues[1];
			ObjectType type = (ObjectType)objectTypeOption.ToInt32();
			if (type == ObjectType.Table)
			{
				RangeList rangeList = RangeList.Intersect(SystemTableObjectNumberRangeList, RangeList.Create(range));
				if (rangeList.IsEmpty)
				{
					return new ReadOnlyRecordBuffer[0];
				}
				range = rangeList.Ranges[0];
			}
			return from _003C_003Eh__TransparentIdentifier0 in GetObjectNumberAndInfoWithinRange(type, range, sortOrder, needNames: true).Select((KeyValuePair<int, AllObjectSnapshotEntry> objectIdAndName) =>
				{
					KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = objectIdAndName;
					return new
					{
						objectIdAndName = objectIdAndName,
						number = keyValuePair.Key
					};
				})
				let snapshotEntry = objectIdAndName.Value
				let recordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, objectTypeOption, NavInteger.Create(number), GetTruncatedTextValue(4, snapshotEntry.Name), GetTruncatedTextValue(20, GetObjectCaption(type, number, snapshotEntry.Name)))
				where recordBuffer.PassesFieldFilters(nonPrimaryKeyFilters)
				select recordBuffer;
		}
		default:
			throw new NotSupportedException();
		}
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		ObjectType objectType = (ObjectType)precedingKeyFieldValues[1].ToInt32();
		if (objectType == ObjectType.Table)
		{
			RangeList rangeList = RangeList.Intersect(SystemTableObjectNumberRangeList, RangeList.Create(range));
			if (rangeList.IsEmpty)
			{
				return 0;
			}
			range = rangeList.Ranges[0];
		}
		return CountObjectsWithinRange(objectType, range);
	}
}
