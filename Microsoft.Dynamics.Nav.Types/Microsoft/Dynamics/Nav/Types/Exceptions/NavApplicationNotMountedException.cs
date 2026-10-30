using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public class NavApplicationNotMountedException : NavException
{
	public const string NavApplicationNotMountedFaultCode = "NavApplicationNotMounted";

	public NavApplicationNotMountedException()
	{
	}

	public NavApplicationNotMountedException(string message)
		: base(message)
	{
	}

	public NavApplicationNotMountedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavApplicationNotMountedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("NavApplicationNotMounted"));
		return new FaultException(reason, code);
	}
}
