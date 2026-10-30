using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidCredentialException : NavException
{
	public const string FailedAuthenticationFaultCode = "FailedAuthentication";

	public const string NameOrPasswordTooLongFaultCode = "NameOrPasswordTooLongFaultCode";

	public const string InvalidCredentialExceptionFaultCode = "InvalidCredentialExceptionFaultCode";

	public NavInvalidCredentialException()
	{
	}

	public NavInvalidCredentialException(string message)
		: base(message)
	{
	}

	public NavInvalidCredentialException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavInvalidCredentialException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public static FaultException CreateFaultException(string identityName = null)
	{
		return NavException.CreateFaultException("FailedAuthentication", string.IsNullOrEmpty(identityName) ? "The server has rejected the client credentials." : string.Format(CultureInfo.InvariantCulture, "The server has rejected the client credentials for the user: {0}.", identityName));
	}
}
