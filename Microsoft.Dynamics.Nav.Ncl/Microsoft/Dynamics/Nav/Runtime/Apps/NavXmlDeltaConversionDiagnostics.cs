using System;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavXmlDeltaConversionDiagnostics
{
	public string Message { get; }

	public Verbosity Severity { get; }

	public NavXmlDeltaConversionDiagnostics(string message, Verbosity severity)
	{
		if (!severity.In(Verbosity.Error, Verbosity.Critical, Verbosity.Warning))
		{
			throw new ArgumentOutOfRangeException("severity");
		}
		Message = message;
		Severity = severity;
	}
}
