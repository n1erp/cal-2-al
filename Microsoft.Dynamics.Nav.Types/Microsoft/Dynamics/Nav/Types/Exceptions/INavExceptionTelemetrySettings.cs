using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

internal interface INavExceptionTelemetrySettings
{
	Verbosity MaxVerbosity { get; }
}
