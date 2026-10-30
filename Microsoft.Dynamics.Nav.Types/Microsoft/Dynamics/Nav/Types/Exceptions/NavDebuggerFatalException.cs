using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerFatalException : NavServerSessionBaseException
{
	public NavDebuggerFatalException()
	{
	}

	public NavDebuggerFatalException(string message)
		: base(message)
	{
	}

	public NavDebuggerFatalException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerFatalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
