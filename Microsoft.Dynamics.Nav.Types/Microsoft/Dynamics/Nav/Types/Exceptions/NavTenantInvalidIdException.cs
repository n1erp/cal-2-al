using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantInvalidIdException : NavException
{
	public const string TenantIdInvalidFaultCode = "TenantIdInvalidFaultCode";

	public NavTenantInvalidIdException()
	{
	}

	public NavTenantInvalidIdException(string message)
		: base(message)
	{
	}

	public NavTenantInvalidIdException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantInvalidIdException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("TenantIdInvalidFaultCode"));
		return new FaultException(reason, code);
	}
}
