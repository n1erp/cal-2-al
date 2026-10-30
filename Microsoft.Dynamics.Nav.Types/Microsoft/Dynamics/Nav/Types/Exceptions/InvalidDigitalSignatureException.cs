using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class InvalidDigitalSignatureException : NavException
{
	public InvalidDigitalSignatureException()
	{
	}

	public InvalidDigitalSignatureException(string message)
		: base(message)
	{
	}

	public InvalidDigitalSignatureException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected InvalidDigitalSignatureException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public static InvalidDigitalSignatureException CreateException(CultureInfo cultureInfo, string fileName)
	{
		return new InvalidDigitalSignatureException(string.Format(cultureInfo, "The assembly {0} is not Authenticode signed by Microsoft.", fileName));
	}
}
