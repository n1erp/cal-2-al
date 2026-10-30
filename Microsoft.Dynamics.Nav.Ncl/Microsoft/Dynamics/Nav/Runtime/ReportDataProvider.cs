using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ReportDataProvider : MetadataDataProvider
{
	public override int TableId => 2000000139;

	public ReportDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		int fieldNo = keyField.FieldNo;
		if (fieldNo == 1)
		{
			foreach (KeyValuePair<int, AllObjectSnapshotEntry> item in GetObjectNumberAndInfoWithinRange(ObjectType.Report, range, sortOrder, needNames: false))
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer;
				try
				{
					NCLMetaReport nCLMetaReport = (NCLMetaReport)NavGlobal.NCLMetadata.GetMetaApplicationObject(ObjectType.Report, item.Key, -1, requireCompiled: false);
					NavValue[] immutableFields = new NavValue[19]
					{
						VirtualDataProvider.VirtualTimeStamp,
						NavInteger.Create(item.Key),
						GetTruncatedTextValue(2, nCLMetaReport.Name),
						GetTruncatedTextValue(3, nCLMetaReport.Caption),
						NavBoolean.Create(nCLMetaReport.UseRequestForm),
						NavBoolean.Create(nCLMetaReport.UseSystemPrinter),
						NavBoolean.Create(nCLMetaReport.EnableExternalImages),
						NavBoolean.Create(nCLMetaReport.EnableHyperlinks),
						NavBoolean.Create(nCLMetaReport.EnableExternalAssemblies),
						NavBoolean.Create(nCLMetaReport.ProcessingOnly),
						NavBoolean.Create(nCLMetaReport.ShowPrintStatus),
						GetOptionValue(11, (int)nCLMetaReport.TransactionType),
						NavInteger.Create(nCLMetaReport.FirstPagePaperSourceKindRaw),
						NavInteger.Create(nCLMetaReport.DefaultPaperSourceKindRaw),
						NavInteger.Create(nCLMetaReport.LastPagePaperSourceKindRaw),
						GetOptionValue(15, (int)nCLMetaReport.PreviewMode),
						GetOptionValue(16, (int)nCLMetaReport.DefaultLayout),
						GetTruncatedTextValue(17, nCLMetaReport.WordMergeDataItem),
						NavInteger.Create(nCLMetaReport.RootDataItemTableNumber)
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
