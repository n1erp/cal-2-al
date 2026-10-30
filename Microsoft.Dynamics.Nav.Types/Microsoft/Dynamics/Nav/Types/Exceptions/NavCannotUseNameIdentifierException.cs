using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCannotUseNameIdentifierException : NavException
{
	public const string CannotMapSecurityTokenToNavUserFaultCode = "CannotMapSecurityTokenToNavUser";

	public NavCannotUseNameIdentifierException()
	{
	}

	public NavCannotUseNameIdentifierException(string message)
		: base(message)
	{
	}

	public NavCannotUseNameIdentifierException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavCannotUseNameIdentifierException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public static FaultException CreateFaultException()
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("CannotMapSecurityTokenToNavUser"));
		return new FaultException("The security token cannot be mapped to an existing user.\r\nPlease enter user name and authentication key, to verify your identity.", code);
	}
}
