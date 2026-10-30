using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

[Serializable]
public class NavEncryptionDataTooLongException : NavNCLException
{
	public NavEncryptionDataTooLongException()
		: base(Lang.SystemEncryptionDataTooLong)
	{
	}

	public NavEncryptionDataTooLongException(string message)
		: base(message)
	{
	}

	public NavEncryptionDataTooLongException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEncryptionDataTooLongException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
