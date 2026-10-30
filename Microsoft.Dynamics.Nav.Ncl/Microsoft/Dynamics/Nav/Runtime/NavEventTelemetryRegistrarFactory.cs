using System;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavEventTelemetryRegistrarFactory
{
	internal const string RecorderName = "NavEventEventReceiver";

	internal static void RegisterListener(Action<NavEventTelemetryEvent> callback, Action errorCallBack)
	{
		NavSession session = NavCurrentThread.Session;
		if (session.EventTelemetryRegistrar != null)
		{
			session.DisposeEventSessionRecorder();
		}
		NavEventTelemetryRegistrar eventTelemetryRegistrar = new NavEventTelemetryRegistrar(session, callback, errorCallBack);
		session.RegisterSessionEventRecorder(eventTelemetryRegistrar);
	}

	internal static void UnregisterListener()
	{
		NavCurrentThread.Session?.DisposeEventSessionRecorder();
	}

	internal static void LogSessionErrorTelemetry(int expectedSessionId, int actualSessionId)
	{
		NavServerEventSource.Log.WriteRecorderSessionError(NavEnvironment.ServerInstanceName, NavCurrentThread.Session.Tenant?.Id, "NavEventEventReceiver", expectedSessionId, actualSessionId);
	}
}
