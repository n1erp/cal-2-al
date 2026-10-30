using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavQueryIllegalCallInTriggerException : NavException
{
	public NavQueryIllegalCallInTriggerException()
	{
	}

	public NavQueryIllegalCallInTriggerException(string message)
		: base(message)
	{
	}

	public NavQueryIllegalCallInTriggerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavQueryIllegalCallInTriggerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
