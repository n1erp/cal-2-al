using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantDataMoveException : NavNCLException
{
	public NavTenantDataMoveException()
	{
	}

	public NavTenantDataMoveException(string message)
		: base(message)
	{
	}

	protected NavTenantDataMoveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavTenantDataMoveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public static NavTenantDataMoveException CreateForNonoperationalDatabase(string tenantDatabaseId, NavTenantDatabaseSettingsDetailedState actualState)
	{
		return new NavTenantDataMoveException(string.Format(CultureInfo.InvariantCulture, "Tenant database {0} is not in the Operational state. Actual database state is {1}. Details: {2}.", tenantDatabaseId, actualState.State, actualState.StateDetails));
	}
}
