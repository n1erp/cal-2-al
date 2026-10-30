using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportSaveToDocumentServiceAsRenderer : ReportSaveAsRenderer
{
	private const string ExcelPrefix = "xlsx";

	private const string WordPrefix = "docx";

	private const string PdfPrefix = "pdf";

	public ReportSaveToDocumentServiceAsRenderer(NavReport report, ReportFormat outputFormat)
		: base(report, Path.GetRandomFileName(), outputFormat)
	{
	}

	protected override void Render()
	{
		byte[] array = RenderLocalReport();
		if (array != null)
		{
			DocumentServiceRecordHelper documentServiceRecordHelper = new DocumentServiceRecordHelper();
			string text;
			using (MemoryStream dataStream = new MemoryStream(array))
			{
				text = documentServiceRecordHelper.SaveDocumentStream(dataStream, FileName, GetFileName);
			}
			if (!string.IsNullOrEmpty(text))
			{
				ALSystemOperatingSystem.ALHyperlink(text, Guid.NewGuid());
			}
		}
	}

	private string GetFileName(string fileName)
	{
		return OutputFormat switch
		{
			ReportFormat.ExcelOpenXml => string.Format(CultureInfo.CurrentCulture, Lang.NavReportFileNamePostFixExcel, Path.ChangeExtension(Path.GetFileName(fileName), "xlsx")), 
			ReportFormat.WordOpenXml => string.Format(CultureInfo.CurrentCulture, Lang.NavReportFileNamePostFixWord, Path.ChangeExtension(Path.GetFileName(fileName), "docx")), 
			ReportFormat.PDF => string.Format(CultureInfo.CurrentCulture, Lang.NavReportFileNamePostFixPdf, Path.ChangeExtension(Path.GetFileName(fileName), "pdf")), 
			_ => null, 
		};
	}
}
