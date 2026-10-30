using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Diagnostic.EventSource;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Diagnostics.Telemetry;

public class EtwTelemetryLog : ITelemetryLogWriter
{
	public string InstanceName { get; set; }

	public EtwTelemetryLog(string instanceName)
	{
		InstanceName = instanceName ?? throw new ArgumentNullException("instanceName");
	}

	public void WriteTelemetryEvent(TelemetryEvent telemetryEvent)
	{
		string assemblyFileVersionName = VersionCompatibility.AssemblyFileVersionName;
		int privacyClassification = (int)telemetryEvent.PrivacyClassification;
		if (PrivacyHelper.IsPersonalOrRestrictedData(telemetryEvent.PrivacyClassification))
		{
			switch (telemetryEvent.Verbosity)
			{
			case Verbosity.Critical:
				NavDiagnosticsEventSource.Log.WriteTransientTelemetryCriticalEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId, telemetryEvent.AadUserId);
				break;
			case Verbosity.Error:
				NavDiagnosticsEventSource.Log.WriteTransientTelemetryErrorEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId, telemetryEvent.AadUserId);
				break;
			case Verbosity.Warning:
				NavDiagnosticsEventSource.Log.WriteTransientTelemetryWarningEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId, telemetryEvent.AadUserId);
				break;
			case Verbosity.Normal:
				NavDiagnosticsEventSource.Log.WriteTransientTelemetryInformationalEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId, telemetryEvent.AadUserId);
				break;
			case Verbosity.Verbose:
				NavDiagnosticsEventSource.Log.WriteTransientTelemetryVerboseEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId, telemetryEvent.AadUserId);
				break;
			}
		}
		else
		{
			switch (telemetryEvent.Verbosity)
			{
			case Verbosity.Critical:
				NavDiagnosticsEventSource.Log.WriteTelemetryCriticalEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId);
				break;
			case Verbosity.Error:
				NavDiagnosticsEventSource.Log.WriteTelemetryErrorEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId);
				break;
			case Verbosity.Warning:
				NavDiagnosticsEventSource.Log.WriteTelemetryWarningEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId);
				break;
			case Verbosity.Normal:
				NavDiagnosticsEventSource.Log.WriteTelemetryInformationalEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId);
				break;
			case Verbosity.Verbose:
				NavDiagnosticsEventSource.Log.WriteTelemetryVerboseEvent(InstanceName, telemetryEvent.NavTenantId, telemetryEvent.Category, telemetryEvent.ClientSessionId, telemetryEvent.ClientActivityId, telemetryEvent.ServerSessionUniqueId, telemetryEvent.ServerActivityId, telemetryEvent.EventTime, telemetryEvent.Message, telemetryEvent.ProcessId, telemetryEvent.Tag, telemetryEvent.ThreadId, telemetryEvent.CounterInformation, telemetryEvent.AlternateTenantIds, assemblyFileVersionName, privacyClassification, telemetryEvent.TransientTelemetryId, telemetryEvent.AadTenantId);
				break;
			}
		}
	}

	public void WriteTelemetryActivityTransfer(TelemetryActivityTransferEvent transferEvent)
	{
		NavDiagnosticsEventSource.Log.WriteTelemetryActivityTransferEvent(InstanceName, transferEvent.NavTenantId, transferEvent.Category, transferEvent.SessionId, transferEvent.OldActivityId, transferEvent.NewActivityId, transferEvent.EventTime, transferEvent.ProcessId, transferEvent.Tag, transferEvent.ThreadId, transferEvent.AadTenantId);
	}
}
