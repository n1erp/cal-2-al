using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerInvalidBreakpointException : NavDebuggerBaseException
{
	public NavDebuggerInvalidBreakpointException()
		: base(Lang.InvalidBreakpoint)
	{
	}

	public NavDebuggerInvalidBreakpointException(string message)
		: base(message)
	{
	}

	public NavDebuggerInvalidBreakpointException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerInvalidBreakpointException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
