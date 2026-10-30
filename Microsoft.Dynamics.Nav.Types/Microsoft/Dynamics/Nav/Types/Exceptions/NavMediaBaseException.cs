using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavMediaBaseException : NavException
{
	protected NavMediaBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavMediaBaseException()
	{
	}

	protected NavMediaBaseException(string message)
		: base(message)
	{
	}

	protected NavMediaBaseException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
