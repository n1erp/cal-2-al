using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.LicensingService.Model.Exceptions;
using Microsoft.Dynamics.Nav.Runtime.TokenCache;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ALAzureAdCodeGrantFlow
{
	private const string InvalidGrant = "invalid_grant";

	private readonly AzureADCodeGrantFlow codeGrantFlow;

	public ALAzureAdCodeGrantFlow(Uri redirectUri)
		: this(redirectUri, NavCurrentThread.Session)
	{
	}

	internal ALAzureAdCodeGrantFlow(Uri redirectUri, NavSession session)
	{
		try
		{
			if (redirectUri == null)
			{
				throw new ArgumentNullException("redirectUri");
			}
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}
			codeGrantFlow = new AzureADCodeGrantFlow(redirectUri, LoadSessionTokenCache(session), ServerUserSettings.Instance);
		}
		catch (Exception exception)
		{
			TraceException(exception);
		}
	}

	public string ALAcquireTokenByAuthorizationCode(string authorizationCode, string resourceName)
	{
		//IL_0014: Expected Obj, but got Unknown
		try
		{
			return codeGrantFlow.AcquireTokenByAuthorizationCode(authorizationCode, resourceName);
		}
		catch (LicenseServiceAdalServiceException ex)
		{
			LicenseServiceAdalServiceException ex2 = ex;
			if (ex2.ErrorCode == "invalid_grant" && NavCurrentThread.Session != null && (NavCurrentThread.Session.ClientConnectionType == ConnectionType.Background || NavCurrentThread.Session.ClientConnectionType == ConnectionType.WebServiceClient || NavCurrentThread.Session.ClientConnectionType == ConnectionType.ManagementClient || NavCurrentThread.Session.ClientConnectionType == ConnectionType.Nas || NavCurrentThread.Session.ClientConnectionType == ConnectionType.UnknownClient))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000024F", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Non-user session call to AcquireTokenByAuthorizationCode failed - ErrorCode is {0} and ServiceErrorCodes {1}", ex2.ErrorCode, ex2.ServiceErrorCodes));
				return string.Empty;
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000249", Category.Security, (Exception)(object)ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Call to AcquireTokenByAuthorizationCode failed - ErrorCode is {0} and ServiceErrorCodes {1}", ex2.ErrorCode, ex2.ServiceErrorCodes)));
		}
		catch (Exception exception)
		{
			TraceException(exception, Verbosity.Warning);
		}
		return string.Empty;
	}

	public string ALAcquireTokenByAuthorizationCodeWithCredentials(string authorizationCode, string clientId, string clientSecret, string resourceName)
	{
		return Catch(() => codeGrantFlow.AcquireTokenByAuthorizationCode(authorizationCode, clientId, clientSecret, resourceName));
	}

	public string ALAcquireTokenFromCache(string resourceName)
	{
		return Catch(() => codeGrantFlow.AcquireTokenFromCache(resourceName));
	}

	public string ALAcquireGuestToken(string resourceName, string guestTenantId)
	{
		return Catch(() => codeGrantFlow.AcquireGuestToken(resourceName, guestTenantId));
	}

	public string ALAcquireTokenFromCacheWithCredentials(string clientId, string clientSecret, string resourceName)
	{
		return Catch(() => codeGrantFlow.AcquireTokenFromCache(clientId, clientSecret, resourceName));
	}

	public static string ALAcquireOnBehalfOfToken(string resourceName)
	{
		return Catch(() => AzureADCodeGrantFlow.AcquireOnBehalfOfToken(resourceName).AccessToken);
	}

	public static string ALAcquireOnBehalfOfToken(string resourceName, out string tokenCacheState)
	{
		try
		{
			AuthenticationInfo val = AzureADCodeGrantFlow.AcquireOnBehalfOfToken(resourceName);
			tokenCacheState = ((val.TokenCacheState != null) ? Convert.ToBase64String(val.TokenCacheState) : string.Empty);
			return val.AccessToken;
		}
		catch (Exception exception)
		{
			TraceException(exception, Verbosity.Warning);
			tokenCacheState = string.Empty;
			return string.Empty;
		}
	}

	public static string ALAcquireTokenFromTokenCacheState(string resourceName, string aadUserId, string tokenCacheState, out string newTokenCacheState)
	{
		try
		{
			AuthenticationInfo val = AzureADCodeGrantFlow.AcquireTokenSilent(resourceName, aadUserId, tokenCacheState);
			newTokenCacheState = Convert.ToBase64String(val.TokenCacheState);
			return val.AccessToken;
		}
		catch (Exception exception)
		{
			TraceException(exception, Verbosity.Warning);
			newTokenCacheState = string.Empty;
			return string.Empty;
		}
	}

	public static string ALAcquireOnBehalfOfTokenByCertificate(string resourceName, string clientId, X509Certificate2 clientCertificate)
	{
		return Catch(() => AzureADCodeGrantFlow.AcquireOnBehalfOfTokenByCertificate(resourceName, clientId, clientCertificate));
	}

	public static string ALAcquireOnBehalfOfTokenBySecret(string resourceName, string clientId, string clientSecret)
	{
		return Catch(() => AzureADCodeGrantFlow.AcquireOnBehalfOfTokenBySecret(resourceName, clientId, clientSecret.ToSecureStringOrNull()));
	}

	public static string ALAcquireApplicationToken(string clientId, string clientSecret, string authority, string resourceName)
	{
		return Catch(() => AzureADCodeGrantFlow.AcquireApplicationToken(clientId, clientSecret.ToSecureStringOrNull(), authority, resourceName));
	}

	public string ALGetSaasClientId()
	{
		return Catch(codeGrantFlow.GetSaasClientId);
	}

	public string ALGetInitialDomainNameFromAad()
	{
		return Catch(codeGrantFlow.GetInitialDomainNameFromAad);
	}

	internal static NavTokenCacheBase LoadSessionTokenCache(NavSession session)
	{
		INavTokenCacheSessionManager navTokenCacheSessionManager = new NavTokenCacheSessionManager(session);
		if (navTokenCacheSessionManager.NavTokenCache == null)
		{
			navTokenCacheSessionManager.NavTokenCache = new PersistentNavTokenCache(navTokenCacheSessionManager, DateTimeNowProvider.Current);
		}
		return navTokenCacheSessionManager.NavTokenCache;
	}

	private static string Catch(Func<string> action)
	{
		try
		{
			return action();
		}
		catch (Exception exception)
		{
			TraceException(exception, Verbosity.Warning);
			return string.Empty;
		}
	}

	private static void TraceException(Exception exception, Verbosity verbosity = Verbosity.Error)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000CY", Category.Security, exception, verbosity);
	}
}
