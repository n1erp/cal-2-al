using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLExclusiveActionException : NavNCLException
{
	public NavNCLExclusiveActionException()
	{
	}

	public NavNCLExclusiveActionException(string message)
		: base(message)
	{
	}

	public NavNCLExclusiveActionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLExclusiveActionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
