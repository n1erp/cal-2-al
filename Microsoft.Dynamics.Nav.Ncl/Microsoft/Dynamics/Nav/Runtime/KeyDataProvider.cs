using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class KeyDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000063;

	public KeyDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		return keyField.FieldNo switch
		{
			1 => GetObjectNumberAndInfoWithinRange(ObjectType.Table, range, sortOrder, needNames: true).Select((KeyValuePair<int, AllObjectSnapshotEntry> pair) =>
			{
				NavValue[] array = new NavValue[4]
				{
					VirtualDataProvider.VirtualTimeStamp,
					null,
					null,
					null
				};
				KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = pair;
				array[1] = NavInteger.Create(keyValuePair.Key);
				keyValuePair = pair;
				array[3] = GetTruncatedTextValue(3, keyValuePair.Value.Name);
				return new ReadOnlyRecordBuffer(array);
			}), 
			2 => GetKeysOnTable((NavInteger)precedingKeyFieldValues[1], (NavText)precedingKeyFieldValues[3], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		NavInteger navInteger = (NavInteger)precedingKeyFieldValues[1];
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			return 0;
		}
		NCLMetaTable metaTableById;
		try
		{
			metaTableById = NclMetadata.GetMetaTableById(navInteger.ToInt32(), -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException)
		{
			return 0;
		}
		int num = 0;
		NCLMetaKey[] keys = metaTableById.Keys;
		foreach (NCLMetaKey nCLMetaKey in keys)
		{
			int num2 = nCLMetaKey.KeyIndex + 1;
			if (num2 >= low && num2 <= high)
			{
				num++;
			}
		}
		return num;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetKeysOnTable(NavInteger tableNumber, NavText tableName, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		NCLMetaTable metaTableById;
		try
		{
			metaTableById = NclMetadata.GetMetaTableById(tableNumber.ToInt32(), -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException)
		{
			yield break;
		}
		NCLMetaKey[] keys = metaTableById.Keys;
		for (int i = ((sortOrder != SortOrder.Ascending) ? (keys.Length - 1) : 0); i >= 0 && i < keys.Length; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			NCLMetaKey nCLMetaKey = keys[i];
			int num = nCLMetaKey.KeyIndex + 1;
			if (num >= low && num <= high)
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, tableNumber, NavInteger.Create(num), tableName, GetTruncatedTextValue(4, CreateListOfFieldNames(nCLMetaKey.KeyFieldsList)), GetTruncatedTextValue(5, CreateListOfFieldNames((nCLMetaKey.SumFieldsList == null) ? null : nCLMetaKey.SumFieldsList.AsNclMetaFields())), GetTruncatedTextValue(6, CreateListOfFieldNames(nCLMetaKey.SqlIndexFields)), NavBoolean.Create(nCLMetaKey.KeyIsActive), NavBoolean.Create(nCLMetaKey.MaintainSqlIndex), NavBoolean.Create(nCLMetaKey.MaintainSumIndex), NavBoolean.Create(nCLMetaKey.Clustered), GetOptionValue(11, (int)nCLMetaKey.ObsoleteState), NavText.Create(nCLMetaKey.ObsoleteReason), NavBoolean.Create(nCLMetaKey.Unique));
				if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
				{
					yield return readOnlyRecordBuffer;
				}
			}
		}
	}

	private static string CreateListOfFieldNames(IEnumerable<NCLMetaField> fieldList)
	{
		if (fieldList != null)
		{
			return string.Join(",", fieldList.Select((NCLMetaField f) => (f.FieldName.IndexOf(',') >= 0) ? ("\"" + f.FieldName + "\"") : f.FieldName));
		}
		return string.Empty;
	}
}
