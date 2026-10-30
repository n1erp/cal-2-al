using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public interface ITelemetryPrivacyClassification
{
	PrivacyClassification Classification { get; }
}
