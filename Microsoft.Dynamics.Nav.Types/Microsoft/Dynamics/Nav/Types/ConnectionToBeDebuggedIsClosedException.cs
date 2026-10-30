using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class ConnectionToBeDebuggedIsClosedException : NavBaseException
{
	public ConnectionToBeDebuggedIsClosedException()
	{
	}

	public ConnectionToBeDebuggedIsClosedException(string message)
		: base(message)
	{
	}

	public ConnectionToBeDebuggedIsClosedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ConnectionToBeDebuggedIsClosedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
