using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionNotCreatedException : NavNCLException
{
	public NavEncryptionNotCreatedException()
		: base(Lang.SystemEncryptionEncryptionKeyNotCreated)
	{
	}

	public NavEncryptionNotCreatedException(string message)
		: base(message)
	{
	}

	public NavEncryptionNotCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionNotCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
