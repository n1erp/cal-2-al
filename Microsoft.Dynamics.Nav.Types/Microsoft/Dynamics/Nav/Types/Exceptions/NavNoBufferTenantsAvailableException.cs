using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNoBufferTenantsAvailableException : NavException
{
	public NavNoBufferTenantsAvailableException()
	{
	}

	public NavNoBufferTenantsAvailableException(string message)
		: base(message)
	{
	}

	public NavNoBufferTenantsAvailableException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavNoBufferTenantsAvailableException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNoBufferTenantsAvailableException CreateForDatabase(string tenantDatabaseId, string applicationDatabaseVersion)
	{
		return new NavNoBufferTenantsAvailableException(string.Format(CultureInfo.InvariantCulture, "There are no buffer tenants in tenant database {0} that are eligible for registration. To register a buffer tenant, its data version must be equal to the application version of the application database ({1}).", tenantDatabaseId, applicationDatabaseVersion));
	}

	public static NavNoBufferTenantsAvailableException Create()
	{
		return new NavNoBufferTenantsAvailableException("There are no buffer tenants that are eligible for registration. To register a buffer tenant its data version must be equal to the application version of the application database.");
	}
}
