using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCannotSpecifyAuthenticationEmailException : NavException
{
	public NavCannotSpecifyAuthenticationEmailException()
	{
	}

	public NavCannotSpecifyAuthenticationEmailException(string message)
		: base(message)
	{
	}

	public NavCannotSpecifyAuthenticationEmailException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavCannotSpecifyAuthenticationEmailException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
