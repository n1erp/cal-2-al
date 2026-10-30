using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavDataSetBuilder
{
	public const string DataSetName = "DataSet";

	public const string DataTableName = "Result";

	public static NavDataSet CreateNavDataSet(MetaReport metaReport)
	{
		NavDataSet navDataSet = new NavDataSet();
		navDataSet.DataSetName = "DataSet";
		NavDataTable navDataTable = navDataSet.Tables.Add("Result");
		navDataTable.Locale = CultureInfo.InvariantCulture;
		int num = 0;
		for (int i = 0; i < metaReport.DataItems.Count; i++)
		{
			MetaDataItem metaDataItem = metaReport.DataItems[i];
			for (int j = 0; j < metaDataItem.DataItemColumns.Count; j++)
			{
				MetaDataItemColumn metaDataItemColumn = metaDataItem.DataItemColumns[j];
				AddColumnToDataTable(metaDataItemColumn, navDataTable);
				metaDataItemColumn.DataSetIndex = num++;
			}
		}
		if (metaReport.Labels == null)
		{
			return navDataSet;
		}
		for (int k = 0; k < metaReport.Labels.Count; k++)
		{
			MetaReportLabel metaReportLabel = metaReport.Labels[k];
			if (metaReportLabel.PerRow)
			{
				NavDataColumn column = new NavDataColumn(metaReportLabel.Name, typeof(string));
				navDataSet.Tables["Result"].Columns.Add(column);
				metaReportLabel.DataSetIndex = num++;
			}
		}
		return navDataSet;
	}

	private static void AddColumnToDataTable(MetaDataItemColumn field, NavDataTable dataTable)
	{
		NavDataColumn navDataColumn = new NavDataColumn();
		Type dataType = ((field.FieldType != NavType.Duration && field.FieldType != NavType.Option && field.FieldType != NavType.RecordID) ? CommonTypeInformation.ResolveClrType(field.FieldType) : typeof(string));
		navDataColumn.DataType = dataType;
		navDataColumn.ColumnName = field.Name;
		dataTable.Columns.Add(navDataColumn);
	}

	public static NavDataSet CreateNavDataSet(MetaQuery metaQuery)
	{
		NavDataSet navDataSet = new NavDataSet();
		navDataSet.DataSetName = "DataSet";
		NavDataTable table = navDataSet.Tables.Add("Result");
		table.Locale = CultureInfo.InvariantCulture;
		metaQuery.DataItems.ForEach((MetaQueryDataItem dataItem) =>
		{
			AddDataItemToDataTable(dataItem, table);
		});
		return navDataSet;
	}

	private static void AddDataItemToDataTable(MetaQueryDataItem dataItem, NavDataTable dataTable)
	{
		foreach (MetaQueryColumn queryColumn in dataItem.QueryColumns)
		{
			if (!queryColumn.FilterOnly)
			{
				NavDataColumn navDataColumn = new NavDataColumn();
				Type dataType = ((queryColumn.ColumnType != NavType.Duration && queryColumn.ColumnType != NavType.Option && queryColumn.ColumnType != NavType.RecordID) ? CommonTypeInformation.ResolveClrType(queryColumn.ColumnType) : typeof(string));
				navDataColumn.DataType = dataType;
				navDataColumn.ColumnName = queryColumn.Name;
				dataTable.Columns.Add(navDataColumn);
			}
		}
	}
}
