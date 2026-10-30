using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavWSFederationUserAuthenticator
{
	private readonly string objectIdentifierClaim;

	private readonly string authenticationEmail;

	private readonly string nameIdentifierClaim;

	private readonly string appIdClaim;

	private readonly string userTypedUserName;

	private readonly string userTypedAuthenticationKey;

	private readonly string issuerTenant;

	private readonly string aadTenantId;

	private readonly NavDiagnostics diagnostics;

	public NavWSFederationUserAuthenticator(Func<SecurityTokenClaimType, string> getClaimValue, string userTypedUserName, string userTypedAuthenticationKey, string currentTenant, NavDiagnostics diagnostics)
	{
		objectIdentifierClaim = getClaimValue(SecurityTokenClaimType.ObjectIdentifier);
		authenticationEmail = SecurityTokenExtensionMethods.AuthenticationEmail(getClaimValue);
		nameIdentifierClaim = getClaimValue(SecurityTokenClaimType.NameIdentifier);
		appIdClaim = getClaimValue(SecurityTokenClaimType.AppId);
		issuerTenant = getClaimValue(SecurityTokenClaimType.TenantId);
		aadTenantId = currentTenant;
		this.userTypedUserName = userTypedUserName;
		this.userTypedAuthenticationKey = userTypedAuthenticationKey;
		this.diagnostics = diagnostics;
	}

	internal void TryAuthenticateUser(NavUserCache navUserCache, out NavUser navUser, out UserActionRequired userActionRequired)
	{
		if (!string.IsNullOrWhiteSpace(objectIdentifierClaim) && !string.IsNullOrWhiteSpace(authenticationEmail))
		{
			TryFindUserByObjectIdOrEmail(navUserCache, out navUser, out userActionRequired);
			diagnostics?.SendTraceTag("000023B", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Authenticating using by object id and name claimm, succeeded: {0}", navUser != null));
		}
		else if (!string.IsNullOrWhiteSpace(appIdClaim) && string.IsNullOrEmpty(authenticationEmail) && aadTenantId.Equals(issuerTenant))
		{
			navUserCache.TryGetNavUserFromApplicationId(new Guid(appIdClaim), out navUser);
			userActionRequired = UserActionRequired.None;
			diagnostics?.SendTraceTag("000023D", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Authenticating using application id, succeeded: {0}", navUser != null));
		}
		else if (!string.IsNullOrWhiteSpace(nameIdentifierClaim))
		{
			TryFindUserByNameIdentifier(navUserCache, out navUser, out userActionRequired);
			diagnostics?.SendTraceTag("000023C", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Authenticating using nanme identifier, succeeded: {0}", navUser != null));
		}
		else
		{
			navUser = null;
			userActionRequired = UserActionRequired.None;
		}
	}

	private void TryFindUserByNameIdentifier(NavUserCache navUserCache, out NavUser navUser, out UserActionRequired userActionRequired)
	{
		navUser = null;
		userActionRequired = UserActionRequired.None;
		NavUser user2;
		if (navUserCache.TryGetNavUserFromNameIdentifier(nameIdentifierClaim, out var user))
		{
			navUser = user;
			userActionRequired = UserActionRequired.None;
		}
		else if (string.IsNullOrEmpty(userTypedAuthenticationKey))
		{
			if (navUserCache.AnyUnmappedAcsUsers)
			{
				navUser = null;
				userActionRequired = UserActionRequired.EnterUserNameAndAuthenticationKey;
			}
		}
		else if (navUserCache.TryGetNavUserFromUserName(userTypedUserName, out user2) && SignatureComparison.AreStringsIdentical(user2.AuthenticationKey, userTypedAuthenticationKey))
		{
			navUser = user2;
			userActionRequired = UserActionRequired.UpdateNameIdentifier;
		}
	}

	private void TryFindUserByObjectIdOrEmail(NavUserCache navUserCache, out NavUser navUser, out UserActionRequired userActionRequired)
	{
		navUserCache.TryGetNavUserFromAuthenticationObjectId(objectIdentifierClaim, out var user);
		navUserCache.TryGetNavUserFromAuthenticationEmail(authenticationEmail, out var user2);
		navUser = null;
		userActionRequired = UserActionRequired.None;
		if ((user != null || user2 != null) && (user == null || user2 == null || user == user2))
		{
			if (user == user2)
			{
				navUser = user;
			}
			else if (user != null)
			{
				navUser = user;
				userActionRequired = UserActionRequired.UpdateAuthenticationEmail;
			}
			else if (string.IsNullOrEmpty(user2.AuthenticationObjectId))
			{
				navUser = user2;
				userActionRequired = UserActionRequired.UpdateAuthenticationObjectId;
			}
		}
	}
}
