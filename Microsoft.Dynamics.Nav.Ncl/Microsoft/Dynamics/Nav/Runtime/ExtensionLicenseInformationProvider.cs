using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Encryption;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ExtensionLicenseInformationProvider
{
	private const string ResourceAppUri = "https://manage.devcenter.microsoft.com";

	private static readonly Func<NavTenant, string, string, string, SecureString, string> TokenProvider = (NavTenant tenant, string authority, string resourceUri, string applicationId, SecureString secret) => AzureADCodeGrantFlow.AcquireApplicationToken(applicationId, secret, authority, resourceUri);

	private static readonly Func<HttpWebRequest, HttpWebResponse> ResponseProvider = (HttpWebRequest request) => request?.GetResponse() as HttpWebResponse;

	public static int ALLicenseCount(string productId, string skuId)
	{
		return ALLicenseCount(productId, skuId, null);
	}

	public static int ALLicenseCount(string productId, string skuId, string isvPrefix)
	{
		NavSession session = NavCurrentThread.Session;
		if (session == null || !session.IsOpen)
		{
			throw new InvalidOperationException("Current Nav session must exist and be open");
		}
		IAzureKeyVaultSecretProvider secretProvider = TenantEncryptionProviderFactory.GetTenantEncryptionProvider(() => session) as IAzureKeyVaultSecretProvider;
		string aadTenantId = session.Tenant.TenantSettings.AadTenantId;
		return LicenseCount(session.Tenant, productId, skuId, isvPrefix, secretProvider, aadTenantId, TokenProvider, ResponseProvider, session.Diagnostics);
	}

	internal static int LicenseCount(NavTenant tenant, string productId, string skuId, string isvPrefix, IAzureKeyVaultSecretProvider secretProvider, string aadTenantId, Func<NavTenant, string, string, string, SecureString, string> tokenProvider, Func<HttpWebRequest, HttpWebResponse> responseProvider, NavDiagnostics diagnostics)
	{
		if (FeatureSwitches.Instance.DisableExtensionLicenseCount)
		{
			return -1;
		}
		if (Guid.TryParse(productId, out var result))
		{
			productId = result.ToString();
		}
		if (Guid.TryParse(skuId, out var result2))
		{
			skuId = result2.ToString();
		}
		if (secretProvider == null)
		{
			throw new ArgumentNullException("secretProvider");
		}
		if (string.IsNullOrEmpty(aadTenantId))
		{
			throw new ArgumentNullException("aadTenantId");
		}
		if (tokenProvider == null)
		{
			throw new ArgumentNullException("tokenProvider");
		}
		if (responseProvider == null)
		{
			throw new ArgumentNullException("responseProvider");
		}
		string secretName = "extension-testCustomerId";
		string secretName2 = "extension-QueryLicenseCount";
		string secretName3 = "extension-cspHostName";
		string secretName4 = "extension-isvTenantId";
		string secretName5 = "extension-secret";
		string secretName6 = "extension-clientId";
		string secretName7 = "extension-authority";
		if (!string.IsNullOrEmpty(isvPrefix))
		{
			secretName4 = string.Format(CultureInfo.InvariantCulture, "extension-{0}-isvTenantId", isvPrefix);
			secretName5 = string.Format(CultureInfo.InvariantCulture, "extension-{0}-secret", isvPrefix);
			secretName6 = string.Format(CultureInfo.InvariantCulture, "extension-{0}-clientId", isvPrefix);
			secretName7 = string.Format(CultureInfo.InvariantCulture, "extension-{0}-authority", isvPrefix);
		}
		try
		{
			string secretWithFailureTrace = GetSecretWithFailureTrace(secretProvider, secretName4, diagnostics);
			SecureString arg = GetSecretWithFailureTrace(secretProvider, secretName5, diagnostics).ToSecureStringOrNull();
			string secretWithFailureTrace2 = GetSecretWithFailureTrace(secretProvider, secretName6, diagnostics);
			string secretWithFailureTrace3 = GetSecretWithFailureTrace(secretProvider, secretName3, diagnostics);
			Uri uri = null;
			string secretWithFailureTrace4 = GetSecretWithFailureTrace(secretProvider, secretName7, diagnostics);
			if (!string.IsNullOrWhiteSpace(secretWithFailureTrace4))
			{
				diagnostics?.SendTraceTag("00001NK", Category.Extensions, Verbosity.Normal, "Using custom authority: {0}", DiagnosticParameter.SystemMetadata(secretWithFailureTrace4));
				uri = new Uri(secretWithFailureTrace4);
			}
			diagnostics?.SendTraceTag("00001NE", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting LicenseCount for ISV with prefix:{0}", isvPrefix);
			string text = tokenProvider(tenant, (uri == null) ? AzureADAuthenticationHelper.GetTokenAuthority(secretWithFailureTrace) : new Uri(uri, secretWithFailureTrace).ToString(), "https://manage.devcenter.microsoft.com", secretWithFailureTrace2, arg);
			if (string.IsNullOrEmpty(text))
			{
				diagnostics?.SendTraceTag("00001G4", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "The authentication result was null or not holding the access token.");
				return -1;
			}
			if (FeatureSwitches.Instance.EnableTestCustomerExtensionLicenseCount)
			{
				aadTenantId = GetSecretWithFailureTrace(secretProvider, secretName, diagnostics);
			}
			string secretWithFailureTrace5 = GetSecretWithFailureTrace(secretProvider, secretName2, diagnostics);
			return GetLicenseCount(string.Format(CultureInfo.InvariantCulture, secretWithFailureTrace5, secretWithFailureTrace3, productId, skuId, aadTenantId), text, responseProvider, diagnostics);
		}
		catch (Exception exception)
		{
			diagnostics?.SendExceptionTag("00001G3", Category.Extensions, exception);
			return -1;
		}
	}

	private static int GetLicenseCount(string uri, string token, Func<HttpWebRequest, HttpWebResponse> responseProvider, NavDiagnostics diagnostics)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
		httpWebRequest.Method = "GET";
		httpWebRequest.ContentType = "Content-Type: application/json";
		httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, string.Format(CultureInfo.InvariantCulture, "{0} {1}", "Bearer", token));
		string text = Guid.NewGuid().ToString();
		httpWebRequest.Headers.Add("client-request-id", text);
		httpWebRequest.Headers.Add("return-client-request-id", "true");
		diagnostics?.SendTraceTag("00001NF", Category.License, Verbosity.Normal, "Sending ISV License request against '{1}' with client request id: {0}", DiagnosticParameter.SystemMetadata(text), DiagnosticParameter.Euii(uri));
		HttpWebResponse httpWebResponse = null;
		using (diagnostics?.CreateTraceScope("00001NI", DiagnosticParameter.SystemMetadata("ExtensionLicenseInformationProvider.GetLicenseCount"), MonitorType.None, Category.License))
		{
			httpWebResponse = responseProvider(httpWebRequest);
		}
		string content = string.Empty;
		if (httpWebResponse != null && httpWebResponse.SupportsHeaders)
		{
			content = httpWebResponse.GetResponseHeader("client-request-id");
		}
		diagnostics?.SendTraceTag("00001NG", Category.License, Verbosity.Normal, "Recieved response from ISV License request with client request id: {0}", DiagnosticParameter.Euii(content));
		if (httpWebResponse.StatusCode != HttpStatusCode.OK)
		{
			diagnostics?.SendTraceTag("00001G5", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "The state from the license count request returned is {0}", httpWebResponse.StatusCode));
			return -1;
		}
		Stream responseStream = httpWebResponse.GetResponseStream();
		if (responseStream != null)
		{
			StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
			string value = streamReader.ReadToEnd();
			if (!string.IsNullOrEmpty(value))
			{
				JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
				IEnumerable<JToken> enumerable = jObject.SelectTokens("$..totalCount");
				int num = 0;
				foreach (JToken item in enumerable)
				{
					num += item.ToObject<int>();
				}
				diagnostics?.SendTraceTag("00001HN", Category.Extensions, Verbosity.Normal, "License count returned by request {0} is {1}", DiagnosticParameter.Euii(uri), DiagnosticParameter.SystemMetadata(num));
				return num;
			}
		}
		diagnostics?.SendTraceTag("00001G6", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "The response stream from the license count request is null, not expected.");
		return -1;
	}

	private static string GetSecretWithFailureTrace(IAzureKeyVaultSecretProvider secretProvider, string secretName, NavDiagnostics diagnostics)
	{
		try
		{
			return secretProvider.GetSecret(secretName);
		}
		catch (NavEncryptionException)
		{
			diagnostics?.SendTraceTag("00001NH", Category.License, Verbosity.Error, PrivacyClassification.SystemMetadata, "Could not retrieve secret for key: {0}", secretName);
		}
		return null;
	}
}
