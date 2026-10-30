using System.Collections.Generic;
using System.Security.Principal;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavAdministration
{
	private static readonly object ValidatorLock = new object();

	private static List<INavAdministratorValidator> Validators;

	public static bool IsNavAdministrator(WindowsIdentity windowsIdentity = null)
	{
		if (Validators == null)
		{
			lock (ValidatorLock)
			{
				if (Validators == null)
				{
					Validators = new List<INavAdministratorValidator>();
					Validators.Add(new NavAdministratorAdminValidator(WindowsBuiltInRole.Administrator));
					if (NavEnvironment.Topology.EnableAdministrationAuthenticationDelegation)
					{
						Validators.Add(new NavAdministratorDelegatedAdminValidator());
					}
					if (!string.IsNullOrEmpty(NavEnvironment.Topology.LocalNavAdministratorGroupName))
					{
						Validators.Add(new NavAdministratorLocalNavAdminValidator(NavEnvironment.Topology.LocalNavAdministratorGroupName));
					}
				}
			}
		}
		foreach (INavAdministratorValidator validator in Validators)
		{
			if (validator.Validate(windowsIdentity, ServiceSecurityContext.Current?.PrimaryIdentity))
			{
				return true;
			}
		}
		return false;
	}

	internal static WindowsIdentity GetCurrentWindowsIdentity()
	{
		if (ServiceSecurityContext.Current != null)
		{
			return ServiceSecurityContext.Current.WindowsIdentity;
		}
		return WindowsIdentity.GetCurrent();
	}
}
