using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerConnectionException : NavDebuggerBaseException
{
	public NavDebuggerConnectionException()
	{
	}

	public NavDebuggerConnectionException(string message)
		: base(message)
	{
	}

	public NavDebuggerConnectionException(string message, string userId)
		: base(CultureInfo.CurrentCulture, message, DiagnosticParameter.Eupi(userId))
	{
	}

	public NavDebuggerConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
