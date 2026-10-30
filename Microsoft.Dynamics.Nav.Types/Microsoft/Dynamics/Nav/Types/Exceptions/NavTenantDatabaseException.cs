using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantDatabaseException : NavException
{
	public NavTenantDatabaseException()
	{
	}

	public NavTenantDatabaseException(string message)
		: base(message)
	{
	}

	public NavTenantDatabaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantDatabaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTenantDatabaseException CreateForDatabaseEntryNotFound(string id)
	{
		return new NavTenantDatabaseException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, id));
	}

	public static NavTenantDatabaseException CreateForCannotRegisterIfDatabaseNotOperational(string id, NavTenantDatabaseState actualState)
	{
		return new NavTenantDatabaseException(string.Format(CultureInfo.InvariantCulture, "Tenant database {0} is not in the Operational state, so you cannot register tenants in it. The actual database state is {1}.", id, EnumHelper<NavTenantDatabaseState>.EnumToString(actualState)));
	}
}
