using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavSetTenantException : NavException
{
	public NavSetTenantException()
	{
	}

	public NavSetTenantException(string message)
		: base(message)
	{
	}

	public NavSetTenantException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavSetTenantException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavSetTenantException CreateForSettingMountedTenantToBuffer(TenantId tenantId)
	{
		return new NavSetTenantException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} cannot be set to buffer tenant because it is mounted. Dismount the tenant first.", tenantId));
	}

	public static NavSetTenantException CreateForSettingNonNormalTenantToBuffer(TenantId tenantId)
	{
		return new NavSetTenantException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} cannot be set to a buffer tenant because its type isn't compatible. Could be of type external, system or buffer.", tenantId));
	}
}
