using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionKeyNotFoundException : NavNCLException
{
	public NavEncryptionKeyNotFoundException()
	{
	}

	public NavEncryptionKeyNotFoundException(string message)
		: base(message)
	{
	}

	public NavEncryptionKeyNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionKeyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
