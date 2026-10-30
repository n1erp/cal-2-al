namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppDeployTargetOperationMetadata : NavAppDeployOperationMetadata, INavAppDeployOperationKey, INavAppOperationKey
{
	public override string Key => this.GetOperationKey();
}
