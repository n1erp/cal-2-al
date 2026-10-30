using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavISignableException : NavBaseException
{
	public NavISignableException()
	{
	}

	public NavISignableException(string message)
		: base(message)
	{
	}

	public NavISignableException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavISignableException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
