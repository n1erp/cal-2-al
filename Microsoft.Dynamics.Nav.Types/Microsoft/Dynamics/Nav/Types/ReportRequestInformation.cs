using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ReportRequestInformation : DataSetRequestInformation
{
	[DataMember(Name = "ClientReportFormat")]
	public ReportFormat ClientReportFormat { get; set; }

	[DataMember(Name = "ShowPrintStatus")]
	public bool ShowPrintStatus { get; set; }

	[DataMember(Name = "IsRunningAsModal")]
	public bool IsRunningAsModal { get; set; }

	[DataMember(Name = "Preview")]
	public bool IsPreview { get; set; }

	[DataMember(Name = "DispSelection")]
	public bool DisplayPrinterSelection { get; set; }

	[DataMember(Name = "DefaultPaperSourceKindRaw")]
	public int? DefaultPaperSourceKindRaw { get; set; }

	[DataMember(Name = "FirstPagePaperSourceKindRaw")]
	public int? FirstPagePaperSourceKindRaw { get; set; }

	[DataMember(Name = "LastPagePaperSourceKindRaw")]
	public int? LastPagePaperSourceKindRaw { get; set; }

	[DataMember(Name = "PreviewMode")]
	public PreviewMode PreviewMode { get; set; }

	[DataMember(Name = "AllowPrint")]
	public bool AllowPrinting { get; set; }

	[DataMember(EmitDefaultValue = true)]
	public string PrinterName { get; set; }

	[DataMember(Name = "Parameters")]
	public KeyValuePair<string, string>[] Parameters { get; set; }

	[DataMember(Name = "IsCustomLayout")]
	public bool IsCustomLayout { get; set; }

	[DataMember(Name = "PdfFontEmbedding")]
	public bool PdfFontEmbedding { get; set; }

	[DataMember(Name = "CalculateBestPaperSize")]
	public bool CalculateBestPaperSize { get; set; }

	public ReportRequestInformation()
		: base(DataSetRequestType.Report)
	{
		IsRunningAsModal = true;
		ShowPrintStatus = true;
		IsPreview = true;
		DisplayPrinterSelection = true;
		PrinterName = string.Empty;
		Caption = string.Empty;
	}
}
