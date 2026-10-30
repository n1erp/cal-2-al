using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AllObjDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000038;

	public AllObjDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
			return from value in GetObjectTypesWithinRange(range, sortOrder, includeObsoleteTables: false)
				select new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, NavOption.Create(keyField.FieldOptionMetadata, (int)value));
		case 3:
		{
			NavOption objectTypeOption = (NavOption)precedingKeyFieldValues[1];
			ObjectType objectType = (ObjectType)objectTypeOption.ToInt32();
			return from _003C_003Eh__TransparentIdentifier0 in GetObjectNumberAndInfoWithinRange(objectType, range, sortOrder, needNames: true, includeObsoleteTables: false).Select((KeyValuePair<int, AllObjectSnapshotEntry> objectNumberAndInfo) =>
				{
					NCLMetaTable metaApplicationObject = MetaTable;
					NavValue[] array = new NavValue[5]
					{
						VirtualDataProvider.VirtualTimeStamp,
						objectTypeOption,
						null,
						null,
						null
					};
					KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = objectNumberAndInfo;
					array[2] = NavInteger.Create(keyValuePair.Key);
					AllObjDataProvider allObjDataProvider = this;
					keyValuePair = objectNumberAndInfo;
					array[3] = allObjDataProvider.GetTruncatedTextValue(4, keyValuePair.Value.Name);
					keyValuePair = objectNumberAndInfo;
					array[4] = NavGuid.CreateFromObject(keyValuePair.Value.AppPackageId ?? Guid.Empty);
					return new
					{
						objectNumberAndInfo = objectNumberAndInfo,
						recordBuffer = new ReadOnlyRecordBuffer(metaApplicationObject, array)
					};
				})
				where _003C_003Eh__TransparentIdentifier0.recordBuffer.PassesFieldFilters(nonPrimaryKeyFilters)
				select _003C_003Eh__TransparentIdentifier0.recordBuffer;
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
}
