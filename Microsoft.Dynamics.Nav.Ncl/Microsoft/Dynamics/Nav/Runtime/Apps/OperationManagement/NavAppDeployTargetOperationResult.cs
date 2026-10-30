using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppDeployTargetOperationResult : NavAppOperationMetadataResult, INavAppDeployOperationKey, INavAppOperationKey
{
	public override NavAppTenantOperationFields.NavAppOperationType OperationType => NavAppTenantOperationFields.NavAppOperationType.DeployTarget;

	public override string Key => this.GetOperationKey();
}
