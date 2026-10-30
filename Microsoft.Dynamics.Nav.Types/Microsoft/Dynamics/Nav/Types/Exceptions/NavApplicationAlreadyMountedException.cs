using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public class NavApplicationAlreadyMountedException : NavException
{
	public const string NavApplicationAlreadyMountedFaultCode = "NavApplicationAlreadyMounted";

	public NavApplicationAlreadyMountedException()
	{
	}

	public NavApplicationAlreadyMountedException(string message)
		: base(message)
	{
	}

	public NavApplicationAlreadyMountedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavApplicationAlreadyMountedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("NavApplicationAlreadyMounted"));
		return new FaultException(reason, code);
	}
}
