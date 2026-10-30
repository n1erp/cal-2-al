using System.Security.Principal;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAdministratorDelegatedAdminValidator : INavAdministratorValidator
{
	public bool Validate(WindowsIdentity windowsIdentity, IIdentity primaryIdentity)
	{
		if (windowsIdentity == null && primaryIdentity != null)
		{
			if (primaryIdentity.AuthenticationType == "NavDelegatedUserPasswordValidator")
			{
				return primaryIdentity.IsAuthenticated;
			}
			return false;
		}
		return false;
	}
}
