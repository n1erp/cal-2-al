using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclHttpClientTimeoutTooLargeException : NavBaseException
{
	public NavNclHttpClientTimeoutTooLargeException()
	{
	}

	public NavNclHttpClientTimeoutTooLargeException(string message)
		: base(message)
	{
	}

	public NavNclHttpClientTimeoutTooLargeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclHttpClientTimeoutTooLargeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
