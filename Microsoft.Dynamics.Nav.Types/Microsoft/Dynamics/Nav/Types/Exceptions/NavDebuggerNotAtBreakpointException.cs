using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerNotAtBreakpointException : NavDebuggerBaseException
{
	public NavDebuggerNotAtBreakpointException()
		: base(Lang.NavDebuggerNotAtBreakpoint)
	{
	}

	public NavDebuggerNotAtBreakpointException(string message)
		: base(message)
	{
	}

	public NavDebuggerNotAtBreakpointException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerNotAtBreakpointException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
