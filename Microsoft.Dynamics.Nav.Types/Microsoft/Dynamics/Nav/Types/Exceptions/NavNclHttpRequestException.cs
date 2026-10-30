using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclHttpRequestException : NavNCLException
{
	public NavNclHttpRequestException()
	{
	}

	public NavNclHttpRequestException(string message)
		: base(message)
	{
	}

	public NavNclHttpRequestException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclHttpRequestException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
