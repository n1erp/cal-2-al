using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class FailedToStartDebuggingSessionException : NavBaseException
{
	public FailedToStartDebuggingSessionException()
	{
	}

	public FailedToStartDebuggingSessionException(string message)
		: base(message)
	{
	}

	public FailedToStartDebuggingSessionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected FailedToStartDebuggingSessionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
