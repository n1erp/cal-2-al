using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantRemoveException : NavNCLException
{
	public NavTenantRemoveException()
	{
	}

	public NavTenantRemoveException(string message)
		: base(message)
	{
	}

	protected NavTenantRemoveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavTenantRemoveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
