using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Office;

internal class NavFormExcelWriter
{
	private readonly IExcelWriter excelWriter;

	public NavFormExcelWriter(IExcelWriter excelWriter)
	{
		this.excelWriter = excelWriter;
	}

	public string SaveAsExcel(DataError errorLevel, int languageId, string fileName = null)
	{
		try
		{
			using (NavCurrentThread.Session.CreateGlobalLanguageScope())
			{
				if (languageId > 0 && languageId != ALSystemLanguage.ALGlobalLanguage)
				{
					NavCurrentThread.Session.LocalLanguage = languageId;
				}
				using (DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).CreateTraceScope("00006BK", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Save Excel document for page={0}, LocalLanguage={1}, Culture={2}", excelWriter.ObjectId(), NavCurrentThread.Session.LocalLanguage, CultureInfo.CurrentCulture.Name)), MonitorType.None, Category.Runtime, createNewActivity: false))
				{
					OfficeExcelListRender officeExcelListRender = new OfficeExcelListRender(excelWriter, InitializePageExport());
					return officeExcelListRender.GenerateExcelDocument(fileName);
				}
			}
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return string.Empty;
	}

	internal List<ColumnInfoElement> InitializePageExport()
	{
		List<ColumnInfoElement> list = new List<ColumnInfoElement>();
		foreach (ControlDefinition item in excelWriter.ControlDefinitions())
		{
			if (string.IsNullOrEmpty(item.DataColumnName) || !NavForm.ControlIsVisible(item) || item.ExtendedDatatype == ExtendedDatatype.Masked)
			{
				continue;
			}
			ColumnInfoElement columnInfoElement = excelWriter.CreateColumnInfoElement(item);
			if (columnInfoElement != null && !string.IsNullOrEmpty(columnInfoElement.ColumnName))
			{
				if (item.WidthSpecified)
				{
					columnInfoElement.PreferredWidth = item.Width;
				}
				list.Add(columnInfoElement);
			}
		}
		if (list.Count == 0)
		{
			throw new NavOfficeDocumentRenderException(string.Format(CultureInfo.CurrentCulture, Lang.OfficeDocumentEmptyOutput, "Excel"));
		}
		return list;
	}
}
