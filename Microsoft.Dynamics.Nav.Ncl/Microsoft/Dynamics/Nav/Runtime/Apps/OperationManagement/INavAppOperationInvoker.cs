using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppOperationInvoker
{
	UploadPackageOperationResponse UploadPackage(string tenantId, EnvironmentInfo environmentInfo, DeploymentSchedule deploymentSchedule, string installLanguage, Stream packageStream);

	DeployTargetOperationResponse DeployTarget(string tenantId, Guid appid, EnvironmentInfo environmentInfo, string installLanguage);

	RefreshOperationResponse RefreshOperation(NavAppTenantOperationRecord operation);

	RefreshOperationDetailsResponse GetDetailedOperationStatus(NavAppTenantOperationRecord operation);
}
