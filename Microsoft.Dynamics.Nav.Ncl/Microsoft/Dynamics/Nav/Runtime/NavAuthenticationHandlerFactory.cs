using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Client;
using Microsoft.Dynamics.Nav.LicensingService.Client.Authentication;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavAuthenticationHandlerFactory
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TokenCacheStateProviderDelegate _003C_003E9__5_0;

		internal byte[] _003CCreateAuthenticationHandlerSetup_003Eb__5_0()
		{
			if (NavCurrentThread.Session != null)
			{
				return ALAzureAdCodeGrantFlow.LoadSessionTokenCache(NavCurrentThread.Session).Serialize();
			}
			return null;
		}
	}

	private static NavMultidimensionMetric authClientDurationMetric;

	public static NavMultidimensionMetric AuthClientDurationMetricInstance
	{
		get
		{
			if (authClientDurationMetric == null)
			{
				authClientDurationMetric = new NavMultidimensionMetric("AuthClientDurationMetric", "Method", "ClientType");
			}
			return authClientDurationMetric;
		}
	}

	public static IAuthenticationHandler CreateHandler(NavDiagnostics diagnosticsInstance)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected Obj, but got Unknown
		if (ServerUserSettings.Instance.EnableLicensingService.Value)
		{
			string text = ServerUserSettings.Instance.LicensingServiceUri.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				diagnosticsInstance?.SendTraceTag("00001SR", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authentication handler connected using uri: {0}", text);
				return ProxyClientFactory.CreateAuthenticationHandlerProxy(new Uri(text), ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
			}
			diagnosticsInstance?.SendTraceTag("0000238", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Default authentication handler connected using default uri");
			return ProxyClientFactory.CreateDefaultAuthenticationHandlerProxy(ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
		}
		diagnosticsInstance?.SendTraceTag("00001SS", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authentication handler connected using on-prem implementation");
		return (IAuthenticationHandler)new AuthenticationHandler(CreateAuthenticationHandlerSetup(diagnosticsInstance));
	}

	public static ISecureAuthenticationHandler CreateSecureHandler(NavDiagnostics diagnosticsInstance, IAuthenticationHandler authenticationHandler)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected Obj, but got Unknown
		if (ServerUserSettings.Instance.EnableLicensingService.Value)
		{
			string text = ServerUserSettings.Instance.LicensingServiceUri.ToString();
			if (text != null && !string.IsNullOrEmpty(text))
			{
				diagnosticsInstance?.SendTraceTag("00001ST", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Secure Authentication handler connected using uri: {0}", text);
				return ProxyClientFactory.CreateSecureAuthenticationHandlerProxy(new Uri(text), ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
			}
			diagnosticsInstance?.SendTraceTag("0000239", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Default secure authentication handler connected using default uri");
			return ProxyClientFactory.CreateSecureAuthenticationHandlerProxy(ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
		}
		diagnosticsInstance?.SendTraceTag("00001SU", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Secure Authentication handler connected using on-prem implementation");
		return (ISecureAuthenticationHandler)new SecureAuthenticationHandler(CreateSecureAuthenticationHandlerSetup(), (AuthenticationHandler)(object)((authenticationHandler is AuthenticationHandler) ? authenticationHandler : null));
	}

	private static AuthenticationHandlerSetup CreateAuthenticationHandlerSetup(NavDiagnostics diagnosticsInstance)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected Obj, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected Obj, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected Obj, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		AuthenticationHandlerSetup val = new AuthenticationHandlerSetup();
		TokenCacheStateProviderDelegate val2 = _003C_003Ec._003C_003E9__5_0;
		if (val2 == null)
		{
			TokenCacheStateProviderDelegate val3 = () => (NavCurrentThread.Session != null) ? ALAzureAdCodeGrantFlow.LoadSessionTokenCache(NavCurrentThread.Session).Serialize() : null;
			_003C_003Ec._003C_003E9__5_0 = val3;
			val2 = val3;
		}
		val.TokenCacheStateProvider = val2;
		val.TraceProvider = (string tag, string message, object[] parameters) =>
		{
			diagnosticsInstance?.SendTraceTag("00001WL", Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "{0}: {1}", tag, message), parameters);
		};
		val.ProcessTraceProvider = (string tag, string name) => diagnosticsInstance?.CreateTraceScope("00001WM", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", tag, name)), MonitorType.None, Category.License);
		val.ErrorTraceProvider = (string tag, Exception exception) =>
		{
			diagnosticsInstance?.SendExceptionTag(tag, Category.Security, exception);
		};
		val.EuiiTraceProviderDelegate = (string tag, string message, object[] parameters) =>
		{
			diagnosticsInstance?.SendTraceTag(tag, Category.Security, Verbosity.Normal, PrivacyClassification.EndUserIdentifiableInformation, message, parameters);
		};
		val.CertificateProvider = CertificateHelper.FindCertificateFromThumbprint;
		return val;
	}

	private static SecureAuthenticationHandlerSetup CreateSecureAuthenticationHandlerSetup()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected Obj, but got Unknown
		string value = ServerUserSettings.Instance.AzureActiveDirectoryClientCertificateThumbprint.Value;
		SecureAuthenticationHandlerSetup val = new SecureAuthenticationHandlerSetup
		{
			Certificate = (string.IsNullOrEmpty(value) ? null : CertificateHelper.FindCertificateFromThumbprint(value)),
			ClientId = ServerUserSettings.Instance.AzureActiveDirectoryClientId.Value.ToString(),
			Secret = ServerUserSettings.Instance.AzureActiveDirectoryClientSecret.Value
		};
		ValidateSetup(val);
		return val;
	}

	private static void ValidateSetup(SecureAuthenticationHandlerSetup setup)
	{
		if (string.IsNullOrEmpty(setup.ClientId))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, "AzureActiveDirectoryClientId"));
		}
		if (setup.Certificate == null && string.IsNullOrEmpty(setup.Secret))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.GraphAuthenticationConfigurationMismatch, "AzureActiveDirectoryClientCertificateThumbprint", "AzureActiveDirectoryClientSecret"));
		}
	}
}
