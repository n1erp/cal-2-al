using System;

namespace Microsoft.Dynamics.Nav.Diagnostic.EventSource;

public interface INavDiagnosticsEventViewerEvents
{
	void WriteTelemetryEvent(int eventId, string serverInstanceName, string navTenantId, string category, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, DateTime eventTime, string message, int processId, string tag, int threadId, string counterInformation, string alternateTenantIds, string platformVersion, int privacyClassification, Guid transientEventId, string aadTenantId, string aadUserId = null);

	void WriteTelemetryActivityTransferEvent(string serverInstanceName, string navTenantId, string category, Guid sessionId, Guid oldActivityId, Guid newActivityId, DateTime eventTime, int processId, string tag, int threadId, string aadTenantId);
}
