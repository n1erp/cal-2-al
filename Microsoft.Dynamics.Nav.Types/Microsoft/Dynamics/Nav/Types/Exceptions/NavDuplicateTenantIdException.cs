using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavDuplicateTenantIdException : NavException
{
	public NavDuplicateTenantIdException()
	{
	}

	public NavDuplicateTenantIdException(string message)
		: base(message)
	{
	}

	public NavDuplicateTenantIdException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavDuplicateTenantIdException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavDuplicateTenantIdException CreateForDuplicateInDatabase(string tenantId, string databaseName)
	{
		return new NavDuplicateTenantIdException(string.Format(CultureInfo.InvariantCulture, "A tenant with ID {0} already exists in database {1}.", tenantId, databaseName));
	}
}
