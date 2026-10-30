using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public class ReportRenderer : IDisposable
{
	internal const string ReportDataSetName = "DataSet";

	internal const string ReportDataTableName = "Result";

	private const string RDLSchemaReportElement = "Report";

	private const string RDLSchemaPageElement = "Page";

	private const string RDLSchemaPageWidth = "PageWidth";

	private const string RDLSchemaPageHeight = "PageHeight";

	private const string RDLSchemaTopMargin = "TopMargin";

	private const string RDLSchemaLeftMargin = "LeftMargin";

	private const string RDLSchemaRightMargin = "RightMargin";

	private const string RDLSchemaBottomMargin = "BottomMargin";

	private const string Inch = "IN";

	private const string Pica = "PC";

	private const string Point = "PT";

	private const string Millimeter = "MM";

	private const string Centimeter = "CM";

	private const int RpcServerUnavailableErrorCode = 1722;

	public static readonly Guid ReportPrintingErrorGuid = new Guid("8B72B0EC-3341-A2A2-9C5F-3EA3CF0F957A");

	public const string RdlNamespace2010 = "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition";

	public const string RdlNamespace2016 = "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition";

	private ReportExecutionItem reportExecutionItem;

	private IReportCallbackHandler reportCallbackHandler;

	private PrintDocument printDocument;

	private PageSettings reportPageSettings;

	private bool landScape;

	private IList<Stream> reportStreams;

	private int currentReportPageIndex;

	private int endPage;

	private bool usePrintDialog;

	private bool paperSourceUsed;

	private int? cachedHardMarginX;

	private int? cachedHardMarginY;

	private bool showPrintStatus;

	protected internal ReportExecutionItem ReportExecutionItem => reportExecutionItem;

	protected IReportCallbackHandler ReportCallbackHandler => reportCallbackHandler;

	internal PrintDocument PrintDocument => printDocument;

	protected bool Canceling { get; set; }

	public virtual bool IsCanceling => Canceling;

	public virtual bool IsProgressNotificationRunning { get; }

	protected bool IsDisposed { get; set; }

	protected internal ReportRenderer(ReportExecutionItem reportExecutionItem, IReportCallbackHandler reportCallbackHandler)
	{
		this.reportExecutionItem = reportExecutionItem;
		this.reportCallbackHandler = reportCallbackHandler;
	}

	protected virtual void ShowReportViewerCore(DataSetRequest request, bool isLogicalFormVisible)
	{
		throw new NotImplementedException();
	}

	public void ShowReportViewer(DataSetRequest request, bool isLogicalFormVisible)
	{
		ShowReportViewerCore(request, isLogicalFormVisible);
	}

	public void ShowReportViewer(DataSetRequest request)
	{
		//IL_001a: Expected Obj, but got Unknown
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		try
		{
			ShowReportViewer(request, isLogicalFormVisible: false);
		}
		catch (LocalProcessingException ex)
		{
			LocalProcessingException ex2 = ex;
			RequestCancellation();
			HandleError(string.Format(CultureInfo.CurrentCulture, Lang.ReportPrintPreviewWindowError, ((Exception)(object)ex2).GetBaseException().Message));
		}
	}

	public bool InitializePrintDirectly(RetrievePrinterSettings printerSettingsDelegate, string printerName, bool displayPrinterSelection, bool calculateBestPaperSize)
	{
		usePrintDialog = displayPrinterSelection;
		try
		{
			VerifyIfThereArePrintersInstalled();
			printDocument = PrinterSetup(printerName);
			reportPageSettings = GetReportPageSettings(ReportExecutionItem.ReportId, reportExecutionItem.ReportHandle.ReportLayout);
			ApplyReportPageSettingsToPrinterSettings(printerName, reportPageSettings, calculateBestPaperSize);
			bool result = false;
			ReportCallbackHandler.RunWithCommunicationBrokerBlocker(() =>
			{
				result = SelectPrinter(printerSettingsDelegate, printerName);
			});
			PrinterSettings printerSettings = printDocument.PrinterSettings;
			if (!VerifyIfPrinterExist(printerSettings.PrinterName) || !printerSettings.IsValid)
			{
				throw new NavReportPrintingException(string.Format(CultureInfo.CurrentCulture, Lang.PrinterSettingsError, printerSettings.PrinterName));
			}
			if (string.IsNullOrEmpty(printerName) ? (!printerSettings.IsDefaultPrinter) : (!string.Equals(printerName, printerSettings.PrinterName, StringComparison.OrdinalIgnoreCase)))
			{
				ApplyReportPageSettingsToPrinterSettings(printerSettings.PrinterName, reportPageSettings, calculateBestPaperSize);
			}
			return result;
		}
		catch (NavReportPrintingException ex)
		{
			HandleException(ex);
			return false;
		}
	}

	public bool ReportPageReadyPrintDirectly(DataSetRequest reportRequest, bool isFirstPage, ReportExecutionHandler reportExecutionHandler)
	{
		bool result = true;
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)reportRequest.Information;
		showPrintStatus = reportRequestInformation.ShowPrintStatus;
		if (isFirstPage)
		{
			if (InitializePrintDirectly(ReportCallbackHandler.PrinterSettings, reportRequestInformation.PrinterName, reportRequestInformation.DisplayPrinterSelection, reportRequestInformation.CalculateBestPaperSize))
			{
				StartProgressNotification(string.Format(CultureInfo.CurrentCulture, Lang.ReportProgressBody, ReportExecutionItem.DataEnumerator.RowsAdded));
			}
			else
			{
				RequestCancellation();
				result = false;
			}
			ReportExecutionItem.Release();
		}
		if (reportRequest.FinalDataSet)
		{
			ReportExecutionItem.Wait();
			UpdateProgressNotificationBodyText(Lang.ReportBeginRenderingProgressBody);
			string text = string.Empty;
			ReportRenderingResult reportRenderingResult = ReportRenderingResult.Failed;
			if (!IsCanceling)
			{
				try
				{
					reportRenderingResult = RenderLocalReport();
				}
				catch (NavReportRenderingException ex)
				{
					text = ex.Message;
				}
			}
			if (!showPrintStatus)
			{
				CloseProgressNotification();
			}
			switch (reportRenderingResult)
			{
			case ReportRenderingResult.Success:
				ReportExecutionItem.ReportRenderer.PrintReport(showPrintStatus);
				reportExecutionHandler.RemoveReportExecutionItem(reportRequest.ExecutionGuid);
				break;
			case ReportRenderingResult.Empty:
				reportExecutionHandler.RemoveReportExecutionItem(reportRequest.ExecutionGuid);
				break;
			default:
				reportExecutionHandler.CancelReport(reportRequest.ExecutionGuid);
				result = false;
				if (reportRenderingResult == ReportRenderingResult.Failed && !string.IsNullOrEmpty(text))
				{
					HandleError(text);
				}
				break;
			}
		}
		return result;
	}

	public bool ReportPageReadyCreateReportFormat(DataSetRequest reportRequest, bool isFirstPage, ReportExecutionHandler reportExecutionHandler)
	{
		bool result = true;
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)reportRequest.Information;
		if (isFirstPage)
		{
			StartProgressNotification(string.Format(CultureInfo.CurrentCulture, Lang.ReportProgressBody, ReportExecutionItem.DataEnumerator.RowsAdded));
		}
		if (reportRequest.FinalDataSet)
		{
			UpdateProgressNotificationBodyText(Lang.ReportBeginRenderingProgressBody);
			string text = string.Empty;
			byte[] outputBuffer = null;
			string fileNameExtension = null;
			if (!IsCanceling)
			{
				try
				{
					outputBuffer = CreateReport(reportRequestInformation.PdfFontEmbedding, reportRequestInformation.ClientReportFormat, out fileNameExtension);
				}
				catch (NavReportRenderingException ex)
				{
					text = ex.Message;
				}
			}
			CloseProgressNotification();
			if (outputBuffer != null)
			{
				if (outputBuffer.Length != 0)
				{
					string actualFileName = "Report" + reportRequestInformation.Id + "." + fileNameExtension;
					string typeFilter = null;
					switch (reportRequestInformation.ClientReportFormat)
					{
					case ReportFormat.PDF:
					case ReportFormat.PdfPreview:
					case ReportFormat.PdfPrint:
						typeFilter = Lang.FileDialogTypeFilterPdf;
						break;
					case ReportFormat.WordOpenXml:
						typeFilter = Lang.FileDialogTypeFilterWord;
						break;
					case ReportFormat.ExcelOpenXml:
						typeFilter = Lang.FileDialogTypeFilterExcel;
						break;
					}
					try
					{
						result = ReportCallbackHandler.FileSave(actualFileName, Lang.LoadFileCaption, null, typeFilter, () => new MemoryStream(outputBuffer), out actualFileName);
					}
					catch (Exception ex2)
					{
						NavFileHelper.MapAndRethrowClrException(ex2, writeExceptionToEventLog: true);
						throw;
					}
				}
				reportExecutionHandler.RemoveReportExecutionItem(reportRequest.ExecutionGuid);
			}
			else
			{
				reportExecutionHandler.CancelReport(reportRequest.ExecutionGuid);
				result = false;
				if (!string.IsNullOrEmpty(text))
				{
					HandleError(text);
				}
			}
		}
		return result;
	}

	private PrintDocument PrinterSetup(string printerName)
	{
		PrintDocument printDocument = new PrintDocument();
		printDocument.PrinterSettings = new PrinterSettings();
		bool flag = string.IsNullOrEmpty(printerName);
		if (!flag)
		{
			printDocument.PrinterSettings.PrinterName = printerName;
		}
		if (!usePrintDialog && (!VerifyIfPrinterExist(printDocument.PrinterSettings.PrinterName) || !printDocument.PrinterSettings.IsValid))
		{
			HandleError(flag ? Lang.NoDefaultPrinterSelected : string.Format(CultureInfo.CurrentCulture, Lang.PrinterNotValid, printerName));
			usePrintDialog = true;
			printDocument.PrinterSettings.PrinterName = null;
		}
		printDocument.PrinterSettings.MinimumPage = 1;
		printDocument.PrinterSettings.FromPage = 1;
		printDocument.PrinterSettings.ToPage = 1;
		printDocument.PrinterSettings.PrintRange = PrintRange.AllPages;
		return printDocument;
	}

	internal static void VerifyIfThereArePrintersInstalled()
	{
		try
		{
			if (PrinterSettings.InstalledPrinters.Count == 0)
			{
				throw new NavReportPrintingException(Lang.PrinterNotInstalled);
			}
		}
		catch (Win32Exception ex) when (ex.NativeErrorCode == 1722)
		{
			throw new NavReportPrintingException(Lang.PrintSpoolerServiceUnavailable, ex);
		}
	}

	public static bool VerifyIfPrinterExist(string printer)
	{
		return PrinterSettings.InstalledPrinters.Cast<string>().Any((string p) => string.Compare(p, printer, StringComparison.OrdinalIgnoreCase) == 0);
	}

	public static PageSettings GetReportPageSettings(int reportId, Stream reportLayout)
	{
		PageSettings pageSettings = new PageSettings
		{
			Margins = 
			{
				Left = 0,
				Right = 0,
				Top = 0,
				Bottom = 0
			}
		};
		int num = 850;
		int num2 = 1100;
		long position = reportLayout.Position;
		reportLayout.Seek(0L, SeekOrigin.Begin);
		using (XmlReader xmlReader = XmlHelper.SafeXmlReader(reportLayout))
		{
			if (xmlReader.ReadToFollowing("Report"))
			{
				if (xmlReader.NamespaceURI != "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition" && xmlReader.NamespaceURI != "http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition")
				{
					throw new NavReportVersionObsoleteException(reportId);
				}
				if (xmlReader.ReadToFollowing("Page"))
				{
					xmlReader.Read();
				}
			}
			while (!xmlReader.EOF)
			{
				if (xmlReader.NodeType == XmlNodeType.Element)
				{
					switch (xmlReader.Name)
					{
					case "PageHeight":
						num2 = ParseSizeString(xmlReader.ReadString());
						break;
					case "PageWidth":
						num = ParseSizeString(xmlReader.ReadString());
						break;
					case "LeftMargin":
						pageSettings.Margins.Left = ParseSizeString(xmlReader.ReadString());
						break;
					case "RightMargin":
						pageSettings.Margins.Right = ParseSizeString(xmlReader.ReadString());
						break;
					case "TopMargin":
						pageSettings.Margins.Top = ParseSizeString(xmlReader.ReadString());
						break;
					case "BottomMargin":
						pageSettings.Margins.Bottom = ParseSizeString(xmlReader.ReadString());
						break;
					}
				}
				xmlReader.Skip();
			}
		}
		pageSettings.PaperSize = new PaperSize(string.Empty, num, num2);
		pageSettings.Landscape = num > num2;
		reportLayout.Seek(position, SeekOrigin.Begin);
		return pageSettings;
	}

	internal static int ParseSizeString(string sizeStr)
	{
		sizeStr = sizeStr.Replace(" ", string.Empty);
		string text = sizeStr.Substring(sizeStr.Length - 2).ToUpperInvariant();
		double num = Convert.ToDouble(sizeStr.Remove(sizeStr.Length - 2), CultureInfo.InvariantCulture);
		double value = -1.0;
		switch (text)
		{
		case "IN":
			value = num * 100.0;
			break;
		case "CM":
			value = num * 100.0 / 2.54;
			break;
		case "MM":
			value = num * 100.0 / 25.4;
			break;
		case "PT":
			value = num * 100.0 / 72.0;
			break;
		case "PC":
			value = num * 100.0 / 6.0;
			break;
		}
		return (int)Math.Round(value, 0);
	}

	private bool SelectPrinter(RetrievePrinterSettings printerSettingsDelegate, string printerName)
	{
		if (usePrintDialog && printerSettingsDelegate != null && printerSettingsDelegate(printDocument, printerName))
		{
			return false;
		}
		return true;
	}

	private void ApplyReportPageSettingsToPrinterSettings(string printerName, PageSettings pageSettings, bool calculateBestPaperSize)
	{
		PrinterSettings printerSettings = printDocument.PrinterSettings;
		PaperSize paperSize = FindPaperSizeInPrinter(VerifyIfPrinterExist(printerName) ? printerName : string.Empty, pageSettings, calculateBestPaperSize, out landScape);
		if (paperSize != null)
		{
			printerSettings.DefaultPageSettings.PaperSize = paperSize;
		}
		printerSettings.DefaultPageSettings.Landscape = landScape;
		if (reportExecutionItem.DefaultPaperSourceKindRaw.HasValue)
		{
			printerSettings.DefaultPageSettings.PaperSource = new PaperSource
			{
				RawKind = reportExecutionItem.DefaultPaperSourceKindRaw.Value
			};
		}
		printerSettings.DefaultPageSettings.Margins = pageSettings.Margins;
	}

	public static PaperSize FindPaperSizeInPrinter(string printerName, PageSettings pageSettings, bool calculateBestPaperSize, out bool landscape)
	{
		NavPrinterSettings printerSettings = null;
		landscape = false;
		if (NavPrinterSettingsCache.TryGetValue(printerName, out printerSettings))
		{
			Tuple<PaperSize, bool> tuple = printerSettings.BestPaperSizeMatch(pageSettings.PaperSize.Width, pageSettings.PaperSize.Height, calculateBestPaperSize);
			if (tuple == null)
			{
				return null;
			}
			landscape = tuple.Item2;
			return tuple.Item1;
		}
		return null;
	}

	public byte[] CreateReport(bool pdfFontEmbedding, ReportFormat reportFormat, out string fileNameExtension)
	{
		//IL_0100: Expected Obj, but got Unknown
		fileNameExtension = null;
		if (reportExecutionItem.DataEnumerator.RowsAdded < 1)
		{
			return new byte[0];
		}
		byte[] result = null;
		string text = string.Empty;
		reportExecutionItem.ReportHandle.AddDataSource("DataSet_Result", reportExecutionItem.DataEnumerator.DecompressRows());
		try
		{
			TraceListener traceListener = Debug.Listeners[string.Empty];
			string[] errors;
			try
			{
				if (traceListener != null && string.IsNullOrEmpty(traceListener.Name))
				{
					Debug.Listeners.Remove(traceListener);
				}
				reportExecutionItem.ReportHandle.SetReportParameters(reportExecutionItem.ReportParameters);
				string deviceInfo = (pdfFontEmbedding ? string.Empty : "<DeviceInfo><EmbedFonts>None</EmbedFonts></DeviceInfo>");
				result = reportExecutionItem.ReportHandle.Render(reportFormat.ToString(), deviceInfo, out var _, out var _, out fileNameExtension, out var _, out errors);
			}
			finally
			{
				if (traceListener != null && traceListener.Name == "Default")
				{
					Debug.Listeners.Add(traceListener);
				}
			}
			if (errors != null && errors.Length != 0)
			{
				text = errors.First();
			}
		}
		catch (LocalProcessingException ex)
		{
			LocalProcessingException ex2 = ex;
			Exception innerException = ((Exception)(object)ex2).InnerException;
			while (innerException != null && innerException.InnerException != null)
			{
				innerException = innerException.InnerException;
			}
			if (innerException is NavReportRenderingCanceledException)
			{
				return null;
			}
			text = ((Exception)(object)ex2).GetBaseException().Message;
		}
		finally
		{
			reportExecutionItem.ReportHandle.ClearDataSources();
		}
		if (!string.IsNullOrEmpty(text))
		{
			throw NavReportRenderingException.Create(CultureInfo.InvariantCulture, text);
		}
		return result;
	}

	public ReportRenderingResult RenderLocalReport()
	{
		//IL_0145: Expected Obj, but got Unknown
		if (reportExecutionItem.DataEnumerator.RowsAdded < 1)
		{
			return ReportRenderingResult.Empty;
		}
		ReportRenderingResult result = ReportRenderingResult.Success;
		string deviceInfo = CreateEMFDeviceInfo(printDocument.PrinterSettings.DefaultPageSettings, isOrientationUnchanged: true, pdfFontEmbedding: true);
		string text = string.Empty;
		reportStreams = new List<Stream>();
		reportExecutionItem.ReportHandle.AddDataSource("DataSet_Result", reportExecutionItem.DataEnumerator.DecompressRows());
		try
		{
			TraceListener traceListener = Debug.Listeners[string.Empty];
			string[] array;
			try
			{
				if (traceListener != null && string.IsNullOrEmpty(traceListener.Name))
				{
					Debug.Listeners.Remove(traceListener);
				}
				reportExecutionItem.ReportHandle.SetReportParameters(reportExecutionItem.ReportParameters);
				if (ReportAppDomain.LegacyCasEnabled || ReportAppDomain.ForceToCurrentAppDomain)
				{
					array = reportExecutionItem.ReportHandle.Render("IMAGE", deviceInfo, CreateStream);
				}
				else
				{
					MemoryStream[] streams = null;
					array = reportExecutionItem.ReportHandle.Render("IMAGE", deviceInfo, out streams);
					reportStreams = new List<Stream>(streams);
				}
			}
			finally
			{
				if (traceListener != null && traceListener.Name == "Default")
				{
					Debug.Listeners.Add(traceListener);
				}
			}
			if (array != null && array.Length != 0)
			{
				text = array.First();
			}
		}
		catch (LocalProcessingException ex)
		{
			LocalProcessingException ex2 = ex;
			Exception innerException = ((Exception)(object)ex2).InnerException;
			while (innerException != null && innerException.InnerException != null)
			{
				innerException = innerException.InnerException;
			}
			if (innerException is NavReportRenderingCanceledException)
			{
				result = ReportRenderingResult.Cancel;
			}
			else
			{
				text = ((Exception)(object)ex2).GetBaseException().Message;
			}
		}
		finally
		{
			reportExecutionItem.ReportHandle.ClearDataSources();
		}
		if (!string.IsNullOrEmpty(text))
		{
			throw NavReportRenderingException.Create(CultureInfo.InvariantCulture, text);
		}
		return result;
	}

	internal string CreateEMFDeviceInfo(PageSettings pageSettings, bool isOrientationUnchanged, bool pdfFontEmbedding)
	{
		bool flag = pageSettings.PaperSize.Height < pageSettings.PaperSize.Width != pageSettings.Landscape;
		int hundredthsOfInch = (flag ? pageSettings.PaperSize.Width : pageSettings.PaperSize.Height);
		int hundredthsOfInch2 = (flag ? pageSettings.PaperSize.Height : pageSettings.PaperSize.Width);
		int hundredthsOfInch3;
		int hundredthsOfInch4;
		int hundredthsOfInch5;
		int hundredthsOfInch6;
		if (reportPageSettings.Landscape != pageSettings.Landscape)
		{
			hundredthsOfInch3 = (isOrientationUnchanged ? pageSettings.Margins.Top : pageSettings.Margins.Right);
			hundredthsOfInch4 = (isOrientationUnchanged ? pageSettings.Margins.Left : pageSettings.Margins.Top);
			hundredthsOfInch5 = (isOrientationUnchanged ? pageSettings.Margins.Right : pageSettings.Margins.Bottom);
			hundredthsOfInch6 = (isOrientationUnchanged ? pageSettings.Margins.Bottom : pageSettings.Margins.Left);
		}
		else
		{
			hundredthsOfInch3 = (isOrientationUnchanged ? pageSettings.Margins.Top : pageSettings.Margins.Left);
			hundredthsOfInch4 = (isOrientationUnchanged ? pageSettings.Margins.Left : pageSettings.Margins.Bottom);
			hundredthsOfInch5 = (isOrientationUnchanged ? pageSettings.Margins.Right : pageSettings.Margins.Top);
			hundredthsOfInch6 = (isOrientationUnchanged ? pageSettings.Margins.Bottom : pageSettings.Margins.Right);
		}
		string arg = string.Format(CultureInfo.InvariantCulture, "<MarginTop>{0}</MarginTop><MarginLeft>{1}</MarginLeft><MarginRight>{2}</MarginRight><MarginBottom>{3}</MarginBottom><PageHeight>{4}</PageHeight><PageWidth>{5}</PageWidth>", ToInchesString(hundredthsOfInch3), ToInchesString(hundredthsOfInch4), ToInchesString(hundredthsOfInch5), ToInchesString(hundredthsOfInch6), ToInchesString(hundredthsOfInch), ToInchesString(hundredthsOfInch2));
		string text = string.Format(CultureInfo.InvariantCulture, "<DeviceInfo><OutputFormat>emf</OutputFormat>{0}{1}</DeviceInfo>", arg, pdfFontEmbedding ? string.Empty : "<EmbedFonts>None</EmbedFonts>");
		NavDiagnostics.GetMostSpecificInstance.SendTraceTag("0000DQW", Category.Runtime, Verbosity.Normal, "Emf Deviceinfo for {0} ID {1}. device info: {2}.", DiagnosticParameter.SystemMetadata(ObjectType.Report.ToString().ToLowerInvariant()), DiagnosticParameter.SystemMetadata(ReportExecutionItem.ReportId), DiagnosticParameter.SystemMetadata(text));
		return text;
	}

	private Stream CreateStream()
	{
		if (IsCanceling)
		{
			throw new NavReportRenderingCanceledException();
		}
		Stream stream = new MemoryStream();
		reportStreams.Add(stream);
		UpdateProgressNotificationBodyText(string.Format(CultureInfo.CurrentCulture, Lang.ReportRenderingProgressBody, reportStreams.Count));
		return stream;
	}

	public void PrintReport(bool showStatus)
	{
		if (IsReportEmpty() || !IsInitialPageToPrintValid())
		{
			return;
		}
		printDocument.PrintController = new StandardPrintController();
		printDocument.DocumentName = reportExecutionItem.ReportCaption;
		SetPageRangeToPrint();
		printDocument.QueryPageSettings += QueryPageSettings;
		printDocument.PrintPage += ReportPrintPage;
		try
		{
			if (showStatus)
			{
				if (!IsProgressNotificationRunning)
				{
					StartProgressNotification(string.Format(CultureInfo.CurrentCulture, Lang.ReportPrintingCaption, reportExecutionItem.ReportCaption, reportExecutionItem.ReportId), string.Empty);
				}
				else
				{
					UpdateProgressNotificationHeaderText(string.Format(CultureInfo.CurrentCulture, Lang.ReportPrintingCaption, reportExecutionItem.ReportCaption, reportExecutionItem.ReportId));
					UpdateProgressNotificationBodyText(string.Empty);
				}
			}
			printDocument.Print();
		}
		catch (NullReferenceException ex)
		{
			NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("00001N7", Category.Runtime, ex);
			string errorMessage = string.Format(CultureInfo.CurrentCulture, Lang.ErrorWhilePrintingNoExceptionCode, ex.Message);
			HandleError(errorMessage);
		}
		catch (InvalidPrinterException ex2)
		{
			HandleException(ex2);
		}
		catch (Win32Exception ex3)
		{
			string errorMessage2 = string.Format(CultureInfo.CurrentCulture, Lang.ErrorWhilePrinting, ex3.Message, ex3.ErrorCode);
			HandleError(errorMessage2);
		}
		finally
		{
			if (showStatus)
			{
				CloseProgressNotification();
			}
		}
	}

	internal bool IsReportEmpty()
	{
		return reportStreams.Count == 0;
	}

	internal bool IsInitialPageToPrintValid()
	{
		return printDocument.PrinterSettings.FromPage <= reportStreams.Count;
	}

	private void SetPageRangeToPrint()
	{
		currentReportPageIndex = printDocument.PrinterSettings.FromPage;
		if (printDocument.PrinterSettings.PrintRange == PrintRange.SomePages && printDocument.PrinterSettings.ToPage <= reportStreams.Count)
		{
			endPage = printDocument.PrinterSettings.ToPage;
		}
		else
		{
			endPage = reportStreams.Count;
		}
		reportExecutionItem.LastPage = endPage;
	}

	private void QueryPageSettings(object sender, QueryPageSettingsEventArgs queryPageSettingsEventArguments)
	{
		int? paperSourceKindRaw = reportExecutionItem.GetPaperSourceKindRaw(currentReportPageIndex);
		if (paperSourceKindRaw.HasValue)
		{
			queryPageSettingsEventArguments.PageSettings.PaperSource = new PaperSource
			{
				RawKind = paperSourceKindRaw.Value
			};
			paperSourceUsed = true;
		}
		else if (paperSourceUsed)
		{
			queryPageSettingsEventArguments.PageSettings.PaperSource = null;
		}
	}

	private void ReportPrintPage(object sender, PrintPageEventArgs printPageEventArguments)
	{
		Stream stream = reportStreams[currentReportPageIndex - 1];
		stream.Position = 0L;
		Metafile image = new Metafile(stream);
		if (!cachedHardMarginX.HasValue)
		{
			cachedHardMarginX = (int)printPageEventArguments.PageSettings.HardMarginX;
		}
		if (!cachedHardMarginY.HasValue)
		{
			cachedHardMarginY = (int)printPageEventArguments.PageSettings.HardMarginY;
		}
		Rectangle rect = new Rectangle(printPageEventArguments.PageBounds.Left - cachedHardMarginX.Value, printPageEventArguments.PageBounds.Top - cachedHardMarginY.Value, printPageEventArguments.PageBounds.Width, printPageEventArguments.PageBounds.Height);
		printPageEventArguments.Graphics.DrawImage(image, rect);
		currentReportPageIndex++;
		printPageEventArguments.HasMorePages = currentReportPageIndex <= endPage;
		if (showPrintStatus && printPageEventArguments.HasMorePages)
		{
			UpdateProgressNotificationBodyText(string.Format(CultureInfo.CurrentCulture, Lang.ReportPrintingBody, currentReportPageIndex, endPage));
			if (IsCanceling)
			{
				RequestCancellation();
				printPageEventArguments.Cancel = true;
			}
		}
	}

	internal static string ToInchesString(int hundredthsOfInch)
	{
		return ((double)hundredthsOfInch / 100.0).ToString(CultureInfo.InvariantCulture) + "in";
	}

	public virtual void StartProgressNotification(string bodyText)
	{
	}

	public virtual void StartProgressNotification(string headerText, string bodyText)
	{
	}

	public virtual void CloseProgressNotification()
	{
	}

	public virtual void UpdateProgressNotificationBodyText(string bodyText)
	{
	}

	public virtual void UpdateProgressNotificationHeaderText(string headerText)
	{
	}

	public void RequestCancellation()
	{
		Canceling = true;
		CloseProgressNotification();
	}

	protected virtual void ReportCanceled()
	{
	}

	internal void CancelReport()
	{
		ReportCanceled();
	}

	public virtual void ReportCompleted(bool allowPrinting)
	{
		CloseProgressNotification();
	}

	public void HandleException(Exception ex)
	{
		ReportCallbackHandler.HandleException(ex);
	}

	public void HandleError(string errorMessage)
	{
		ReportCallbackHandler.HandleError(errorMessage);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}
		if (disposing)
		{
			reportExecutionItem = null;
			if (reportStreams != null)
			{
				foreach (Stream reportStream in reportStreams)
				{
					reportStream.Dispose();
				}
				reportStreams.Clear();
			}
			if (printDocument != null)
			{
				printDocument.Dispose();
				printDocument = null;
			}
		}
		IsDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~ReportRenderer()
	{
		Dispose(disposing: false);
	}
}
