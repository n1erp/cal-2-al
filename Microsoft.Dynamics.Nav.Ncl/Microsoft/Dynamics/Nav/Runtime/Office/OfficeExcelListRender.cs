using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.OpenXml.Spreadsheet;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class OfficeExcelListRender
{
	private const int MaxColumnNumber = 16384;

	private readonly IExcelWriter excelWriter;

	private readonly IOfficeDownloadHandler downloadHandler;

	private readonly OfficeFilenameManager officeFilenameManager;

	private readonly List<ColumnInfoElement> pageColumns;

	public OfficeExcelListRender(IExcelWriter excelWriter, List<ColumnInfoElement> pageColumns)
	{
		this.excelWriter = excelWriter;
		this.pageColumns = pageColumns;
		officeFilenameManager = new OfficeFilenameManager(DocumentServiceRecordHelper.Enabled());
		downloadHandler = excelWriter.DownloadHandler() ?? throw new NavExportException("The Microsoft Excel document could not be downloaded because a download handler was not generated.");
	}

	public string GenerateExcelDocument(string filename = null)
	{
		if (pageColumns.Count > 16384)
		{
			throw new NavExportException(string.Format(CultureInfo.CurrentCulture, Lang.ExcelMaxColumns, 16384));
		}
		filename = officeFilenameManager.GenerateDocumentFileName(filename, "xlsx", excelWriter.ObjectCaption());
		using MemoryStream dataStream = new MemoryStream();
		Stream documentStream = (MemoryStream)WriteDataStream(dataStream);
		return downloadHandler.Download(documentStream, filename);
	}

	private Stream WriteDataStream(Stream dataStream)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected Obj, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected Obj, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected Obj, but got Unknown
		WorkbookWriter val = WorkbookWriter.Create(dataStream);
		try
		{
			WorksheetWriter firstWorksheet = val.FirstWorksheet;
			excelWriter.ApplyFilters(pageColumns);
			((WorksheetBase)firstWorksheet).Name = excelWriter.ObjectCaption();
			WorksheetListWriter val2 = new WorksheetListWriter(firstWorksheet);
			int num = 1;
			val2.WriteCaptions(num++, 1, (Func<IEnumerable<ICellValueTypeFormula>>)GenerateCaption, CellFormatManager.CaptionCellDecorator, true);
			int num2 = excelWriter.RowCount();
			if (num2 > ServerUserSettings.Instance.MaxRowsToExportToExcel.Value)
			{
				throw new NavOfficeDocumentRenderMaxRowsExceededException(string.Format(CultureInfo.CurrentCulture, Lang.OfficeDocumentRenderMaxRowsExceeded, num2, ServerUserSettings.Instance.MaxRowsToExportToExcel));
			}
			Worksheet worksheet = ((WorksheetBase)firstWorksheet).Worksheet;
			WorksheetSheetViewWrapper val3 = new WorksheetSheetViewWrapper(worksheet);
			val3.SetFreezePane(num, 1);
			if (num2 > 0)
			{
				WorksheetTableWriter val4 = new WorksheetTableWriter(firstWorksheet);
				val4.CreateTable(1, 1, num2, (Func<IEnumerable<ICellValueTypeFormula>>)GenerateCaption);
			}
			((WorkbookBase)val).Close();
			dataStream.Seek(0L, SeekOrigin.Begin);
			dataStream = ReplaceDataSheet(dataStream, firstWorksheet, excelWriter.GenerateData(pageColumns));
			return dataStream;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static Stream ReplaceDataSheet(Stream dataStream, WorksheetWriter worksheetWriter, IEnumerable<IEnumerable<ICellValueTypeFormula>> valueLists)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected Obj, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected Obj, but got Unknown
		using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(dataStream, isEditable: true))
		{
			WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
			WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
			WorkbookStylesPart workbookStylesPart = workbookPart.GetPartsOfType<WorkbookStylesPart>().First();
			worksheetWriter.StyleManager = new CellFormatManager(workbookStylesPart.Stylesheet);
			string origninalSheetId = workbookPart.GetIdOfPart(worksheetPart);
			WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
			string idOfPart = workbookPart.GetIdOfPart(worksheetPart2);
			foreach (TableDefinitionPart item in worksheetPart.GetPartsOfType<TableDefinitionPart>())
			{
				string idOfPart2 = worksheetPart.GetIdOfPart(item);
				worksheetPart2.AddPart(item, idOfPart2 + "Update");
			}
			OpenXmlReader openXmlReader = OpenXmlReader.Create(worksheetPart);
			OpenXmlWriter openXmlWriter = OpenXmlWriter.Create(worksheetPart2);
			Row elementObject = new Row();
			bool flag = false;
			List<Cell> list = new List<Cell>();
			while (openXmlReader.Read())
			{
				if (openXmlReader.ElementType == typeof(SheetData))
				{
					flag = true;
					if (openXmlReader.IsEndElement)
					{
						flag = false;
						continue;
					}
					openXmlWriter.WriteStartElement(new SheetData());
					openXmlReader.Read();
					if (openXmlReader.ElementType == typeof(Row))
					{
						OpenXmlElement elementObject2 = openXmlReader.LoadCurrentElement();
						openXmlWriter.WriteElement(elementObject2);
					}
					int num = 2;
					int num2 = 1;
					CellPosition val = new CellPosition(num2, num);
					foreach (IEnumerable<ICellValueTypeFormula> valueList in valueLists)
					{
						openXmlWriter.WriteStartElement(elementObject);
						num2 = 1;
						val.Row = num;
						foreach (ICellValueTypeFormula item2 in valueList)
						{
							val.Column = num2;
							Cell cell;
							if (num == 2)
							{
								cell = worksheetWriter.NewCell(val, item2, item2.CellDecorator);
								list.Add(cell);
							}
							else
							{
								cell = worksheetWriter.ModifyCell(list[num2 - 1], val, item2, (CellDecorator)null);
							}
							try
							{
								openXmlWriter.WriteElement(cell);
							}
							catch (ArgumentException innerException)
							{
								throw new NavOfficeDocumentRenderException(string.Format(CultureInfo.CurrentCulture, Lang.OfficeDocumentWriteDataError, "Excel"), innerException);
							}
							num2++;
						}
						openXmlWriter.WriteEndElement();
						num++;
					}
					openXmlWriter.WriteEndElement();
				}
				else
				{
					if (flag)
					{
						continue;
					}
					if (openXmlReader.IsStartElement)
					{
						if (openXmlReader.ElementType == typeof(TablePart))
						{
							TablePart tablePart = (TablePart)openXmlReader.LoadCurrentElement();
							tablePart.Id = string.Concat(tablePart.Id, "Update");
							openXmlWriter.WriteElement(tablePart);
						}
						else
						{
							openXmlWriter.WriteStartElement(openXmlReader);
						}
					}
					else if (openXmlReader.IsEndElement && openXmlReader.ElementType != typeof(TablePart))
					{
						openXmlWriter.WriteEndElement();
					}
				}
			}
			list.Clear();
			openXmlReader.Close();
			openXmlWriter.Close();
			Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().First((Sheet s) => s.Id.Value.Equals(origninalSheetId));
			sheet.Id.Value = idOfPart;
			workbookPart.DeletePart(worksheetPart);
		}
		dataStream.Seek(0L, SeekOrigin.Begin);
		return dataStream;
	}

	private IEnumerable<ICellValueTypeFormula> GenerateCaption()
	{
		string textFormat = NumberingFormatManager.NumberFormatsIndexedById(49u);
		HashSet<string> hashSet = pageColumns.Select((ColumnInfoElement field) => field.DisplayName).ToHashSet();
		if (hashSet.Count != pageColumns.Count)
		{
			EnsureUniqueColumnCaptions(hashSet);
		}
		return ((IEnumerable<ColumnInfoElement>)pageColumns).Select((Func<ColumnInfoElement, CellValueTypeFormula>)((ColumnInfoElement field) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected Obj, but got Unknown
			return new CellValueTypeFormula
			{
				NumberFormat = textFormat,
				Value = field.DisplayName,
				CellMetaWidth = field.PreferredWidth
			};
		})).Cast<ICellValueTypeFormula>().ToList();
	}

	private void EnsureUniqueColumnCaptions(HashSet<string> captions)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ColumnInfoElement pageColumn in pageColumns)
		{
			if (hashSet.Contains(pageColumn.DisplayName))
			{
				int num = 1;
				string text = pageColumn.DisplayName + num;
				while (hashSet.Contains(text) || captions.Contains(text))
				{
					num++;
					text = pageColumn.DisplayName + num;
				}
				pageColumn.DisplayName = text;
			}
			hashSet.Add(pageColumn.DisplayName);
		}
	}
}
