using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantInvalidStateException : NavException
{
	public NavTenantInvalidStateException()
	{
	}

	public NavTenantInvalidStateException(string message)
		: base(message)
	{
	}

	public NavTenantInvalidStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantInvalidStateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
