using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = true, LogType = EventLogEntryType.Information)]
public class NavSessionTerminatedException : NavServerSessionBaseException, INavExceptionTelemetrySettings
{
	public Verbosity MaxVerbosity => Verbosity.Normal;

	public NavSessionTerminatedException()
	{
	}

	public NavSessionTerminatedException(string message)
		: base(message)
	{
	}

	public NavSessionTerminatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSessionTerminatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
