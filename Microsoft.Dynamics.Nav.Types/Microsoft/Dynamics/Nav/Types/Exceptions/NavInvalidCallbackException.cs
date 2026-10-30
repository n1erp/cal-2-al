using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidCallbackException : Exception
{
	public NavInvalidCallbackException()
	{
	}

	public NavInvalidCallbackException(string message)
		: base(message)
	{
	}

	public NavInvalidCallbackException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidCallbackException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
