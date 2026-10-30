using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDebuggerActivityAbortedException : NavNCLException
{
	public NavNCLDebuggerActivityAbortedException()
	{
	}

	public NavNCLDebuggerActivityAbortedException(string message)
		: base(message)
	{
	}

	public NavNCLDebuggerActivityAbortedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDebuggerActivityAbortedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
