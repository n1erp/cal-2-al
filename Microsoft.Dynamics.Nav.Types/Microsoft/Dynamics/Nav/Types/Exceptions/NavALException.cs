using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavALException : NavException
{
	public NavALException()
	{
	}

	public NavALException(string message)
		: base(message)
	{
	}

	public NavALException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavALException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
