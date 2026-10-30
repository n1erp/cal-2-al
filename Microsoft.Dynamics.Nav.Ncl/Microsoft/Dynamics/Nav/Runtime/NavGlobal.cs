using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavGlobal
{
	internal static INavAppRoutesUpdater NavAppRoutesUpdater;

	public static NavSystemTenant SystemTenant => NavEnvironment.Instance.Tenants.SystemTenant;

	public static NavTenant DefaultTenant => NavEnvironment.Instance.Tenants.DefaultTenant;

	public static NavDatabase AppDatabase => SystemTenant?.Database;

	public static NCLMetadata NCLMetadata => SystemTenant.NCLMetadata;

	public static INCLCodeLoader NCLCodeLoader => SystemTenant.NCLCodeLoader;

	public static INavAppClrTypeRetriever NavAppClrTypeRetriever => SystemTenant.NavAppClrTypeRetriever;

	public static INCLObjectXmlMetadataLoader NCLObjectXmlMetadataLoader => SystemTenant.NCLObjectXmlMetadataLoader;

	public static MetadataProvider MetadataProvider => SystemTenant.MetadataProvider;

	internal static INavAppTenantOperationManagementProvider NavAppTenantOperationManagementProvider => SystemTenant.NavAppTenantOperationManagementProvider;

	public static INavAppGroupResolver NavAppGroupResolver => NavEnvironment.Instance.NavAppGroupResolver;

	public static NavEventSubscriptionMetadata EventSubscriptionMetadata => SystemTenant.EventSubscriptionMetadata;
}
