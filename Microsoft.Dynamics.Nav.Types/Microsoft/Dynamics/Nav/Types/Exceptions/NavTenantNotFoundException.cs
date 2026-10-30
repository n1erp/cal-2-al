using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantNotFoundException : NavException
{
	public const string TenantNotFoundFaultCode = "TenantNotFoundFaultCode";

	public NavTenantNotFoundException()
	{
	}

	public NavTenantNotFoundException(string message)
		: base(message)
	{
	}

	public NavTenantNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTenantNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static FaultException CreateFaultException(string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode("TenantNotFoundFaultCode"));
		return new FaultException(reason, code);
	}

	public static NavTenantNotFoundException CreateForGeneralNotFound(string tenantId)
	{
		return new NavTenantNotFoundException(string.Format(CultureInfo.InvariantCulture, "The tenant '{0}' was not found.", tenantId));
	}

	public static NavTenantNotFoundException CreateForMountedTenantNotFound(string tenantId)
	{
		return new NavTenantNotFoundException(string.Format(CultureInfo.InvariantCulture, "The tenant '{0}' is not mounted or does not exist.", tenantId));
	}

	public static NavTenantNotFoundException CreateForTenantRequiredButNotSpecified()
	{
		return new NavTenantNotFoundException("A tenant was not specified, but is needed because the system has multiple tenants. Contact your system administrator.");
	}

	public static NavTenantNotFoundException CreateForNoTenantsMounted()
	{
		return new NavTenantNotFoundException("There are no tenants mounted on the system. Contact your system administrator.");
	}
}
