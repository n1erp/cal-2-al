using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Authentication;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUser
{
	internal struct FieldDefinition
	{
		public string Field { get; set; }

		public string Table { get; set; }

		public string TableAlias { get; set; }

		public string EmptyValue { get; set; }
	}

	private static readonly Guid defaultSuperGuid = new Guid("{00000000-0000-0000-0000-000000000001}");

	private readonly object writeLockObj = new object();

	private readonly object readLockObj = new object();

	private readonly string userWindowsGroupsCachePrefix = "WINGROUPS_";

	private const int UserWindowsGroupsCacheExpiryTimeInHours = 1;

	private Guid userGuid;

	private string userName;

	private string fullName;

	private NavUserState state;

	private NavDateTime expiryDate;

	private string windowsSID;

	private string passwordHash;

	private string nameIdentifier;

	private string authenticationKey;

	private string webAccountKey;

	private NavDateTime webAccountExpiryDate;

	private LicenseType licenseType;

	private string authenticationEmail;

	private string authenticationObjectId;

	private Guid applicationId;

	private string contactEmail;

	private IdentityReferenceCollection windowsIdentityGroups = new IdentityReferenceCollection();

	private const string CommandGetUsers = "SELECT US.[User Security ID],US.[User Name],US.[Full Name],US.[State],US.[Expiry Date],US.[Windows Security ID],UP.[Password],UP.[Name Identifier],UP.[Authentication Key],UP.[WebServices Key],UP.[WebServices Key Expiry Date],US.[License Type]{1} FROM {0}[User] AS US";

	private const string CommandJoinUserProperties = " JOIN {0}[User Property] AS UP";

	private const string CommandJoinArgs = " ON US.[User Security ID] = UP.[User Security ID]";

	private const string CommandWhereArgsWithCompanyId = " WHERE US.\"$companyId\" = @companyid AND UP.\"$companyId\" = @companyid";

	internal static FieldDefinition[] FieldDefinitions = new FieldDefinition[6]
	{
		new FieldDefinition
		{
			Field = "Exchange Identifier",
			Table = "[User]",
			TableAlias = "US",
			EmptyValue = "N''"
		},
		new FieldDefinition
		{
			Field = "Application ID",
			Table = "[User]",
			TableAlias = "US",
			EmptyValue = "CAST(0x0 AS UNIQUEIDENTIFIER)"
		},
		new FieldDefinition
		{
			Field = "Authentication Email",
			Table = "[User]",
			TableAlias = "US",
			EmptyValue = "N''"
		},
		new FieldDefinition
		{
			Field = "Authentication Object ID",
			Table = "[User Property]",
			TableAlias = "UP",
			EmptyValue = "N''"
		},
		new FieldDefinition
		{
			Field = "Contact Email",
			Table = "[User]",
			TableAlias = "US",
			EmptyValue = "N''"
		},
		new FieldDefinition
		{
			Field = "Directory Role ID",
			Table = "[User Property]",
			TableAlias = "UP",
			EmptyValue = "N''"
		}
	};

	internal UserMembershipEntitlements MembershipEntitlements;

	public static Guid DefaultSuperGuid => defaultSuperGuid;

	public string UserWindowsGroupsCachePrefix => userWindowsGroupsCachePrefix;

	internal bool IsEnabled
	{
		get
		{
			if (state == NavUserState.Enabled)
			{
				if (!(NavDateTime.Undefined == expiryDate))
				{
					return NavDateTimeValue.Compare(expiryDate, NavDateTime.Now) > 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsDefaultSuper { get; private set; }

	public bool IsNavAdminUser { get; private set; }

	public string Name
	{
		get
		{
			return new NavCode(userName.Length, userName).Value;
		}
		internal set
		{
			userName = value;
		}
	}

	public string DisplayName
	{
		get
		{
			if (!string.IsNullOrEmpty(fullName))
			{
				return fullName;
			}
			return userName;
		}
	}

	public NavUserState State => state;

	public NavDateTime ExpiryDate => expiryDate;

	public Guid Id => userGuid;

	public string Sid => windowsSID;

	public string AuthenticationKey => authenticationKey;

	public string NameIdentifier => nameIdentifier;

	public string AuthenticationObjectId => authenticationObjectId;

	public string DirectoryRoleId { get; private set; }

	public bool IsTenantAdmin => "62e90394-69f5-4237-9190-012177145e10".Equals(DirectoryRoleId);

	public bool IsDelegatedAdmin
	{
		get
		{
			if (string.IsNullOrEmpty(AuthenticationObjectId))
			{
				return IsTenantAdmin;
			}
			return false;
		}
	}

	public bool IsDelegatedHelpdesk
	{
		get
		{
			if (string.IsNullOrEmpty(AuthenticationObjectId))
			{
				return "729827e3-9c14-49f7-bb1b-9608f156bbb8".Equals(DirectoryRoleId);
			}
			return false;
		}
	}

	public bool IsDelegatedUser
	{
		get
		{
			if (string.IsNullOrEmpty(AuthenticationObjectId))
			{
				return !string.IsNullOrEmpty(DirectoryRoleId);
			}
			return false;
		}
	}

	public string WebAccountKey => webAccountKey;

	public SecurityIdentifier SecurityIdentifier
	{
		get
		{
			if (!string.IsNullOrEmpty(windowsSID))
			{
				return new SecurityIdentifier(windowsSID);
			}
			return null;
		}
	}

	public string ExchangeIdentifier { get; internal set; }

	public LicenseType LicenseType => licenseType;

	public string AuthenticationEmail => authenticationEmail;

	public string ContactEmail => contactEmail;

	public string Puid { get; private set; }

	public Guid ApplicationId => applicationId;

	public IdentityReferenceCollection Groups
	{
		get
		{
			return windowsIdentityGroups;
		}
		private set
		{
			if (value != null)
			{
				lock (writeLockObj)
				{
					windowsIdentityGroups = value;
				}
			}
		}
	}

	internal bool CanAccessProductionCompany => MembershipEntitlements?.HasProductionMembershipEntitlements ?? FeatureSwitches.Instance.EntitlementByAadPresenceOnly;

	internal NavUser()
	{
	}

	public void ReplaceDefaultSuperGuid(Guid value)
	{
		userGuid = value;
		IsDefaultSuper = userGuid == defaultSuperGuid;
	}

	internal void RestoreDefaultSuperGuid()
	{
		userGuid = defaultSuperGuid;
		IsDefaultSuper = true;
	}

	internal void EnsurePuid(string puid)
	{
		Puid = puid ?? string.Empty;
	}

	internal void ReplaceNameIdentifier(string nameId)
	{
		nameIdentifier = nameId;
	}

	internal void ReplaceAuthenticationEmail(string upn)
	{
		authenticationEmail = upn;
	}

	internal void ReplaceAppId(string appId)
	{
		applicationId = new Guid(appId);
	}

	internal void ReplaceAuthenticationObjectId(string objectId)
	{
		authenticationObjectId = objectId;
	}

	internal void ReplaceDirectoryRoleId(string roleId)
	{
		DirectoryRoleId = roleId;
	}

	internal static bool TryAuthenticate(NavUser cachedUser, UserNameSecurityToken token, NavTenant tenant)
	{
		if (!TryAuthenticateCore(cachedUser))
		{
			tenant.Diagnostics.SendTraceTag("00001FX", Category.Security, Verbosity.Normal, "User:{0} cannot be Authenticated", DiagnosticParameter.Euii(cachedUser?.Name));
			return false;
		}
		string text = token.Password;
		if (token is NavUserNameBasicAuthenticationToken)
		{
			if (!string.IsNullOrEmpty(text) && cachedUser.webAccountKey == text && (NavDateTime.Undefined == cachedUser.webAccountExpiryDate || NavDateTimeValue.Compare(cachedUser.webAccountExpiryDate, NavDateTime.Now) > 0))
			{
				tenant.Diagnostics.SendTraceTag("00001FY", Category.Security, Verbosity.Normal, "User: {0} authenticated successfully using web service key.", DiagnosticParameter.Euii(cachedUser.Name));
				AuthenticationLockoutPolicy.RecordAuthenticationSucceeded(cachedUser, tenant);
				return true;
			}
			tenant.Diagnostics.SendTraceTag("00001G2", Category.Security, Verbosity.Normal, "User: {0} failed to authenticate using web service key.", DiagnosticParameter.Euii(cachedUser.Name));
			if (ServerUserSettings.Instance.ClientServicesCredentialType.Value == NavClientCredentialType.AccessControlService)
			{
				tenant.Diagnostics.SendTraceTag("000001V", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User cannot be authenticated using web service key. Client credential type AccessControlService does not allow NavUserPassword authentication.");
				if (!string.IsNullOrEmpty(cachedUser.webAccountKey))
				{
					AuthenticationLockoutPolicy.RecordAuthenticationFailed(cachedUser, tenant);
				}
				return false;
			}
			text = CryptographyHelper.GeneratePasswordHash(text);
		}
		if (cachedUser.passwordHash != CryptographyHelper.GenerateSaltedPasswordHash(text, cachedUser.Id))
		{
			tenant.Diagnostics.SendTraceTag("00001FZ", Category.Security, Verbosity.Normal, "User {0} attempted to authenticate with an invalid password", DiagnosticParameter.Euii(cachedUser.Name));
			if (!string.IsNullOrEmpty(cachedUser.passwordHash))
			{
				AuthenticationLockoutPolicy.RecordAuthenticationFailed(cachedUser, tenant);
			}
			return false;
		}
		AuthenticationLockoutPolicy.RecordAuthenticationSucceeded(cachedUser, tenant);
		return true;
	}

	internal static bool TryAuthenticate(NavUser cachedUser, WindowsIdentity windowsIdentity)
	{
		if (windowsIdentity == null)
		{
			throw new ArgumentNullException("windowsIdentity");
		}
		if (!TryAuthenticateCore(cachedUser))
		{
			return false;
		}
		if (string.IsNullOrEmpty(cachedUser.windowsSID))
		{
			cachedUser.windowsSID = windowsIdentity.User.ToString();
		}
		if (string.IsNullOrEmpty(cachedUser.Name))
		{
			cachedUser.userName = windowsIdentity.Name;
		}
		cachedUser.Groups = windowsIdentity.Groups;
		return true;
	}

	internal static bool TryAuthenticateCore(NavUser cachedUser)
	{
		if (cachedUser == null || !cachedUser.IsEnabled || cachedUser.LicenseType == LicenseType.WindowsGroup)
		{
			return false;
		}
		return true;
	}

	internal static NavUser CreateDefaultWindowsSuperUser(WindowsIdentity windowsIdentity, bool isDefaultSuper, bool isNavAdminUser)
	{
		if (windowsIdentity == null)
		{
			throw new ArgumentNullException("windowsIdentity");
		}
		return new NavUser
		{
			userGuid = defaultSuperGuid,
			userName = windowsIdentity.Name,
			fullName = windowsIdentity.Name,
			state = NavUserState.Enabled,
			expiryDate = NavDateTime.Undefined,
			windowsSID = windowsIdentity.User.ToString(),
			IsDefaultSuper = isDefaultSuper,
			IsNavAdminUser = isNavAdminUser,
			licenseType = LicenseType.Full,
			windowsIdentityGroups = windowsIdentity.Groups
		};
	}

	internal static NavUser CreateDefaultDelegatedWindowsSuperUser(string userName, bool isDefaultSuper, bool isNavAdminUser)
	{
		return new NavUser
		{
			userGuid = defaultSuperGuid,
			userName = userName,
			fullName = userName,
			state = NavUserState.Enabled,
			expiryDate = NavDateTime.Undefined,
			windowsSID = Guid.Empty.ToString(),
			IsDefaultSuper = isDefaultSuper,
			IsNavAdminUser = isNavAdminUser,
			licenseType = LicenseType.Full,
			windowsIdentityGroups = null
		};
	}

	internal static NavUser CreateDefaultNamedSuperUser(string username)
	{
		return new NavUser
		{
			userGuid = defaultSuperGuid,
			userName = username,
			fullName = username,
			state = NavUserState.Enabled,
			expiryDate = NavDateTime.Undefined,
			windowsSID = string.Empty,
			IsDefaultSuper = true,
			IsNavAdminUser = false,
			licenseType = LicenseType.Full
		};
	}

	internal static NavUser CreateDefaultSystemUser()
	{
		return new NavUser
		{
			userGuid = defaultSuperGuid,
			userName = "SYSTEM",
			fullName = "System Account",
			state = NavUserState.Enabled,
			expiryDate = NavDateTime.Undefined,
			IsDefaultSuper = false,
			IsNavAdminUser = true,
			licenseType = LicenseType.Full
		};
	}

	internal static NavUser CreateUserFromNavAzureADUserTokenInfo(UserSecurityTokenHelper tokenInfo)
	{
		string text = tokenInfo.DirectoryRoleId;
		if (!string.IsNullOrEmpty(text) && text.Length > Guid.Empty.ToString().Length)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QV", Category.Security, Verbosity.Warning, "Token info for authentication object id {0} contains several directory role ids: {1}.", DiagnosticParameter.Eupi(tokenInfo.AuthenticationObjectId), DiagnosticParameter.Eupi(text));
			int length = 80;
			text = text.Substring(0, length);
		}
		return new NavUser
		{
			userGuid = Guid.NewGuid(),
			state = NavUserState.Enabled,
			authenticationObjectId = tokenInfo.AuthenticationObjectId,
			DirectoryRoleId = text,
			authenticationEmail = tokenInfo.AuthenticationEmail,
			userName = tokenInfo.GetUserName(),
			fullName = tokenInfo.FullName,
			contactEmail = tokenInfo.ContactEmail,
			expiryDate = NavDateTime.Undefined,
			IsDefaultSuper = false,
			IsNavAdminUser = false,
			licenseType = LicenseType.Full,
			nameIdentifier = tokenInfo.NameIdentifier
		};
	}

	private static NavDateTime NavDateTimeFromSqlDateTime(SqlDataReader reader, int columnIndex)
	{
		if (reader.IsDBNull(columnIndex))
		{
			return NavDateTime.Default;
		}
		DateTime initValue = new DateTime(reader.GetDateTime(columnIndex).Ticks, DateTimeKind.Utc);
		if (initValue.Equals(NavDate.SqlDateTimeUndefined))
		{
			return NavDateTime.Undefined;
		}
		return NavDateTime.Create(initValue, DateTimeReferenceFrame.Server, unspecifiedAsLocal: false);
	}

	public static IdentityReferenceCollection CopyWindowsGroups(IdentityReferenceCollection groups)
	{
		IdentityReferenceCollection identityReferenceCollection = new IdentityReferenceCollection(groups.Count);
		foreach (IdentityReference group in groups)
		{
			identityReferenceCollection.Add(new SecurityIdentifier(group.Value));
		}
		return identityReferenceCollection;
	}

	public void TryUpdateUserGroupsFromTenantCache(NavTenant tenant)
	{
		string cacheKey = UserWindowsGroupsCachePrefix + Sid;
		lock (readLockObj)
		{
			IdentityReferenceCollection fromCache = tenant.GetFromCache<IdentityReferenceCollection>(cacheKey);
			if (fromCache == null)
			{
				int minutes = ((!TryRefreshUserWindowsGroups(tenant, ContextType.Machine) || !TryRefreshUserWindowsGroups(tenant, ContextType.Domain)) ? 5 : 60);
				tenant.AddToCache(cacheKey, Groups, DateTime.UtcNow.Add(new TimeSpan(0, minutes, 0)), TimeSpan.Zero);
			}
			else
			{
				Groups = fromCache;
			}
		}
	}

	public bool TryRefreshUserWindowsGroups(NavTenant tenant, ContextType contextType)
	{
		IdentityReferenceCollection identityReferenceCollection = CopyWindowsGroups(Groups);
		bool result = true;
		try
		{
			tenant.Diagnostics.SendTraceTag("00001HW", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Refresh User Groups with ContextType={0}.", contextType);
			JoinUserGroupsWith(tenant.Diagnostics, identityReferenceCollection, contextType);
		}
		catch (Exception ex) when (ex is COMException || ex is PrincipalException || ex is NotSupportedException)
		{
			result = false;
		}
		Groups = identityReferenceCollection;
		return result;
	}

	private void JoinUserGroupsWith(NavDiagnostics diagnostics, IdentityReferenceCollection listOfGroups, ContextType typeOfGroups)
	{
		PrincipalContext principalContext;
		UserPrincipal userPrincipal;
		if (typeOfGroups == ContextType.Machine || !NavUserADHelper.IsADForest)
		{
			principalContext = new PrincipalContext(typeOfGroups);
			userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.Sid, Sid);
			if (userPrincipal == null)
			{
				diagnostics.SendTraceTag("00009W1", Category.Security, Verbosity.Normal, PrivacyClassification.EndUserIdentifiableInformation, "Cannot get local UserPrincipal from Sid {0} for user {1} with ID {2}.", Sid, userName, userGuid);
				return;
			}
		}
		else
		{
			string distinguishedName = NavUserADHelper.GetDistinguishedName(Sid);
			if (distinguishedName == null)
			{
				diagnostics.SendTraceTag("00009U4", Category.Security, Verbosity.Warning, PrivacyClassification.EndUserIdentifiableInformation, "Cannot get distinguished name for user {0} with ID {1} and Sid ({2}).", userName, userGuid, Sid);
				return;
			}
			principalContext = new PrincipalContext(typeOfGroups, null, distinguishedName);
			userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.DistinguishedName, distinguishedName);
			if (userPrincipal == null)
			{
				diagnostics.SendTraceTag("00009U5", Category.Security, Verbosity.Warning, PrivacyClassification.EndUserIdentifiableInformation, "Cannot get UserPrincipal from distinguished name {0} for user {1} with ID {2}.", distinguishedName, userName, userGuid);
				return;
			}
		}
		using (principalContext)
		{
			using (userPrincipal)
			{
				foreach (Principal group in getAuthorizationGroups(userPrincipal))
				{
					if (!listOfGroups.Any((IdentityReference g) => g.Value.Equals(group.Sid.Value, StringComparison.OrdinalIgnoreCase)))
					{
						listOfGroups.Add(new SecurityIdentifier(group.Sid.Value));
					}
				}
			}
		}
	}

	private static IEnumerable<Principal> getAuthorizationGroups(UserPrincipal user)
	{
		PrincipalSearchResult<Principal> authorizationGroups = user.GetAuthorizationGroups();
		List<Principal> list = new List<Principal>();
		IEnumerator<Principal> enumerator = authorizationGroups.GetEnumerator();
		using (enumerator)
		{
			while (enumerator.MoveNext())
			{
				try
				{
					Principal current = enumerator.Current;
					list.Add(current);
				}
				catch (NoMatchingPrincipalException)
				{
				}
			}
			return list;
		}
	}

	internal string GetDetailsForLog(NavSession session)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Tenant: " + session?.Tenant?.Id);
		stringBuilder.AppendLine("SessionId: " + session?.Id.ToString(CultureInfo.CurrentCulture));
		stringBuilder.AppendLine("Id: " + Id.ToString());
		stringBuilder.AppendLine("Name: " + Name);
		stringBuilder.AppendLine("DisplayName: " + DisplayName);
		stringBuilder.AppendLine("AuthenticationEmail: " + AuthenticationEmail);
		stringBuilder.AppendLine("IsEnabled: " + IsEnabled);
		stringBuilder.AppendLine("IsDefaultSuper: " + IsDefaultSuper);
		stringBuilder.AppendLine("IsNavAdminUser: " + IsNavAdminUser);
		return stringBuilder.ToString();
	}

	internal static string GenerateGetUsersCommand(NavSqlConnection connection, NavDatabase database, CompanyId? companyId)
	{
		string value = NavSqlStatementHelper.GenerateSelectStatementHints(locking: false, noExpand: false, connection);
		StringBuilder stringBuilder = new StringBuilder("SELECT US.[User Security ID],US.[User Name],US.[Full Name],US.[State],US.[Expiry Date],US.[Windows Security ID],UP.[Password],UP.[Name Identifier],UP.[Authentication Key],UP.[WebServices Key],UP.[WebServices Key Expiry Date],US.[License Type]{1} FROM {0}[User] AS US");
		stringBuilder.Append(value);
		stringBuilder.Append(" JOIN {0}[User Property] AS UP");
		stringBuilder.Append(value);
		stringBuilder.Append(" ON US.[User Security ID] = UP.[User Security ID]");
		if (companyId.HasValue)
		{
			stringBuilder.Append(" WHERE US.\"$companyId\" = @companyid AND UP.\"$companyId\" = @companyid");
		}
		string format = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		NavSqlStatementHelper.AppendDatabaseAndSchemaQualification(stringBuilder2, database, "dbo");
		string text = stringBuilder2.ToString();
		stringBuilder2.Clear();
		AppendFieldForSqlQueryIfExists(stringBuilder2, connection, text, FieldDefinitions);
		return string.Format(CultureInfo.InvariantCulture, format, text, stringBuilder2.ToString());
	}

	private static void AppendFieldForSqlQueryIfExists(StringBuilder sb, NavSqlConnection connection, string databaseAndSchemaName, IList<FieldDefinition> fieldDefinitions)
	{
		if (fieldDefinitions.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder("SELECT ");
		for (int i = 0; i < fieldDefinitions.Count; i++)
		{
			FieldDefinition fieldDefinition = fieldDefinitions[i];
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\r\n                declare @col{0} as BIT;\r\n                SELECT @col{0} = CASE WHEN COLUMNPROPERTY(OBJECT_ID(N'{1}{2}'), N'{3}', 'ColumnId') IS NULL THEN 0 ELSE 1 END;\r\n            ", i, databaseAndSchemaName, fieldDefinition.Table, fieldDefinition.Field).AppendLine();
			stringBuilder2.Append("@col").Append(i).Append(',');
		}
		stringBuilder2.Length--;
		stringBuilder.AppendLine(stringBuilder2.ToString());
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(connection);
		using (navSqlCommand.ExecuteReader(stringBuilder.ToString()))
		{
			if (navSqlCommand.ReadNextRow())
			{
				for (int j = 0; j < fieldDefinitions.Count; j++)
				{
					FieldDefinition fieldDefinition2 = fieldDefinitions[j];
					if (navSqlCommand.Reader.GetBoolean(j))
					{
						sb.AppendFormat(CultureInfo.InvariantCulture, ",{0}.[{1}]", fieldDefinition2.TableAlias, fieldDefinition2.Field);
					}
					else
					{
						sb.Append(',').Append(fieldDefinition2.EmptyValue);
					}
				}
			}
			navSqlCommand.CloseReader(forceClose: false);
		}
	}

	internal static List<NavUser> GetAllUsers(NavDatabase database)
	{
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		try
		{
			List<NavUser> users = new List<NavUser>();
			CompanyId? companyId = database.GetCompanyIdForSystemTables(2000000120);
			try
			{
				conScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(database, "GETALLUSERS", companyId.HasValue.ToString(CultureInfo.InvariantCulture)), () => GenerateGetUsersCommand(conScope.Connection, database, companyId), (NavSqlCommand command) =>
				{
					if (companyId.HasValue)
					{
						command.Command.Parameters.Add(new SqlParameter("@companyid", SqlDbType.Int));
					}
				}, (NavSqlCommand command) =>
				{
					if (companyId.HasValue)
					{
						command.Command.Parameters[0].Value = companyId.Value;
					}
				}, (NavSqlCommand command) =>
				{
					using (command.ExecuteReader())
					{
						while (command.ReadNextRow())
						{
							NavUser item = new NavUser
							{
								userGuid = command.Reader.GetGuid(0),
								userName = command.Reader.GetString(1),
								fullName = command.Reader.GetString(2),
								state = (NavUserState)command.Reader.GetInt32(3),
								expiryDate = NavDateTimeFromSqlDateTime(command.Reader, 4),
								windowsSID = command.Reader.GetString(5),
								passwordHash = command.Reader.GetString(6),
								nameIdentifier = command.Reader.GetString(7),
								authenticationKey = command.Reader.GetString(8),
								webAccountKey = command.Reader.GetString(9),
								webAccountExpiryDate = NavDateTimeFromSqlDateTime(command.Reader, 10),
								licenseType = (LicenseType)command.Reader.GetInt32(11),
								ExchangeIdentifier = command.Reader.GetString(12),
								applicationId = command.Reader.GetGuid(13),
								authenticationEmail = command.Reader.GetString(14),
								authenticationObjectId = command.Reader.GetString(15),
								contactEmail = command.Reader.GetString(16),
								DirectoryRoleId = command.Reader.GetString(17)
							};
							users.Add(item);
						}
						command.CloseReader(forceClose: false);
					}
					return 0;
				});
			}
			catch (NavSqlException ex) when (((SqlException)ex.InnerException).Number == 208)
			{
			}
			return users;
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}
}
