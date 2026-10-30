using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedFormatLanguageException : NavNotSupportedLanguageException
{
	public NavNotSupportedFormatLanguageException()
	{
	}

	public NavNotSupportedFormatLanguageException(string message)
		: base(message)
	{
	}

	public NavNotSupportedFormatLanguageException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedFormatLanguageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
