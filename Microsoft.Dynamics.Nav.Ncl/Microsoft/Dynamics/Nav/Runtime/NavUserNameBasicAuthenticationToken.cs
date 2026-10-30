using System.IdentityModel.Tokens;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUserNameBasicAuthenticationToken : UserNameSecurityToken
{
	public NavUserNameBasicAuthenticationToken(string userName, string password)
		: base(userName, password)
	{
	}
}
