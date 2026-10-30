using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public abstract class TelemetryEventBase
{
	public int ProcessId { get; set; }

	public int ThreadId { get; set; }

	public string Category { get; set; }

	public DateTime EventTime { get; set; }

	public string Tag { get; set; }
}
