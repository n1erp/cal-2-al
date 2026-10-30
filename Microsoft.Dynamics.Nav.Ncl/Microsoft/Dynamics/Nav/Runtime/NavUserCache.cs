using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUserCache
{
	private enum SearchUserFields
	{
		UserId,
		Sid,
		UserName,
		AuthenticationKey,
		NameIdentifier,
		objectIdentifier
	}

	private TimeSpan defaultCacheExpirationInterval = TimeSpan.FromMinutes(10.0);

	private TimeSpan defaultRefreshNoHitInterval = TimeSpan.FromSeconds(5.0);

	private DateTime nextCacheExpirationTimeUtc;

	private DateTime nextRefreshNoHitTimeUtc;

	private List<NavUser> navUsers = new List<NavUser>();

	private readonly Func<NavDatabase, List<NavUser>> getAllUsers;

	private readonly NavDatabase database;

	internal bool IsEmpty => navUsers.Count == 0;

	public bool AnyUnmappedAcsUsers => GetUser((NavUser u) => !string.IsNullOrEmpty(u.AuthenticationKey)) != null;

	public NavUserCache(NavDatabase database)
		: this(NavUser.GetAllUsers)
	{
		this.database = database;
	}

	public NavUserCache(Func<NavDatabase, List<NavUser>> getAllUsers)
	{
		this.getAllUsers = getAllUsers;
	}

	public bool TryGetNavUserFromId(Guid userId, out NavUser user)
	{
		user = GetUser((NavUser u) => u.Id == userId);
		if (user == null)
		{
			return false;
		}
		return true;
	}

	public bool TryGetNavUserFromSid(string sid, out NavUser user)
	{
		user = null;
		if (string.IsNullOrEmpty(sid))
		{
			return false;
		}
		return TryGetNavUser((NavUser u) => u.Sid == sid, out user);
	}

	public bool TryGetWindowsGroupFromSid(string sid, out NavUser user)
	{
		return TryGetNavUser((NavUser u) => u.Sid == sid && u.LicenseType == LicenseType.WindowsGroup && u.IsEnabled, out user);
	}

	public bool TryGetNavUserFromUserName(string userName, out NavUser user)
	{
		int num = NavCurrentThread.Session?.NCLMetadata?.GetMetaTableById(2000000120).GetFieldByNo(2).FieldDefinedLength ?? 50;
		if (userName.Length > num)
		{
			userName = userName.Substring(0, num);
		}
		return TryGetNavUser((NavUser u) => u.Name == new NavCode(userName.Length, userName).Value, out user);
	}

	public bool TryGetNavUserFromAuthenticationEmail(string authenticationEmail, out NavUser user)
	{
		user = null;
		if (string.IsNullOrWhiteSpace(authenticationEmail))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004P", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Authentication email is empty");
			return false;
		}
		bool flag = TryGetNavUser((NavUser u) => authenticationEmail.Equals(u.AuthenticationEmail, StringComparison.CurrentCultureIgnoreCase), out user);
		if (flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004Q", Category.Security, Verbosity.Verbose, "Authentication email {0} mapped to NAV user {1}", DiagnosticParameter.Euii(authenticationEmail), DiagnosticParameter.Eupi(user.Id));
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004R", Category.Security, Verbosity.Verbose, "No NAV user found which could be mapped to authentication email {0}", DiagnosticParameter.Euii(authenticationEmail));
		}
		return flag;
	}

	public bool TryGetEnabledNavUserFromAuthenticationEmail(string authenticationEmail, out NavUser user)
	{
		user = null;
		if (string.IsNullOrWhiteSpace(authenticationEmail))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QV", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Authentication email is empty");
			return false;
		}
		return TryGetNavUser((NavUser u) => u.State.Equals(NavUserState.Enabled) && authenticationEmail.Equals(u.AuthenticationEmail, StringComparison.CurrentCultureIgnoreCase), out user);
	}

	public bool TryGetNavUserFromApplicationId(Guid applicationId, out NavUser user)
	{
		user = null;
		if (applicationId == Guid.Empty)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004S", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Application id is empty");
			return false;
		}
		bool flag = TryGetNavUser((NavUser u) => applicationId.Equals(u.ApplicationId), out user);
		if (flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004T", Category.Security, Verbosity.Verbose, "Application id {0} mapped to NAV user {1}", DiagnosticParameter.SystemMetadata(applicationId), DiagnosticParameter.Eupi(user.Id));
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004U", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "No NAV user found which is mapped to application id {0}", applicationId);
		}
		return flag;
	}

	public bool TryGetNavUserFromExchangeIdentifier(string exchangeIdentifier, out NavUser user)
	{
		user = null;
		if (string.IsNullOrWhiteSpace(exchangeIdentifier))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004V", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Exchange id is empty");
			return false;
		}
		bool flag = TryGetNavUser((NavUser u) => exchangeIdentifier.Equals(u.ExchangeIdentifier, StringComparison.CurrentCultureIgnoreCase), out user);
		if (flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000042", Category.Security, Verbosity.Verbose, "Exchange id {0} mapped to NAV user {1}", DiagnosticParameter.Eupi(exchangeIdentifier), DiagnosticParameter.Eupi(user.Id));
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000043", Category.Security, Verbosity.Verbose, "No NAV user found which is mapped to exchange id {0}", DiagnosticParameter.Eupi(exchangeIdentifier));
		}
		return flag;
	}

	public bool TryGetNavUserFromAuthenticationObjectId(string authenticationObjectId, out NavUser user)
	{
		user = null;
		if (string.IsNullOrEmpty(authenticationObjectId))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004W", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Authentication object ID is empty");
			return false;
		}
		bool flag = TryGetNavUser((NavUser u) => authenticationObjectId.Equals(u.AuthenticationObjectId, StringComparison.Ordinal), out user);
		if (flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000044", Category.Security, Verbosity.Verbose, "Object id {0} mapped to NAV user {1}", DiagnosticParameter.Eupi(authenticationObjectId), DiagnosticParameter.Eupi(user.Id));
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000045", Category.Security, Verbosity.Verbose, "No NAV user found which is mapped to object id {0}", DiagnosticParameter.Eupi(authenticationObjectId));
		}
		return flag;
	}

	public bool TryGetNavUserFromNameIdentifier(string nameIdentifier, out NavUser user)
	{
		user = null;
		if (string.IsNullOrEmpty(nameIdentifier))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004X", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Name identifier is empty");
			return false;
		}
		bool flag = TryGetNavUser((NavUser u) => u.NameIdentifier == nameIdentifier, out user);
		if (flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004Y", Category.Security, Verbosity.Verbose, "Name identifier {0} mapped to NAV user {1}", DiagnosticParameter.Euii(nameIdentifier), DiagnosticParameter.Eupi(user.Id));
		}
		else
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000004Z", Category.Security, Verbosity.Verbose, "No NAV user found which is mapped to name identifier {0}", DiagnosticParameter.Euii(nameIdentifier));
		}
		return flag;
	}

	private bool TryGetNavUser(Func<NavUser, bool> match, out NavUser user)
	{
		user = GetUser(match);
		return user != null;
	}

	private NavUser GetUser(Func<NavUser, bool> match)
	{
		IEnumerable<NavUser> nonExpiredList = GetNonExpiredList();
		NavUser navUser = nonExpiredList.FirstOrDefault(match);
		if (navUser != null)
		{
			return navUser;
		}
		if (NoHitsInCache())
		{
			RefreshList();
			return GetUser(match);
		}
		return null;
	}

	internal bool IsExpired()
	{
		return DateTime.UtcNow >= nextCacheExpirationTimeUtc;
	}

	internal void ExpireCache()
	{
		nextCacheExpirationTimeUtc = DateTime.UtcNow;
	}

	private bool NoHitsInCache()
	{
		return DateTime.UtcNow > nextRefreshNoHitTimeUtc;
	}

	private IEnumerable<NavUser> GetNonExpiredList()
	{
		if (IsExpired())
		{
			RefreshList();
		}
		return navUsers;
	}

	internal void RefreshList()
	{
		navUsers = getAllUsers(database);
		nextCacheExpirationTimeUtc = DateTime.UtcNow.Add(defaultCacheExpirationInterval);
		nextRefreshNoHitTimeUtc = DateTime.UtcNow.Add(defaultRefreshNoHitInterval);
	}
}
