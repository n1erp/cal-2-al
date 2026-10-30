using System;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAdministratorLocalNavAdminValidator : INavAdministratorValidator
{
	private readonly string localNavAdministratorGroupName;

	private readonly object localNavAdministratorGroupLock = new object();

	private SecurityIdentifier localNavAdministratorGroupSid;

	public NavAdministratorLocalNavAdminValidator(string localNavAdministratorGroupName)
	{
		this.localNavAdministratorGroupName = localNavAdministratorGroupName ?? throw new ArgumentNullException("localNavAdministratorGroupName");
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
			SecurityIdentifier sidForLocalNavAdministratorGroup = GetSidForLocalNavAdministratorGroup(localNavAdministratorGroupName);
			if (sidForLocalNavAdministratorGroup == null)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00008BC", Category.Security, Verbosity.Warning, PrivacyClassification.SystemMetadata, "The group " + localNavAdministratorGroupName + " was expected, but not found on the machine.");
				return false;
			}
			return windowsPrincipal.IsInRole(sidForLocalNavAdministratorGroup);
		}
		catch (SystemException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00008BL", Category.Security, exception, Verbosity.Warning);
		}
		return false;
	}

	internal SecurityIdentifier GetSidForLocalNavAdministratorGroup(string groupName)
	{
		if (localNavAdministratorGroupSid == null)
		{
			lock (localNavAdministratorGroupLock)
			{
				if (localNavAdministratorGroupSid != null)
				{
					return localNavAdministratorGroupSid;
				}
				PrincipalContext context = new PrincipalContext(ContextType.Machine);
				try
				{
					localNavAdministratorGroupSid = GroupPrincipal.FindByIdentity(context, groupName)?.Sid;
				}
				catch (MultipleMatchesException exception)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00008BD", Category.Security, exception);
				}
			}
		}
		return localNavAdministratorGroupSid;
	}
}
