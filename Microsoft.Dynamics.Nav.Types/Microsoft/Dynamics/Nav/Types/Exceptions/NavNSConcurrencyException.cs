using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSConcurrencyException : NavNSException
{
	public NavNSConcurrencyException()
	{
	}

	public NavNSConcurrencyException(string message)
		: base(message)
	{
	}

	public NavNSConcurrencyException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavNSConcurrencyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
