using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavNSException : NavException
{
	protected NavNSException()
	{
	}

	protected NavNSException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNSException(string message)
		: base(message)
	{
	}

	protected NavNSException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
