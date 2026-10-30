using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionException : NavNCLException
{
	public NavEncryptionException()
	{
	}

	public NavEncryptionException(string message)
		: base(message)
	{
	}

	public NavEncryptionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
