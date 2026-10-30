using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class AnotherDebuggerAlreadyConnectedException : NavBaseException
{
	public AnotherDebuggerAlreadyConnectedException()
	{
	}

	public AnotherDebuggerAlreadyConnectedException(string message)
		: base(message)
	{
	}

	public AnotherDebuggerAlreadyConnectedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected AnotherDebuggerAlreadyConnectedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
