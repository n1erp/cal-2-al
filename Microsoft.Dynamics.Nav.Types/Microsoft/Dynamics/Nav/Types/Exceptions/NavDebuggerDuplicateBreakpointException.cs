using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerDuplicateBreakpointException : NavDebuggerBaseException
{
	public NavDebuggerDuplicateBreakpointException()
		: base(Lang.DuplicateBreakpoint)
	{
	}

	public NavDebuggerDuplicateBreakpointException(string message)
		: base(message)
	{
	}

	public NavDebuggerDuplicateBreakpointException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerDuplicateBreakpointException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
