using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAuthenticationException : NavException
{
	public const string TechnicalProblemFaultCode = "TechnicalProblemFaultCode";

	public const string InvoiceApplicationNotEnabledFaultCode = "InvoiceApplicationNotEnabledFaultCode";

	public NavAuthenticationException()
	{
	}

	public NavAuthenticationException(string message)
		: base(message)
	{
	}

	public NavAuthenticationException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavAuthenticationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
