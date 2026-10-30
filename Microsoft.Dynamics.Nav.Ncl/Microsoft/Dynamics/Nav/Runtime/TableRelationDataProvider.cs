using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableRelationDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000141;

	public TableRelationDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => GetTableIDs(range, sortOrder), 
			2 => GetFieldNos(range, sortOrder, precedingKeyFieldValues), 
			3 => GetRelationNos(range, sortOrder, precedingKeyFieldValues), 
			4 => GetConditionNos(range, sortOrder, precedingKeyFieldValues, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetTableIDs(Range range, SortOrder sortOrder)
	{
		foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.Table, range, sortOrder, needNames: false))
		{
			yield return CreateNewTableRelationEntry(NavInteger.Create(item.Key), null, null, null, null, null);
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetFieldNos(Range range, SortOrder sortOrder, ReadOnlyRecordBuffer precedingKeyFieldValues)
	{
		NavInteger tableID = (NavInteger)precedingKeyFieldValues[1];
		MetaTable metaTable;
		try
		{
			metaTable = MetadataProvider.GetTableMetadata(tableID);
		}
		catch (NavMetadataNotFoundException)
		{
			yield break;
		}
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		int num = ((sortOrder != SortOrder.Ascending) ? (metaTable.Fields.Count - 1) : 0);
		for (int fieldNumber = num; fieldNumber >= 0 && fieldNumber < metaTable.Fields.Count; fieldNumber += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			MetaField metaField = metaTable.Fields[fieldNumber];
			if (metaField.Id >= low && metaField.Id <= high && metaField.Relations.Count != 0)
			{
				yield return CreateNewTableRelationEntry(tableID, metaField, null, null, null, null);
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetRelationNos(Range range, SortOrder sortOrder, ReadOnlyRecordBuffer precedingKeyFieldValues)
	{
		NavInteger tableID = (NavInteger)precedingKeyFieldValues[1];
		NavInteger fieldNo = (NavInteger)precedingKeyFieldValues[2];
		MetaTable tableMetadata;
		try
		{
			tableMetadata = MetadataProvider.GetTableMetadata(tableID);
		}
		catch (NavMetadataNotFoundException)
		{
			yield break;
		}
		MetaField metaField = tableMetadata.Fields.Find((MetaField x) => x.Id == (int)fieldNo);
		if (metaField != null && range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			int num = ((sortOrder != SortOrder.Ascending) ? Math.Min(metaField.Relations.Count, high) : Math.Max(1, low));
			for (int relationNumber = num; relationNumber >= Math.Max(1, low) && relationNumber <= Math.Min(metaField.Relations.Count, high); relationNumber += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				yield return CreateNewTableRelationEntry(tableID, metaField, NavInteger.Create(relationNumber), null, null, null);
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetConditionNos(Range range, SortOrder sortOrder, ReadOnlyRecordBuffer precedingKeyFieldValues, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(TableId, -1, requireCompiled: false);
		metaTableById.GetFieldByNo(7);
		NavInteger tableID = (NavInteger)precedingKeyFieldValues[1];
		NavInteger fieldNo = (NavInteger)precedingKeyFieldValues[2];
		NavInteger relationNo = (NavInteger)precedingKeyFieldValues[3];
		MetaTable tableMetadata;
		try
		{
			tableMetadata = MetadataProvider.GetTableMetadata(tableID);
		}
		catch (NavMetadataNotFoundException)
		{
			yield break;
		}
		MetaField metaField = tableMetadata.Fields.Find((MetaField x) => x.Id == (int)fieldNo);
		if (metaField == null || (int)relationNo > metaField.Relations.Count)
		{
			yield break;
		}
		MetaFieldRelation metaFieldRelation = metaField.Relations[(int)relationNo - 1];
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		if (metaFieldRelation.Conditions.Count > 0)
		{
			int num = ((sortOrder != SortOrder.Ascending) ? Math.Min(metaFieldRelation.Conditions.Count, high) : Math.Max(1, low));
			for (int conditionNumber = num; conditionNumber >= Math.Max(1, low) && conditionNumber <= Math.Min(metaFieldRelation.Conditions.Count, high); conditionNumber += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer = CreateNewTableRelationEntry(tableID, metaField, relationNo, NavInteger.Create(conditionNumber), metaFieldRelation, metaFieldRelation.Conditions[conditionNumber - 1]);
				if (readOnlyRecordBuffer != null && readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
				{
					yield return readOnlyRecordBuffer;
				}
			}
		}
		else if (low <= 1 && high >= 1)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = CreateNewTableRelationEntry(tableID, metaField, relationNo, NavInteger.Create(1), metaFieldRelation, null);
			if (readOnlyRecordBuffer != null && readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}

	private ReadOnlyRecordBuffer CreateNewTableRelationEntry(NavInteger tableID, MetaField metaField, NavInteger relationNo, NavInteger conditionNo, MetaFieldRelation metaFieldRelation, MetaCondition fieldRelationCondition)
	{
		try
		{
			int fieldDefinedLength = MetaTable.GetFieldByNo(9).FieldDefinedLength;
			int num = 0;
			if (metaFieldRelation != null)
			{
				num = metaFieldRelation.FieldId;
				if (num == 0)
				{
					MetaTable tableMetadata = MetadataProvider.GetTableMetadata(metaFieldRelation.TableId);
					num = tableMetadata.Keys[0].FieldRelations[0].Id;
				}
			}
			List<NavValue> list = new List<NavValue>
			{
				VirtualDataProvider.VirtualTimeStamp,
				tableID,
				(metaField != null) ? NavInteger.Create(metaField.Id) : null,
				relationNo,
				conditionNo,
				(metaFieldRelation != null) ? NavInteger.Create(metaFieldRelation.TableId) : null,
				(metaFieldRelation != null) ? NavInteger.Create(num) : null,
				GetOptionValue(7, (int)(fieldRelationCondition?.ConditionType ?? FilterType.CONST)),
				(fieldRelationCondition != null) ? NavInteger.Create(fieldRelationCondition.FieldId) : NavInteger.Default,
				(fieldRelationCondition != null) ? NavText.CreateTruncated(fieldDefinedLength, fieldRelationCondition.ConditionValue) : NavText.Default(fieldDefinedLength),
				(metaField != null) ? NavBoolean.Create(metaField.TestRelation) : NavBoolean.Default,
				(metaField != null) ? NavBoolean.Create(metaField.ValidateRelation) : NavBoolean.Default
			};
			return new ReadOnlyRecordBuffer(MetaTable, list.ToArray());
		}
		catch (NavMetadataNotFoundException)
		{
			return null;
		}
	}
}
