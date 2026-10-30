using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUnexecutedUIHandlerException : NavNCLException
{
	public NavNCLUnexecutedUIHandlerException()
	{
	}

	public NavNCLUnexecutedUIHandlerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLUnexecutedUIHandlerException(string message)
		: base(message)
	{
	}

	protected NavNCLUnexecutedUIHandlerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
