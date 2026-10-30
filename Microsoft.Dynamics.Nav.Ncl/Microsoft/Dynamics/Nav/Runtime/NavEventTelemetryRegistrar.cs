using System;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavEventTelemetryRegistrar
{
	private readonly Action<NavEventTelemetryEvent> callback;

	private readonly Action errorCallBack;

	internal NavSession Session { get; private set; }

	internal NavEventTelemetryRegistrar(NavSession session, Action<NavEventTelemetryEvent> callback, Action errorCallBack)
	{
		this.callback = callback ?? throw new ArgumentNullException("callback");
		this.errorCallBack = errorCallBack ?? throw new ArgumentNullException("errorCallBack");
		Session = session;
	}

	internal void OnEventRaised(NavEventTelemetryEvent telemetryEvent)
	{
		try
		{
			callback(telemetryEvent);
		}
		catch (Exception)
		{
			Session.DisposeEventSessionRecorder();
			errorCallBack();
		}
	}
}
