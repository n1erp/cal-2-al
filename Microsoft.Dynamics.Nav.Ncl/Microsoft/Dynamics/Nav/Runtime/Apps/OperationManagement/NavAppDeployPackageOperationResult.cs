using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppDeployPackageOperationResult : NavAppOperationMetadataResult, INavAppVersionedDeployOperationKey, INavAppDeployOperationKey, INavAppOperationKey
{
	public override NavAppTenantOperationFields.NavAppOperationType OperationType => NavAppTenantOperationFields.NavAppOperationType.DeployPackage;

	public Version Version { get; set; }

	public override string Key => this.GetOperationKey();
}
