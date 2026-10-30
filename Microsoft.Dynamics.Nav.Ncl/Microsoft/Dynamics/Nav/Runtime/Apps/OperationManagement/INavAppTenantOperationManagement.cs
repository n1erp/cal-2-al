using System;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppTenantOperationManagement
{
	void RefreshOperationStatus(Guid operationId);

	Guid UploadPackage(string language, INavAppReference appInfo, DeploymentSchedule deploymentSchedule, Stream packageStream);

	Guid DeployTarget(string language, Guid appId);

	void NotifyExtensionInstallOperation(NavAppOperationMetadataResult installResult);

	string GetOperationDetailedStatusMessage(Guid operationId);

	NavAppDeployOperationMetadata GetDeployOperationMetadata(Guid operationId);
}
