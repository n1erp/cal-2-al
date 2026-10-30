using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidCallException : NavWatsonException
{
	public NavInvalidCallException()
	{
	}

	public NavInvalidCallException(string message)
		: base(message)
	{
	}

	public NavInvalidCallException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidCallException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
