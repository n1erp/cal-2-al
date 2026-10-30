using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionCreatedException : NavNCLException
{
	public NavEncryptionCreatedException()
		: base(Lang.SystemEncryptionEncryptionKeyAlreadyCreated)
	{
	}

	public NavEncryptionCreatedException(string message)
		: base(message)
	{
	}

	public NavEncryptionCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
