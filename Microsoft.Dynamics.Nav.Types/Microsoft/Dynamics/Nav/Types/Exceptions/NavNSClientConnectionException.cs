using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSClientConnectionException : NavNSException
{
	public NavNSClientConnectionException()
	{
	}

	public NavNSClientConnectionException(string message)
		: base(message)
	{
	}

	public NavNSClientConnectionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavNSClientConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
