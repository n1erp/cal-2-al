using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.LicensingService.Model.Exceptions;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions.Authentication;

namespace Microsoft.Dynamics.Nav.Authentication;

public static class OpenAuthorizationTokenHelper
{
	private const string ServiceUnavailableErrorCode = "temporarily_unavailable";

	public static AuthenticationInfo ExchangeCurrentUserToken(NavSession userSession, string resource)
	{
		return ExchangeCurrentUserToken(userSession, resource, (string aadTenantId, string userAccessToken, string authority, string userAssertionType, string userName) =>
		{
			ISecureAuthenticationHandler secureAuthenticationHandler = userSession.Tenant.SecureAuthenticationHandler;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "ExchangeCurrentUserToken_ISecureAuthenticationHandler.AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				return secureAuthenticationHandler.AcquireTokenAsync(aadTenantId, userAccessToken, authority, resource, userAssertionType, userName).WaitThrowingOriginalException();
			}
		});
	}

	public static AuthenticationInfo ExchangeCurrentUserToken(NavSession userSession, string clientId, X509Certificate2 certificate, string resource)
	{
		return ExchangeCurrentUserToken(userSession, resource, (string aadTenantId, string userAccessToken, string authority, string userAssertionType, string userName) =>
		{
			IAuthenticationHandler authenticationHandler = userSession.Tenant.AuthenticationHandler;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "ExchangeCurrentUserToken_IAuthenticationHandler.AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				return authenticationHandler.AcquireTokenAsync(aadTenantId, userAccessToken, authority, resource, userAssertionType, userName, clientId, certificate).WaitThrowingOriginalException();
			}
		});
	}

	public static AuthenticationInfo GetAccessTokenSilent(string resource, string aadUserId, byte[] tokenCacheState)
	{
		return GetAccessTokenSilent(NavCurrentThread.Session, (string aadTenantId, string authority) =>
		{
			ISecureAuthenticationHandler secureAuthenticationHandler = NavCurrentThread.Session.Tenant.SecureAuthenticationHandler;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "GetAccessTokenSilent_ISecureAuthenticationHandler.AcquireTokenByRefreshToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				return secureAuthenticationHandler.AcquireTokenSilentAsync(aadTenantId, authority, resource, aadUserId, true, tokenCacheState).WaitThrowingOriginalException();
			}
		});
	}

	public static AuthenticationInfo ExchangeCurrentUserToken(NavSession userSession, string clientId, SecureString credential, string resource)
	{
		return ExchangeCurrentUserToken(userSession, resource, (string aadTenantId, string userAccessToken, string authority, string userAssertionType, string userName) =>
		{
			IAuthenticationHandler authenticationHandler = userSession.Tenant.AuthenticationHandler;
			using (new IfxTimingScope(NavAuthenticationHandlerFactory.AuthClientDurationMetricInstance, "ExchangeCurrentUserToken_IAuthenticationHandler.AcquireToken", ServerUserSettings.Instance.LicensingServiceClientType))
			{
				return authenticationHandler.AcquireTokenAsync(aadTenantId, userAccessToken, authority, resource, userAssertionType, userName, clientId, credential.ConvertToUnsecureString()).WaitThrowingOriginalException();
			}
		});
	}

	private static AuthenticationInfo ExchangeCurrentUserToken(NavSession userSession, string resource, Func<string, string, string, string, string, AuthenticationInfo> acquireTokenFunc)
	{
		//IL_0156: Expected Obj, but got Unknown
		INavSecurityToken navSecurityToken = userSession?.Authenticator?.SecurityToken;
		if (navSecurityToken == null)
		{
			userSession.Diagnostics.SendTraceTag("00001HG", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "An on-behalf-of token cannot be acquired because current user did not authenticate with a security token.");
			throw NavNCLTokenAcquisitionFailedException.CreateForResource(resource);
		}
		string text = navSecurityToken.SerializedToken?.ConvertToUnsecureString();
		if (string.IsNullOrEmpty(text))
		{
			userSession.Diagnostics.SendTraceTag("00001HH", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "The current session was created with a security token that does not support the on-behalf-of process.");
			throw NavNCLTokenAcquisitionFailedException.CreateForResource(resource);
		}
		string openAuthorizationGrantType = navSecurityToken.OpenAuthorizationGrantType;
		if (string.IsNullOrEmpty(openAuthorizationGrantType))
		{
			userSession.Diagnostics.SendTraceTag("00001HI", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "The current session was created with a security token type that does not support the on-behalf-of process.");
			throw NavNCLTokenAcquisitionFailedException.CreateForResource(resource);
		}
		string authorizationEmail = navSecurityToken.AuthorizationEmail;
		string arg = "urn:ietf:params:oauth:grant-type:" + openAuthorizationGrantType;
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority(userSession.Tenant.TenantSettings.AadTenantId);
		AuthenticationInfo val = null;
		int num = 0;
		bool flag;
		do
		{
			flag = false;
			try
			{
				userSession.Diagnostics.SendTraceTag("00001TE", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring on behalf of token for resource " + resource);
				using (userSession.Diagnostics.CreateTimingScope("00001HM", Category.Security, DiagnosticParameter.SystemMetadata("Acquiring on behalf of token for resource")))
				{
					val = acquireTokenFunc(userSession.Tenant.TenantSettings.AadTenantId, text, tokenAuthority, arg, authorizationEmail);
				}
			}
			catch (LicenseServiceAdalException ex)
			{
				LicenseServiceAdalException ex2 = ex;
				if (ex2.ErrorCode == "temporarily_unavailable")
				{
					userSession.Diagnostics.SendTraceTag("00001HJ", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquire on behalf of token failed due to irresponsive service. Retry will start shortly.");
					flag = true;
					num++;
					Thread.Sleep(100);
					continue;
				}
				userSession.Diagnostics.SendExceptionTag("00001HK", Category.Security, (Exception)(object)ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Unable to acquire on-behalf-of token."));
				throw NavNCLTokenAcquisitionFailedException.CreateForResource(resource);
			}
		}
		while (flag && num < 2);
		if (string.IsNullOrEmpty((val != null) ? val.AccessToken : null))
		{
			userSession.Diagnostics.SendTraceTag("00001HL", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Unable to acquire on-behalf-of token, the returned token was empty.");
			throw NavNCLTokenAcquisitionFailedException.CreateForResource(resource);
		}
		return val;
	}

	private static AuthenticationInfo GetAccessTokenSilent(NavSession userSession, Func<string, string, AuthenticationInfo> acquireTokenFunc)
	{
		//IL_0082: Expected Obj, but got Unknown
		int num = 0;
		AuthenticationInfo val = null;
		string tokenAuthority = AzureADAuthenticationHelper.GetTokenAuthority(userSession.Tenant.TenantSettings.AadTenantId);
		bool flag;
		do
		{
			flag = false;
			try
			{
				userSession.Diagnostics.SendTraceTag("00001WP", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring access token from refresh token");
				using (userSession.Diagnostics.CreateTimingScope("00001WS", Category.Security, DiagnosticParameter.SystemMetadata("Acquiring access token from refresh token")))
				{
					val = acquireTokenFunc(userSession.Tenant.TenantSettings.AadTenantId, tokenAuthority);
				}
			}
			catch (LicenseServiceAdalException ex)
			{
				LicenseServiceAdalException ex2 = ex;
				if (ex2.ErrorCode == "temporarily_unavailable")
				{
					userSession.Diagnostics.SendTraceTag("00001WQ", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquire token from refresh token failed due to irresponsive service. Retry will start shortly.");
					flag = true;
					num++;
					Thread.Sleep(100);
					continue;
				}
				userSession.Diagnostics.SendExceptionTag("00001WO", Category.Security, (Exception)(object)ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Unable to acquire access token from refresh token."));
				throw new NavNCLTokenAcquisitionFailedException();
			}
		}
		while (flag && num < 2);
		if (string.IsNullOrEmpty((val != null) ? val.AccessToken : null))
		{
			userSession.Diagnostics.SendTraceTag("00001WR", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Unable to acquire access token from refresh token, the returned token was empty.");
			throw new NavNCLTokenAcquisitionFailedException();
		}
		return val;
	}
}
