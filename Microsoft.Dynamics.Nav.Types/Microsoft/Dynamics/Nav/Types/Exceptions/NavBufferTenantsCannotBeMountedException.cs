using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavBufferTenantsCannotBeMountedException : NavException
{
	public NavBufferTenantsCannotBeMountedException()
	{
	}

	public NavBufferTenantsCannotBeMountedException(string message)
		: base(message)
	{
	}

	public NavBufferTenantsCannotBeMountedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavBufferTenantsCannotBeMountedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavBufferTenantsCannotBeMountedException Create(string tenantId, string databaseName)
	{
		return new NavBufferTenantsCannotBeMountedException(string.Format(CultureInfo.InvariantCulture, "Buffer tenants cannot be mounted. You attempted to mount buffer tenant {0} in database {1}.", tenantId, databaseName));
	}
}
