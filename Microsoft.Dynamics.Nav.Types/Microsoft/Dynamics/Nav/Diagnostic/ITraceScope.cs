using System;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public interface ITraceScope : IDisposable
{
	void SetResult(NavDiagnosticsScopeResult result);

	void SetResult(Exception exception);
}
