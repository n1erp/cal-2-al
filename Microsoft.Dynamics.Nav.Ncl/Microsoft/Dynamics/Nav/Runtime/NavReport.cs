using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Common.Printing;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Office;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavReport : DataItemIterator
{
	internal enum ReportExecutionResult
	{
		Success,
		Fallback,
		Cancel,
		Quit,
		Failed
	}

	internal enum RunMode
	{
		Normal,
		ParametersOnly,
		Execute,
		Print,
		SaveAs,
		RunModal
	}

	private enum MergeAction
	{
		Pdf,
		Word,
		Excel,
		Preview,
		Print,
		Html,
		None
	}

	private FormResult requestPageResponse;

	private CustomLayout? selectedLayoutKind;

	private bool? isCustomLayout;

	private ILocalReportHandle effectiveLocalReportHandle;

	private readonly List<KeyValuePair<string, string>> reportParameters = new List<KeyValuePair<string, string>>();

	private bool hasRun;

	private bool showPrintStatus;

	private bool quitCalledOnReportTrigger;

	private readonly List<MetaReportLabel> reportLabels = new List<MetaReportLabel>();

	private int[] reportLabelsDataSetIndexes;

	private bool disposed;

	private bool useSystemPrinter;

	private bool preview;

	private Dictionary<string, NCLCaptionStrings> labelsCaptionStrings = new Dictionary<string, NCLCaptionStrings>();

	private Dictionary<int, NavRecord> reportRecords = new Dictionary<int, NavRecord>();

	private NavForm requestOptionsPage;

	private RunMode runMode;

	private Dictionary<string, NavValue> requestPageVariables;

	private string requestPageParameters;

	private bool success;

	private readonly Dictionary<string, NCLCaptionStrings> metaColumnOptionCaptions = new Dictionary<string, NCLCaptionStrings>();

	private static readonly NavMetric2Dimensions ReportAppDomainIsolationElevationMetric = new NavMetric2Dimensions("ReportAppDomainIsolationElevationMetric", "TenantId", "ReportId");

	private NCLMetaReport nclMetaReport;

	public bool ClientCanceled { get; set; }

	public CustomLayout LayoutFormat
	{
		get
		{
			if (!selectedLayoutKind.HasValue)
			{
				DetermineLayoutFormat();
			}
			return selectedLayoutKind.Value;
		}
	}

	public bool IsCustomLayout
	{
		get
		{
			if (!isCustomLayout.HasValue)
			{
				DetermineLayoutFormat();
			}
			return isCustomLayout.Value;
		}
	}

	public NCLMetaReport NclMetaReport
	{
		get
		{
			if (nclMetaReport == null)
			{
				nclMetaReport = Session.NCLMetadata.GetMetaReportById(ObjectId.ObjectNumber, OriginalAppGroupId);
			}
			return nclMetaReport;
		}
	}

	protected NavForm RequestOptionsPage
	{
		get
		{
			if (requestOptionsPage == null)
			{
				throw NavNCLReportRequestPageNotInitializedException.Create(CultureInfo.InvariantCulture, ReportId);
			}
			return requestOptionsPage;
		}
		set
		{
			if (requestOptionsPage != null && requestOptionsPage.SaveValues)
			{
				requestOptionsPage.ApplyReportOptions -= OnApplyReportOptions;
				requestOptionsPage.GetReportOptions -= OnGetReportOptions;
			}
			new TreeObjectReference(this, value);
			requestOptionsPage = value;
			if (requestOptionsPage.SaveValues)
			{
				requestOptionsPage.ApplyReportOptions += OnApplyReportOptions;
				requestOptionsPage.GetReportOptions += OnGetReportOptions;
			}
		}
	}

	protected internal NavRecord Rec
	{
		get
		{
			if (RequestOptionsPage != null)
			{
				return RequestOptionsPage.SourceTable;
			}
			return null;
		}
		set
		{
			RequestOptionsPage.SetSourceTable(value, clone: true);
		}
	}

	protected internal NavRecord xRec => RequestOptionsPage.OldRecord;

	public ReportFormat ClientReportFormat { get; set; }

	public bool ShowPrintStatus => showPrintStatus;

	public bool Preview => preview;

	public bool PrintOnServer
	{
		get
		{
			if (Session.ClientConnectionType != ConnectionType.Nas && Session.ClientConnectionType != ConnectionType.Background)
			{
				return Session.ClientConnectionType == ConnectionType.WebServiceClient;
			}
			return true;
		}
	}

	public string PrinterName { get; internal set; }

	public bool IsRunningAsModal { get; private set; }

	public int? DefaultPaperSourceKindRaw { get; set; }

	public int? FirstPagePaperSourceKindRaw { get; set; }

	public int? LastPagePaperSourceKindRaw { get; set; }

	public PreviewMode PreviewMode { get; set; }

	public bool PdfFontEmbedding { get; set; }

	internal DefaultLayout DefaultLayoutMode { get; set; }

	public bool UseRequestForm { get; set; }

	public bool PreviewCanPrint { get; set; }

	public override string ObjectName => NclMetaReport.Name;

	protected override DataItemIteratorType IteratorType => DataItemIteratorType.Report;

	protected bool AnyPublicFunctionsCalled { get; set; }

	public bool UseSystemPrinter => useSystemPrinter;

	public override Type HandleType => typeof(NavReportHandle);

	internal override NCLMetaApplicationObject MetaApplicationObject => NclMetaReport;

	public override NavNclType NclType => NavNclType.NavReport;

	public static void Run(int reportId)
	{
		RunReport(reportId, null, null, isRunningAsModal: false, null, null, RunMode.Normal);
	}

	public static void Run(int reportId, bool requestWindow)
	{
		RunReport(reportId, requestWindow, null, isRunningAsModal: false, null, null, RunMode.Normal);
	}

	public static void Run(int reportId, bool requestWindow, bool systemPrinter)
	{
		RunReport(reportId, requestWindow, systemPrinter, isRunningAsModal: false, null, null, RunMode.Normal);
	}

	public static void Run(int reportId, bool requestWindow, bool systemPrinter, NavRecord record)
	{
		RunReport(reportId, requestWindow, systemPrinter, isRunningAsModal: false, record, null, RunMode.Normal);
	}

	public static void Run(int reportId, bool requestWindow, bool systemPrinter, NavRecord record, IList<string> textualFilters)
	{
		RunReport(reportId, requestWindow, systemPrinter, isRunningAsModal: false, record, textualFilters, RunMode.Normal);
	}

	public static void RunModal(int reportId)
	{
		RunReport(reportId, null, null, isRunningAsModal: true, null, null, RunMode.RunModal);
	}

	public static void RunModal(int reportId, bool requestWindow)
	{
		RunReport(reportId, requestWindow, null, isRunningAsModal: true, null, null, RunMode.RunModal);
	}

	public static void RunModal(int reportId, bool requestWindow, bool systemPrinter)
	{
		RunReport(reportId, requestWindow, systemPrinter, isRunningAsModal: true, null, null, RunMode.RunModal);
	}

	public static void RunModal(int reportId, bool requestWindow, bool systemPrinter, NavRecord record)
	{
		RunReport(reportId, requestWindow, systemPrinter, isRunningAsModal: true, record, null, RunMode.RunModal);
	}

	public static bool SaveAsPdf(int reportId, string fileName)
	{
		return SaveAsPdf(DataError.ThrowError, reportId, fileName, null);
	}

	public static bool SaveAsPdf(DataError errorLevel, int reportId, string fileName)
	{
		return SaveAsPdf(errorLevel, reportId, fileName, null);
	}

	public static bool SaveAsPdf(int reportId, string fileName, NavRecord record)
	{
		return SaveAsPdf(DataError.ThrowError, reportId, fileName, record);
	}

	public static bool SaveAsPdf(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		return SaveAs(errorLevel, reportId, fileName, record, ReportFormat.PDF);
	}

	public static bool SaveAsExcel(int reportId, string fileName)
	{
		return SaveAsExcel(DataError.ThrowError, reportId, fileName, null);
	}

	public static bool SaveAsExcel(DataError errorLevel, int reportId, string fileName)
	{
		return SaveAsExcel(errorLevel, reportId, fileName, null);
	}

	public static bool SaveAsExcel(int reportId, string fileName, NavRecord record)
	{
		return SaveAsExcel(DataError.ThrowError, reportId, fileName, record);
	}

	public static bool SaveAsExcel(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		return SaveAs(errorLevel, reportId, fileName, record, ReportFormat.ExcelOpenXml);
	}

	public static bool SaveAsXml(int reportId, string fileName)
	{
		return SaveAsXml(DataError.ThrowError, reportId, fileName, null);
	}

	public static bool SaveAsXml(DataError errorLevel, int reportId, string fileName)
	{
		return SaveAsXml(errorLevel, reportId, fileName, null);
	}

	public static bool SaveAsXml(int reportId, string fileName, NavRecord record)
	{
		return SaveAsXml(DataError.ThrowError, reportId, fileName, record);
	}

	public static bool SaveAsXml(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		return SaveAsXmlTree(errorLevel, reportId, fileName, record);
	}

	public static bool SaveAsHtml(int reportId, string fileName)
	{
		return SaveAsHtml(DataError.ThrowError, reportId, fileName, null);
	}

	public static bool SaveAsHtml(DataError errorLevel, int reportId, string fileName)
	{
		return SaveAsHtml(errorLevel, reportId, fileName, null);
	}

	public static bool SaveAsHtml(int reportId, string fileName, NavRecord record)
	{
		return SaveAsHtml(DataError.ThrowError, reportId, fileName, record);
	}

	public static bool SaveAsHtml(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		return SaveAs(errorLevel, reportId, fileName, record, ReportFormat.Html);
	}

	public static bool SaveAsWord(int reportId, string fileName)
	{
		return SaveAsWord(DataError.ThrowError, reportId, fileName, null);
	}

	public static bool SaveAsWord(DataError errorLevel, int reportId, string fileName)
	{
		return SaveAsWord(errorLevel, reportId, fileName, null);
	}

	public static bool SaveAsWord(int reportId, string fileName, NavRecord record)
	{
		return SaveAsWord(DataError.ThrowError, reportId, fileName, record);
	}

	public static bool SaveAsWord(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		return SaveAs(errorLevel, reportId, fileName, record, ReportFormat.WordOpenXml);
	}

	private static bool SaveAs(DataError errorLevel, int reportId, string fileName, NavRecord record, ReportFormat format)
	{
		reportId = GetReportToRun(reportId, RunMode.SaveAs, string.Empty, record);
		NavReport navReport = null;
		IResultSetProcessor resultSetProcessor = null;
		string tempFilePath = string.Empty;
		try
		{
			navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
			if (format == ReportFormat.Html && navReport.LayoutFormat != CustomLayout.Word)
			{
				if (errorLevel == DataError.ThrowError)
				{
					throw new NavReportUnsupportedFormatException(Lang.ReportOutputFormatNotSupported, "html");
				}
				return false;
			}
			navReport.UseRequestForm = false;
			resultSetProcessor = navReport.GetSaveAsRenderer(fileName, format, ref tempFilePath);
			return navReport.SaveReportAsFormat(record, resultSetProcessor, fileName, format, tempFilePath);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			navReport?.Dispose();
			resultSetProcessor?.Dispose();
		}
	}

	private IResultSetProcessor GetSaveAsRenderer(string fileName, ReportFormat format, ref string tempFilePath)
	{
		if (LayoutFormat == CustomLayout.Word)
		{
			tempFilePath = NavFile.GetTempFileName();
			return new ReportSaveAsXmlTreeRenderer(this, tempFilePath, null);
		}
		return new ReportSaveAsRenderer(this, fileName, format);
	}

	private IResultSetProcessor GetSaveAsRenderer(NavOutStream outStream, ReportFormat format, ref string tempFilePath)
	{
		if (LayoutFormat == CustomLayout.Word)
		{
			tempFilePath = NavFile.GetTempFileName();
			return new ReportSaveAsWordRenderer(this, tempFilePath, outStream);
		}
		if (format == ReportFormat.Xml)
		{
			tempFilePath = NavFile.GetTempFileName();
			return new ReportSaveAsXmlTreeRenderer(this, tempFilePath, outStream);
		}
		return new ReportSaveAsRenderer(this, outStream, format);
	}

	public static string WordXmlPart(int reportId, bool extendedInformation = false)
	{
		MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
		return OfficeCustomXmlPart.Generate(reportMetadata, extendedInformation);
	}

	public static NavDefaultLayout DefaultLayout(int reportId)
	{
		MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
		return DefaultLayoutInternal(reportMetadata);
	}

	private static NavDefaultLayout DefaultLayoutInternal(MetaReport report)
	{
		if (report.ProcessingOnly)
		{
			return NavDefaultLayout.None;
		}
		if (report.DefaultLayout == Microsoft.Dynamics.Nav.Types.DefaultLayout.RDLC)
		{
			return NavDefaultLayout.RDLC;
		}
		return NavDefaultLayout.Word;
	}

	private static bool SaveAsXmlTree(DataError errorLevel, int reportId, string fileName, NavRecord record)
	{
		reportId = GetReportToRun(reportId, RunMode.SaveAs, string.Empty, record);
		NavReport navReport = null;
		ReportSaveAsXmlTreeRenderer reportSaveAsXmlTreeRenderer = null;
		try
		{
			navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
			reportSaveAsXmlTreeRenderer = new ReportSaveAsXmlTreeRenderer(navReport, fileName, null);
			return navReport.SaveReportAsFormat(record, reportSaveAsXmlTreeRenderer, fileName, ReportFormat.None, string.Empty);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			navReport?.Dispose();
			reportSaveAsXmlTreeRenderer?.Dispose();
		}
	}

	public static bool WordLayout(DataError errorLevel, int reportId, NavInStream inStream)
	{
		try
		{
			MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
			return GetWordLayout(reportMetadata.WordLayout, errorLevel, inStream, reportId);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public static bool RDLCLayout(DataError errorLevel, int reportId, NavInStream inStream)
	{
		try
		{
			MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
			return GetWellFormedRDLCLayout(errorLevel, inStream, reportMetadata);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	private static bool GetWellFormedRDLCLayout(DataError errorLevel, NavInStream inStream, MetaReport report)
	{
		if (string.IsNullOrEmpty(report.RDLCData))
		{
			return GetRDLCLayout(null, errorLevel, inStream, report.Id);
		}
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		using StringReader textReader = new StringReader(report.RDLCData);
		using XmlReader reader = XmlHelper.SafeXmlReader(textReader);
		xmlDocument.Load(reader);
		string s = ReportRdlPatchingHelper.SaveXmlDocument(xmlDocument);
		return GetRDLCLayout(Encoding.UTF8.GetBytes(s), errorLevel, inStream, report.Id);
	}

	public static string RunRequestPage(int reportId)
	{
		return RunRequestPage(reportId, null);
	}

	public static string RunRequestPage(int reportId, string parameters)
	{
		reportId = GetReportToRun(reportId, RunMode.ParametersOnly, string.Empty, null);
		using NavReport navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session, string.IsNullOrEmpty(parameters));
		navReport.requestPageParameters = ExtractParameters(parameters, reportId, out var isXml);
		if (!isXml)
		{
			navReport.requestOptionsPage.ParameterSet = NavText.Create(parameters);
			if (FeatureSwitches.Instance.DisableReportSettingsEdit)
			{
				navReport.requestOptionsPage.IsLatestSavedSettings = NavBoolean.Create(navReport.requestOptionsPage.ParameterSet == NavForm.LatestSavedSettings);
			}
			navReport.requestOptionsPage.ApplySourceTableViewAndSavedValues();
		}
		navReport.RunReport(true, null, runReportModal: false, null, null, RunMode.ParametersOnly);
		return navReport.GetReportParameters();
	}

	public static void Execute(int reportId, string parameters)
	{
		Execute(reportId, parameters, null);
	}

	public static void Execute(int reportId, string parameters, NavRecordRef recordRef)
	{
		reportId = GetReportToRun(reportId, RunMode.Execute, parameters, recordRef?.Record);
		using NavReport navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
		navReport.requestPageParameters = navReport.ExtractParameters(parameters);
		navReport.RunReport(false, null, runReportModal: false, recordRef?.Record, null, RunMode.Execute);
	}

	public static void Print(int reportId, string parameters)
	{
		Print(reportId, parameters, string.Empty);
	}

	public static void Print(int reportId, string parameters, string printerName)
	{
		Print(reportId, parameters, printerName, null);
	}

	public static void Print(int reportId, string parameters, string printerName, NavRecordRef recordRef)
	{
		reportId = GetReportToRun(reportId, RunMode.Print, parameters, recordRef?.Record);
		using NavReport navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
		navReport.requestPageParameters = navReport.ExtractParameters(parameters);
		navReport.PrinterName = printerName;
		navReport.RunReport(false, null, runReportModal: false, recordRef?.Record, null, RunMode.Print);
	}

	public static bool SaveAs(DataError errorLevel, int reportId, string parameters, NavReportFormat reportFormat, NavOutStream outStream)
	{
		return SaveAs(errorLevel, reportId, parameters, reportFormat, outStream, null);
	}

	public static bool SaveAs(DataError errorLevel, int reportId, string parameters, NavReportFormat reportFormat, NavOutStream outStream, NavRecordRef recordRef)
	{
		reportId = GetReportToRun(reportId, RunMode.SaveAs, parameters, recordRef?.Record);
		bool result = false;
		try
		{
			if (outStream == null || outStream.InternalStream == null)
			{
				throw NavDiagnostics.GlobalInstance.SendExceptionTag("00009S0", Category.Runtime, new NavNCLStreamAccessException(), Verbosity.Error, new DiagnosticParameter(string.Format(CultureInfo.InvariantCulture, "Null Stream in SaveAs, report id={0}, format={1}.", reportId, reportFormat), PrivacyClassification.ObjectMetadata));
			}
			using NavReport navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
			navReport.UseRequestForm = false;
			navReport.requestPageParameters = navReport.ExtractParameters(parameters);
			result = navReport.SaveAs(errorLevel, outStream, (ReportFormat)reportFormat, recordRef);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
		}
		return result;
	}

	public static int GetSubstituteReportId(int reportId)
	{
		return GetReportToRun(reportId, RunMode.Normal, string.Empty, null);
	}

	public void Run()
	{
		RunReport(null, null, runReportModal: false, null, null, RunMode.Normal);
	}

	public void RunModal()
	{
		RunReport(null, null, runReportModal: true, null, null, RunMode.RunModal);
	}

	public bool SaveAsPdf(string fileName)
	{
		return SaveAsPdf(DataError.ThrowError, fileName);
	}

	public bool SaveAsPdf(DataError errorLevel, string fileName)
	{
		return SaveAs(errorLevel, fileName, ReportFormat.PDF);
	}

	public bool SaveAsExcel(string fileName)
	{
		return SaveAsExcel(DataError.ThrowError, fileName);
	}

	public bool SaveAsExcel(DataError errorLevel, string fileName)
	{
		return SaveAs(errorLevel, fileName, ReportFormat.ExcelOpenXml);
	}

	public bool SaveAsWord(string fileName)
	{
		return SaveAsWord(DataError.ThrowError, fileName);
	}

	public bool SaveAsWord(DataError errorLevel, string fileName)
	{
		return SaveAs(errorLevel, fileName, ReportFormat.WordOpenXml);
	}

	public bool SaveAsXml(string fileName)
	{
		return SaveAsXml(DataError.ThrowError, fileName);
	}

	public bool SaveAsXml(DataError errorLevel, string fileName)
	{
		return SaveAsXmlTree(errorLevel, fileName);
	}

	public bool SaveAsHtml(string fileName)
	{
		return SaveAsHtml(DataError.ThrowError, fileName);
	}

	public bool SaveAsHtml(DataError errorLevel, string fileName)
	{
		return SaveAs(errorLevel, fileName, ReportFormat.Html);
	}

	private bool SaveAs(DataError errorLevel, string fileName, ReportFormat format)
	{
		IResultSetProcessor resultSetProcessor = null;
		string tempFilePath = string.Empty;
		if (format == ReportFormat.Html && LayoutFormat != CustomLayout.Word)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavReportUnsupportedFormatException(Lang.ReportOutputFormatNotSupported, "html");
			}
			return false;
		}
		try
		{
			resultSetProcessor = GetSaveAsRenderer(fileName, format, ref tempFilePath);
			return SaveReportAsFormat(null, resultSetProcessor, fileName, format, tempFilePath);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			resultSetProcessor?.Dispose();
		}
	}

	private bool SaveAs(DataError errorLevel, NavOutStream outStream, ReportFormat format, NavRecordRef recordRef)
	{
		IResultSetProcessor resultSetProcessor = null;
		string tempFilePath = string.Empty;
		try
		{
			runMode = RunMode.SaveAs;
			resultSetProcessor = GetSaveAsRenderer(outStream, format, ref tempFilePath);
			return SaveReportAsFormat(recordRef?.Record, resultSetProcessor, null, format, tempFilePath);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			resultSetProcessor?.Dispose();
		}
	}

	public string WordXmlPart(bool extendedInformation = false)
	{
		return OfficeCustomXmlPart.Generate(Metadata, extendedInformation);
	}

	public NavDefaultLayout DefaultLayout()
	{
		return DefaultLayoutInternal(Metadata);
	}

	private bool SaveAsXmlTree(DataError errorLevel, string fileName)
	{
		ReportSaveAsXmlTreeRenderer reportSaveAsXmlTreeRenderer = null;
		try
		{
			reportSaveAsXmlTreeRenderer = new ReportSaveAsXmlTreeRenderer(this, fileName, null);
			return SaveReportAsFormat(null, reportSaveAsXmlTreeRenderer, fileName, ReportFormat.None, string.Empty);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			reportSaveAsXmlTreeRenderer?.Dispose();
		}
	}

	public bool WordLayout(DataError errorLevel, NavInStream inStream)
	{
		try
		{
			return GetWordLayout(Metadata.WordLayout, errorLevel, inStream, ObjectId.ObjectNumber);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public bool RDLCLayout(DataError errorLevel, NavInStream inStream)
	{
		try
		{
			return GetWellFormedRDLCLayout(errorLevel, inStream, Metadata);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public string RunRequestPage()
	{
		return RunRequestPage(null);
	}

	public string RunRequestPage(string parameters)
	{
		requestPageParameters = ExtractParameters(parameters);
		RunReport(true, null, runReportModal: false, null, null, RunMode.ParametersOnly);
		return GetReportParameters();
	}

	public void Execute(string parameters)
	{
		Execute(parameters, null);
	}

	public void Execute(string parameters, NavRecordRef recordRef)
	{
		requestPageParameters = ExtractParameters(parameters);
		RunReport(false, null, runReportModal: false, recordRef?.Record, null, RunMode.Execute);
	}

	public void Print(string parameters)
	{
		Print(parameters, string.Empty);
	}

	public void Print(string parameters, string printerName)
	{
		Print(parameters, printerName, null);
	}

	public void Print(string parameters, string printerName, NavRecordRef recordRef)
	{
		requestPageParameters = ExtractParameters(parameters);
		PrinterName = printerName;
		RunReport(false, null, runReportModal: false, recordRef?.Record, null, RunMode.Print);
	}

	public bool SaveAs(DataError errorLevel, string parameters, NavReportFormat reportFormat, NavOutStream outStream)
	{
		return SaveAs(errorLevel, parameters, reportFormat, outStream, null);
	}

	public bool SaveAs(DataError errorLevel, string parameters, NavReportFormat reportFormat, NavOutStream outStream, NavRecordRef recordRef)
	{
		bool result = false;
		try
		{
			UseRequestForm = false;
			requestPageParameters = ExtractParameters(parameters);
			return SaveAs(errorLevel, outStream, (ReportFormat)reportFormat, recordRef);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return result;
		}
	}

	private static bool GetWordLayout(byte[] layoutBytes, DataError errorLevel, NavInStream inStream, int reportId)
	{
		if (layoutBytes == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw NavNCLReportNoLayoutException.Create(CultureInfo.CurrentCulture, Lang.NoWordLayout, reportId);
			}
			return false;
		}
		MemoryStream memoryStream = new MemoryStream(layoutBytes);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		inStream.ALAssign(new NavInStream(inStream, new NavStreamProvider(memoryStream)));
		return true;
	}

	private static bool GetRDLCLayout(byte[] layoutBytes, DataError errorLevel, NavInStream inStream, int reportId)
	{
		if (layoutBytes == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw NavNCLReportNoLayoutException.Create(CultureInfo.CurrentCulture, Lang.NoRDLCLayout, reportId);
			}
			return false;
		}
		MemoryStream memoryStream = new MemoryStream(layoutBytes);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		inStream.ALAssign(new NavInStream(inStream, new NavStreamProvider(memoryStream), TextEncoding.UTF8));
		return true;
	}

	protected NavReport(ITreeObject parent, int objectId)
		: base(parent, new ApplicationObjectId(ObjectType.Report, objectId))
	{
		PreviewCanPrint = true;
		PrinterName = string.Empty;
		NavCurrentThread.Session.Company.RegisterReport(this);
	}

	protected NavReport(ITreeObject parent, int objectId, NCLMetaReport metaReport)
		: base(parent, new ApplicationObjectId(ObjectType.Report, objectId))
	{
		nclMetaReport = metaReport;
		PreviewCanPrint = true;
		PrinterName = string.Empty;
		NavCurrentThread.Session.Company.RegisterReport(this);
	}

	private static void RunReport(int reportId, bool? requestWindow, bool? systemPrinter, bool isRunningAsModal, NavRecord record, IList<string> textualFilters, RunMode runMode)
	{
		reportId = GetReportToRun(reportId, runMode, string.Empty, record);
		using NavReport navReport = NavGlobal.NCLMetadata.GetMetaReportById(reportId).CreateObjectInstance(NavCurrentThread.Session);
		navReport.RunReport(requestWindow, systemPrinter, isRunningAsModal, record, textualFilters, runMode);
	}

	private void RunReport(bool? requestWindow, bool? systemPrinter, bool runReportModal, NavRecord record, IList<string> reportTextualFilters, RunMode mode)
	{
		bool flag = false;
		string text = string.Empty;
		try
		{
			if (requestWindow.HasValue)
			{
				UseRequestForm = requestWindow.Value;
			}
			if (UseRequestForm && (!Session.TestExecution.InTest || Session.TestExecution.CurrentTransactionModel != TestTransactionModel.AutoRollback))
			{
				NavCurrentThread.Session.BeginTransactionWorld();
				flag = true;
			}
			if (systemPrinter.HasValue)
			{
				useSystemPrinter = systemPrinter.Value;
			}
			runMode = mode;
			IsRunningAsModal = runReportModal;
			SetTextualFilters(reportTextualFilters);
			success = false;
			if (mode != RunMode.ParametersOnly && PrintOnServer)
			{
				UseRequestForm = false;
			}
			if (Metadata.ProcessingOnly)
			{
				ResultSetProcessor = DataItemIterator.NullResultSetProcessor;
			}
			else if (LayoutFormat == CustomLayout.Word)
			{
				text = NavFile.GetTempFileName();
				ResultSetProcessor = new ReportSaveAsXmlTreeRenderer(this, text, null);
			}
			else if (PrintOnServer)
			{
				showPrintStatus = false;
				ResultSetProcessor = new ReportServerResultSetProcessor(this);
			}
			else if (!(ResultSetProcessor is ReportClientCallback))
			{
				ResultSetProcessor = new ReportClientCallback(this);
			}
			ReportExecutionResult reportExecutionResult = RunReportInternal(record, displayResult: true);
			success = reportExecutionResult == ReportExecutionResult.Success;
			if (requestPageResponse != FormResult.Schedule && LayoutFormat == CustomLayout.Word && success)
			{
				GenerateWordDocument(text);
			}
		}
		finally
		{
			if (ResultSetProcessor != null)
			{
				ResultSetProcessor.Dispose();
			}
			if (!string.IsNullOrEmpty(text) && File.Exists(text))
			{
				File.Delete(text);
			}
			if (flag)
			{
				NavCurrentThread.Session.EndTransactionWorld();
			}
		}
	}

	private void GenerateWordDocument(string tempFilePath)
	{
		string fileName = string.Empty;
		bool flag = DocumentServiceRecordHelper.Enabled() && (ClientReportFormat != ReportFormat.PDF || ClientReportFormat != ReportFormat.PdfPreview);
		MergeAction value;
		if (flag)
		{
			value = MergeAction.Word;
			fileName = NavFile.GetTempFileName();
		}
		else
		{
			switch (ClientReportFormat)
			{
			case ReportFormat.None:
				if (preview)
				{
					value = MergeAction.Preview;
					break;
				}
				value = MergeAction.Print;
				fileName = PrinterName;
				break;
			case ReportFormat.PDF:
			case ReportFormat.PdfPreview:
			case ReportFormat.PdfPrint:
				value = MergeAction.Pdf;
				break;
			case ReportFormat.WordOpenXml:
				value = MergeAction.Word;
				break;
			case ReportFormat.ExcelOpenXml:
				value = MergeAction.Excel;
				break;
			case ReportFormat.Html:
				value = MergeAction.Html;
				break;
			default:
				throw new NavNotSupportedException();
			}
		}
		using (FileStream stream = new FileStream(tempFilePath, FileMode.Open))
		{
			Session.SystemCodeunitFactory.ReportingTriggers.InvokeMergeDocument(NavOption.Create(NCLOptionMetadata.Default, 0), ReportId, NavOption.Create(NCLOptionMetadata.Default, (int)value), new NavInStream(this, new NavStreamProvider(stream)), fileName);
		}
		if (flag)
		{
			SaveDocumentToService(fileName);
		}
	}

	private void SaveDocumentToService(string fileName)
	{
		using FileStream dataStream = new FileStream(fileName, FileMode.Open);
		DocumentServiceRecordHelper documentServiceRecordHelper = new DocumentServiceRecordHelper();
		string text = documentServiceRecordHelper.SaveDocumentStream(dataStream, Path.GetRandomFileName(), GetFileName);
		if (!string.IsNullOrEmpty(text))
		{
			ALSystemOperatingSystem.ALHyperlink(text, Guid.NewGuid());
		}
	}

	private string GetFileName(string fileName)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavReportFileNamePostFixWord, Path.ChangeExtension(Path.GetFileName(fileName), "docx"));
	}

	private void DetermineLayoutFormat()
	{
		if (selectedLayoutKind.HasValue && isCustomLayout.HasValue)
		{
			return;
		}
		selectedLayoutKind = CustomLayout.None;
		isCustomLayout = false;
		if (runMode != RunMode.ParametersOnly && !Metadata.ProcessingOnly)
		{
			selectedLayoutKind = Session.SystemCodeunitFactory.ReportingTriggers.InvokeHasCustomLayout(NavOption.Create(NCLOptionMetadata.Default, 0), ReportId);
			isCustomLayout = selectedLayoutKind != CustomLayout.None;
			if (!IsCustomLayout)
			{
				selectedLayoutKind = ((DefaultLayoutMode != Microsoft.Dynamics.Nav.Types.DefaultLayout.Word) ? CustomLayout.Rdlc : CustomLayout.Word);
			}
		}
	}

	private ReportExecutionResult RunReportInternal(NavRecord record, bool displayResult)
	{
		return ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			if (displayResult && (UseRequestForm || !Metadata.ProcessingOnly) && NavCurrentThread.Session.TestExecution.TestHandleReport(this))
			{
				return ReportExecutionResult.Failed;
			}
			if (hasRun)
			{
				throw new NavNCLObjectHasAlreadyBeenRunException(CultureInfo.CurrentCulture, ObjectId);
			}
			hasRun = true;
			if (quitCalledOnReportTrigger)
			{
				return ReportExecutionResult.Quit;
			}
			ReportExecutionResult result = ReportExecutionResult.Failed;
			try
			{
				PushLocalLanguage();
				using (new NavOpenDialogTracking())
				{
					bool flag = !UseRequestForm && (runMode == RunMode.Execute || runMode == RunMode.Print || runMode == RunMode.SaveAs);
					MetaDataItems.Clear();
					if (runMode == RunMode.ParametersOnly && !string.IsNullOrEmpty(requestPageParameters))
					{
						Dictionary<string, NavValue> variables = RequestPageParameterHelper.ConvertXmlToVariables(requestPageParameters, RequestOptionsPage);
						RequestOptionsPage.ApplyLatestValues(variables);
						ApplyXmlViews(requestPageParameters, record);
					}
					else if (!flag && record != null)
					{
						SetTableView(record);
					}
					ApplySetTableViewForAllDataItems();
					if ((UseRequestForm & displayResult) && RequestOptionsPage != null)
					{
						try
						{
							requestPageResponse = InternalRunRequestPage();
						}
						catch (NavNCLControlException ex) when (ex.ControlStatement == NavControlStatement.Quit)
						{
							return ReportExecutionResult.Quit;
						}
						if (requestPageResponse == FormResult.Cancel)
						{
							return ReportExecutionResult.Cancel;
						}
						if (requestPageResponse == FormResult.Schedule)
						{
							if (runMode != RunMode.Normal)
							{
								throw NavTestActionNotFoundException.Create(CultureInfo.CurrentCulture, requestPageResponse);
							}
							success = true;
							return (!Session.SystemCodeunitFactory.ReportingTriggers.InvokeScheduleReport(ReportId, GetReportParameters())) ? ReportExecutionResult.Cancel : ReportExecutionResult.Success;
						}
						if (runMode == RunMode.ParametersOnly && requestPageResponse == FormResult.OK)
						{
							return ReportExecutionResult.Success;
						}
					}
					else if (flag)
					{
						Dictionary<string, NavValue> variables2 = RequestPageParameterHelper.ConvertXmlToVariables(requestPageParameters, RequestOptionsPage);
						RequestOptionsPage.ApplyLatestValues(variables2);
						ApplyXmlViews(requestPageParameters, record);
						RequestOptionsPage.OpenForm();
						RequestOptionsPage.CloseForm(FormResult.Parameters);
						if (runMode == RunMode.Execute && !Metadata.ProcessingOnly)
						{
							preview = true;
						}
					}
					if (ClientReportFormat == ReportFormat.None)
					{
						NavCurrentThread.Session.TestExecution.TestHandleReportData(this, displayResult && !Metadata.ProcessingOnly);
					}
					using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("0000051", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Running report processing for report ID {0}", ObjectId)), MonitorType.ALStatementsExecuted, Category.Runtime))
					{
						bool flag2 = !FeatureSwitches.Instance.DisableAppDomainMonitoring;
						if (flag2 && !AppDomain.MonitoringIsEnabled)
						{
							AppDomain.MonitoringIsEnabled = true;
						}
						long num = (flag2 ? AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize : 0);
						long num2 = (flag2 ? AppDomain.CurrentDomain.MonitoringSurvivedMemorySize : 0);
						TimeSpan timeSpan = (flag2 ? AppDomain.CurrentDomain.MonitoringTotalProcessorTime : TimeSpan.Zero);
						SendFilterTrace();
						bool commit = false;
						NavCurrentThread.Session.BeginTransaction();
						ALDatabase.ALCurrentTransactionType = Metadata.TransactionType;
						GetReportRecords();
						try
						{
							if (!useSystemPrinter && (runMode != RunMode.Print || string.IsNullOrEmpty(PrinterName)))
							{
								PrinterName = Session.SystemCodeunitFactory.ReportingTriggers.InvokeGetPrinterName(ObjectId.ObjectNumber);
							}
							if (TextualFilters != null && TextualFilters.Count > 0)
							{
								preview = displayResult;
								ProcessTextualFilters();
							}
							StoreSetTableViewForAllDataItems();
							ExecuteDataItemIterator();
							result = ReportExecutionResult.Success;
							commit = true;
						}
						catch (NavControlException ex2)
						{
							switch (ex2.ControlStatement)
							{
							case NavControlStatement.Quit:
								result = ReportExecutionResult.Quit;
								break;
							case NavControlStatement.Cancel:
								ViewerMustBeClosedOnQuit = false;
								result = ReportExecutionResult.Cancel;
								break;
							case NavControlStatement.ClientCancel:
								ClientCanceled = true;
								ViewerMustBeClosedOnQuit = false;
								result = ReportExecutionResult.Cancel;
								break;
							}
						}
						finally
						{
							NavCurrentThread.Session.EndTransaction(commit);
							if ((!ClientCanceled && ViewerMustBeClosedOnQuit) & displayResult)
							{
								ResultSetProcessor.Abort();
							}
							if (IsCustomLayout && LayoutFormat == CustomLayout.Rdlc)
							{
								ReportAppDomainIsolationElevationMetric.LogValue(1L, NavCurrentThread.Session.Tenant.Id, ReportId.ToString(CultureInfo.InvariantCulture));
							}
							long num3 = (flag2 ? AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize : 0);
							long num4 = (flag2 ? AppDomain.CurrentDomain.MonitoringSurvivedMemorySize : 0);
							TimeSpan timeSpan2 = (flag2 ? AppDomain.CurrentDomain.MonitoringTotalProcessorTime : TimeSpan.Zero);
							NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00009AG", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Report={ObjectId} running in appDomain {AppDomain.CurrentDomain.FriendlyName}, DeltaMemoryAllocated={num3 - num}, DeltaMemorySurvived={num4 - num2}, DeltaProcessorTime={timeSpan2 - timeSpan}"));
						}
					}
				}
			}
			finally
			{
				PopLocalLanguage();
			}
			return result;
		});
	}

	private void SendFilterTrace()
	{
		StringBuilder filterFields = new StringBuilder();
		DataItems.ForEach((DataItem item) =>
		{
			foreach (NavFilterGroup item in from filterGroup in item.Record.GetCurrentFilters()
				where filterGroup.Filters != null
				select filterGroup)
			{
				filterFields.Append(string.Format(CultureInfo.InvariantCulture, "Record type {1} - FilterGroup {0} : Field ", item.FilterGroupNo, item.Record.TableID));
				NavFilter[] filters = item.Filters;
				foreach (NavFilter navFilter in filters)
				{
					filterFields.Append(navFilter.FilterField.ToString(CultureInfo.InvariantCulture)).Append(", ");
				}
				filterFields.Length -= 2;
			}
		});
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000016D", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Report {0} called with these filters fields set: {1}", ReportId, filterFields.ToString());
	}

	private void ApplyRequestPageParameters(string parameters)
	{
		Dictionary<string, NavValue> variables = RequestPageParameterHelper.ConvertXmlToVariables(parameters, RequestOptionsPage);
		RequestOptionsPage.ApplyLatestValues(variables);
		ApplyXmlViews(parameters, Rec);
	}

	private void ApplyXmlViews(string parameters, NavRecord record)
	{
		RequestPageParameterHelper.ApplyXmlViews(parameters, ((string name, string view) args) =>
		{
			DataItems.Where((DataItem dataItem) => dataItem.MetaData.DataItemVarName == args.name).FirstOrDefault()?.Record.ALSetView(args.view);
		});
		if (record != null)
		{
			foreach (DataItem dataItem in DataItems)
			{
				if (record.TableID == dataItem.Record.TableID)
				{
					dataItem.Record.RecordImplementation.CopyFiltersSortingMarkList(record);
					break;
				}
			}
		}
		RequestOptionsPage.SetTableViewUsed = true;
		ApplyChangesOnDataItems();
	}

	private void ApplyChangesOnDataItems()
	{
		StoreSetTableViewForAllDataItems();
		ApplyDataItemTableViewAndRequestFormFilters();
	}

	private void InitializeReportLabels()
	{
		if (Metadata.Labels == null)
		{
			return;
		}
		for (int i = 0; i < Metadata.Labels.Count; i++)
		{
			MakeLabelCaptionString(Metadata.Labels[i]);
		}
		List<int> list = new List<int>();
		for (int j = 0; j < Metadata.Labels.Count; j++)
		{
			MetaReportLabel label = Metadata.Labels[j];
			if (!label.PerRow)
			{
				string text = EvaluateLabel(label);
				reportParameters.Add(new KeyValuePair<string, string>(label.Name, string.IsNullOrEmpty(text) ? label.Name : text));
				continue;
			}
			if (label.ParentDataItem == -1)
			{
				reportLabels.Add(label);
				list.Add(label.DataSetIndex);
				continue;
			}
			DataItems.Exists((DataItem dataItem) =>
			{
				if (label.ParentDataItem == dataItem.MetaData.Id)
				{
					dataItem.Labels.Add(label);
					return true;
				}
				return false;
			});
		}
		reportLabelsDataSetIndexes = list.ToArray();
		DataItems.ForEach((DataItem dataItem) =>
		{
			int count = dataItem.Labels.Count;
			int[] array = new int[count];
			for (int k = 0; k < count; k++)
			{
				array[k] = dataItem.Labels[k].DataSetIndex;
			}
			dataItem.LabelDataSetIndexes = array;
		});
	}

	private void AddDefaultParameters()
	{
		if (!IsSaveAsXmlTreeProcessor)
		{
			reportParameters.AddRange(ReportRdlPatchingHelper.CreateDefaultParameters(ALDatabase.ALUserID, NavDateTime.Now.ClientLocalValue.ToString("o", CultureInfo.InvariantCulture)));
		}
	}

	private void MakeLabelCaptionString(MetaReportLabel label)
	{
		if (!string.IsNullOrEmpty(label.CaptionML))
		{
			NCLCaptionStrings value = NCLCaptionStrings.CreateNCLCaptionStrings(label.CaptionML, label.Name);
			labelsCaptionStrings.Add(label.Name, value);
		}
	}

	private void GetReportRecords()
	{
		DataItems.ForEach((DataItem dataItem) =>
		{
			if (!reportRecords.ContainsKey(dataItem.Record.TableID))
			{
				reportRecords.Add(dataItem.Record.TableID, dataItem.Record);
			}
		});
	}

	private string GetReportParameters(bool requireSuccess = true)
	{
		if (requireSuccess && !success)
		{
			return string.Empty;
		}
		IEnumerable<(string, string)> enumerable = DataItems.Select((DataItem dataItem) => (DataItemVarName: dataItem.MetaData.DataItemVarName, dataItem.Record.ALGetView(useCaptions: false)));
		return RequestPageParameterHelper.GetReportParameterXml(ObjectName, ObjectId.ObjectNumber, requestPageVariables, enumerable);
	}

	private FormResult InternalRunRequestPage()
	{
		if (SetTableViewUsed)
		{
			RequestOptionsPage.SetTableViewUsed = true;
		}
		RequestOptionsPage.TableViewIsSet = TableViewIsSet;
		requestPageVariables = null;
		SubscribeOnSaveValues();
		RequestOptionsPage.ReportRequestOptions.ParametersOnly = runMode == RunMode.ParametersOnly;
		RequestOptionsPage.ReportRequestOptions.ReportLayoutFormat = LayoutFormat;
		RequestOptionsPage.ReportRequestOptions.ShowScheduleButton = runMode == RunMode.Normal;
		FormResult formResult = RequestOptionsPage.RunModal();
		bool flag = false;
		switch (formResult)
		{
		case FormResult.Preview:
		{
			if (NavCurrentThread.Session.TestExecution.ReportOutputFormat == FormResult.Xml)
			{
				selectedLayoutKind = CustomLayout.Rdlc;
				ClientReportFormat = ReportFormat.Xml;
				flag = true;
				break;
			}
			preview = true;
			ConnectionType clientConnectionType = NavCurrentThread.Session.ClientConnectionType;
			if ((uint)(clientConnectionType - 116) <= 3u)
			{
				ClientReportFormat = ReportFormat.PdfPreview;
				PdfFontEmbedding = true;
			}
			break;
		}
		case FormResult.Pdf:
			ClientReportFormat = ReportFormat.PDF;
			break;
		case FormResult.Word:
			ClientReportFormat = ReportFormat.WordOpenXml;
			break;
		case FormResult.Excel:
			ClientReportFormat = ReportFormat.ExcelOpenXml;
			break;
		case FormResult.PreviewPrint:
			ClientReportFormat = ReportFormat.PdfPrint;
			break;
		}
		if (flag)
		{
			ResultSetProcessor = new ReportSaveAsXmlRenderer(NavCurrentThread.Session.TestExecution.ReportParameterOutputFileName, GetParameters, NavCurrentThread.Session.TestExecution.ReportOutputFileName, NavDataSetBuilder.CreateNavDataSet(Metadata));
			NavCurrentThread.Session.TestExecution.ReportOutputFileName = null;
			NavCurrentThread.Session.TestExecution.ReportParameterOutputFileName = null;
			NavCurrentThread.Session.TestExecution.ReportOutputFormat = FormResult.None;
		}
		else if (ClientReportFormat != ReportFormat.None && NavCurrentThread.Session.TestExecution != null && NavCurrentThread.Session.TestExecution.ReportOutputFileName != null)
		{
			ResultSetProcessor = new ReportSaveAsRenderer(this, NavCurrentThread.Session.TestExecution.ReportOutputFileName, ClientReportFormat);
			NavCurrentThread.Session.TestExecution.ReportOutputFileName = null;
		}
		else if ((ClientReportFormat == ReportFormat.ExcelOpenXml || ClientReportFormat == ReportFormat.WordOpenXml) && DocumentServiceRecordHelper.Enabled() && LayoutFormat != CustomLayout.Word)
		{
			ResultSetProcessor = new ReportSaveToDocumentServiceAsRenderer(this, ClientReportFormat);
		}
		return formResult;
	}

	protected void SubscribeOnSaveValues()
	{
		RequestOptionsPage.OnSaveValues += (object sender, Dictionary<string, NavValue> values) =>
		{
			requestPageVariables = values;
		};
	}

	private bool SaveReportAsFormat(NavRecord record, IResultSetProcessor resultSetProcessor, string fileName, ReportFormat format, string tempFilePath)
	{
		ReportExecutionResult reportExecutionResult = ReportExecutionResult.Failed;
		ResultSetProcessor = resultSetProcessor;
		try
		{
			if (Metadata.ProcessingOnly)
			{
				throw NavNCLReportProcessingOnlyException.Create(CultureInfo.CurrentCulture, ObjectId.ObjectNumber);
			}
			reportExecutionResult = RunReportInternal(record, displayResult: false);
			if (!string.IsNullOrEmpty(tempFilePath) && reportExecutionResult == ReportExecutionResult.Success)
			{
				MergeAction mergeAction = MergeAction.None;
				switch (format)
				{
				case ReportFormat.PDF:
				case ReportFormat.PdfPreview:
				case ReportFormat.PdfPrint:
					mergeAction = MergeAction.Pdf;
					break;
				case ReportFormat.WordOpenXml:
					mergeAction = MergeAction.Word;
					break;
				case ReportFormat.Html:
					mergeAction = MergeAction.Html;
					break;
				default:
					throw new NavNotSupportedException();
				case ReportFormat.Xml:
					break;
				}
				if (mergeAction != MergeAction.None)
				{
					using FileStream stream = new FileStream(tempFilePath, FileMode.Open);
					ReportSaveAsWordRenderer reportSaveAsWordRenderer = ResultSetProcessor as ReportSaveAsWordRenderer;
					if (reportSaveAsWordRenderer != null)
					{
						fileName = NavFile.GetTempFileName();
					}
					Session.SystemCodeunitFactory.ReportingTriggers.InvokeMergeDocument(NavOption.Create(NCLOptionMetadata.Default, 0), ReportId, NavOption.Create(NCLOptionMetadata.Default, (int)mergeAction), new NavInStream(this, new NavStreamProvider(stream)), fileName);
					if (reportSaveAsWordRenderer != null)
					{
						try
						{
							using FileStream source = File.Open(fileName, FileMode.Open);
							NCLStreamHelper.CopyStream(source, reportSaveAsWordRenderer.WordOutStream.InternalStream);
						}
						catch (Exception ex)
						{
							NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
							throw;
						}
						finally
						{
							if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
							{
								File.Delete(fileName);
							}
						}
					}
				}
			}
		}
		finally
		{
			if (reportExecutionResult != ReportExecutionResult.Success)
			{
				if (IsSaveAsXmlTreeProcessor)
				{
					ResultSetProcessor.CleanupOnError();
				}
				else if (fileName != null)
				{
					NavFile.ALErase(DataError.TrapError, fileName);
				}
			}
			if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
			{
				File.Delete(tempFilePath);
			}
		}
		return reportExecutionResult == ReportExecutionResult.Success;
	}

	public virtual string EvaluateCaptionClass(int controlId)
	{
		return string.Empty;
	}

	protected abstract NavValue EvaluateSourceExpression(string fieldName);

	protected override void EvaluateDataColumn(NavRecord record, MetaDataItemColumn metaColumn)
	{
		try
		{
			NavValue navValue = ((metaColumn.FieldType != NavType.Variant) ? EvaluateSourceExpression(metaColumn.Name) : EvaluateSourceExpression(metaColumn.Name).InnerValue);
			if (navValue == null)
			{
				throw new NavNCLArgumentException(Lang.ReportNullSourceExprException);
			}
			SetResultSetProcessorValue(metaColumn, navValue);
		}
		catch (NavException ex)
		{
			throw NavNCLReportSourceExpressionException.CreateMessage(metaColumn.Name, ex.Message);
		}
	}

	private void SetResultSetProcessorValue(MetaDataItemColumn metaColumn, NavValue value)
	{
		object obj = value.ClientObject;
		if (obj != null)
		{
			switch (value.NavType)
			{
			case NavType.Decimal:
				if (metaColumn.DecimalFormatter != null)
				{
					ResultSetProcessor.SetValue(metaColumn, obj, EvaluateSourceExpression(metaColumn.DecimalFormatter).ToString());
					return;
				}
				break;
			case NavType.Duration:
				ResultSetProcessor.SetValue(metaColumn, value.ToString());
				return;
			case NavType.Option:
				ResultSetProcessor.SetValue(metaColumn, GetOptionValue((NavOption)value, metaColumn));
				return;
			case NavType.RecordID:
				ResultSetProcessor.SetValue(metaColumn, ((NavRecordId)value).ToString());
				return;
			case NavType.Date:
				ResultSetProcessor.SetValue(metaColumn, value.IsZeroOrEmpty ? DBNull.Value : obj);
				return;
			case NavType.Time:
			{
				if (value.IsZeroOrEmpty)
				{
					obj = DBNull.Value;
					break;
				}
				DateTime dateTime = (DateTime)obj;
				obj = new DateTime(1899, 12, 30, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Local);
				break;
			}
			case NavType.DateTime:
				ResultSetProcessor.SetValue(metaColumn, value.IsZeroOrEmpty ? DBNull.Value : ((object)((NavDateTime)value).ClientLocalValue));
				return;
			case NavType.BLOB:
				ResultSetProcessor.SetValue(metaColumn, ((NavBLOB)value).IsInMemory ? obj : DBNull.Value);
				return;
			case NavType.Media:
				ResultSetProcessor.SetValue(metaColumn, MediaContent(value.ToGuid()));
				return;
			case NavType.MediaSet:
			{
				IList<Guid> containedMedia = ((NavMediaSet)value).GetContainedMedia();
				object value2 = DBNull.Value;
				if (containedMedia.Count > 0)
				{
					value2 = MediaContent(containedMedia[0]);
				}
				ResultSetProcessor.SetValue(metaColumn, value2);
				return;
			}
			}
			ResultSetProcessor.SetValue(metaColumn, obj);
		}
		else
		{
			ResultSetProcessor.SetValue(metaColumn, DBNull.Value);
		}
	}

	private static object MediaContent(Guid mediaguid)
	{
		bool allowCache = false;
		string mimeType = string.Empty;
		byte[] mediaContent = NavMediaHelper.GetMediaContent(mediaguid, ref allowCache, ref mimeType);
		if (mediaContent != null && mediaContent.Length != 0)
		{
			return mediaContent;
		}
		return DBNull.Value;
	}

	private string GetOptionValue(NavOption option, MetaDataItemColumn metaColumn)
	{
		NCLCaptionStrings value = null;
		string valueOrDefault;
		if (metaColumnOptionCaptions.TryGetValue(metaColumn.Name, out value) && !string.IsNullOrEmpty(valueOrDefault = value.GetValueOrDefault()))
		{
			return NCLOptionMetadata.Create(valueOrDefault).GetCaptionFromIndex(option.Value);
		}
		if (metaColumn.FieldNo != -1 && option.NavOptionMetadata.HasCaption)
		{
			return option.NavOptionMetadata.GetCaptionFromIndex(option.Value);
		}
		if (!string.IsNullOrEmpty(metaColumn.OptionString))
		{
			return NCLOptionMetadata.Create(metaColumn.OptionString).GetOptionFromIndex(option.Value);
		}
		return option.NavOptionMetadata.GetOptionFromIndex(option.Value);
	}

	protected override void Add(DataItem dataItem)
	{
		dataItem.MetaData = Metadata.DataItems[DataItems.Count];
		dataItem.PrintOnlyIfDetail = Metadata.DataItems[DataItems.Count].PrintOnlyIfDetail;
		dataItem.SetAutoCalcFields();
		foreach (MetaDataItemColumn item in dataItem.MetaData.DataItemColumns.Where((MetaDataItemColumn metaDataItemColumn) => metaDataItemColumn.OptionCaptionML.Count > 0))
		{
			metaColumnOptionCaptions.Add(item.Name, new NCLCaptionStrings(item.OptionCaptionML.ToArray(), item.OptionCaptionML.First().Value));
		}
		base.Add(dataItem);
	}

	protected override void AppendDataItemSpecificData(DataItem dataItem)
	{
		if (RecalculateSpecificData || !dataItem.LabelsProcessed)
		{
			dataItem.LabelsProcessed = true;
			dataItem.Labels.ForEach(ProcessLabel);
		}
		else if (dataItem.LabelDataSetIndexes != null)
		{
			ResultSetProcessor.InsertRepeatedData(dataItem.LabelDataSetIndexes);
		}
	}

	protected override void AppendSpecificData()
	{
		if (RecalculateSpecificData)
		{
			reportLabels.ForEach(ProcessLabel);
		}
		else if (reportLabelsDataSetIndexes != null)
		{
			ResultSetProcessor.InsertRepeatedData(reportLabelsDataSetIndexes);
		}
	}

	private void ProcessLabel(MetaReportLabel label)
	{
		string value = EvaluateLabel(label);
		ResultSetProcessor.SetValue(label.DataSetIndex, value);
	}

	private string EvaluateLabel(MetaReportLabel label)
	{
		NavText dynamicCaption = GetDynamicCaption(label.Id);
		if (dynamicCaption != NavText.Empty)
		{
			return dynamicCaption.Value;
		}
		if (!string.IsNullOrEmpty(label.CaptionML))
		{
			NCLCaptionStrings value = null;
			if (labelsCaptionStrings.TryGetValue(label.Name, out value))
			{
				return value.GetValueOrDefault(Language);
			}
		}
		if (label.FieldNo.HasValue && label.TableNo.HasValue)
		{
			NavRecord value2 = null;
			if (reportRecords.TryGetValue(label.TableNo.Value, out value2))
			{
				return value2.ALFieldCaption(label.FieldNo.Value);
			}
		}
		return string.Empty;
	}

	protected void Break()
	{
		throw new NavControlException(NavControlStatement.Break);
	}

	protected void Skip()
	{
		throw new NavControlException(NavControlStatement.Skip);
	}

	protected void Quit()
	{
		throw new NavControlException(NavControlStatement.Quit);
	}

	protected override void BeginInitialization()
	{
		NavCurrentThread.Session.VerifyExecutePermission(this);
		Metadata = Tree.Session.MetadataProvider.GetReportMetadata(NclMetaReport);
		DefaultPaperSourceKindRaw = Metadata.DefaultPaperSourceKindRaw;
		FirstPagePaperSourceKindRaw = Metadata.FirstPagePaperSourceKindRaw;
		LastPagePaperSourceKindRaw = Metadata.LastPagePaperSourceKindRaw;
		PreviewMode = Metadata.PreviewMode;
		DefaultLayoutMode = Metadata.DefaultLayout;
		PdfFontEmbedding = Metadata.PdfFontEmbedding;
		base.BeginInitialization();
	}

	protected override void EndInitialization()
	{
		showPrintStatus = Metadata.ShowPrintStatus;
		UseRequestForm = Metadata.UseRequestForm;
		useSystemPrinter = Metadata.UseSystemPrinter;
		base.EndInitialization();
		try
		{
			PushLocalLanguage();
			DataItemIterator.ExecuteTrigger(OnInitReport);
		}
		catch (NavControlException ex)
		{
			quitCalledOnReportTrigger = ex.ControlStatement == NavControlStatement.Quit;
		}
		finally
		{
			PopLocalLanguage();
		}
	}

	protected virtual void OnInitReport()
	{
	}

	protected virtual void OnPreReport()
	{
	}

	protected virtual void OnPostReport()
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			NavSession navSession = Session;
			if (navSession.IsCompanyOpen)
			{
				navSession.Company.UnregisterReport(this);
			}
		}
		base.Dispose(disposing);
	}

	private void OnGetReportOptions(object sender, GetReportOptionsEventArgs reportOptionsEventArgs)
	{
		reportOptionsEventArgs.ReportOptions = GetReportParameters(requireSuccess: false);
		reportOptionsEventArgs.ObjectType = ObjectType.Report;
	}

	private void OnApplyReportOptions(object sender, EventArgs<string> parameters)
	{
		NavObjectOptions navObjectOptions = new NavObjectOptions(ObjectType.Report, ObjectId.ObjectNumber, parameters.Data);
		ApplyRequestPageParameters(navObjectOptions.Load());
	}

	private string ExtractParameters(string parameters)
	{
		bool isXml;
		return ExtractParameters(parameters, ReportId, out isXml);
	}

	private static string ExtractParameters(string parameters, int reportId, out bool isXml)
	{
		isXml = string.IsNullOrEmpty(parameters) || parameters.Substring(0, Math.Min(10, parameters.Length)).Contains("<?xml");
		if (!isXml)
		{
			return new NavObjectOptions(ObjectType.Report, reportId, parameters).Load();
		}
		return parameters;
	}

	public int PageNo()
	{
		return 1;
	}

	public int PageNo(int number)
	{
		return number;
	}

	public NavText GetDynamicCaption(int controlId)
	{
		string value = EvaluateCaptionClass(controlId);
		if (string.IsNullOrEmpty(value))
		{
			return NavText.Empty;
		}
		return Session.SystemCodeunitFactory.UIHelperTriggers.InvokeCaptionClassTranslate(Language, new NavText(value));
	}

	protected override void OnPreTrigger()
	{
		DataItemIterator.ExecuteTrigger(OnPreReport);
		int firstPage = (Metadata.FirstPagePaperSourceKindRaw.HasValue ? Metadata.FirstPagePaperSourceKindRaw.Value : 0);
		int defaultPage = (Metadata.DefaultPaperSourceKindRaw.HasValue ? Metadata.DefaultPaperSourceKindRaw.Value : 0);
		int lastPage = (Metadata.LastPagePaperSourceKindRaw.HasValue ? Metadata.LastPagePaperSourceKindRaw.Value : 0);
		Session.SystemCodeunitFactory.ReportingTriggers.InvokeGetPaperTrayForReport(ReportId, ref firstPage, ref defaultPage, ref lastPage);
		if (firstPage != 0)
		{
			FirstPagePaperSourceKindRaw = firstPage;
		}
		else
		{
			FirstPagePaperSourceKindRaw = null;
		}
		if (defaultPage != 0)
		{
			DefaultPaperSourceKindRaw = defaultPage;
		}
		else
		{
			DefaultPaperSourceKindRaw = null;
		}
		if (lastPage != 0)
		{
			LastPagePaperSourceKindRaw = lastPage;
		}
		else
		{
			LastPagePaperSourceKindRaw = null;
		}
		InitializeReportLabels();
		AddDefaultParameters();
	}

	protected override void OnPostTrigger()
	{
		DataItemIterator.ExecuteTrigger(OnPostReport);
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		NavReportHandle navReportHandle = new NavReportHandle(parent, ObjectId.ObjectNumber);
		navReportHandle.Target = this;
		return navReportHandle;
	}

	public KeyValuePair<string, string>[] GetParameters()
	{
		return reportParameters.ToArray();
	}

	public ILocalReportHandle EffectiveLocalReportHandle()
	{
		if (effectiveLocalReportHandle == null)
		{
			if (LayoutFormat == CustomLayout.Rdlc && IsCustomLayout)
			{
				NavText navText = Session.SystemCodeunitFactory.ReportingTriggers.InvokeReportGetCustomRdlc(ReportId);
				byte[] reportLayout = MetaReport.CheckAndPrepareRdlc(ReportId, navText.Value);
				bool enableReportViewerInternalAppDomainIsolation = ((!ServerUserSettings.Instance.PreviousReportAppDomainIsolation.Value) ? ServerUserSettings.Instance.ReportAppDomainIsolation.Value : (!NavEnvironment.Topology.IsServiceRunningInLocalEnvironment || ServerUserSettings.Instance.ReportAppDomainIsolation.Value));
				effectiveLocalReportHandle = new LocalReportHandle(reportLayout, Metadata.EnableExternalImages, Metadata.EnableHyperlinks, Metadata.EnableExternalAssemblies, enableReportViewerInternalAppDomainIsolation, enableAppDomainMonitor: true);
			}
			else
			{
				effectiveLocalReportHandle = NavCurrentThread.Session.LocalReportCache.GetLocalReport(Metadata);
			}
		}
		return effectiveLocalReportHandle;
	}

	private static int GetReportToRun(int reportId, RunMode runMode, string requestPageParameters, NavRecord record)
	{
		int newReportIdToRun = -1;
		if (NavCurrentThread.Session.SystemCodeunitFactory.ReportingTriggers.InvokeSubstituteReport(reportId, runMode, requestPageParameters, record, ref newReportIdToRun))
		{
			return newReportIdToRun;
		}
		return reportId;
	}
}
