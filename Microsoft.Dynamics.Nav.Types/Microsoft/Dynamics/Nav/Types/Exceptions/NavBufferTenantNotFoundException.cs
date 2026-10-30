using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavBufferTenantNotFoundException : NavException
{
	public NavBufferTenantNotFoundException()
	{
	}

	public NavBufferTenantNotFoundException(string message)
		: base(message)
	{
	}

	public NavBufferTenantNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavBufferTenantNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavBufferTenantNotFoundException Create(string tenantId, string tenantDatabaseId)
	{
		return new NavBufferTenantNotFoundException(string.Format(CultureInfo.InvariantCulture, "There is no buffer tenant with ID {0} in database {1}.", tenantId, tenantDatabaseId));
	}
}
