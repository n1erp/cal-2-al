using System;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public static class SecurityTokenExtensionMethods
{
	public static DiagnosticParameter GetDetailsForLog(this INavSecurityToken navSecurityToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Issuer: " + navSecurityToken.Issuer);
		stringBuilder.AppendLine("Expiry (UTC): " + navSecurityToken.UtcExpiresOn.ToString());
		AppendClaimValueIfNotNull(stringBuilder, navSecurityToken, SecurityTokenClaimType.TenantId);
		AppendClaimValueIfNotNull(stringBuilder, navSecurityToken, SecurityTokenClaimType.AppId);
		AppendClaimValueIfNotNull(stringBuilder, navSecurityToken, SecurityTokenClaimType.IdentityProvider);
		AppendClaimValueIfNotNull(stringBuilder, navSecurityToken, SecurityTokenClaimType.Wids);
		AppendClaimValueIfNotNull(stringBuilder, navSecurityToken, SecurityTokenClaimType.Version);
		return DiagnosticParameter.Oii(stringBuilder);
	}

	public static DiagnosticParameter GetTransientIdentityDetailsForLog(this INavSecurityToken navSecurityToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Issuer: " + navSecurityToken.Issuer);
		stringBuilder.AppendLine("Expiry (UTC): " + navSecurityToken.UtcExpiresOn.ToString());
		stringBuilder.AppendLine("Claims in token:");
		foreach (string claimName in navSecurityToken.ClaimNames)
		{
			stringBuilder.AppendLine("  " + claimName + ": " + navSecurityToken.GetClaimValue(claimName));
		}
		return DiagnosticParameter.Euii(stringBuilder);
	}

	public static string AuthenticationEmail(Func<SecurityTokenClaimType, string> getClaimValue)
	{
		string text = getClaimValue(SecurityTokenClaimType.Name);
		if (string.IsNullOrEmpty(text))
		{
			text = getClaimValue(SecurityTokenClaimType.UserPrincipalName);
		}
		if (string.IsNullOrEmpty(text))
		{
			text = getClaimValue(SecurityTokenClaimType.EmailAddress);
		}
		return text;
	}

	private static void AppendClaimValueIfNotNull(StringBuilder sb, INavSecurityToken navSecurityToken, SecurityTokenClaimType claimType)
	{
		string claimValue = navSecurityToken.GetClaimValue(claimType);
		if (!string.IsNullOrEmpty(claimValue))
		{
			sb.AppendLine(" " + claimType.ToString() + ": " + claimValue);
		}
	}
}
