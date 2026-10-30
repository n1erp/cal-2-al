namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface IEnvironmentInfoProvider
{
	EnvironmentInfo GetInfoForTenant(string tenantId);
}
