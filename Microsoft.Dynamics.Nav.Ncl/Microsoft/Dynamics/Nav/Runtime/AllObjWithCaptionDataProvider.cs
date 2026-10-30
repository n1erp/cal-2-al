using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AllObjWithCaptionDataProvider : MetadataDataProvider
{
	private static readonly NavText emptySubtype = NavText.Default(0);

	public override int TableId => 2000000058;

	public AllObjWithCaptionDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
			foreach (ObjectType item in GetObjectTypesWithinRange(range, sortOrder, includeObsoleteTables: false))
			{
				yield return new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, NavOption.Create(keyField.FieldOptionMetadata, (int)item));
			}
			break;
		case 3:
		{
			NavOption objectTypeOption = (NavOption)precedingKeyFieldValues[1];
			ObjectType type = (ObjectType)objectTypeOption.ToInt32();
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item2 in GetObjectNumberAndInfoWithinRange(type, range, sortOrder, needNames: true, includeObsoleteTables: false))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, objectTypeOption, NavInteger.Create(item2.Key), GetTruncatedTextValue(4, item2.Value.Name), GetTruncatedTextValue(20, GetObjectCaption(type, item2.Key, item2.Value.Name)), GetTruncatedTextValue(30, ((NavText)GetObjectSubtype(type, item2.Key)).Value), NavGuid.CreateFromObject(item2.Value.AppPackageId ?? Guid.Empty));
				}
				catch (NavMetadataNotFoundException)
				{
					continue;
				}
				if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
				{
					yield return readOnlyRecordBuffer;
				}
			}
			break;
		}
		default:
			throw new NotSupportedException();
		}
	}

	protected override int CountValuesWithinRange(ReadOnlyRecordBuffer precedingKeyFieldValues, Range range)
	{
		ObjectType objectType = (ObjectType)precedingKeyFieldValues[1].ToInt32();
		return CountObjectsWithinRange(objectType, range, includeObsoleteTables: false);
	}

	private NavValue GetObjectSubtype(ObjectType type, int objectNumber)
	{
		return type switch
		{
			ObjectType.Page => GetTruncatedTextValue(30, NclMetadata.GetMetaFormById(objectNumber, -1, requireCompiled: false).PageType.ToString()), 
			ObjectType.CodeUnit => NclMetadata.GetMetaCodeunitById(objectNumber, -1, requireCompiled: false).Subtype switch
			{
				CodeunitSubType.Test => (NavValue)GetTruncatedTextValue(30, "Test"), 
				CodeunitSubType.TestRunner => GetTruncatedTextValue(30, "TestRunner"), 
				CodeunitSubType.Upgrade => GetTruncatedTextValue(30, "Upgrade"), 
				_ => emptySubtype, 
			}, 
			ObjectType.Query => GetTruncatedTextValue(30, NclMetadata.GetMetaQueryById(objectNumber, -1, requireCompiled: false).QueryType.ToString()), 
			_ => emptySubtype, 
		};
	}
}
