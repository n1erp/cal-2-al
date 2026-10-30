using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedLanguageException : NavBaseException
{
	public NavNotSupportedLanguageException()
	{
	}

	public NavNotSupportedLanguageException(string message)
		: base(message)
	{
	}

	public NavNotSupportedLanguageException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedLanguageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
