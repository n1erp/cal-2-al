using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUserAuthenticationCache
{
	private class ExpiringNavUserAuthenticationValue : ExpiringValue<NavUserAuthentication>
	{
		internal ExpiringNavUserAuthenticationValue(NavUserAuthentication value)
			: base(value.PendingUserActionRequired ? value : value.Clone(), (DateTime.UtcNow.Add(ServerUserSettings.Instance.AuthenticationCacheExpirationTime.Value) > value.TokenExpiresOn) ? value.TokenExpiresOn : DateTime.UtcNow.Add(ServerUserSettings.Instance.AuthenticationCacheExpirationTime.Value))
		{
		}
	}

	private ConcurrentDictionary<string, ExpiringNavUserAuthenticationValue> cache;

	private ConcurrentDictionary<string, string> userNameToKey;

	private NavTenant navTenant;

	private bool enableCache;

	private long cleanupRunning;

	private List<Guid> usersToExpire;

	internal int Count => cache.Count;

	internal NavUserAuthenticationCache(NavTenant tenant)
	{
		navTenant = tenant;
		enableCache = ServerUserSettings.Instance.EnableMembershipEntitlement.Value;
		cache = new ConcurrentDictionary<string, ExpiringNavUserAuthenticationValue>();
		userNameToKey = new ConcurrentDictionary<string, string>();
		usersToExpire = new List<Guid>();
	}

	public NavUserAuthentication GetNavUserAuthentication(object token, string navAppId, NavDiagnostics diagnostics, Func<object, NavUserAuthentication> authenticationFunc, AuthorizationCodeContext authorizationCodeContext = null, NavClientCredentialType authenticationType = NavClientCredentialType.AccessControlService)
	{
		if (usersToExpire.Count > 0)
		{
			ExpireUser(Guid.Empty);
		}
		string cacheKeyFromAuthenticatedUser = GetCacheKeyFromAuthenticatedUser(token, navAppId ?? ServerUserSettings.Instance.DefaultApplicationId.Value, authenticationType);
		NavUserAuthentication value = null;
		if (TryGetActiveValue(cacheKeyFromAuthenticatedUser, out value))
		{
			if (value.RefreshUserAuthorization(authorizationCodeContext, token, authenticationType) && authenticationType != NavClientCredentialType.TaskScheduler)
			{
				diagnostics?.SendTraceTag("0000132", Category.Security, Verbosity.Normal, "Refreshing the NavUserAuthentication object with AuthorizationCodeContext key = {0}, AuthenticationType = {1}", DiagnosticParameter.Eupi(value.CacheKey), DiagnosticParameter.SystemMetadata(authenticationType));
				UpdateCache(value);
			}
			return value;
		}
		diagnostics?.SendTraceTag("00000ZY", Category.Security, Verbosity.Normal, "Unable to find a matching NavUserAuthentication object in the cache with key {0}", DiagnosticParameter.Eupi(cacheKeyFromAuthenticatedUser));
		value = authenticationFunc(token);
		diagnostics?.SendTraceTag("00000ZZ", Category.Security, Verbosity.Normal, "Updateing the cache with the NavUserAuthentication object with key {0}", DiagnosticParameter.Eupi(value.CacheKey));
		UpdateCache(value);
		return value;
	}

	public NavUserAuthentication GetNavUserAuthentication(string userName)
	{
		if (userNameToKey.TryGetValue(userName, out var value))
		{
			TryGetActiveValue(value, out var value2);
			return value2;
		}
		return null;
	}

	internal void UpdateUserNameToCacheKey(string userName, string cacheKey)
	{
		RemoveExpiredValues();
		if (cache.ContainsKey(cacheKey))
		{
			userNameToKey.AddOrUpdate(userName, cacheKey, (string user, string oldCacheKey) => cacheKey);
		}
	}

	internal static string CreateCacheKey(string appId, Guid userGuid)
	{
		return string.Format(CultureInfo.InvariantCulture, "UserGuid={0},AppId={1}", userGuid.ToString("B").ToUpperInvariant(), appId.ToUpperInvariant());
	}

	internal void ExpireUser(Guid userId)
	{
		if (Interlocked.CompareExchange(ref cleanupRunning, 1L, 0L) != 0L)
		{
			if (userId != Guid.Empty)
			{
				lock (usersToExpire)
				{
					usersToExpire.Add(userId);
					return;
				}
			}
			return;
		}
		try
		{
			Guid[] matches;
			lock (usersToExpire)
			{
				usersToExpire.Add(userId);
				matches = usersToExpire.Distinct().ToArray();
				usersToExpire = new List<Guid>();
			}
			foreach (ExpiringNavUserAuthenticationValue value in cache.Values)
			{
				if (!value.Expired && value.Value.User.Id.In(matches))
				{
					value.ExpireItem();
				}
			}
		}
		finally
		{
			Interlocked.CompareExchange(ref cleanupRunning, 0L, 1L);
		}
	}

	private bool TryGetActiveValue(string key, out NavUserAuthentication value)
	{
		value = null;
		if (!string.IsNullOrEmpty(key) && cache.TryGetValue(key, out var value2) && !value2.Expired && value2.Value.IsAuthenticatedUserEnabled)
		{
			value = (value2.Value.PendingUserActionRequired ? value2.Value : value2.Value.Clone());
			navTenant?.Diagnostics?.SendTraceTag("00008YS", Category.Security, Verbosity.Normal, "Found a matching NavUserAuthentication object in the cache with key {0}", DiagnosticParameter.Eupi(key));
			return true;
		}
		return false;
	}

	private string GetCacheKeyFromAuthenticatedUser(object token, string navAppId, NavClientCredentialType authenticationType)
	{
		if (token == null)
		{
			return null;
		}
		if (!enableCache)
		{
			return null;
		}
		NavUser user = null;
		switch (authenticationType)
		{
		case NavClientCredentialType.AccessControlService:
			if (token is INavSecurityToken navSecurityToken)
			{
				NavWSFederationUserAuthenticator navWSFederationUserAuthenticator = new NavWSFederationUserAuthenticator(navSecurityToken.GetClaimValue, null, null, navTenant?.TenantSettings?.AadTenantId, navTenant.Diagnostics);
				navWSFederationUserAuthenticator.TryAuthenticateUser(navTenant.Database.NavUsers, out user, out var _);
			}
			break;
		case NavClientCredentialType.NavUserPassword:
			if (token is NavUserNameBasicAuthenticationToken navUserNameBasicAuthenticationToken && navTenant.Database.NavUsers.TryGetNavUserFromUserName(navUserNameBasicAuthenticationToken.UserName, out user) && !NavUser.TryAuthenticate(user, navUserNameBasicAuthenticationToken, navTenant))
			{
				return null;
			}
			break;
		case NavClientCredentialType.TaskScheduler:
			if (token is NavTaskSchedulerAuthenticationToken navTaskSchedulerAuthenticationToken && navTenant.Database.NavUsers.TryGetNavUserFromId(navTaskSchedulerAuthenticationToken.UserId, out user) && user?.Name != navTaskSchedulerAuthenticationToken.UserName)
			{
				return null;
			}
			break;
		default:
			return null;
		}
		if (!NavUser.TryAuthenticateCore(user))
		{
			return null;
		}
		return CreateCacheKey(navAppId, user.Id);
	}

	internal void UpdateCache(NavUserAuthentication navUserAuthentication)
	{
		RemoveExpiredValues();
		if (!navUserAuthentication.Authenticated)
		{
			return;
		}
		ExpiringNavUserAuthenticationValue expiringValue = new ExpiringNavUserAuthenticationValue(navUserAuthentication);
		if (!expiringValue.Expired)
		{
			navTenant?.Diagnostics?.SendTraceTag("00008YT", Category.Security, Verbosity.Normal, "Updating NavUserAuthentication object in the cache with key {0}", DiagnosticParameter.Eupi(navUserAuthentication.CacheKey));
			cache.AddOrUpdate(navUserAuthentication.CacheKey, expiringValue, (string key, ExpiringNavUserAuthenticationValue oldValue) => expiringValue);
		}
	}

	private void RemoveExpiredValues()
	{
		if (Interlocked.CompareExchange(ref cleanupRunning, 1L, 0L) != 0L)
		{
			return;
		}
		try
		{
			List<string> list = new List<string>();
			foreach (string item in cache.Keys.ToList())
			{
				if (cache.TryGetValue(item, out var value) && (value.Expired || !value.Value.IsAuthenticatedUserEnabled))
				{
					cache.TryRemove(item, out value);
					list.Add(item);
				}
			}
			foreach (string item2 in userNameToKey.Keys.ToList())
			{
				if (userNameToKey.TryGetValue(item2, out var value2) && list.Contains(value2))
				{
					userNameToKey.TryRemove(item2, out value2);
				}
			}
		}
		finally
		{
			Interlocked.CompareExchange(ref cleanupRunning, 0L, 1L);
		}
	}
}
