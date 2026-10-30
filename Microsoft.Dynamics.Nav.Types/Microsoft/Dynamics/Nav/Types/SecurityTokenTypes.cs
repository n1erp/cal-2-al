namespace Microsoft.Dynamics.Nav.Types;

public static class SecurityTokenTypes
{
	public const string Saml20 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";

	public const string Saml = "urn:oasis:names:tc:SAML:1.0:assertion";

	public const string Saml20AcsStyle = "urn:oasis:names:tc:SAML:2.0:assertion";

	public const string Swt = "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0";

	public const string Jwt = "urn:ietf:params:oauth:token-type:jwt";

	public const string BinaryJwt = "BinaryJwt";

	public const string AppUserAssertion = "app_asserted_user_v1";

	public const string AppServiceAssertion = "service_asserted_app_v1";
}
