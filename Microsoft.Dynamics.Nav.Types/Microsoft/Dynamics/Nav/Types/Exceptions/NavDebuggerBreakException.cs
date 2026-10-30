using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerBreakException : NavDebuggerBaseException
{
	public NavDebuggerBreakException()
		: base(Lang.NavDebuggerNotAtBreakpoint)
	{
	}

	public NavDebuggerBreakException(string message)
		: base(message)
	{
	}

	public NavDebuggerBreakException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerBreakException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
