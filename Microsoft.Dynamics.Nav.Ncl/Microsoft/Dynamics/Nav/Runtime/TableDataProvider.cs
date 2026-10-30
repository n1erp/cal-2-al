using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	public override int TableId => 2000000136;

	public TableDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int fieldNo = keyField.FieldNo;
		if (fieldNo == 1)
		{
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.Table, range, sortOrder, needNames: false))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(item.Key, -1, requireCompiled: false);
					readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(item.Key), NavText.Create(metaTableById.TableName), NavText.Create(metaTableById.TableCaptionSafe), NavBoolean.Create(metaTableById.DataPerCompany), NavInteger.Create(metaTableById.LookupFormId), NavInteger.Create(metaTableById.DrillDownPageId), NavText.Create(metaTableById.DataCaptionFields), NavBoolean.Create(metaTableById.PasteIsValid), NavBoolean.Create(metaTableById.IsLinked), NavBoolean.Create(metaTableById.IsExternalDataSource), GetOptionValue(11, (int)metaTableById.TableType), NavText.Create(metaTableById.ExternalName), GetOptionValue(13, (int)metaTableById.ObsoleteState), NavText.Create(metaTableById.ObsoleteReason), GetOptionValue(15, (int)metaTableById.DataClassification), NavBoolean.Create(metaTableById.ReplicateData), GetOptionValue(17, (int)metaTableById.CompressionType));
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
			yield break;
		}
		throw new NotSupportedException();
	}
}
