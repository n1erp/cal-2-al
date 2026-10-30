using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavXmlDeltaConversionDiagnosticsExtensions
{
	public static bool IsError(this NavXmlDeltaConversionDiagnostics diagnostic)
	{
		return diagnostic.Severity.In(Verbosity.Critical, Verbosity.Error);
	}

	public static bool IsWarning(this NavXmlDeltaConversionDiagnostics diagnostic)
	{
		return diagnostic.Severity == Verbosity.Warning;
	}
}
