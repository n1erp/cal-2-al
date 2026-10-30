using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavExchangeException : NavNCLException
{
	public NavExchangeException()
	{
	}

	public NavExchangeException(string message)
		: base(message)
	{
	}

	protected NavExchangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavExchangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
