using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantStateSynchronizationException : NavException
{
	public NavTenantStateSynchronizationException()
	{
	}

	public NavTenantStateSynchronizationException(string message)
		: base(message)
	{
	}

	public NavTenantStateSynchronizationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantStateSynchronizationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
