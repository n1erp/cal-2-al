using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class QueryDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000142;

	public QueryDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int fieldNo = keyField.FieldNo;
		if (fieldNo == 1)
		{
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.Query, range, sortOrder, needNames: false))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					NCLMetaQuery nCLMetaQuery = (NCLMetaQuery)NavGlobal.NCLMetadata.GetMetaApplicationObject(ObjectType.Query, item.Key, -1, requireCompiled: false);
					NavValue[] immutableFields = new NavValue[10]
					{
						VirtualDataProvider.VirtualTimeStamp,
						NavInteger.Create(item.Key),
						GetTruncatedTextValue(2, nCLMetaQuery.Name),
						GetTruncatedTextValue(3, nCLMetaQuery.Caption ?? nCLMetaQuery.Name),
						GetTruncatedTextValue(4, nCLMetaQuery.ApiPublisher),
						GetTruncatedTextValue(5, nCLMetaQuery.ApiGroup),
						GetTruncatedTextValue(6, string.Join(",", nCLMetaQuery.ApiVersionList)),
						GetTruncatedTextValue(7, nCLMetaQuery.EntitySetName),
						GetTruncatedTextValue(8, nCLMetaQuery.EntityName),
						GetTruncatedTextValue(9, nCLMetaQuery.QueryCategory)
					};
					readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, immutableFields);
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
