using System;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal class ExceptionalExternalDeployer : INavAppExternalDeployer, IDisposable
{
	public UploadPackageOperationResponse UploadPackage(Stream packageStream, string tenantId, Version releaseVersion, string language, DeploymentSchedule schedule)
	{
		throw new NavAppExternalDeployException(Lang.NavAppExternalDeploymentNotEnabled.FormatWithResolvedCulture());
	}

	public DeployTargetOperationResponse DeployTarget(AppId appId, string tenantId, Version releaseVersion, string language)
	{
		throw new NavAppExternalDeployException(Lang.NavAppExternalDeploymentNotEnabled.FormatWithResolvedCulture());
	}

	public RefreshOperationResponse RefreshDeploymentStatus(Guid jobId)
	{
		throw new NavAppExternalDeployException(Lang.NavAppExternalDeploymentNotEnabled.FormatWithResolvedCulture());
	}

	public RefreshOperationDetailsResponse RefreshDetailedDeploymentStatus(Guid jobId)
	{
		throw new NavAppExternalDeployException(Lang.NavAppExternalDeploymentNotEnabled.FormatWithResolvedCulture());
	}

	public void Dispose()
	{
	}
}
