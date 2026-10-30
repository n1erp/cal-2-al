using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerBaseException : NavNCLException
{
	public NavDebuggerBaseException()
	{
	}

	public NavDebuggerBaseException(string message)
		: base(message)
	{
	}

	public NavDebuggerBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavDebuggerBaseException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}
}
