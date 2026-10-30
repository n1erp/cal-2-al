using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class ExcelWriterTable : IExcelWriter
{
	private readonly NavForm form;

	private readonly IOfficeDownloadHandler downloadHandler;

	internal ExcelWriterTable(NavForm form, IOfficeDownloadHandler downloadHandler = null)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		if (!form.IsInitialized)
		{
			throw new NavNCLFormNotOpenedException();
		}
		if (form.SourceTable == null)
		{
			throw new NavNCLFormSourceTableException(form.Name, string.Format(CultureInfo.InvariantCulture, "Page {0}", form.FormId));
		}
		this.form = form;
		this.downloadHandler = downloadHandler;
	}

	public List<ElementDefinition> ControlDefinitions()
	{
		return form.MetadataHelper.ControlDefinitions;
	}

	public ColumnInfoElement CreateColumnInfoElement(ControlDefinition control)
	{
		if (form.SourceExpressions.TryGetValue(control.Visible, out var value) && value != null && (!(value.Get() is NavBoolean navBoolean) || !navBoolean.ToBoolean()))
		{
			return null;
		}
		ColumnInfoElement columnInfoElement = new ColumnInfoElement();
		string text = control.DataColumnName;
		bool flag = false;
		if (!string.IsNullOrEmpty(control.CaptionClass) || !string.IsNullOrEmpty(control.CaptionColumnName))
		{
			string key = string.Format(CultureInfo.InvariantCulture, "Control{0}_DynamicCaption", control.ID);
			if (form.SourceExpressions.TryGetValue(key, out var value2) && value2 != null)
			{
				NavValue navValue = value2.Get();
				text = navValue.ToString();
				flag = !string.IsNullOrEmpty(text);
			}
		}
		if (int.TryParse(control.DataColumnName, out var result) && result > 0)
		{
			if (!flag)
			{
				string text2 = control.Caption;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = form.SourceTable.ALFieldCaption(result);
				}
				text = text2;
			}
			columnInfoElement.FieldNumber = result;
			if (!string.IsNullOrEmpty(control.FormatColumnName))
			{
				NCLMetaTable nCLMetaTable = form.SourceTable.GetNCLMetaTable();
				NCLMetaField fieldByNo = nCLMetaTable.GetFieldByNo(result);
				if (fieldByNo.FieldNclType == NavNclType.NavDecimal)
				{
					string decimalString = form.GetDecimalString(form.SourceTable, result, control.ID);
					if (!string.IsNullOrEmpty(decimalString))
					{
						columnInfoElement.DecimalFormatString = ALCompiler.ConvertToDotNetFormatString(decimalString);
					}
				}
			}
		}
		else
		{
			if (!flag)
			{
				if (!string.IsNullOrEmpty(control.Caption))
				{
					text = control.Caption;
				}
				else if (!string.IsNullOrEmpty(control.Name))
				{
					text = control.Name;
				}
			}
			columnInfoElement.FieldNumber = -1 * control.ID;
			columnInfoElement.SourceExpression = control.DataColumnName;
		}
		columnInfoElement.ColumnName = text;
		columnInfoElement.DisplayName = text;
		return columnInfoElement;
	}

	public int RowCount()
	{
		int num = form.SourceTable.ALCount;
		if (num > 0)
		{
			return num;
		}
		if (form.FindRecord("-"))
		{
			do
			{
				form.RaiseOnAfterGetRecord();
				num++;
			}
			while (form.RaiseOnNextRecord(1) > 0);
		}
		return num;
	}

	public void ApplyFilters(List<ColumnInfoElement> pageColumns)
	{
	}

	public IEnumerable<List<ICellValueTypeFormula>> GenerateData(List<ColumnInfoElement> columns)
	{
		int[] calculatedFields = form.GetCalculatedFields();
		if (calculatedFields != null && calculatedFields.Length != 0)
		{
			form.SourceTable.CalcFields(DataError.TrapError, calculatedFields);
		}
		if (!form.FindRecord("-"))
		{
			yield break;
		}
		do
		{
			form.RaiseOnAfterGetRecord();
			List<ICellValueTypeFormula> list = new List<ICellValueTypeFormula>();
			foreach (ColumnInfoElement column in columns)
			{
				NavValue cellValue;
				if (column.FieldNumber > 0)
				{
					cellValue = form.SourceTable.GetFieldValue(column.FieldNumber);
				}
				else
				{
					NavFormSourceExpression navFormSourceExpression = form.SourceExpressions[column.SourceExpression];
					cellValue = navFormSourceExpression.Get();
				}
				list.Add((ICellValueTypeFormula)(object)NavValueOfficeExcelConverter.NavValueToExcelCellValue(cellValue, column.DecimalFormatString));
			}
			yield return list;
		}
		while (form.RaiseOnNextRecord(1) > 0);
	}

	public IOfficeDownloadHandler DownloadHandler()
	{
		return downloadHandler ?? OfficeDownloadHandlerFactory.Create(form);
	}

	public int ObjectId()
	{
		return form.FormId;
	}

	public string ObjectCaption()
	{
		return form.PageCaption;
	}
}
