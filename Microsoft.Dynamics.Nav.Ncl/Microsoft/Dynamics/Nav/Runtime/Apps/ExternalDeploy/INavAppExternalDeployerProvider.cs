namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal interface INavAppExternalDeployerProvider
{
	INavAppExternalDeployer CreateDeployer();
}
