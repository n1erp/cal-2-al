using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class GraphTableConnection : TableConnection, IGraphTableConnection
{
	private class GraphTestDataProvider : TempTableDataProvider
	{
		private readonly int primaryKeyIndex;

		internal GraphTestDataProvider(NavSession session, NCLMetaTable table)
			: base(session, table)
		{
			primaryKeyIndex = table.Keys[0].GetKeyFieldByIndex(0).FieldIndex;
		}

		public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
		{
			bool flag = string.IsNullOrEmpty(recordBuffer[primaryKeyIndex].ToString());
			if (flag)
			{
				recordBuffer[primaryKeyIndex] = new NavText(Guid.NewGuid().ToString());
			}
			InsertResult insertResult = base.Insert(companyToken, recordBuffer, allowBuffering, out outputRecordBuffer);
			if ((insertResult != InsertResult.Success) & flag)
			{
				recordBuffer[primaryKeyIndex] = NavText.Empty;
			}
			return insertResult;
		}
	}

	internal const string UserToken = "{USER}";

	internal const string PasswordToken = "{PASSWORD}";

	private const string EntityEndpointToken = "{ENTITYENDPOINT}";

	private const string EntityListEndpointToken = "{ENTITYLISTENDPOINT}";

	private const string RequestHeadersToken = "{REQUESTHEADERS}";

	private const string ProxyToken = "{PROXY}";

	private const string ExchangeOnlineAuthenticationResourceUriToken = "{EXORESOURCEURI}";

	private const string ExchangeOnlineAuthenticationResourceRoleToken = "{EXORESOURCEROLE}";

	private const string MaxPageSizeToken = "{MAXPAGESIZE}";

	internal const string SharedContactsMailboxIdentifierToken = "{SHAREDCONTACTS}";

	internal const string AuthTokenToken = "{TOKEN}";

	internal const string DisableIfMatchToken = "{DISABLEIFMATCH}";

	private const int DefaultMazPageSize = 512;

	private UserNamePasswordCredentials credentials;

	private Tuple<DateTime, SecureString> cachedToken;

	private readonly StringBuilder logConnectionParamters = new StringBuilder();

	private readonly Func<NavSession, string> initialDomainProvider;

	private string initialTenantDomain;

	private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	private const string TestConnectionString = "@@test@@";

	private const string MockConnectionToken = "{MOCK}";

	private Dictionary<int, DataProvider> testDataProviders;

	public NavSession Session { get; }

	public string EntityEndpoint { get; private set; }

	public string EntityListEndpoint { get; private set; }

	public string RequestHeaders { get; private set; }

	public string Proxy { get; private set; }

	public string ExchangeOnlineResourceEndpoint { get; private set; }

	public string[] ExchangeOnlineResourceRoles { get; private set; }

	public int MaxPageSize { get; set; }

	public bool CanRequestCredentials { get; set; }

	public bool DisableIfMatch { get; private set; }

	public NavDiagnostics Diagnostics => Session.Diagnostics;

	internal bool IsTestConnection { get; private set; }

	internal override TableConnectionType ConnectionType => TableConnectionType.MicrosoftGraph;

	internal override string ConnectionTypeName => Lang.Graph;

	internal override bool SupportsTransactions => false;

	private string InitialTenantDomain
	{
		get
		{
			if (string.IsNullOrEmpty(initialTenantDomain))
			{
				initialTenantDomain = initialDomainProvider(Session);
			}
			return initialTenantDomain;
		}
	}

	private bool NeedsCredential()
	{
		if (!IsTestConnection && (string.IsNullOrWhiteSpace(credentials?.UserName) || string.IsNullOrWhiteSpace(credentials.Password)))
		{
			return string.IsNullOrEmpty(ExchangeOnlineResourceEndpoint);
		}
		return false;
	}

	private void ReadConnectionString(string connectionString)
	{
		Func<string, Match> getFieldMatch = (string field) => Regex.Match(connectionString, field + "=([^;]*)?", RegexOptions.IgnoreCase);
		Func<string, string> func = (string field) =>
		{
			Match match = getFieldMatch(field);
			return (!match.Success) ? string.Empty : match.Groups[1].Value;
		};
		string text = func("{TOKEN}");
		string password = (string.IsNullOrEmpty(text) ? func("{PASSWORD}") : text);
		credentials = new UserNamePasswordCredentials
		{
			UserName = func("{USER}"),
			Password = password
		};
		CanRequestCredentials = getFieldMatch("{USER}").Success || getFieldMatch("{PASSWORD}").Success;
		IsTestConnection |= func("{MOCK}") == "1";
		AddLogConnectionParameter("{MOCK}", IsTestConnection);
		EntityEndpoint = func("{ENTITYENDPOINT}");
		AddLogConnectionParameter("{ENTITYENDPOINT}", EntityEndpoint);
		EntityListEndpoint = func("{ENTITYLISTENDPOINT}");
		AddLogConnectionParameter("{ENTITYLISTENDPOINT}", EntityListEndpoint);
		RequestHeaders = func("{REQUESTHEADERS}");
		Proxy = func("{PROXY}");
		AddLogConnectionParameter("{PROXY}", Proxy);
		ExchangeOnlineResourceEndpoint = func("{EXORESOURCEURI}");
		AddLogConnectionParameter("{EXORESOURCEURI}", ExchangeOnlineResourceEndpoint);
		ExchangeOnlineResourceRoles = func("{EXORESOURCEROLE}")?.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		AddLogConnectionParameter("{EXORESOURCEROLE}", func("{EXORESOURCEROLE}"));
		DisableIfMatch = getFieldMatch("{DISABLEIFMATCH}").Success;
		AddLogConnectionParameter("{DISABLEIFMATCH}", DisableIfMatch);
		MaxPageSize = 512;
		int result = 0;
		if (int.TryParse(func("{MAXPAGESIZE}"), out result))
		{
			MaxPageSize = ((result > 0) ? result : 512);
		}
		AddLogConnectionParameter("{MAXPAGESIZE}", MaxPageSize);
		if (!IsTestConnection)
		{
			PrepareEndpointsForSharedContacts();
		}
	}

	private void AddLogConnectionParameter(string token, object value)
	{
		logConnectionParamters.Append(token).Append(" = ").Append(value);
	}

	void IGraphTableConnection.SetAuthorizationHeader(HttpWebRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		WebHeaderCollection headers = request.Headers;
		if (!string.IsNullOrEmpty(ExchangeOnlineResourceEndpoint))
		{
			if (string.IsNullOrEmpty(credentials.UserName) && !string.IsNullOrEmpty(credentials.Password))
			{
				headers["Authorization"] = "Bearer " + credentials.Password;
			}
			else
			{
				headers["Authorization"] = CreateAuthorizationHeaderForExchangeOnline();
			}
			return;
		}
		if (NeedsCredential())
		{
			if (!CanRequestCredentials)
			{
				return;
			}
			if (!RequestCredentials(out var userNamePasswordCredentials))
			{
				throw new NavGraphException(Lang.GraphNoCredentials);
			}
			credentials = userNamePasswordCredentials;
		}
		if (!IsTestConnection)
		{
			headers["Authorization"] = AzureADAuthenticationHelper.GetAuthorizationHeaderFromBasicUserCredentials(credentials.UserName, credentials.Password);
		}
		else
		{
			request.UseDefaultCredentials = true;
		}
	}

	void IGraphTableConnection.ClearCredentialCache()
	{
		Session.Diagnostics.SendTraceTag("00000H3", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Clearing cached credentials");
		cachedToken = null;
	}

	public string ReplaceSharedMailboxToken(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		if (!input.Contains("{SHAREDCONTACTS}"))
		{
			return input;
		}
		string text = InitialTenantDomain;
		string text2 = ServerUserSettings.Instance.MicrosoftGraphSharedContactsMailboxName.Value + "@" + text;
		Session.Diagnostics.SendTraceTag("00000I2", Category.Runtime, Verbosity.Verbose, "Replacing SharedMailboxToken, {0}, in {1}", DiagnosticParameter.Oii(text2), DiagnosticParameter.SystemMetadata(input));
		return input.Replace("{SHAREDCONTACTS}", text2);
	}

	public static string GetInitialDomainFromAad(NavSession session)
	{
		TenantInfo tenantDetail = new AzureADGraphQuery(session).GetTenantDetail();
		if (tenantDetail == null)
		{
			return null;
		}
		return tenantDetail.InitialDomain;
	}

	internal GraphTableConnection()
		: base(null, null)
	{
	}

	private GraphTableConnection(ITreeObject parent, NavSession session, string name, string connectionString, Func<NavSession, string> aadInitialDomainProvider = null)
		: base(parent, name)
	{
		Session = session;
		initialDomainProvider = aadInitialDomainProvider ?? new Func<NavSession, string>(GetInitialDomainFromAad);
		IsTestConnection = connectionString == "@@test@@";
		if (!IsTestConnection)
		{
			ReadConnectionString(connectionString);
		}
		session.Diagnostics.SendTraceTag("00000CW", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "GraphTableConnection constructed with the following arguments: name={0}, connection parameters={1}", name, logConnectionParamters);
	}

	internal static GraphTableConnection Create(ITreeObject parent, NavSession session, string name, string connectionString, Func<NavSession, string> addInitialDomainProvider = null)
	{
		return new GraphTableConnection(parent, session, name, connectionString, addInitialDomainProvider);
	}

	internal DataAccess CreateDataAccess(NCLMetaTable table, GlobalFilters globalFilters)
	{
		CheckDisposed();
		NCLMetaField nCLMetaField = (table.Keys.Any((NCLMetaKey t) => t.IsPrimaryKey) ? table.GetFieldByIndex(table.Keys.First((NCLMetaKey t) => t.IsPrimaryKey).DefinedKeyFields.First().ColumnIndex) : null);
		return new DataAccess(Session, ((IsTestConnection && string.IsNullOrEmpty(EntityEndpoint)) || nCLMetaField == null) ? CreateGraphTestDataProvider(table) : new GraphDataProvider(() => Session.TableConnectionManager.GetCurrentTableConnection<GraphTableConnection>(TableConnectionType.MicrosoftGraph), table), globalFilters, DataAccessTableVersionTokens.CreateForTempTable(), VirtualAndTempTransactionalDataCache.Instance);
	}

	private string CreateAuthorizationHeaderForExchangeOnline()
	{
		if (cachedToken == null || DateTime.UtcNow.AddMinutes(10.0) >= cachedToken.Item1)
		{
			cachedToken = null;
			AzureADAuthenticationHelper azureADAuthenticationHelper = new AzureADAuthenticationHelper(Session.Tenant);
			string tokenForApplication = azureADAuthenticationHelper.GetTokenForApplication(ExchangeOnlineResourceEndpoint);
			string serviceAssertedAppV1Token = GetServiceAssertedAppV1Token(tokenForApplication, azureADAuthenticationHelper.ClientId, InitialTenantDomain, ExchangeOnlineResourceRoles, out var expiryDateTime, Session.Diagnostics);
			Session.Diagnostics.SendTraceTag("00000H4", Category.Security, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, "S2S token for created: TenantDomain {0}", initialTenantDomain);
			cachedToken = new Tuple<DateTime, SecureString>(expiryDateTime, SecureStringExtension.CreateFromUnsecureString("Bearer" + " " + serviceAssertedAppV1Token));
		}
		Session.Diagnostics.SendTraceTag("00000H5", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Getting S2S token from cache. Found: {0}", cachedToken != null);
		return cachedToken.Item2.ConvertToUnsecureString();
	}

	private void PrepareEndpointsForSharedContacts()
	{
		if (EntityEndpoint.Contains("{SHAREDCONTACTS}") || EntityListEndpoint.Contains("{SHAREDCONTACTS}"))
		{
			EntityEndpoint = ReplaceSharedMailboxToken(EntityEndpoint);
			EntityListEndpoint = ReplaceSharedMailboxToken(EntityListEndpoint);
			if (!RequestHeaders.Contains("X-AnchorMailbox"))
			{
				RequestHeaders = ReplaceSharedMailboxToken(RequestHeaders + "|X-AnchorMailbox:" + "{SHAREDCONTACTS}");
			}
		}
	}

	private static string GetServiceAssertedAppV1Token(string encodedAppToken, string serviceAppId, string initialTenantDomain, string[] customRoles, out DateTime expiryDateTime, NavDiagnostics diagnostics)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Expected Obj, but got Unknown
		diagnostics.SendTraceTag("00000H6", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Creating app S2S token for EXO");
		JwtSecurityTokenHandler val = new JwtSecurityTokenHandler();
		JwtSecurityToken val2 = (JwtSecurityToken)((SecurityTokenHandler)val).ReadToken(encodedAppToken);
		string text = string.Format(CultureInfo.InvariantCulture, "{0}@{1}", serviceAppId, initialTenantDomain);
		string text2 = val2.Claims.FirstOrDefault((Claim t) => t.Type.Equals("tid", StringComparison.OrdinalIgnoreCase))?.Value;
		string text3 = val2.Claims.FirstOrDefault((Claim t) => t.Type.Equals("aud", StringComparison.OrdinalIgnoreCase))?.Value;
		string text4 = val2.Claims.FirstOrDefault((Claim t) => t.Type.Equals("appid", StringComparison.OrdinalIgnoreCase))?.Value;
		string s = val2.Claims.FirstOrDefault((Claim t) => t.Type.Equals("exp", StringComparison.OrdinalIgnoreCase))?.Value;
		expiryDateTime = DateTime.UtcNow.AddHours(1.0);
		if (long.TryParse(s, out var result))
		{
			expiryDateTime = FromEpocTime(result);
			if (expiryDateTime > DateTime.UtcNow.AddMinutes(5.0))
			{
				expiryDateTime = expiryDateTime.AddMinutes(-5.0);
			}
		}
		if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
		{
			NavGraphException ex = NavGraphException.Create(string.Format(CultureInfo.CurrentCulture, Lang.GraphApplicationTokenDetailsMissing, text2, text3, text4));
			diagnostics.SendExceptionTag("00000H7", Category.Security, ex);
			throw ex;
		}
		List<Claim> newClaims = new List<Claim>
		{
			new Claim("ver", "service_asserted_app_v1"),
			new Claim("tid", text2),
			new Claim("appid", text4),
			new Claim("clientappid", text4),
			new Claim("iat", EpocTime(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture)),
			new Claim("nbf", EpocTime(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture)),
			new Claim("exp", EpocTime(expiryDateTime).ToString(CultureInfo.InvariantCulture)),
			new Claim("actortoken", encodedAppToken)
		};
		if (customRoles != null && customRoles.Length != 0)
		{
			customRoles.ForEach((string role) =>
			{
				newClaims.Add(new Claim("roles", role));
			});
		}
		else
		{
			newClaims.Add(new Claim("roles", "Contacts.Read.Shared"));
			newClaims.Add(new Claim("roles", "Contacts.ReadWrite.Shared"));
		}
		JwtSecurityToken val3 = new JwtSecurityToken(text, text3, (IEnumerable<Claim>)newClaims, (DateTime?)null, (DateTime?)null, (SigningCredentials)null);
		return ((SecurityTokenHandler)val).WriteToken((SecurityToken)(object)val3);
	}

	private static long EpocTime(DateTime time)
	{
		return (long)(time - UnixEpoch).TotalSeconds;
	}

	private static DateTime FromEpocTime(long epocTime)
	{
		return UnixEpoch.AddSeconds(epocTime);
	}

	private DataProvider CreateGraphTestDataProvider(NCLMetaTable table)
	{
		Session.Diagnostics.SendTraceTag("00000CX", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Creates a GraphTestDataProvider for TableName={0}, ExternalName={1}", table.TableName, table.ExternalName);
		if (testDataProviders == null)
		{
			testDataProviders = new Dictionary<int, DataProvider>();
		}
		if (!testDataProviders.TryGetValue(table.TableId, out var value))
		{
			value = new GraphTestDataProvider(Session, table);
			testDataProviders.Add(table.TableId, value);
		}
		return value;
	}
}
