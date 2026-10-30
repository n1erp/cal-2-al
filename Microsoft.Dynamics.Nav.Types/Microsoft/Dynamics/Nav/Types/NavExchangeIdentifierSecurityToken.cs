using System;
using System.Collections.Generic;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types;

public class NavExchangeIdentifierSecurityToken : INavSecurityToken
{
	public string Issuer => string.Empty;

	public DateTime UtcExpiresOn { get; }

	public string TokenId { get; }

	public object TokenObject { get; }

	public string AuthorizationEmail => GetClaimValue("upn");

	public ICollection<string> ClaimNames => new List<string>();

	public SecureString SerializedToken => null;

	public string OpenAuthorizationGrantType => null;

	public Guid? IdentityProviderTenantId => null;

	private NavExchangeIdentifierSecurityToken()
	{
	}

	public NavExchangeIdentifierSecurityToken(object value, string tokenId, DateTime expires)
	{
		TokenObject = value;
		TokenId = tokenId;
		UtcExpiresOn = expires.AddHours(ServerUserSettings.Instance.ExtendedSecurityTokenLifetime.Value);
	}

	public string GetClaimValue(string claimName)
	{
		return claimName;
	}

	public string GetClaimValue(SecurityTokenClaimType claimType)
	{
		return SecurityTokenClaimNames.GetDefaultClaimName(claimType);
	}
}
