using Microsoft.Dynamics.Nav.Client;
using Microsoft.Dynamics.Nav.Common.Printing;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReportServerHandler : ReportHandler
{
	private static ReportHandler reportServerHandlerSingleton = new ReportServerHandler();

	private static IReportCallbackHandler reportCallbackHandlerSingleton = new ReportServerCallbackHandler();

	protected override void EndClientCallEarly(DataSetRequest reportRequest, IService service, bool isCanceling, ReportHandlerResponse response, bool isFirstPage)
	{
	}

	public static void PrintReportOnServer(DataSetRequest reportRequest)
	{
		using ReportExecutionHandler reportExecutionHandler = new ReportExecutionHandler();
		reportServerHandlerSingleton.ReportPageReady(reportRequest, reportExecutionHandler, null, reportCallbackHandlerSingleton, NavEnvironment.Topology.IsServiceRunningInLocalEnvironment);
	}
}
