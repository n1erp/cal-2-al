using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public class ReportExecutionHandler : IDisposable
{
	private readonly Dictionary<Guid, ReportExecutionItem> reportExecutionItems = new Dictionary<Guid, ReportExecutionItem>();

	protected bool IsDisposed { get; set; }

	public ReportExecutionItem CreateReportExecutionItem(DataSetRequest reportRequest, IService service, IReportCallbackHandler reportCallbackHandler)
	{
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)reportRequest.Information;
		ReportExecutionItem reportExecutionItem = new ReportExecutionItem(reportRequest.ExecutionGuid, new NavDataTableCollectionEnumerator(waitForEndSignal: true, ZoomToolState.UseZoomToolForReports), reportCallbackHandler.GetReportHandle(reportRequest.Information.Id, reportRequest.ExecutionGuid, !reportRequestInformation.IsCustomLayout), reportRequest.Information.Caption, reportRequest.Information.Id, reportCallbackHandler, reportRequestInformation.DefaultPaperSourceKindRaw, reportRequestInformation.FirstPagePaperSourceKindRaw, reportRequestInformation.LastPagePaperSourceKindRaw, reportRequestInformation.Parameters);
		reportExecutionItem.ReportRenderer = CreateReportRenderer(reportExecutionItem, service, reportCallbackHandler, reportRequestInformation);
		reportExecutionItems.Add(reportRequest.ExecutionGuid, reportExecutionItem);
		return reportExecutionItem;
	}

	protected virtual ReportRenderer CreateReportRenderer(ReportExecutionItem reportExecutionItem, IService service, IReportCallbackHandler reportCallbackHandler, ReportRequestInformation information)
	{
		return new ReportRenderer(reportExecutionItem, reportCallbackHandler);
	}

	public ReportExecutionItem GetReportExecutionItem(Guid reportUniqueIdentifier)
	{
		reportExecutionItems.TryGetValue(reportUniqueIdentifier, out var value);
		return value;
	}

	public void RemoveReportExecutionItem(Guid reportUniqueIdentifier)
	{
		if (reportExecutionItems.TryGetValue(reportUniqueIdentifier, out var value))
		{
			reportExecutionItems.Remove(reportUniqueIdentifier);
			value.Dispose();
		}
	}

	public void CancelReport(Guid reportUniqueIdentifier)
	{
		ReportExecutionItem reportExecutionItem = GetReportExecutionItem(reportUniqueIdentifier);
		if (reportExecutionItem != null)
		{
			reportExecutionItem.Cancel();
			RemoveReportExecutionItem(reportUniqueIdentifier);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (IsDisposed)
		{
			return;
		}
		if (disposing)
		{
			foreach (ReportExecutionItem value in reportExecutionItems.Values)
			{
				value.Dispose();
			}
			reportExecutionItems.Clear();
		}
		IsDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~ReportExecutionHandler()
	{
		Dispose(disposing: false);
	}
}
