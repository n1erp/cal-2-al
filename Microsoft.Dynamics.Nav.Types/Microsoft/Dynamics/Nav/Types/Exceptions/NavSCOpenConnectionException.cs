using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSCOpenConnectionException : NavException
{
	public NavSCOpenConnectionException()
	{
	}

	public NavSCOpenConnectionException(string message)
		: base(message)
	{
	}

	public NavSCOpenConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSCOpenConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
