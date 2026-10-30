using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionExistingKeyImportException : NavNCLException
{
	public NavEncryptionExistingKeyImportException()
		: base(Lang.SystemEncryptionCantOverwriteEncryptionKey)
	{
	}

	public NavEncryptionExistingKeyImportException(string message)
		: base(message)
	{
	}

	public NavEncryptionExistingKeyImportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionExistingKeyImportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
