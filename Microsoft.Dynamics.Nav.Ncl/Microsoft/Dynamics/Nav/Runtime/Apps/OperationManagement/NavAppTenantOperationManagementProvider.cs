using Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppTenantOperationManagementProvider : INavAppTenantOperationManagementProvider
{
	public NavAppTenantOperationManagement CreateForTenant(NavTenant tenant)
	{
		return new NavAppTenantOperationManagement(new DatabaseNavAppTenantOperationRepository(tenant), new NavAppOperationInvoker(new ServerConfigurationExternalDeployerProvider(ServerUserSettings.Instance)), new SystemCodeunitEnvironmentInfoProvider());
	}
}
