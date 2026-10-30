using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientInvalidCallException : NavWatsonException
{
	public NavClientInvalidCallException()
	{
	}

	public NavClientInvalidCallException(string message)
		: base(message)
	{
	}

	public NavClientInvalidCallException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavClientInvalidCallException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
