using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.V10;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class UserManagement
{
	public static void SetAuthenticationKey(NavSession session, Guid userSecurityId, string authenticationKey)
	{
		if (!SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		bool flag = false;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			try
			{
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				navRecord.SetFieldValueSafe(4, NavType.Text, new NavText(authenticationKey));
				navRecord.SetFieldValueSafe(3, NavType.Text, new NavText(string.Empty));
				navRecord.ALModify();
				if (flag)
				{
					session.EndTransaction(commit: true);
				}
			}
			finally
			{
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(commit: false);
				}
			}
		}
	}

	public static string GetAuthenticationKey(NavSession session, Guid userSecurityId)
	{
		if (!SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			return navRecord.GetFieldValue(4).ToString();
		}
	}

	internal static void SetNameIdentifier(NavSession session, Guid userSecurityId, string nameIdentifier)
	{
		bool flag = false;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			string text = navRecord.GetFieldValueSafe(3, NavType.Text).ToString();
			if (text.Equals(nameIdentifier, StringComparison.Ordinal))
			{
				return;
			}
			if (!string.IsNullOrEmpty(text))
			{
				throw new NavCannotUseNameIdentifierException(string.Format(CultureInfo.CurrentCulture, Lang.NameIdentifierNotEmpty, ProductNames.ProductNameShort));
			}
			try
			{
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				navRecord.SetFieldValueSafe(3, NavType.Text, new NavText(nameIdentifier));
				navRecord.SetFieldValueSafe(4, NavType.Text, NavText.Empty);
				navRecord.ALModify();
				if (flag)
				{
					session.EndTransaction(commit: true);
				}
			}
			finally
			{
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(commit: false);
				}
			}
		}
	}

	public static string GetNameIdentifier(NavSession session, Guid userSecurityId)
	{
		if (!SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			return navRecord.GetFieldValue(3).ToString();
		}
	}

	public static void SetAuthenticationEmail(NavSession session, Guid userSecurityId, string authenticationEmail)
	{
		if (authenticationEmail == null)
		{
			throw new ArgumentNullException("authenticationEmail");
		}
		if (session.User.Id != userSecurityId && !SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw new NavCannotSpecifyAuthenticationEmailException(Lang.CurrentUserIsNotAllowedToChangeAuthenticationEmailNotValid);
		}
	}

	public static string GetAuthenticationEmail(NavSession session, Guid userSecurityId)
	{
		if (session.User.Id != userSecurityId && !session.Permissions.IsSuperForAllCompanies)
		{
			return string.Empty;
		}
		using (new MaximizedPermissionScope(session, new int[1] { 2000000120 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000120);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			return navRecord.GetFieldValueSafe(11, NavType.Text).ToString();
		}
	}

	public static void SetUserExchangeIdentifier(NavSession session, INavSecurityToken exchangeToken)
	{
		if (exchangeToken == null)
		{
			throw session.Diagnostics.SendExceptionTag("000001A", Category.ExchangeAuthentication, new NavInvalidSecurityTokenException("exchangeToken"));
		}
		DirectSetUserFieldValue(session, session.User.Id, 2000000120, 15, NavType.Text, new NavText(exchangeToken.TokenId));
	}

	internal static void DirectSetAuthenticationEmail(NavSession session, Guid userSecurityId, string authenticationEmail)
	{
		DirectSetUserFieldValue(session, userSecurityId, 2000000120, 11, NavType.Text, new NavText(authenticationEmail));
	}

	internal static void DirectSetApplicationId(NavSession session, Guid userSecurityId, string appId)
	{
		DirectSetUserFieldValue(session, userSecurityId, 2000000120, 16, NavType.GUID, new NavGuid(appId));
	}

	public static void SetAuthenticationObjectId(NavSession session, Guid userSecurityId, string authenticationObjectId)
	{
		if (!string.IsNullOrEmpty(authenticationObjectId) && !SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		DirectSetAuthenticationObjectId(session, userSecurityId, authenticationObjectId);
	}

	internal static void DirectSetAuthenticationObjectId(NavSession session, Guid userSecurityId, string authenticationObjectId)
	{
		DirectSetUserFieldValue(session, userSecurityId, 2000000121, 7, NavType.Text, new NavText(authenticationObjectId));
	}

	internal static void DirectSetDirectoryRoleId(NavSession session, Guid userSecurityId, string directoryRoleId)
	{
		DirectSetUserFieldValue(session, userSecurityId, 2000000121, 8, NavType.Text, new NavText(directoryRoleId));
	}

	public static string GetAuthenticationObjectId(NavSession session, Guid userSecurityId)
	{
		if (!SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
		using (new MaximizedPermissionScope(session, new int[1] { 2000000120 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			return navRecord.GetFieldValueSafe(7, NavType.Text).ToString();
		}
	}

	public static AuthenticationStatus GetAuthenticationStatus(NavSession session, Guid userSecurityId)
	{
		using (new MaximizedPermissionScope(session, new int[2] { 2000000120, 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000120);
			using NavRecord navRecord2 = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind(DataError.ThrowError, "-");
			navRecord2.ALSetCurrentKey(1);
			navRecord2.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord2.ALFind(DataError.ThrowError, "-");
			string value = navRecord.GetFieldValueSafe(11, NavType.Text).ToString();
			string value2 = navRecord2.GetFieldValueSafe(7, NavType.Text).ToString();
			if (string.IsNullOrEmpty(value2))
			{
				return (!string.IsNullOrEmpty(value)) ? AuthenticationStatus.Inactive : AuthenticationStatus.Disabled;
			}
			return AuthenticationStatus.Active;
		}
	}

	public static string CreateWebServicesKey(NavSession session, Guid userSecurityId, DateTime expiryDate)
	{
		bool flag = false;
		if (session.User.Id == userSecurityId || SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			string text;
			using (HMACSHA256 hMACSHA = new HMACSHA256())
			{
				text = Convert.ToBase64String(hMACSHA.ComputeHash(Guid.NewGuid().ToByteArray()));
			}
			using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000121);
				navRecord.ALSetCurrentKey(1);
				navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
				navRecord.ALFind("-");
				try
				{
					if (!session.IsTransactionActive())
					{
						session.BeginTransaction();
						flag = true;
					}
					navRecord.SetFieldValueSafe(5, NavType.Text, new NavText(text));
					navRecord.SetFieldValueSafe(6, NavType.DateTime, NavDateTime.Create(expiryDate));
					navRecord.ALModify();
					if (flag)
					{
						session.EndTransaction(commit: true);
					}
				}
				finally
				{
					if (flag && session.IsTransactionActive())
					{
						session.EndTransaction(commit: false);
					}
				}
			}
			return text;
		}
		throw new NavReadDeniedPermissionException(Lang.NoAccessToWebServiceKey);
	}

	public static void ClearWebServicesKey(NavSession session, Guid userSecurityId)
	{
		bool flag = false;
		if (session.User.Id == userSecurityId || SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000121);
				navRecord.ALSetCurrentKey(1);
				navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
				navRecord.ALFind("-");
				try
				{
					if (!session.IsTransactionActive())
					{
						session.BeginTransaction();
						flag = true;
					}
					navRecord.ClearFieldValue(5);
					navRecord.ClearFieldValue(6);
					navRecord.ALModify();
					if (flag)
					{
						session.EndTransaction(commit: true);
					}
					return;
				}
				finally
				{
					if (flag && session.IsTransactionActive())
					{
						session.EndTransaction(commit: false);
					}
				}
			}
		}
		throw new NavReadDeniedPermissionException(Lang.NoAccessToWebServiceKey);
	}

	public static string GetWebServicesKey(NavSession session, Guid userSecurityId, out DateTime expiryDate)
	{
		expiryDate = NavDateTime.Default.ToDateTime();
		if (session.User.Id == userSecurityId || SessionHasSuperOrSecurityPermissionsForUser(session, userSecurityId))
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000121);
				navRecord.ALSetCurrentKey(1);
				navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
				navRecord.ALFind(DataError.ThrowError, "-");
				expiryDate = navRecord.GetFieldValue(6).ToDateTime();
				return navRecord.GetFieldValue(5).ToString();
			}
		}
		throw NavNCLSuperRoleRequiredException.Create();
	}

	public static bool HasNavUserPassword(NavSession session, Guid userSecurityId)
	{
		using (new MaximizedPermissionScope(session, new int[1] { 2000000121 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000121);
			navRecord.ALSetCurrentKey(1);
			navRecord.ALSetFilter(1, "%1", new NavGuid(userSecurityId));
			navRecord.ALFind("-");
			if (!string.IsNullOrEmpty(navRecord.GetFieldValueSafe(2, NavType.Text).ToString()))
			{
				return true;
			}
		}
		return false;
	}

	public static bool SessionHasSuperOrSecurityPermissionsForUser(NavSession session, Guid userSecurityId)
	{
		if (session.AdminSession || session.Permissions.IsSuperForAllCompanies)
		{
			return true;
		}
		bool flag = false;
		bool flag2 = true;
		bool flag3 = true;
		using (NavRecord navRecord = new NavRecord(session, 2000000053))
		{
			NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
			navRecord.ALSetRange(1, new NavGuid(userSecurityId));
			navRecord.ALSetRange(8, NavOption.Create(fieldOptionMetadata, 0));
			navRecord.ALSetRange(9, new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value));
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					int companyNameToken = session.Database.GetCompanyNameToken(navRecord.GetFieldValue(3).ToString());
					string text = navRecord.GetFieldValue(2).ToString();
					flag = text.Equals("SUPER") | flag;
					flag3 = session.Permissions.HasRole(companyNameToken, "SECURITY", PermissionScope.System, AppId.None) & flag3;
					flag2 = session.Permissions.HasRole(companyNameToken, "SUPER", PermissionScope.System, AppId.None) & flag2;
				}
				while (navRecord.ALNext() != 0);
			}
		}
		if (flag2)
		{
			return true;
		}
		if (flag3 && !flag)
		{
			return true;
		}
		return false;
	}

	public static Guid CreateUserFromAzureADObjectId(NavSession session, string objectId, UserInfo userInfo = null)
	{
		session.Diagnostics.SendTraceTag("000002G", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Creating NAV user from Azure AD object Id.");
		if (!session.AdminSession && !session.Permissions.IsSuperForAllCompanies)
		{
			session.Diagnostics.SendTraceTag("000002H", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Operation requires admin or super privilegdes.");
			throw NavNCLSuperRoleRequiredException.Create();
		}
		if (userInfo == null)
		{
			NavLicenseServiceClient navLicenseServiceClient = new NavLicenseServiceClient(session.Tenant);
			LicenseDetails licenseDetailsByObjectId = navLicenseServiceClient.GetLicenseDetailsByObjectId(session.Tenant.TenantSettings.AadTenantId, objectId, session.Diagnostics);
			userInfo = ((licenseDetailsByObjectId != null) ? licenseDetailsByObjectId.User : null);
		}
		if (userInfo == null)
		{
			session.Diagnostics.SendTraceTag("000002I", Category.Security, Verbosity.Verbose, "Azure AD user was not found with object id {0}.", DiagnosticParameter.Euii(objectId));
			throw new NavNCLArgumentOutOfRangeException("objectId");
		}
		if (session.Tenant.Database.NavUsers.TryGetNavUserFromAuthenticationObjectId(((UserInfo)userInfo).ObjectId, out var _))
		{
			session.Diagnostics.SendTraceTag("000002J", Category.Security, Verbosity.Verbose, "User with object id {0} was already created.", DiagnosticParameter.Euii(objectId));
			return Guid.Empty;
		}
		UserSecurityTokenHelper tokenInfo = UserSecurityTokenHelper.Create(((UserInfo)userInfo).ObjectId, null, userInfo);
		NavUser newNavUser = NavUser.CreateUserFromNavAzureADUserTokenInfo(tokenInfo);
		((ITenantSessionHandler)session.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession navSession) =>
		{
			newNavUser = SaveOrRefreshUser(session.Tenant, newNavUser);
		}), true, false, false, (LanguageSetting)null);
		session.Diagnostics.SendTraceTag("000002K", Category.Security, Verbosity.Verbose, "New NAV user created from Azure AD object Id: {0}", DiagnosticParameter.Euii(objectId));
		return newNavUser.Id;
	}

	private static string GetUniqueUserName(string username, int maxUserNameLength, NavUserCache userCache)
	{
		if (userCache == null)
		{
			throw new ArgumentNullException("userCache");
		}
		string text = username;
		if (text.Length > maxUserNameLength)
		{
			text = text.Substring(0, maxUserNameLength);
		}
		string text2 = text;
		if (userCache.TryGetNavUserFromUserName(text2, out var user))
		{
			int num = 1;
			do
			{
				string text3 = " " + num.ToString(CultureInfo.InvariantCulture);
				text2 = ((text.Length + text3.Length > maxUserNameLength) ? text.Substring(0, maxUserNameLength - text3.Length) : text) + text3;
				num++;
			}
			while (userCache.TryGetNavUserFromUserName(text2, out user));
		}
		return text2;
	}

	internal static NavUser SaveOrRefreshUser(NavTenant tenant, NavUser user, bool enableSuperRole = false)
	{
		int timeout = 5000;
		bool retry = false;
		try
		{
			string text = ((!string.IsNullOrEmpty(user.NameIdentifier)) ? user.NameIdentifier : user.Name);
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenant.Database, TransactionType.Update);
			using NavSqlDatabaseLock navSqlDatabaseLock = NavSqlDatabaseLock.LockResource(conScope, tenant.Id + text, timeout);
			if (!navSqlDatabaseLock.IsLockAcquired())
			{
				throw NavNCLUserTableUserNameMustBeUniqueException.Create();
			}
			((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession session) =>
			{
				using NavRecord navRecord = new NavRecord(session, 2000000120);
				try
				{
					using NavRecord navRecord2 = new NavRecord(session, 2000000121);
					navRecord.ALInit();
					navRecord.SetFieldValue(1, new NavGuid(user.Id));
					NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
					navRecord.SetFieldValue(fieldByNo, new NavCode(fieldByNo.FieldDefinedLength, user.Name));
					fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
					string value = ((user.DisplayName.Length > fieldByNo.FieldDefinedLength) ? user.DisplayName.Substring(0, fieldByNo.FieldDefinedLength) : user.DisplayName);
					navRecord.SetFieldValue(fieldByNo, new NavText(fieldByNo.FieldDefinedLength, value));
					navRecord.SetFieldValue(5, NavDateTime.Create(user.ExpiryDate));
					navRecord.SetFieldValue(10, NavOption.Create(NCLOptionMetadata.Default, (int)user.LicenseType));
					navRecord.SetFieldValue(11, new NavText(user.AuthenticationEmail));
					navRecord.SetFieldValue(14, new NavText(user.ContactEmail));
					navRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
					if (!string.IsNullOrEmpty(user.AuthenticationObjectId) || !string.IsNullOrEmpty(user.NameIdentifier))
					{
						navRecord2.ALSetCurrentKey(1);
						navRecord2.ALSetFilter(1, "%1", new NavGuid(user.Id));
						navRecord2.ALFind(DataError.TrapError, "-");
						if (!string.IsNullOrEmpty(user.AuthenticationObjectId))
						{
							navRecord2.SetFieldValueSafe(7, NavType.Text, new NavText(user.AuthenticationObjectId));
						}
						if (!string.IsNullOrEmpty(user.DirectoryRoleId))
						{
							navRecord2.SetFieldValueSafe(8, NavType.Text, new NavText(user.DirectoryRoleId));
						}
						if (!string.IsNullOrEmpty(user.NameIdentifier))
						{
							navRecord2.SetFieldValueSafe(3, NavType.Text, new NavText(user.NameIdentifier));
						}
						navRecord2.ALModify(DataError.ThrowError, runApplicationTrigger: false, isBulkModify: false);
					}
					if (enableSuperRole)
					{
						using (NavRecord navRecord3 = new NavRecord(session, 2000000053))
						{
							navRecord3.ALInit();
							navRecord3.SetFieldValue(1, new NavGuid(user.Id));
							navRecord3.SetFieldValue(2, new NavCode(2048, "SUPER"));
							navRecord3.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
							return;
						}
					}
				}
				catch (NavNCLUserTableUserNameMustBeUniqueException)
				{
					session.Diagnostics.SendTraceTag("0000203", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Try to create user with an existing username.");
					session.Tenant.Database.NavUsers.RefreshList();
					NavUser user2 = null;
					if (!string.IsNullOrEmpty(user.AuthenticationObjectId) && session.Tenant.Database.NavUsers.TryGetNavUserFromAuthenticationObjectId(user.AuthenticationObjectId, out user2))
					{
						session.Diagnostics.SendTraceTag("0000204", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User already created, reloaded from the cache by object id.");
						user = user2;
					}
					else if (!string.IsNullOrEmpty(user.NameIdentifier) && session.Tenant.Database.NavUsers.TryGetNavUserFromNameIdentifier(user.NameIdentifier, out user2))
					{
						session.Diagnostics.SendTraceTag("0000205", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User alreade created, reloaded from the cache by name identifier.");
						user = user2;
					}
					else
					{
						SetUniqueUserName(tenant, user, navRecord.MetaTable.GetFieldByNo(2).FieldDefinedLength);
						retry = true;
					}
				}
				catch (NavNCLUserPropertyTableNameIdentifierMustBeUniqueException)
				{
					if (session.Tenant.Database.NavUsers.TryGetNavUserFromNameIdentifier(user.NameIdentifier, out var user3))
					{
						session.Diagnostics.SendTraceTag("0000206", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User alreade created, reloaded from the cache by name identifier.");
						user = user3;
						return;
					}
					throw;
				}
			}), true, false, false, (LanguageSetting)null);
		}
		finally
		{
			if (retry)
			{
				user = SaveOrRefreshUser(tenant, user, enableSuperRole);
			}
		}
		return user;
	}

	private static void SetUniqueUserName(NavTenant tenant, NavUser user, int nameFieldLength)
	{
		user.Name = GetUniqueUserName(user.Name, nameFieldLength, tenant.Database.NavUsers);
	}

	private static void DirectSetUserFieldValue(NavSession session, Guid userSecurityId, int tableId, int fieldNo, NavType type, NavValue value)
	{
		bool flag = false;
		using (new MaximizedPermissionScope(session, new int[1] { tableId }))
		{
			using NavRecord navRecord = new NavRecord(session, tableId);
			navRecord.Get(DataError.ThrowError, new NavRecordId(tableId, new NavValue[1]
			{
				new NavGuid(userSecurityId)
			}));
			try
			{
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				navRecord.SetFieldValueSafe(fieldNo, type, value);
				navRecord.ALModify();
				if (flag)
				{
					session.EndTransaction(commit: true);
				}
			}
			finally
			{
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(commit: false);
				}
			}
		}
	}

	public static string GetEffectivePermissionForObject(NavSession session, Guid userSecurityId, string companyName, ApplicationObjectId objectId)
	{
		NavUser userWithEntitlement = GetUserWithEntitlement(session, userSecurityId);
		PermissionMask effectivePermissionForObject = new NavUserPermissions(userWithEntitlement, session).GetEffectivePermissionForObject(companyName, objectId);
		return PermissionHelper.GetPermission(effectivePermissionForObject).ToString();
	}

	public static string GetEntitlementPermissionForObject(NavSession session, Guid userSecurityId, ApplicationObjectId objectId)
	{
		NavUser userWithEntitlement = GetUserWithEntitlement(session, userSecurityId);
		PermissionMask entitlementPermissions = EntitlementPermissionSet.GetEntitlementPermissions(userWithEntitlement, objectId);
		return PermissionHelper.GetPermission(entitlementPermissions).ToString();
	}

	internal static void SetUserStateDisabled(NavUser user, NavTenant tenant)
	{
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession session) =>
		{
			DirectSetUserFieldValue(session, user.Id, 2000000120, 4, NavType.Option, NavOption.Create(NCLOptionMetadata.Default, 1));
		}), false, false, false, (LanguageSetting)null);
	}

	private static NavUser GetUserWithEntitlement(NavSession session, Guid userSecurityId)
	{
		if (!session.Tenant.Database.NavUsers.TryGetNavUserFromId(userSecurityId, out var user))
		{
			throw new ArgumentException("User does not exist.");
		}
		LoadUserEntitlement(user, session, NavGlobal.SystemTenant.EntitlementProvider);
		return user;
	}

	private static void LoadUserEntitlement(NavUser user, NavSession session, EntitlementProvider entitlementProvider)
	{
		if (user.MembershipEntitlements == null)
		{
			if (user.Id == session.User.Id && user.MembershipEntitlements == null)
			{
				user.MembershipEntitlements = session.User.MembershipEntitlements;
			}
			else
			{
				session.Authenticator.LoadEntitlementsSimulatedForUser(user, entitlementProvider);
			}
		}
	}
}
