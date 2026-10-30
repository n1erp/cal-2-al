using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTimeoutException : NavException
{
	public NavTimeoutException()
	{
	}

	public NavTimeoutException(string message)
		: base(message)
	{
	}

	public NavTimeoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTimeoutException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
