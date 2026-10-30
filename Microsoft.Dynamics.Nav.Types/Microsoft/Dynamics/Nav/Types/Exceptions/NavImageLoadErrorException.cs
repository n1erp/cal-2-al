using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavImageLoadErrorException : NavMediaBaseException
{
	public NavImageLoadErrorException()
	{
	}

	public NavImageLoadErrorException(string message)
		: base(message)
	{
	}

	public NavImageLoadErrorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavImageLoadErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
