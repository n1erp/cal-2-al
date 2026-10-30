using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDotNetException : NavDotNetBaseException
{
	public NavDotNetException()
	{
	}

	public NavDotNetException(string message)
		: base(message)
	{
	}

	public NavDotNetException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDotNetException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
