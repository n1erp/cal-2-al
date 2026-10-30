using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavErrorReporter
{
	void ReportError(string errorMessage, Exception exception);
}
