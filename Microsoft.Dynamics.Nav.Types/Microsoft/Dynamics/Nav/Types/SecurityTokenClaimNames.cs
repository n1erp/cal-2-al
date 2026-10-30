namespace Microsoft.Dynamics.Nav.Types;

public static class SecurityTokenClaimNames
{
	public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

	public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

	public const string AcsIdentityProvider = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

	public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

	public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

	public const string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";

	public const string DisplayName = "http://schemas.microsoft.com/identity/claims/displayname";

	public const string UserPrincipalName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";

	public const string AppId = "appid";

	public const string Upn = "upn";

	public const string Puid = "http://schemas.microsoft.com/identity/claims/puid";

	public const string EmailAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

	public const string Wids = "http://schemas.microsoft.com/ws/2008/06/identity/claims/wids";

	public const string Version = "http://schemas.microsoft.com/ws/2008/06/identity/claims/version";

	internal static string GetDefaultClaimName(SecurityTokenClaimType claimType)
	{
		return claimType switch
		{
			SecurityTokenClaimType.Name => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", 
			SecurityTokenClaimType.NameIdentifier => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", 
			SecurityTokenClaimType.AcsIdentityProvider => "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", 
			SecurityTokenClaimType.TenantId => "http://schemas.microsoft.com/identity/claims/tenantid", 
			SecurityTokenClaimType.ObjectIdentifier => "http://schemas.microsoft.com/identity/claims/objectidentifier", 
			SecurityTokenClaimType.IdentityProvider => "http://schemas.microsoft.com/identity/claims/identityprovider", 
			SecurityTokenClaimType.DisplayName => "http://schemas.microsoft.com/identity/claims/displayname", 
			SecurityTokenClaimType.UserPrincipalName => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", 
			SecurityTokenClaimType.AppId => "appid", 
			SecurityTokenClaimType.Puid => "http://schemas.microsoft.com/identity/claims/puid", 
			SecurityTokenClaimType.EmailAddress => "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", 
			SecurityTokenClaimType.Wids => "http://schemas.microsoft.com/ws/2008/06/identity/claims/wids", 
			SecurityTokenClaimType.Version => "http://schemas.microsoft.com/ws/2008/06/identity/claims/version", 
			_ => string.Empty, 
		};
	}
}
