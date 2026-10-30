using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FieldDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	public override int TableId => 2000000041;

	public FieldDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
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
			2 => GetFieldsOnTable((NavInteger)precedingKeyFieldValues[1], (NavText)precedingKeyFieldValues[3], range, sortOrder, nonPrimaryKeyFilters), 
			_ => throw new NotSupportedException(), 
		};
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		int tableId = precedingKeyFieldValues[1].ToInt32();
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			return 0;
		}
		NCLMetaTable metaTableById;
		try
		{
			metaTableById = NclMetadata.GetMetaTableById(tableId, -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException)
		{
			return 0;
		}
		int num = 0;
		NCLMetaField[] allFields = metaTableById.AllFields;
		for (int i = 0; i < allFields.Length; i++)
		{
			int fieldNo = allFields[i].FieldNo;
			if (fieldNo >= low && fieldNo <= high)
			{
				num++;
			}
		}
		allFields = metaTableById.DisabledFields;
		int num2 = 0;
		while (allFields != null && num2 < allFields.Length)
		{
			int fieldNo2 = allFields[num2].FieldNo;
			if (fieldNo2 >= low && fieldNo2 <= high)
			{
				num++;
			}
			num2++;
		}
		return num;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetFieldsOnTable(NavInteger tableNo, NavText tableName, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		NCLMetaTable metaTableById;
		try
		{
			metaTableById = NclMetadata.GetMetaTableById(tableNo.ToInt32(), -1, requireCompiled: false);
		}
		catch (NavMetadataNotFoundException)
		{
			yield break;
		}
		NCLMetaField[] fields = metaTableById.AllFields;
		bool flag = metaTableById.DisabledFields != null && metaTableById.DisabledFields.Length != 0;
		if (!metaTableById.FieldNumbersAscending | flag)
		{
			NCLMetaField[] array = new NCLMetaField[fields.Length + (flag ? metaTableById.DisabledFields.Length : 0)];
			Array.Copy(fields, array, fields.Length);
			if (flag)
			{
				Array.Copy(metaTableById.DisabledFields, 0, array, fields.Length, metaTableById.DisabledFields.Length);
			}
			Array.Sort(array, (NCLMetaField f1, NCLMetaField f2) => f1.FieldNo.CompareTo(f2.FieldNo));
			fields = array;
		}
		for (int i = ((sortOrder == SortOrder.Ascending) ? 1 : (fields.Length - 1)); i >= 1 && i < fields.Length; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			NCLMetaField nCLMetaField = fields[i];
			if (nCLMetaField.FieldNo > high || nCLMetaField.FieldNo < low)
			{
				if ((sortOrder == SortOrder.Ascending && nCLMetaField.FieldNo > high) || (sortOrder == SortOrder.Descending && nCLMetaField.FieldNo < low))
				{
					break;
				}
				continue;
			}
			GetFirstFieldRelationIfSimple(nCLMetaField.FieldRelations, out var sourceTableNumber, out var sourceFieldNumber);
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, tableNo, NavInteger.Create(nCLMetaField.FieldNo), tableName, GetTruncatedTextValue(4, nCLMetaField.FieldName), GetTypeOptionValue(nCLMetaField), NavInteger.Create(nCLMetaField.FieldDefinedLength), GetOptionValue(7, (int)nCLMetaField.FieldClass), NavBoolean.Create(nCLMetaField.FieldActive), GetFieldTypeName(nCLMetaField), GetTruncatedTextValue(20, nCLMetaField.FieldCaption), NavInteger.Create(sourceTableNumber), NavInteger.Create(sourceFieldNumber), GetOptionValue(23, (int)GetFieldSqlDataType(nCLMetaField)), GetOptionString(nCLMetaField), GetOptionValue(25, (int)nCLMetaField.ObsoleteState), NavText.Create(nCLMetaField.ObsoleteReason), GetOptionValue(27, (int)nCLMetaField.DataClassification));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}

	private NavOption GetTypeOptionValue(NCLMetaField field)
	{
		return GetOptionValue(5, (int)(field.FieldNavType - 1));
	}

	private static SqlDataType GetFieldSqlDataType(NCLMetaField field)
	{
		try
		{
			NavNclType fieldNclType = field.FieldNclType;
			if (fieldNclType == NavNclType.NavOemCode || fieldNclType == NavNclType.NavCode)
			{
				return (field.SqlDataType != SqlDataType.Default) ? field.SqlDataType : SqlDataType.VarChar;
			}
		}
		catch (NavMetadataNotFoundException)
		{
		}
		return SqlDataType.VarChar;
	}

	private static void GetFirstFieldRelationIfSimple(ReadOnlyCollection<NCLMetaFieldRelation> fieldRelations, out int sourceTableNumber, out int sourceFieldNumber)
	{
		if (fieldRelations.Count > 0)
		{
			NCLMetaFieldRelation nCLMetaFieldRelation = fieldRelations[0];
			if (nCLMetaFieldRelation.SourceTableFilters.Count == 0 && nCLMetaFieldRelation.Conditions.Count == 0)
			{
				sourceTableNumber = nCLMetaFieldRelation.SourceTableId;
				sourceFieldNumber = nCLMetaFieldRelation.SourceFieldId;
				return;
			}
		}
		sourceTableNumber = 0;
		sourceFieldNumber = 0;
	}

	private static NavText GetOptionString(NCLMetaField field)
	{
		if (field.FieldNclType == NavNclType.NavOption)
		{
			return NavText.Create(field.FieldOptionMetadata.OptionString);
		}
		return NavText.Empty;
	}

	private NavText GetFieldTypeName(NCLMetaField field)
	{
		bool flag;
		switch (field.FieldNclType)
		{
		case NavNclType.NavOemText:
		case NavNclType.NavOemCode:
		case NavNclType.NavBigText:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		string text = EnumHelper<NavType>.EnumToString(field.FieldNavType);
		if (flag)
		{
			text += field.FieldDefinedLength.ToString(CultureInfo.InvariantCulture);
		}
		return GetTruncatedTextValue(9, text);
	}
}
