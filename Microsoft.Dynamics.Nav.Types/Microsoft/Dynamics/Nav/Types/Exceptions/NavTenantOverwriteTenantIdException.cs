using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantOverwriteTenantIdException : NavException
{
	public const string NavTenantOverwriteTenantIdFaultCode = "NavTenantOverwriteTenantIdFaultCode";

	public NavTenantOverwriteTenantIdException()
	{
	}

	public NavTenantOverwriteTenantIdException(string message)
		: base(message)
	{
	}

	public NavTenantOverwriteTenantIdException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantOverwriteTenantIdException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("NavTenantOverwriteTenantIdFaultCode"));
		return new FaultException(reason, code);
	}
}
