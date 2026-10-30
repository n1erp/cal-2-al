using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public interface IReportCallbackHandler
{
	RetrievePrinterSettings PrinterSettings { get; }

	bool FileSave(string fileName, string title, string initialFolder, string typeFilter, RetrieveFileContents retrieveFileContents, out string actualFileName);

	void HandleException(Exception ex);

	void HandleError(string errorMessage);

	ILocalReportHandle GetReportHandle(int reportId, Guid reportHandle, bool useCache);

	void RunWithCommunicationBrokerBlocker(Action action);
}
