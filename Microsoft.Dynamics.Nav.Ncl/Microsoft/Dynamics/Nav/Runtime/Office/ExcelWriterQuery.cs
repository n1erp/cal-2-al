using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class ExcelWriterQuery : IExcelWriter
{
	private readonly Dictionary<int, string> decimalPlaces;

	private readonly IOfficeDownloadHandler downloadHandler;

	private readonly MasterPage page;

	private readonly NavFilterGroup[] filterGroups;

	private readonly NavQuery query;

	internal ExcelWriterQuery(NavQuery query, MasterPage page, Dictionary<int, string> decimalPlaces, IOfficeDownloadHandler downloadHandler = null, NavFilterGroup[] filterGroups = null)
	{
		this.query = query;
		this.page = page;
		this.decimalPlaces = decimalPlaces;
		this.downloadHandler = downloadHandler;
		this.filterGroups = filterGroups;
	}

	public List<ElementDefinition> ControlDefinitions()
	{
		return page.FindAll((ElementDefinition element) => element is ControlDefinition);
	}

	public ColumnInfoElement CreateColumnInfoElement(ControlDefinition control)
	{
		IEnumerable<NCLMetaField> source = from column in query.NCLMetaQuery.QueryDefinition.DefinedColumns
			where control.Name.Equals(column.Name)
			select column.SourceTableField;
		if (!source.Any())
		{
			return null;
		}
		ColumnInfoElement columnInfoElement = new ColumnInfoElement();
		if (!int.TryParse(control.DataColumnName, out var result))
		{
			return null;
		}
		columnInfoElement.FieldNumber = result;
		GetControlNames(control.Name, control.Caption, control.DataColumnName, out var columnName, out var displayName);
		columnInfoElement.ColumnName = columnName;
		columnInfoElement.DisplayName = displayName;
		if (decimalPlaces.TryGetValue(source.First().FieldNo, out var value))
		{
			columnInfoElement.DecimalFormatString = ALCompiler.ConvertToDotNetFormatString(value);
		}
		return columnInfoElement;
	}

	public IOfficeDownloadHandler DownloadHandler()
	{
		return downloadHandler ?? OfficeDownloadHandlerFactory.Create(query);
	}

	public IEnumerable<List<ICellValueTypeFormula>> GenerateData(List<ColumnInfoElement> pageColumns)
	{
		IEnumerable<string> pageColumnNames = pageColumns.Select((ColumnInfoElement pageControl) => pageControl.ColumnName);
		IEnumerable<NCLMetaQueryColumn> queryColumns = query.NCLMetaQuery.QueryDefinition.DefinedColumns.Where((NCLMetaQueryColumn nCLMetaQueryColumn) => pageColumnNames.Contains(nCLMetaQueryColumn.Name));
		query.RunOnBeforeOpentrigger();
		query.Open();
		while (query.GetNextRow())
		{
			List<ICellValueTypeFormula> list = new List<ICellValueTypeFormula>(queryColumns.Count());
			foreach (NCLMetaQueryColumn column in queryColumns)
			{
				IEnumerable<string> source = from pageColumn in pageColumns
					where pageColumn.ColumnName == column.Name
					select pageColumn.DecimalFormatString;
				string numberFormatString = ((source.Count() == 1) ? source.First() : null);
				NavValue columnValueSafe = query.GetColumnValueSafe(column.Id, column.NavType);
				list.Add((ICellValueTypeFormula)(object)NavValueOfficeExcelConverter.NavValueToExcelCellValue(columnValueSafe, numberFormatString));
			}
			yield return list;
		}
		query.Close();
	}

	public string ObjectCaption()
	{
		return page.Caption;
	}

	public int ObjectId()
	{
		return page.ID;
	}

	public int RowCount()
	{
		return query.GetCount();
	}

	public void ApplyFilters(List<ColumnInfoElement> pageColumns)
	{
		if (filterGroups == null)
		{
			return;
		}
		NavFilterGroup[] array = filterGroups;
		string columnName;
		foreach (NavFilterGroup navFilterGroup in array)
		{
			if (navFilterGroup.FilterGroupNo == -1)
			{
				throw new NotSupportedException();
			}
			NavFilter[] filters = navFilterGroup.Filters;
			foreach (NavFilter filter in filters)
			{
				IEnumerable<string> source = from pageColumn in pageColumns
					where pageColumn.FieldNumber == filter.FilterField
					select pageColumn.ColumnName;
				if (source.Count() != 0)
				{
					columnName = source.First();
					IEnumerable<NCLMetaQueryColumn> source2 = query.NCLMetaQuery.QueryDefinition.DefinedColumns.Where((NCLMetaQueryColumn column) => columnName.Equals(column.Name));
					if (source2.Count() != 0)
					{
						NCLMetaQueryColumn metaColumn = source2.First();
						string filterValue = filter.FilterValue;
						query.SetFilter(metaColumn, filterValue);
					}
				}
			}
		}
	}

	private void GetControlNames(string name, string caption, string dataColumnName, out string columnName, out string displayName)
	{
		columnName = null;
		displayName = null;
		if (!string.IsNullOrEmpty(caption))
		{
			columnName = caption;
			displayName = caption;
			return;
		}
		IEnumerable<MetaQueryDataItem> dataItems = query.NCLMetaQuery.MetaQuery.DataItems;
		foreach (MetaQueryDataItem item in dataItems)
		{
			IEnumerable<MetaQueryColumn> source = item.QueryColumns.Where((MetaQueryColumn column) => name.Equals(column.Name));
			if (source.Count() != 0)
			{
				columnName = source.First().Name;
				string caption2 = source.First().Caption;
				ref string reference = ref displayName;
				string text;
				if (string.IsNullOrEmpty(caption2))
				{
					text = ((!string.IsNullOrEmpty(name)) ? name : dataColumnName);
				}
				else
				{
					text = caption2;
				}
				reference = text;
				break;
			}
		}
	}
}
