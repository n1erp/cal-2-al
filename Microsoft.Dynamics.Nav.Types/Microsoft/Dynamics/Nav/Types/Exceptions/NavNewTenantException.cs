using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNewTenantException : NavNCLException
{
	public NavNewTenantException()
	{
	}

	public NavNewTenantException(string message)
		: base(message)
	{
	}

	protected NavNewTenantException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNewTenantException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public static NavNewTenantException Create(string tenantDatabaseId)
	{
		return new NavNewTenantException(string.Format(CultureInfo.InvariantCulture, "The tenant database {0} is not a shared schema database, and so cannot contain more than one tenant.", tenantDatabaseId));
	}
}
