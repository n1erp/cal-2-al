using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavDotNetBaseException : NavBaseException
{
	protected NavDotNetBaseException()
	{
	}

	protected NavDotNetBaseException(string message)
		: base(message)
	{
	}

	protected NavDotNetBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDotNetBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
