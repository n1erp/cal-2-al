using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavImageBufferException : NavMediaBaseException
{
	public NavImageBufferException()
	{
	}

	public NavImageBufferException(string message)
		: base(message)
	{
	}

	public NavImageBufferException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavImageBufferException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
