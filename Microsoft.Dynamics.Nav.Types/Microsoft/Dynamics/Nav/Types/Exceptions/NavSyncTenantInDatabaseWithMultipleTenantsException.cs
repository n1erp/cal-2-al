using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavSyncTenantInDatabaseWithMultipleTenantsException : NavException
{
	private const string TenantIdName = "TenantId";

	private const string TenantDatabaseIdName = "TenantDatabaseId";

	public string TenantId { get; set; }

	public string TenantDatabaseId { get; set; }

	public NavSyncTenantInDatabaseWithMultipleTenantsException()
	{
	}

	public NavSyncTenantInDatabaseWithMultipleTenantsException(string message)
		: base(message)
	{
	}

	public NavSyncTenantInDatabaseWithMultipleTenantsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavSyncTenantInDatabaseWithMultipleTenantsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		TenantId = new TenantId(info.GetString("TenantId"));
		TenantDatabaseId = new TenantDatabaseId(info.GetString("TenantDatabaseId"));
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TenantId", TenantId);
		info.AddValue("TenantDatabaseId", TenantDatabaseId);
	}

	public static NavSyncTenantInDatabaseWithMultipleTenantsException Create(string tenantId, string tenantDatabaseId)
	{
		return new NavSyncTenantInDatabaseWithMultipleTenantsException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} in tenant database {1} cannot be synchronized because the database contains multiple tenants. You can synchronize the database instead of the tenant.", tenantId, tenantDatabaseId))
		{
			TenantId = tenantId,
			TenantDatabaseId = tenantDatabaseId
		};
	}

	public static NavSyncTenantInDatabaseWithMultipleTenantsException CreateForCmdlet(string tenantId, string tenantDatabaseId, Exception innerException = null)
	{
		return new NavSyncTenantInDatabaseWithMultipleTenantsException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} in tenant database {1} cannot be synchronized by using the Sync-NAVTenant cmdlet because the database contains multiple tenants. Use the Sync-NAVTenantDatabase cmdlet on tenant database {1} instead.", tenantId, tenantDatabaseId), innerException)
		{
			TenantId = tenantId,
			TenantDatabaseId = tenantDatabaseId
		};
	}
}
