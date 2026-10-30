using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class TelemetryActivityTransferEvent : TelemetryEventBase
{
	public string NavTenantId { get; set; }

	public Guid SessionId { get; set; }

	public Guid OldActivityId { get; set; }

	public Guid NewActivityId { get; set; }

	public string AadTenantId { get; set; }
}
