using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavMustBeSuperUserException : NavBaseException
{
	public NavMustBeSuperUserException()
	{
	}

	public NavMustBeSuperUserException(string message)
		: base(message)
	{
	}

	public NavMustBeSuperUserException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavMustBeSuperUserException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
