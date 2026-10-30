namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppTenantOperationManagementProvider
{
	NavAppTenantOperationManagement CreateForTenant(NavTenant tenant);
}
