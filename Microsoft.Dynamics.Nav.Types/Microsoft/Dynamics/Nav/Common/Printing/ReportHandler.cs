using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Common.Printing;

[CLSCompliant(false)]
public class ReportHandler
{
	protected virtual bool ReportPageReadyPreview(DataSetRequest reportRequest, ReportExecutionItem reportExecutionItem, bool isFirstPage, ReportExecutionHandler reportExecutionHandler)
	{
		return false;
	}

	protected virtual bool ReportPageReadyPrintDirectly(DataSetRequest reportRequest, ReportExecutionItem reportExecutionItem, bool isFirstPage, ReportExecutionHandler reportExecutionHandler)
	{
		return reportExecutionItem.ReportRenderer.ReportPageReadyPrintDirectly(reportRequest, isFirstPage, reportExecutionHandler);
	}

	protected virtual bool ReportPageReadyCreateReportFormat(DataSetRequest reportRequest, ReportExecutionItem reportExecutionItem, bool isFirstPage, ReportExecutionHandler reportExecutionHandler)
	{
		return reportExecutionItem.ReportRenderer.ReportPageReadyCreateReportFormat(reportRequest, isFirstPage, reportExecutionHandler);
	}

	protected virtual void EndClientCallEarly(DataSetRequest reportRequest, IService service, bool isCanceling, ReportHandlerResponse response, bool isFirstPage)
	{
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)reportRequest.Information;
		if (!isCanceling && (!(reportRequest.FinalDataSet | isFirstPage) || reportRequestInformation.IsPreview))
		{
			response.EndClientCall = false;
			response.EndClientCallResult = false;
			response.ClientCallHasEnded = true;
			service.EndClientCall(false);
		}
	}

	public virtual ReportHandlerResponse ReportPageReady(DataSetRequest reportRequest, ReportExecutionHandler reportExecutionHandler, IService service, IReportCallbackHandler reportCallbackHandler)
	{
		return ReportPageReady(reportRequest, reportExecutionHandler, service, reportCallbackHandler, enableDirectPrint: true);
	}

	public virtual ReportHandlerResponse ReportPageReady(DataSetRequest reportRequest, ReportExecutionHandler reportExecutionHandler, IService service, IReportCallbackHandler reportCallbackHandler, bool enableDirectPrint)
	{
		if (reportRequest == null)
		{
			throw new ArgumentNullException("reportRequest");
		}
		if (reportExecutionHandler == null)
		{
			throw new ArgumentNullException("reportExecutionHandler");
		}
		if (reportCallbackHandler == null)
		{
			throw new ArgumentNullException("reportCallbackHandler");
		}
		ReportRequestInformation reportRequestInformation = (ReportRequestInformation)reportRequest.Information;
		bool isFirstPage = false;
		bool flag = false;
		ReportExecutionItem reportExecutionItem = reportExecutionHandler.GetReportExecutionItem(reportRequest.ExecutionGuid);
		ReportHandlerResponse reportHandlerResponse = new ReportHandlerResponse();
		if (!reportRequest.Cancel)
		{
			if (reportExecutionItem == null)
			{
				isFirstPage = true;
				reportExecutionItem = reportExecutionHandler.CreateReportExecutionItem(reportRequest, service, reportCallbackHandler);
			}
			reportExecutionItem.AddDataSetPage(reportRequest);
			flag = reportExecutionItem.ReportRenderer.IsCanceling;
			EndClientCallEarly(reportRequest, service, flag, reportHandlerResponse, isFirstPage);
		}
		if (reportRequest.Cancel | flag)
		{
			reportExecutionHandler.CancelReport(reportRequest.ExecutionGuid);
			reportHandlerResponse.EndClientCall = true;
			reportHandlerResponse.EndClientCallResult = true;
			return reportHandlerResponse;
		}
		if (reportRequestInformation.IsPreview)
		{
			ReportPageReadyPreview(reportRequest, reportExecutionItem, isFirstPage, reportExecutionHandler);
			reportHandlerResponse.EndClientCall = false;
			reportHandlerResponse.EndClientCallResult = false;
			return reportHandlerResponse;
		}
		bool flag2;
		if (reportRequestInformation.ClientReportFormat != ReportFormat.None)
		{
			flag2 = ReportPageReadyCreateReportFormat(reportRequest, reportExecutionItem, isFirstPage, reportExecutionHandler);
		}
		else
		{
			if (!enableDirectPrint)
			{
				string caption = reportRequestInformation.Caption;
				int id = reportRequestInformation.Id;
				throw new NavNCLInvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.ReportServerPrintNotSupported, caption, id));
			}
			flag2 = ReportPageReadyPrintDirectly(reportRequest, reportExecutionItem, isFirstPage, reportExecutionHandler);
		}
		reportHandlerResponse.EndClientCall = !reportHandlerResponse.ClientCallHasEnded;
		reportHandlerResponse.EndClientCallResult = !flag2;
		return reportHandlerResponse;
	}
}
