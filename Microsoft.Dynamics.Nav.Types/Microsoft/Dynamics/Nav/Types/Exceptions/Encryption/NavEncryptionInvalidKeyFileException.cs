using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionInvalidKeyFileException : NavNCLException
{
	public NavEncryptionInvalidKeyFileException()
		: base(Lang.SystemEncryptionInvalidKeyFile)
	{
	}

	public NavEncryptionInvalidKeyFileException(string message)
		: base(message)
	{
	}

	public NavEncryptionInvalidKeyFileException(Exception innerException)
		: this(Lang.SystemEncryptionInvalidKeyFile, innerException)
	{
	}

	public NavEncryptionInvalidKeyFileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionInvalidKeyFileException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
