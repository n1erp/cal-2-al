using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLGetUrlException : NavNCLException
{
	public NavNCLGetUrlException()
	{
	}

	public NavNCLGetUrlException(string message)
		: base(message)
	{
	}

	public NavNCLGetUrlException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLGetUrlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
