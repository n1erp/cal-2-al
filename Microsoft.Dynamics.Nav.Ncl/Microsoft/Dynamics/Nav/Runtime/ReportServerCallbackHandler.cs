using System;
using System.Drawing.Printing;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Printing;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportServerCallbackHandler : IReportCallbackHandler
{
	private const string NoDefaultPrinterError = "Default printer is not set.";

	private static int printedReportId = 0;

	private static string printedReportCaption = "";

	public RetrievePrinterSettings PrinterSettings { get; private set; }

	public ReportServerCallbackHandler()
	{
		PrinterSettings = GetPrinterSettingsOnServer;
	}

	public bool FileSave(string fileName, string title, string initialFolder, string typeFilter, RetrieveFileContents retrieveFileContents, out string actualFileName)
	{
		throw new NotImplementedException();
	}

	public void HandleException(Exception ex)
	{
		throw ex;
	}

	public void HandleError(string errorMessage)
	{
		throw NavNCLReportRenderingException.Create(CultureInfo.InvariantCulture, errorMessage);
	}

	public ILocalReportHandle GetReportHandle(int reportId, Guid reportHandle, bool useCache)
	{
		NavReport registeredReport = NavCurrentThread.Session.Company.GetRegisteredReport(reportHandle);
		printedReportId = reportId;
		printedReportCaption = registeredReport.Caption;
		return registeredReport.EffectiveLocalReportHandle();
	}

	public void RunWithCommunicationBrokerBlocker(Action action)
	{
		action();
	}

	public static bool GetPrinterSettingsOnServer(PrintDocument document, string printerName)
	{
		if (string.Compare(document.PrinterSettings.PrinterName, "Default printer is not set.", StringComparison.OrdinalIgnoreCase) == 0)
		{
			throw NavNCLReportNoDefaultPrinterException.Create();
		}
		if (string.IsNullOrEmpty(document.PrinterSettings.PrinterName))
		{
			PrinterSettings printerSettings = new PrinterSettings();
			if (printerSettings.IsDefaultPrinter && printerSettings.IsValid)
			{
				document.PrinterSettings.PrinterName = printerSettings.PrinterName;
				return false;
			}
			throw NavNCLReportNoDefaultPrinterException.Create();
		}
		foreach (string installedPrinter in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
		{
			if (string.Compare(document.PrinterSettings.PrinterName, installedPrinter, StringComparison.OrdinalIgnoreCase) == 0 && document.PrinterSettings.IsValid)
			{
				return false;
			}
		}
		throw NavNCLPrinterNotInstalledOnServerException.Create(CultureInfo.InvariantCulture, document.PrinterSettings.PrinterName, printedReportId, printedReportCaption);
	}
}
