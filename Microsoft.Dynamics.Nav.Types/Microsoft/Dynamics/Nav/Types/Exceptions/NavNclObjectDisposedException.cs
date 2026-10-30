using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclObjectDisposedException : NavBaseException
{
	public NavNclObjectDisposedException()
	{
	}

	public NavNclObjectDisposedException(string message)
		: base(message)
	{
	}

	public NavNclObjectDisposedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclObjectDisposedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
