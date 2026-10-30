using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportServerResultSetProcessor : DataItemResultSetProcessor
{
	private DataSetRequest dataSetRequest;

	private NavReport navReport;

	private bool isFirstPage = true;

	public ReportServerResultSetProcessor(NavReport report)
		: base(NavDataSetBuilder.CreateNavDataSet(report.Metadata))
	{
		navReport = report;
	}

	public override void Finish()
	{
		dataSetRequest = CreateRequest(isFinalDataSetPiece: true);
		ReportServerHandler.PrintReportOnServer(dataSetRequest);
	}

	protected DataSetRequest CreateRequest(bool isFinalDataSetPiece)
	{
		DataSetRequest dataSetRequest = new DataSetRequest();
		dataSetRequest.DataSet = DataSet;
		dataSetRequest.FinalDataSet = isFinalDataSetPiece;
		dataSetRequest.ExecutionGuid = navReport.ExecutionGuid;
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)(dataSetRequest.Information = new ReportRequestInformation());
		reportRequestInformation.AllowPrinting = navReport.PreviewCanPrint;
		reportRequestInformation.IsPreview = navReport.Preview;
		reportRequestInformation.DisplayPrinterSelection = true;
		reportRequestInformation.PrinterName = navReport.PrinterName;
		reportRequestInformation.Caption = navReport.Caption;
		reportRequestInformation.Id = navReport.ReportId;
		reportRequestInformation.ShowPrintStatus = navReport.ShowPrintStatus;
		reportRequestInformation.IsRunningAsModal = navReport.IsRunningAsModal;
		reportRequestInformation.ClientReportFormat = navReport.ClientReportFormat;
		reportRequestInformation.DefaultPaperSourceKindRaw = navReport.DefaultPaperSourceKindRaw;
		reportRequestInformation.FirstPagePaperSourceKindRaw = navReport.FirstPagePaperSourceKindRaw;
		reportRequestInformation.LastPagePaperSourceKindRaw = navReport.LastPagePaperSourceKindRaw;
		reportRequestInformation.PreviewMode = navReport.PreviewMode;
		reportRequestInformation.PdfFontEmbedding = navReport.PdfFontEmbedding;
		reportRequestInformation.CalculateBestPaperSize = ServerUserSettings.Instance.CalculateBestPaperSizeForReportPrinting.Value;
		if (isFirstPage)
		{
			reportRequestInformation.Parameters = navReport.GetParameters();
			isFirstPage = false;
		}
		return dataSetRequest;
	}
}
