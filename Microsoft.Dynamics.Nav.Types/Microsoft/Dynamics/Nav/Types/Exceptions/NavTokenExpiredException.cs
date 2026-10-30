using System;
using System.Runtime.Serialization;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTokenExpiredException : NavException
{
	internal SecureString Token { get; private set; }

	public NavTokenExpiredException()
	{
	}

	public NavTokenExpiredException(string message)
		: base(message)
	{
	}

	public NavTokenExpiredException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavTokenExpiredException(SecureString token, Exception innerException)
		: base(string.Empty, innerException)
	{
		Token = token;
	}

	protected NavTokenExpiredException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
