using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMissingUIHandlerException : NavNCLException
{
	public NavNCLMissingUIHandlerException()
	{
	}

	public NavNCLMissingUIHandlerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMissingUIHandlerException(string message)
		: base(message)
	{
	}

	protected NavNCLMissingUIHandlerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
