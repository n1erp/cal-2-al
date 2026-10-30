using System;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;
using Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy.ProxySvc;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal class ServerConfigurationExternalDeployerProvider : INavAppExternalDeployerProvider
{
	private readonly ServerUserSettings settings;

	public ServerConfigurationExternalDeployerProvider(ServerUserSettings settings)
	{
		this.settings = settings ?? throw new ArgumentNullException("settings");
	}

	public INavAppExternalDeployer CreateDeployer()
	{
		if (!string.IsNullOrWhiteSpace(settings.ExternalNavAppDeploymentEndpoint.Value.OriginalString))
		{
			return new ProxySvcExternalDeployer(BuildProxyDeployerClient());
		}
		return new ExceptionalExternalDeployer();
	}

	private IProxySvcClient BuildProxyDeployerClient()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		ProxySvcClientBuilder val = new ProxySvcClientBuilder(settings.ExternalNavAppDeploymentEndpoint.Value).WithTelemetry((IPrivacyClassifiedTelemetry)(object)new NavAppDiagnosticTelemetry()).WithApiKey(settings.LocalServiceApiKey?.Value).WithTimeout((int)settings.ExternalNavAppDeploymentRequestTimeout.Value.TotalMilliseconds);
		string text = settings.ReverseProxyCertificateThumbprint?.Value;
		val = ((!string.IsNullOrWhiteSpace(text)) ? val.UseHttpsCertificateValidation(text, false) : val);
		return val.Build();
	}
}
