using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavOfficeEntitlementNotEnabledException : NavException
{
	public NavOfficeEntitlementNotEnabledException()
	{
	}

	public NavOfficeEntitlementNotEnabledException(string message)
		: base(message)
	{
	}

	public NavOfficeEntitlementNotEnabledException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavOfficeEntitlementNotEnabledException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
