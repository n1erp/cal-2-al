using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IExceptionLogger
{
	void LogException(Exception ex);

	void LogError(string value, IDictionary<string, string> properties);
}
