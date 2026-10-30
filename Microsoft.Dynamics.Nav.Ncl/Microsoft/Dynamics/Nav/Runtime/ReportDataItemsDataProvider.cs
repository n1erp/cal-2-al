using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ReportDataItemsDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	private static readonly char[] RequestFilterFieldSeparator = new char[1] { ',' };

	public override int TableId => 2000000203;

	public ReportDataItemsDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
			return GetObjectNumberAndInfoWithinRange(ObjectType.Report, range, sortOrder, needNames: false).Select((KeyValuePair<int, AllObjectSnapshotEntry> pair) =>
			{
				NavValue[] array = new NavValue[3]
				{
					VirtualDataProvider.VirtualTimeStamp,
					null,
					null
				};
				KeyValuePair<int, AllObjectSnapshotEntry> keyValuePair = pair;
				array[1] = NavInteger.Create(keyValuePair.Key);
				return new ReadOnlyRecordBuffer(array);
			});
		case 2:
		{
			NavInteger reportId = (NavInteger)precedingKeyFieldValues[1];
			return GetReportDataItems(reportId, range, sortOrder, nonPrimaryKeyFilters);
		}
		default:
			throw new NotSupportedException();
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetReportDataItems(NavInteger reportId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		MetaReport reportMetadata = MetadataProvider.GetReportMetadata(reportId.Value);
		IEnumerable<FilterControlDefinition> filterControls = reportMetadata.RequestPageDefinition.FindAll((ElementDefinition x) => x is FilterControlDefinition).Cast<FilterControlDefinition>();
		IEnumerable<KeyValuePair<MetaDataItem, int>> source = reportMetadata.DataItems.Select((MetaDataItem x, int i) => new KeyValuePair<MetaDataItem, int>(x, i));
		IEnumerable<KeyValuePair<MetaDataItem, int>> enumerable = ((sortOrder == SortOrder.Ascending) ? source.OrderBy((KeyValuePair<MetaDataItem, int> x) => x.Key.Id) : source.OrderByDescending((KeyValuePair<MetaDataItem, int> x) => x.Key.Id));
		foreach (KeyValuePair<MetaDataItem, int> item in enumerable)
		{
			MetaDataItem key = item.Key;
			if (key.Id > high || key.Id < low)
			{
				if ((sortOrder == SortOrder.Ascending && key.Id > high) || (sortOrder == SortOrder.Descending && key.Id < low))
				{
					break;
				}
				continue;
			}
			string dataColumnName = string.Format(CultureInfo.InvariantCulture, "DataItem{0}TableView", item.Value);
			FilterControlDefinition filterControlDefinition = filterControls.SingleOrDefault((FilterControlDefinition x) => x.DataColumnName == dataColumnName);
			NCLMetaTable metaTableById = NclMetadata.GetMetaTableById(key.DataItemTable, -1, requireCompiled: false);
			string sortingFieldsIfAny = GetSortingFieldsIfAny(metaTableById, key.DataItemTableView);
			string requestFilterFieldsIfAny = GetRequestFilterFieldsIfAny(metaTableById, filterControlDefinition.ReqFilterFields);
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, reportId, NavInteger.Create(key.Id), NavText.Create(key.DataItemVarName), NavText.Create(requestFilterFieldsIfAny), NavInteger.Create(key.DataItemTable), NavInteger.Create(key.DataItemIndent), NavText.Create(sortingFieldsIfAny), NavText.Create(key.DataItemTableView));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return readOnlyRecordBuffer;
			}
		}
	}

	private static string GetSortingFieldsIfAny(NCLMetaTable metaTable, string dataItemTableView)
	{
		if (string.IsNullOrEmpty(dataItemTableView))
		{
			return string.Empty;
		}
		TableViewResolver tableViewResolver = new TableViewResolver(metaTable, dataItemTableView);
		TableViewParser.ParseTableView(dataItemTableView, tableViewResolver);
		if (tableViewResolver.SortingFields == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (NCLMetaField sortingField in tableViewResolver.SortingFields)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(sortingField.FieldNo);
		}
		return stringBuilder.ToString();
	}

	private static string GetRequestFilterFieldsIfAny(NCLMetaTable metaTable, string requestFilterFields)
	{
		if (string.IsNullOrEmpty(requestFilterFields))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = requestFilterFields.Split(RequestFilterFieldSeparator, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string fieldIdentifier in array2)
		{
			NCLMetaField nCLMetaField = metaTable.FindFieldMatch(fieldIdentifier, prefixFallback: true, trapError: true);
			if (nCLMetaField != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(nCLMetaField.FieldNo);
			}
		}
		return stringBuilder.ToString();
	}
}
