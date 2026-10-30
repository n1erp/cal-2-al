using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavWatsonException : Exception
{
	protected NavWatsonException()
	{
	}

	protected NavWatsonException(string message)
		: base(message)
	{
	}

	protected NavWatsonException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavWatsonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
