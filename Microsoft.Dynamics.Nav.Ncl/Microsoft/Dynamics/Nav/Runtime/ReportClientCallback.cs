using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportClientCallback : DataItemIteratorClientCallback
{
	private readonly NavReport navReport;

	private bool isFirstPage = true;

	public ReportClientCallback(NavReport report)
		: base(NavDataSetBuilder.CreateNavDataSet(report.Metadata), report.ExecutionGuid, report.Paging)
	{
		ClientCallback.VerifyCallbackAllowed(report);
		navReport = report;
	}

	protected override void CancelOnTargetDevice()
	{
		DataSetRequest dataSetRequest = new DataSetRequest();
		dataSetRequest.Cancel = true;
		dataSetRequest.ExecutionGuid = navReport.ExecutionGuid;
		dataSetRequest.Information = new ReportRequestInformation();
		ClientCallback.DataSetPageReady(dataSetRequest);
	}

	protected override DataSetRequest CreateRequest(bool isFinalDataSetPiece)
	{
		DataSetRequest dataSetRequest = base.CreateRequest(isFinalDataSetPiece);
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)(dataSetRequest.Information = new ReportRequestInformation());
		reportRequestInformation.AllowPrinting = navReport.PreviewCanPrint;
		reportRequestInformation.IsPreview = navReport.Preview;
		reportRequestInformation.DisplayPrinterSelection = navReport.UseRequestForm;
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
		reportRequestInformation.IsCustomLayout = navReport.IsCustomLayout;
		reportRequestInformation.PdfFontEmbedding = navReport.PdfFontEmbedding;
		reportRequestInformation.CalculateBestPaperSize = ServerUserSettings.Instance.CalculateBestPaperSizeForReportPrinting.Value;
		if (isFirstPage)
		{
			reportRequestInformation.Parameters = navReport.GetParameters();
			isFirstPage = false;
		}
		return dataSetRequest;
	}

	public override void Start()
	{
		base.Start();
		SendDataSetToTargetDevice(isFinalDataSetPiece: false);
	}
}
