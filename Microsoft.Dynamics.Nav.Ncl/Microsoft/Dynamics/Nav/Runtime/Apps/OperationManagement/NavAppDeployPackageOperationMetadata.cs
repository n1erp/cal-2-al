using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppDeployPackageOperationMetadata : NavAppDeployOperationMetadata, INavAppVersionedDeployOperationKey, INavAppDeployOperationKey, INavAppOperationKey
{
	public Version Version { get; set; }

	public override string Key => this.GetOperationKey();
}
