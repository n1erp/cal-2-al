using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavUnknownEncryptionException : NavNCLException
{
	public NavUnknownEncryptionException()
	{
	}

	public NavUnknownEncryptionException(string message)
		: base(message)
	{
	}

	public NavUnknownEncryptionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUnknownEncryptionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
