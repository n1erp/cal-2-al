using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportSaveAsRenderer : SaveAsRenderer
{
	private NavReport report;

	private ReportFormat outputFormat;

	private ILocalReportHandle localReportHandle;

	private bool disposed;

	public ReportFormat OutputFormat
	{
		get
		{
			return outputFormat;
		}
		set
		{
			outputFormat = value;
		}
	}

	public ReportSaveAsRenderer(NavReport report, string fileName, ReportFormat outputFormat)
		: base(GenerateFileName(report.Caption, fileName, outputFormat), NavDataSetBuilder.CreateNavDataSet(report.Metadata))
	{
		this.report = report;
		this.outputFormat = outputFormat;
	}

	public ReportSaveAsRenderer(NavReport report, NavOutStream outStream, ReportFormat outputFormat)
		: base(outStream, NavDataSetBuilder.CreateNavDataSet(report.Metadata))
	{
		this.report = report;
		this.outputFormat = outputFormat;
	}

	public override void Start()
	{
		localReportHandle = report.EffectiveLocalReportHandle();
		base.Start();
	}

	protected override void Render()
	{
		if (!string.IsNullOrEmpty(FileName))
		{
			byte[] array = null;
			using (NavFile navFile = new NavFile(report))
			{
				navFile.ALCreate(FileName);
				array = RenderLocalReport();
				if (array != null)
				{
					navFile.InternalStream.Write(array, 0, array.Length);
				}
			}
			if (array == null)
			{
				NavFile.ALErase(DataError.TrapError, FileName);
			}
		}
		else if (OutStream != null)
		{
			if (OutStream.InternalStream == null)
			{
				throw NavDiagnostics.GlobalInstance.SendExceptionTag("00009S1", Category.Runtime, new NavNCLStreamAccessException(), Verbosity.Error, new DiagnosticParameter(string.Format(CultureInfo.InvariantCulture, "Null Stream in ReportSaveAsRenderer, report id={0}.", report.ObjectId.ObjectNumber), PrivacyClassification.ObjectMetadata));
			}
			byte[] array2 = RenderLocalReport();
			if (array2 != null)
			{
				OutStream.InternalStream.Write(array2, 0, array2.Length);
			}
		}
	}

	protected byte[] RenderLocalReport()
	{
		//IL_012b: Expected Obj, but got Unknown
		if (DataSet.Tables["Result"].Rows.Count < 1)
		{
			return null;
		}
		if (FeatureSwitches.Instance.EnableStreamingReportDataSet)
		{
			localReportHandle.AddDataSource("DataSet_Result", DataSet.Tables["Result"].Rows);
		}
		else
		{
			DataTable dataTable = DataSet.Tables["Result"].GetDataTable(mediaColumnsAsByteArray: true);
			localReportHandle.AddDataSource("DataSet_Result", dataTable);
		}
		TraceListener traceListener = Debug.Listeners[string.Empty];
		try
		{
			if (traceListener != null && string.IsNullOrEmpty(traceListener.Name))
			{
				Debug.Listeners.Remove(traceListener);
			}
			byte[] result = null;
			string text = string.Empty;
			try
			{
				localReportHandle.SetReportParameters(report.GetParameters());
				string deviceInfo = (report.PdfFontEmbedding ? string.Empty : "<DeviceInfo><EmbedFonts>None</EmbedFonts></DeviceInfo>");
				result = localReportHandle.Render(outputFormat.ToString(), deviceInfo, out var _, out var _, out var _, out var _, out var errors);
				if (errors != null && errors.Length != 0)
				{
					text = errors.First();
				}
			}
			catch (LocalProcessingException ex)
			{
				LocalProcessingException ex2 = ex;
				Exception baseException = ((Exception)(object)ex2).GetBaseException();
				text = baseException.Message;
			}
			if (!string.IsNullOrEmpty(text))
			{
				throw NavNCLReportRenderingException.Create(CultureInfo.InvariantCulture, text);
			}
			return result;
		}
		finally
		{
			if (traceListener != null && traceListener.Name == "Default")
			{
				Debug.Listeners.Add(traceListener);
			}
		}
	}

	private static string GenerateFileName(string report, string fileName, ReportFormat format)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			switch (format)
			{
			case ReportFormat.ExcelOpenXml:
				return report + ".xlsx";
			case ReportFormat.WordOpenXml:
				return report + ".docx";
			case ReportFormat.PDF:
			case ReportFormat.PdfPreview:
			case ReportFormat.PdfPrint:
				return report + ".pdf";
			case ReportFormat.Html:
				return report + ".html";
			}
		}
		return fileName;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			localReportHandle?.Dispose();
			disposed = true;
		}
		base.Dispose(disposing);
	}
}
