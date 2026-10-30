using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLOpenConnectionException : NavNCLException
{
	public NavNCLOpenConnectionException()
	{
	}

	public NavNCLOpenConnectionException(string message)
		: base(message)
	{
	}

	public NavNCLOpenConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLOpenConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
