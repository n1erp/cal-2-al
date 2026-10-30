using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CodeUnitDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000137;

	public CodeUnitDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int fieldNo = keyField.FieldNo;
		if (fieldNo == 1)
		{
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.CodeUnit, range, sortOrder, needNames: true))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					NCLMetaCodeunit metaCodeunitById = NclMetadata.GetMetaCodeunitById(item.Key, -1, requireCompiled: false);
					readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(item.Key), NavText.Create(item.Value.Name), NavInteger.Create(metaCodeunitById.TableId), NavBoolean.Create(metaCodeunitById.IsSingleInstance || item.Key == 1), GetOptionValue(5, (int)metaCodeunitById.Subtype));
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
