using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Diagnostics;

internal static class NavServerEventWriter
{
	internal static void WriteEventRaised(NavEventTelemetryRegistrar registrar, int objectType, int objectId, string eventName, string elementName, int eventType, int callingObjectType = 0, int callingObjectId = 0, string callingMethodName = null)
	{
		registrar.OnEventRaised(NavEventTelemetryEvent.CreateEvent(registrar.Session.Id, objectType, objectId, eventName, elementName, eventType, callingObjectType, callingObjectId, callingMethodName ?? string.Empty));
	}
}
