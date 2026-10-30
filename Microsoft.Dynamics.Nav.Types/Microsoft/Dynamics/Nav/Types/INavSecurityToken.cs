using System;
using System.Collections.Generic;
using System.Security;

namespace Microsoft.Dynamics.Nav.Types;

public interface INavSecurityToken
{
	string Issuer { get; }

	DateTime UtcExpiresOn { get; }

	string TokenId { get; }

	string AuthorizationEmail { get; }

	ICollection<string> ClaimNames { get; }

	SecureString SerializedToken { get; }

	string OpenAuthorizationGrantType { get; }

	Guid? IdentityProviderTenantId { get; }

	string GetClaimValue(string claimName);

	string GetClaimValue(SecurityTokenClaimType claimType);
}
