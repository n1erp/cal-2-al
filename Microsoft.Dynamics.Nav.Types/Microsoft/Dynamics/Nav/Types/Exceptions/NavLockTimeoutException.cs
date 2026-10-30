using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavLockTimeoutException : NavBaseException
{
	public NavLockTimeoutException()
	{
	}

	public NavLockTimeoutException(string message)
		: base(message)
	{
	}

	public NavLockTimeoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavLockTimeoutException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
