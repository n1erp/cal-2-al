using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class NavAppOperationInvoker : INavAppOperationInvoker
{
	private readonly INavAppExternalDeployerProvider provider;

	public NavAppOperationInvoker(INavAppExternalDeployerProvider provider)
	{
		this.provider = provider ?? throw new ArgumentNullException("provider");
	}

	public UploadPackageOperationResponse UploadPackage(string tenantId, EnvironmentInfo environmentInfo, DeploymentSchedule deploymentSchedule, string installLanguage, Stream packageStream)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Uploading per-tenant package -- Tenant:{0}; ReleaseVersion:{1}; Language:{2}; Schedule:{3}", tenantId, environmentInfo?.ReleaseVersion, installLanguage, deploymentSchedule);
		using (DiagnosticsResolver.CreateTraceScope("00001RJ", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			return InvokeOperation((INavAppExternalDeployer d) => d.UploadPackage(packageStream, tenantId, environmentInfo?.ReleaseVersion, installLanguage, deploymentSchedule));
		}
	}

	public DeployTargetOperationResponse DeployTarget(string tenantId, Guid appid, EnvironmentInfo environmentInfo, string language)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Publishing target extension -- Tenant:{0}; AppId:{1}; ReleaseVersion:{2}; Language:{3}", tenantId, appid, environmentInfo?.ReleaseVersion, language);
		using (DiagnosticsResolver.CreateTraceScope("00001RK", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			return InvokeOperation((INavAppExternalDeployer d) => d.DeployTarget(AppId.op_Implicit(appid), tenantId, environmentInfo?.ReleaseVersion, language));
		}
	}

	public RefreshOperationResponse RefreshOperation(NavAppTenantOperationRecord operation)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Refreshing external publish status -- JobId:{0}", operation.Metadata.JobId);
		using (DiagnosticsResolver.CreateTraceScope("00001RL", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			return InvokeOperation((INavAppExternalDeployer d) => d.RefreshDeploymentStatus(operation.Metadata.JobId));
		}
	}

	public RefreshOperationDetailsResponse GetDetailedOperationStatus(NavAppTenantOperationRecord operation)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Retriving external deploy Detailed status message -- JobId:{0}", operation.Metadata.JobId);
		using (DiagnosticsResolver.CreateTraceScope("00001Y2", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			return InvokeOperation((INavAppExternalDeployer d) => d.RefreshDetailedDeploymentStatus(operation.Metadata.JobId));
		}
	}

	private TResponse InvokeOperation<TResponse>(Func<INavAppExternalDeployer, TResponse> task) where TResponse : class
	{
		try
		{
			using INavAppExternalDeployer arg = provider.CreateDeployer();
			return task(arg);
		}
		catch (Exception ex)
		{
			string text = null;
			if (ex is AggregateException ex2)
			{
				text = string.Join("; ", ex2.InnerExceptions.Select((Exception ex3) => ex3.Message));
			}
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001RI", Category.Extensions, new NavAppExternalDeployException(string.IsNullOrWhiteSpace(text) ? ex.Message : text));
		}
	}
}
