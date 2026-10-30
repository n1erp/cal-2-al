using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAppException : NavBaseException
{
	public NavAppException()
	{
	}

	public NavAppException(string message)
		: base(message)
	{
	}

	public NavAppException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
