using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal class NavAppDiagnosticTelemetry : IPrivacyClassifiedTelemetry
{
	public IDiagnosticParameter CreateParameter(PrivacyClassification classification, object content)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return (IDiagnosticParameter)(object)new NavAppDiagnosticParameter
		{
			Classification = classification,
			Content = content
		};
	}

	public void SendExceptionTag(string message, params IDiagnosticParameter[] parameters)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XR", Category.Extensions, Verbosity.Error, message, (parameters.First() as NavAppDiagnosticParameter)?.ToDiagnosticParameter(), (from p in parameters.Skip(1).ToArray()
			select (p as NavAppDiagnosticParameter)?.ToDiagnosticParameter()).ToArray());
	}

	public void SendTraceTag(string message, params IDiagnosticParameter[] parameters)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XS", Category.Extensions, Verbosity.Normal, message, (parameters.First() as NavAppDiagnosticParameter)?.ToDiagnosticParameter(), (from p in parameters.Skip(1).ToArray()
			select (p as NavAppDiagnosticParameter)?.ToDiagnosticParameter()).ToArray());
	}

	public void SendWarningTag(string message, params IDiagnosticParameter[] parameters)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001XT", Category.Extensions, Verbosity.Warning, message, (parameters.First() as NavAppDiagnosticParameter)?.ToDiagnosticParameter(), (from p in parameters.Skip(1).ToArray()
			select (p as NavAppDiagnosticParameter)?.ToDiagnosticParameter()).ToArray());
	}
}
