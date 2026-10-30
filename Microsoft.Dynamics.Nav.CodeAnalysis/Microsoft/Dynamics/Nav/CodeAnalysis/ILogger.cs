using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ILogger
{
	void LogMessage(LogLevel level, string message);

	void LogException(Exception e);
}
