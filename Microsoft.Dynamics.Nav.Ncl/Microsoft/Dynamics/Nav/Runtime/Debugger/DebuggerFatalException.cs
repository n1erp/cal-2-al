using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

[Serializable]
public class DebuggerFatalException : Exception
{
	public DebuggerFatalException()
	{
	}

	public DebuggerFatalException(string message)
		: base(message)
	{
	}

	public DebuggerFatalException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected DebuggerFatalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
