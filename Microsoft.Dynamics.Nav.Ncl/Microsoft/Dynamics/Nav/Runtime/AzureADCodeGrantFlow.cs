using System;
using System.Globalization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Dynamics.Nav.Authentication;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.Runtime.TokenCache;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AzureADCodeGrantFlow
{
	private readonly Uri redirectUri;

	private readonly NavTokenCacheBase navTokenCache;

	private readonly ServerUserSettings serverUserSettings;

	private string initialTenantDomainName;

	public AzureADCodeGrantFlow(Uri redirectUri, NavTokenCacheBase tokenCache, ServerUserSettings serverUserSettings)
	{
		if (redirectUri == null)
		{
			throw new ArgumentNullException("redirectUri");
		}
		if (tokenCache == null)
		{
			throw new ArgumentNullException("tokenCache");
		}
		if (serverUserSettings == null)
		{
			throw new ArgumentNullException("serverUserSettings");
		}
		this.redirectUri = redirectUri;
		navTokenCache = tokenCache;
		this.serverUserSettings = serverUserSettings;
	}

	public string AcquireTokenByAuthorizationCode(string authorizationCode, string resourceName)
	{
		if (string.IsNullOrWhiteSpace(authorizationCode))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "authorizationCode");
		}
		if (string.IsNullOrWhiteSpace(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		NavTenant navTenant = NavCurrentThread.Session?.Tenant;
		if (navTenant == null)
		{
			return null;
		}
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority("common");
		string aadTenantId = navTenant.TenantSettings.AadTenantId;
		SendTraceTag(PrivacyClassification.SystemMetadata, "Redeeming auth code for resource {0} against authority {1}.", resourceName, tokenAuthority);
		ISecureAuthenticationHandler secureAuthenticationHandler = navTenant.SecureAuthenticationHandler;
		AuthenticationInfo val;
		using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireTokenByAuthorizationCode", ServerUserSettings.Instance.LicensingServiceClientType))
		{
			val = secureAuthenticationHandler.AcquireTokenByAuthorizationCodeAsync(aadTenantId, tokenAuthority, authorizationCode, redirectUri, resourceName).WaitThrowingOriginalException();
		}
		if (val.TokenCacheState != null)
		{
			navTokenCache.PersistTokenCacheState(val.TenantId, val.UniqueUserId, val.TokenCacheState);
		}
		else
		{
			navTokenCache.StoreAadTenantId(val.TenantId);
		}
		return val.AccessToken;
	}

	public string AcquireTokenByAuthorizationCode(string authorizationCode, string clientId, string clientSecret, string resourceName)
	{
		if (string.IsNullOrWhiteSpace(authorizationCode))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "authorizationCode");
		}
		if (string.IsNullOrWhiteSpace(clientId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientId");
		}
		if (string.IsNullOrWhiteSpace(clientSecret))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientSecret");
		}
		if (string.IsNullOrWhiteSpace(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority("common");
		SendTraceTag(PrivacyClassification.SystemMetadata, "Redeeming auth code for resource {0} against authority {1} using On-Premise/PaaS client ID {2} and client secret.", resourceName, tokenAuthority, clientId);
		NavTenant navTenant = NavCurrentThread.Session?.Tenant;
		if (navTenant == null)
		{
			return null;
		}
		string aadTenantId = navTenant.TenantSettings.AadTenantId;
		IAuthenticationHandler authenticationHandler = navTenant.AuthenticationHandler;
		AuthenticationInfo val;
		using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireTokenByAuthorizationCode_With_ClientId_ClientSecret", ServerUserSettings.Instance.LicensingServiceClientType))
		{
			val = authenticationHandler.AcquireTokenByAuthorizationCodeAsync(aadTenantId, tokenAuthority, authorizationCode, redirectUri, resourceName, clientId, clientSecret).WaitThrowingOriginalException();
		}
		if (val.TokenCacheState != null)
		{
			navTokenCache.PersistTokenCacheState(val.TenantId, val.UniqueUserId, val.TokenCacheState);
		}
		else
		{
			navTokenCache.StoreAadTenantId(val.TenantId);
		}
		return val.AccessToken;
	}

	public string AcquireTokenFromCache(string resourceName)
	{
		if (string.IsNullOrWhiteSpace(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		AuthenticationInfo val = AcquireTokenFromCacheInternal(resourceName);
		if (val == null)
		{
			return null;
		}
		return val.AccessToken;
	}

	public string AcquireTokenFromCache(string clientId, string clientSecret, string resourceName)
	{
		if (string.IsNullOrWhiteSpace(clientId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientId");
		}
		if (string.IsNullOrWhiteSpace(clientSecret))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientSecret");
		}
		if (string.IsNullOrWhiteSpace(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		NavTenant navTenant = NavCurrentThread.Session?.Tenant;
		if (navTenant == null)
		{
			return null;
		}
		if (!TryGetValidIdentifiers(out var identifiers))
		{
			return null;
		}
		string aadTenantId = identifiers.AadTenantId;
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority(aadTenantId);
		string aadUserUniqueId = identifiers.AadUserUniqueId;
		navTokenCache.Refresh();
		IAuthenticationHandler authenticationHandler = navTenant.AuthenticationHandler;
		AuthenticationInfo val;
		using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireTokenFromCache_AcquireTokenSilent", ServerUserSettings.Instance.LicensingServiceClientType))
		{
			val = authenticationHandler.AcquireTokenSilentAsync(navTenant.TenantSettings.AadTenantId, tokenAuthority, resourceName, aadUserUniqueId, clientId, clientSecret, false, navTokenCache.Serialize()).WaitThrowingOriginalException();
		}
		if (val.TokenCacheState != null)
		{
			navTokenCache.PersistTokenCacheState(val.TenantId, val.UniqueUserId, val.TokenCacheState);
		}
		return val.AccessToken;
	}

	public string AcquireGuestToken(string resourceName, string guestTenantId)
	{
		if (string.IsNullOrWhiteSpace(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		if (string.IsNullOrWhiteSpace(guestTenantId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "guestTenantId");
		}
		if (guestTenantId.Contains("\\") || guestTenantId.Contains("/"))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidParameterFormat, guestTenantId, "String", "guestTenantId"));
		}
		ValidateServerAadConfiguration(out var appIdUri);
		AuthenticationInfo val = AcquireTokenFromCacheInternal(appIdUri);
		if (val != null)
		{
			NavTenant navTenant = NavCurrentThread.Session?.Tenant;
			if (navTenant == null)
			{
				return null;
			}
			string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority(guestTenantId);
			string aadTenantId = navTenant.TenantSettings.AadTenantId;
			SendTraceTag(PrivacyClassification.SystemMetadata, "Acquiring guest token against guest tenant {0} (authority {1}) for resource {2}.", guestTenantId, tokenAuthority, resourceName);
			ISecureAuthenticationHandler secureAuthenticationHandler = navTenant.SecureAuthenticationHandler;
			AuthenticationInfo val2;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireGuestToken_AcquireTokenSilent", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				val2 = secureAuthenticationHandler.AcquireTokenSilentAsync(aadTenantId, tokenAuthority, resourceName, val.UniqueUserId, true, val.TokenCacheState).WaitThrowingOriginalException();
			}
			if (val2 == null)
			{
				return null;
			}
			return val2.AccessToken;
		}
		return null;
	}

	public static string AcquireApplicationToken(string clientId, SecureString clientSecret, string authority, string resourceUri)
	{
		return AzureADGraphQuery.ExecuteActionRethrowAggregateException(() =>
		{
			NavTenant tenant = NavCurrentThread.Session?.Tenant;
			string aadTenantId = tenant.TenantSettings.AadTenantId;
			string cacheKey = clientId + resourceUri;
			bool flag = !NavEnvironment.Instance.Tenants.IsSingleTenancyMode && NavEnvironment.Instance.Tenants.TryGetTenantById(NavEnvironment.CurrentTenantId, out tenant);
			string fromCache;
			if (flag)
			{
				fromCache = tenant.GetFromCache<string>(cacheKey);
				if (fromCache != null)
				{
					return fromCache;
				}
			}
			IAuthenticationHandler authenticationHandler = tenant.AuthenticationHandler;
			AuthenticationInfo val;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireApplicationToken_AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				val = authenticationHandler.AcquireTokenAsync(aadTenantId, authority, resourceUri, clientId, clientSecret.ConvertToUnsecureString()).WaitThrowingOriginalException();
			}
			fromCache = val.AccessToken;
			if (flag)
			{
				tenant.AddToCache(cacheKey, fromCache, val.ExpiresOn.DateTime, TimeSpan.Zero);
			}
			return fromCache;
		}, "AcquireApplicationToken");
	}

	public static AuthenticationInfo AcquireTokenSilent(string resource, string aadUserid, string tokenCacheState)
	{
		if (string.IsNullOrEmpty(resource))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resource");
		}
		if (string.IsNullOrEmpty(aadUserid))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "aadUserid");
		}
		if (string.IsNullOrEmpty(tokenCacheState))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "tokenCacheState");
		}
		byte[] tokenCacheState2 = Convert.FromBase64String(tokenCacheState);
		return OpenAuthorizationTokenHelper.GetAccessTokenSilent(resource, aadUserid, tokenCacheState2);
	}

	public static AuthenticationInfo AcquireOnBehalfOfToken(string resourceName)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		_ = NavCurrentThread.Session;
		return AzureADAuthenticationHelper.GetOnBehalfOfToken(resourceName);
	}

	public static string AcquireOnBehalfOfTokenByCertificate(string resourceName, string clientId, X509Certificate2 certificate)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		if (string.IsNullOrEmpty(clientId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientId");
		}
		if (certificate == null)
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "certificate");
		}
		NavSession session = NavCurrentThread.Session;
		return OpenAuthorizationTokenHelper.ExchangeCurrentUserToken(session, clientId, certificate, resourceName).AccessToken;
	}

	public static string AcquireOnBehalfOfTokenBySecret(string resourceName, string clientId, SecureString secret)
	{
		if (string.IsNullOrEmpty(resourceName))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "resourceName");
		}
		if (string.IsNullOrEmpty(clientId))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "clientId");
		}
		if (secret == null || secret.Length == 0)
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "secret");
		}
		NavSession session = NavCurrentThread.Session;
		return OpenAuthorizationTokenHelper.ExchangeCurrentUserToken(session, clientId, secret, resourceName).AccessToken;
	}

	public string GetSaasClientId()
	{
		Guid value = serverUserSettings.AzureActiveDirectoryClientId.Value;
		if (!(value == Guid.Empty))
		{
			return value.ToString("D");
		}
		return null;
	}

	public string GetInitialDomainNameFromAad()
	{
		if (string.IsNullOrEmpty(initialTenantDomainName))
		{
			try
			{
				TenantInfo tenantDetail = new AzureADGraphQuery().GetTenantDetail();
				initialTenantDomainName = ((tenantDetail != null) ? tenantDetail.InitialDomain : null);
			}
			catch (NavLicenseServiceException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001ZY", Category.Runtime, exception);
			}
			if (string.IsNullOrEmpty(initialTenantDomainName))
			{
				NavLicenseServiceException ex = new NavLicenseServiceException(Lang.ResolveInitialTenantDomainFailed);
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001ZX", Category.Runtime, ex);
				throw ex;
			}
		}
		return initialTenantDomainName;
	}

	private bool TryGetValidIdentifiers(out UserTokenIdentifiers identifiers)
	{
		identifiers = null;
		UserTokenIdentifiers currentIdentifiers = navTokenCache.GetCurrentIdentifiers();
		if (!string.IsNullOrWhiteSpace(currentIdentifiers?.AadTenantId) && !string.IsNullOrWhiteSpace(currentIdentifiers.AadUserUniqueId))
		{
			identifiers = currentIdentifiers;
		}
		return identifiers != null;
	}

	private AuthenticationInfo AcquireTokenFromCacheInternal(string resourceName)
	{
		NavTenant navTenant = NavCurrentThread.Session?.Tenant;
		if (navTenant == null)
		{
			return null;
		}
		if (!TryGetValidIdentifiers(out var identifiers))
		{
			return null;
		}
		string aadTenantId = identifiers.AadTenantId;
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority(aadTenantId);
		string aadUserUniqueId = identifiers.AadUserUniqueId;
		navTokenCache.Refresh();
		SendTraceTag(PrivacyClassification.SystemMetadata, "Redeeming auth code for resource {0} against authority {1}.", resourceName, tokenAuthority);
		ISecureAuthenticationHandler secureAuthenticationHandler = navTenant.SecureAuthenticationHandler;
		AuthenticationInfo val;
		using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "AcquireTokenFromCacheInternal_AcquireTokenSilent", ServerUserSettings.Instance.LicensingServiceClientType))
		{
			val = secureAuthenticationHandler.AcquireTokenSilentAsync(navTenant.TenantSettings.AadTenantId, tokenAuthority, resourceName, aadUserUniqueId, false, navTokenCache.Serialize()).WaitThrowingOriginalException();
		}
		if (val.TokenCacheState != null)
		{
			navTokenCache.PersistTokenCacheState(val.TenantId, val.UniqueUserId, val.TokenCacheState);
		}
		return val;
	}

	private void ValidateServerAadConfiguration(out string appIdUri)
	{
		appIdUri = serverUserSettings.AppIdUri.Value.OriginalString;
		if (string.IsNullOrWhiteSpace(appIdUri))
		{
			throw new NavConfigurationException(Lang.ConfigurationInvalidValue, "AppIdUri");
		}
	}

	private static void SendTraceTag(PrivacyClassification classification, string messageFormat, params object[] parameters)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000DC", Category.Security, Verbosity.Normal, classification, messageFormat, parameters);
	}
}
