using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Authentication;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AzureADAuthenticationHelper
{
	private readonly NavTenant tenant;

	internal string ClientId { get; private set; }

	internal string AadTenantId { get; private set; }

	internal static string GraphApiEndpoint
	{
		get
		{
			string text = ServerUserSettings.Instance.GraphApiEndpoint.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Format(CultureInfo.InvariantCulture, "https://graph.microsoft.com/{0}", string.Empty);
			}
			return text.TrimEnd('/');
		}
	}

	internal static string OfficePortalEndpoint
	{
		get
		{
			string text = ServerUserSettings.Instance.OfficePortalEndpoint.ToString();
			if (string.IsNullOrWhiteSpace(text))
			{
				if (!"https://graph.microsoft.com/{0}".StartsWith(GraphApiEndpoint, StringComparison.OrdinalIgnoreCase))
				{
					return "https://portal.officeppe.com";
				}
				return "https://portal.office.com";
			}
			return text.TrimEnd('/');
		}
	}

	internal AzureADAuthenticationHelper(NavTenant tenant)
		: this(tenant?.TenantSettings.AadTenantId ?? ServerUserSettings.Instance.AadTenantId)
	{
		ClientId = ServerUserSettings.Instance.AzureActiveDirectoryClientId.Value.ToString();
		this.tenant = tenant;
	}

	internal AzureADAuthenticationHelper(string aadTenantId)
	{
		AadTenantId = aadTenantId;
		if (string.IsNullOrWhiteSpace(AadTenantId))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationInvalidValue, AadTenantId));
		}
	}

	internal Task<string> AcquireTokenAsyncForApplication(string resourceUri)
	{
		return Task.FromResult(GetTokenForApplication(resourceUri));
	}

	internal string GetTokenForApplication(string resourceUri)
	{
		return AzureADGraphQuery.ExecuteActionRethrowAggregateException(() =>
		{
			string tokenAuthority = GetTokenAuthority(AadTenantId);
			ISecureAuthenticationHandler secureAuthenticationHandler = tenant.SecureAuthenticationHandler;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "GetTokenForApplication_AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				return secureAuthenticationHandler.AcquireTokenAsync(AadTenantId, tokenAuthority, resourceUri).WaitThrowingOriginalException().AccessToken;
			}
		}, "GetTokenForApplication");
	}

	internal static AuthenticationInfo GetOnBehalfOfToken(string resource)
	{
		return OpenAuthorizationTokenHelper.ExchangeCurrentUserToken(NavCurrentThread.Session, resource);
	}

	internal static string GetAuthorizationHeaderFromBasicUserCredentials(string username, string password)
	{
		string s = username + ":" + password;
		return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(s));
	}

	internal static string GetTokenAuthority(string domainNameOrTenantId)
	{
		if (string.IsNullOrWhiteSpace(domainNameOrTenantId) || domainNameOrTenantId.Equals("{AADTENANTID}", StringComparison.OrdinalIgnoreCase))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000LU", Category.Security, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, "The aad tenant id has been replaced: Original value: {0}. New value: {1}", domainNameOrTenantId, "common");
			domainNameOrTenantId = "common";
		}
		Uri value = ServerUserSettings.Instance.TokenAuthorityEndpoint.Value;
		if (string.IsNullOrWhiteSpace(value.ToString()))
		{
			return GetDefaultTokenAuthority(domainNameOrTenantId);
		}
		return new Uri(value, domainNameOrTenantId).ToString();
	}

	private static string GetDefaultTokenAuthority(string domainNameOrTenantId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "https://login.windows.net/{0}", domainNameOrTenantId);
		return stringBuilder.ToString();
	}
}
