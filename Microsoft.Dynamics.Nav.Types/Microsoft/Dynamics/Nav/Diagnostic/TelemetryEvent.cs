using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public sealed class TelemetryEvent : TelemetryEventBase
{
	public string NavTenantId { get; set; }

	public Guid ClientActivityId { get; set; }

	public Guid ClientSessionId { get; set; }

	public Guid ServerSessionUniqueId { get; set; }

	public Guid ServerActivityId { get; set; }

	public string Message { get; set; }

	public PrivacyClassification PrivacyClassification { get; set; }

	public Guid TransientTelemetryId { get; set; }

	public Verbosity Verbosity { get; set; }

	public string CounterInformation { get; set; }

	public string AlternateTenantIds { get; set; }

	public string AadTenantId { get; set; }

	public string AadUserId { get; set; }
}
