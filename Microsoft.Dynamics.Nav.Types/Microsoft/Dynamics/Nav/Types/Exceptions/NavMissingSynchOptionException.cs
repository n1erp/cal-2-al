using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMissingSynchOptionException : NavBaseException
{
	public NavMissingSynchOptionException()
	{
	}

	public NavMissingSynchOptionException(string message)
		: base(message)
	{
	}

	public NavMissingSynchOptionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMissingSynchOptionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
