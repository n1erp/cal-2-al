using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class TenantLicenseStateProvider
{
	public static void ALExtendTrialLicense()
	{
		TenantLicenseStateHelper.ExtendTrialLicense(NavCurrentThread.Session);
	}

	public static bool ALValidateUserAllowedToExtendTrialLicense()
	{
		try
		{
			TenantLicenseStateHelper.ValidateUserAllowedtoExtendTrialLicense(NavCurrentThread.Session);
		}
		catch (NavPermissionException)
		{
			return false;
		}
		return true;
	}
}
