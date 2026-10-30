using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavBufferTenantDataVersionMismatchException : NavException
{
	public NavBufferTenantDataVersionMismatchException()
	{
	}

	public NavBufferTenantDataVersionMismatchException(string message)
		: base(message)
	{
	}

	public NavBufferTenantDataVersionMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavBufferTenantDataVersionMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavBufferTenantNotFoundException CreateForSpecificTenant(string tenantId, string tenantDatabaseId, string tenantDataVersion, string applicationDatabaseVersion)
	{
		return new NavBufferTenantNotFoundException(string.Format(CultureInfo.InvariantCulture, "The buffer tenant {0} in tenant database {1} cannot be registered. Its data version ({2}) must be equal to the application version of the application database ({3}).", tenantId, tenantDatabaseId, tenantDataVersion, applicationDatabaseVersion));
	}
}
