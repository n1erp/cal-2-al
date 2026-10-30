using System.Net;

namespace Microsoft.Dynamics.Nav.Types.Constants;

public static class HttpConstants
{
	public static class Headers
	{
		public static class Xms
		{
			public const string ActionTrackingId = "x-ms-action-tracking-id";

			public const string ActivityVector = "x-ms-activity-vector";

			public const string ClientRequestId = "x-ms-client-request-id";

			public const string ClientTrackingId = "x-ms-client-tracking-id";

			public const string CorrelationId = "x-ms-correlation-id";

			public const string TrackingId = "x-ms-tracking-id";
		}

		public const string Accept = "Accept";

		public const string AcceptCharset = "Accept-Charset";

		public const string AcceptEncoding = "Accept-Encoding";

		public const string AcceptLanguage = "Accept-Language";

		public const string All = "*";

		public const string Allow = "Allow";

		public const string Authorization = "Authorization";

		public const string AuthorizationBasic = "Basic";

		public const string AuthorizationDigest = "Digest";

		public const string AuthorizationNegotiate = "Negotiate";

		public const string AuthorizationNTLM = "NTLM";

		public const string AuthorizationOAuth = "Bearer";

		public const string ClientRequestId = "client-request-id";

		public const string Connection = "Connection";

		public const string ContentEncoding = "Content-Encoding";

		public const string ContentLanguage = "Content-Language";

		public const string ContentLength = "Content-Length";

		public const string ContentType = "Content-Type";

		public const string CorsAllowCredentials = "Access-Control-Allow-Credentials";

		public const string CorsAllowCredentialsValue = "true";

		public const string CorsAllowHeaders = "Access-Control-Allow-Headers";

		public const string CorsAllowMethods = "Access-Control-Allow-Methods";

		public const string CorsAllowOrigin = "Access-Control-Allow-Origin";

		public const string CorsApiDeprecatedVersionsHeaders = "api-deprecated-versions";

		public const string CorsApiSupportedVersionsHeaders = "api-supported-versions";

		public const string CorsExposeHeaders = "Access-Control-Expose-Headers";

		public const string CorsMaxAge = "Access-Control-Max-Age";

		public const string CorsRequestHeaders = "Access-Control-Request-Headers";

		public const string CorsRequestMethod = "Access-Control-Request-Method";

		public const string Date = "Date";

		public const string Expect = "Expect";

		public const string Forwarded = "Forwarded";

		public const string Host = "Host";

		public const string IfMatch = "If-Match";

		public const string Location = "Location";

		public const string MaxDataServiceVersion = "MaxDataServiceVersion";

		public const string MaxForwards = "max-forwards";

		public const string MSDynAid = "ms-dyn-aid";

		public const string MSDynUserAgent = "ms-dyn-useragent";

		public const string NavCompany = "Company";

		public const string NavOriginalServiceRoot = "original-client-serviceroot";

		public const string ODataMaxVersion = "OData-MaxVersion";

		public const string ODataVersion = "OData-Version";

		public const string Origin = "Origin";

		public const string PostmanToken = "postman-token";

		public const string Prefer = "Prefer";

		public const string PreferContinueOnError = "odata.continue-on-error";

		public const string PreferenceApplied = "Preference-Applied";

		public const string PreferReturnNoContentValue = "return-no-content";

		public const string PreferReturnNoDataValue = "return-no-data";

		public const string PreferTrackChanges = "odata.track-changes";

		public const string RequestId = "request-id";

		public const string ReturnClientRequestId = "return-client-request-id";

		public const string Server = "Server";

		public const string TransferEncoding = "Transfer-Encoding";

		public const string UserAgent = "User-Agent";

		public const string WwwAuthenticate = "WWW-Authenticate";

		public const string WwwAuthenticateBasicValue = "realm";

		public const string WwwAuthenticateBearerValue = "authorization_uri";

		public const string XForwardedHost = "x-forwarded-host";

		public const string XForwardedFor = "x-forwarded-for";

		public const string XForwardedPath = "x-forwarded-path";

		public const string XForwardedPort = "x-forwarded-port";

		public const string XForwardedProto = "x-forwarded-proto";

		public const string XOriginalFor = "x-original-for";

		public const string XOriginalHost = "x-original-host";

		public const string XOriginalUrl = "x-original-url";
	}

	public static class SignalR
	{
		public const string SignalRPath = "/signalr";

		public const string AuthenticationHeader = "Authentication";
	}

	public static class Methods
	{
		public const string ActionContext = "ACTIONCONTEXT";

		public const string Delete = "DELETE";

		public const string Get = "GET";

		public const string Options = "OPTIONS";

		public const string Patch = "PATCH";

		public const string Post = "POST";

		public const string Put = "PUT";
	}

	public static class Properties
	{
		public const string CancellationTokenSource = "NavODataCancellationTokenSource";

		public const string NavDiagnosticCategory = "DiagnosticCategory";

		public const string NavUserAuthentication = "NavUserAuthentication";

		public const string NavSession = "NavSession";

		public const string NavPerformanceLog = "NavPerfLog";

		public const string NavOriginalServiceBaseAddress = "NavOriginalServiceBaseAddress";

		public const string OwinHttpListener = "Microsoft.Owin.Host.HttpListener.OwinHttpListener";

		public const string ODataDeltaTokenRedemption = "Microsoft.Dynamics.Nav.DeltaTokenRedemption";

		public const string ODataDeltaCalculatorFactory = "Microsoft.Dynamics.Nav.DeltaCalculatorFactory";

		public const string ODataMaskedRequest = "Microsoft.Dynamics.Nav.Service.OData.HttpRequestMessage.ODataMaskedRequest";
	}

	public static class StatusCodes
	{
		public const HttpStatusCode UnprocessableEntity = (HttpStatusCode)422;
	}

	public static class OData
	{
		public const string ODataV4RouteName = "ODataV4";

		public const string ODataRouteName = "OData";

		public const string ApiRouteName = "api";

		public const string BetaVersionName = "beta";
	}

	public static class UriQueryString
	{
		public const string Company = "company";

		public const string Tenant = "tenant";
	}

	public static class DomainFragment
	{
		public const string FinancialDomainFragment = "financials.dynamics";

		public const string InvoiceDomainFragment = "invoicing.office";

		public const string AccountingDomainFragment = "accountants.dynamics";
	}

	public static class ContentType
	{
		public const string StreamContent = "application/octet-stream";

		public const string JsonContent = "application/json";
	}
}
