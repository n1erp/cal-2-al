namespace Microsoft.Dynamics.Nav.Diagnostic;

public interface ITelemetryLogWriter
{
	string InstanceName { get; set; }

	void WriteTelemetryEvent(TelemetryEvent telemetryEvent);

	void WriteTelemetryActivityTransfer(TelemetryActivityTransferEvent transferEvent);
}
