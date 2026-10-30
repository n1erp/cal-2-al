using System;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal interface INavAppExternalDeployer : IDisposable
{
	UploadPackageOperationResponse UploadPackage(Stream packageStream, string tenantId, Version releaseVersion, string language, DeploymentSchedule schedule);

	DeployTargetOperationResponse DeployTarget(AppId appId, string tenantId, Version releaseVersion, string language);

	RefreshOperationResponse RefreshDeploymentStatus(Guid jobId);

	RefreshOperationDetailsResponse RefreshDetailedDeploymentStatus(Guid jobId);
}
