using System;
using System.Security.Principal;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAdministratorAdminValidator : INavAdministratorValidator
{
	private readonly WindowsBuiltInRole administratorRole;

	public NavAdministratorAdminValidator(WindowsBuiltInRole administratorRole)
	{
		this.administratorRole = administratorRole;
	}

	public bool Validate(WindowsIdentity windowsIdentity, IIdentity primaryIdentity)
	{
		WindowsIdentity windowsIdentity2 = windowsIdentity ?? NavAdministration.GetCurrentWindowsIdentity();
		if (windowsIdentity2 == null || !windowsIdentity2.IsAuthenticated)
		{
			return false;
		}
		try
		{
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity2);
			return windowsPrincipal.IsInRole(administratorRole);
		}
		catch (SystemException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00008AN", Category.Security, exception, Verbosity.Warning);
			return false;
		}
	}
}
