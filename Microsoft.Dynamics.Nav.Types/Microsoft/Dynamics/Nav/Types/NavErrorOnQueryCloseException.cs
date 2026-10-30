using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavErrorOnQueryCloseException : NavBaseException
{
	public NavErrorOnQueryCloseException()
	{
	}

	public NavErrorOnQueryCloseException(string message)
		: base(message)
	{
	}

	public NavErrorOnQueryCloseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavErrorOnQueryCloseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
