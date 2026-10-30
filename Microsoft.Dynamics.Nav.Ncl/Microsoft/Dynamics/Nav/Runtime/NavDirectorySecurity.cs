using System;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavDirectorySecurity
{
	private static readonly SecurityIdentifier builtinAdmins = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

	internal static DirectorySecurity CreateSecurityForDomainDirectory()
	{
		if (NavEnvironment.Topology.IsServiceRunningInLocalEnvironment || FeatureSwitches.Instance.EnableDirectoryACLinAzure)
		{
			DirectorySecurity directorySecurity = new DirectorySecurity();
			directorySecurity.SetAccessRule(new FileSystemAccessRule(builtinAdmins, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
			directorySecurity.SetAccessRule(new FileSystemAccessRule(NavEnvironment.Topology.DomainDirectoryAccessIdentity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
			directorySecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
			return directorySecurity;
		}
		return null;
	}

	internal static DirectoryInfo CreateDirectoryWithFolderSecurity(string path, DirectorySecurity directorySecurity, NavSession session)
	{
		try
		{
			return Directory.CreateDirectory(path, directorySecurity);
		}
		catch (Exception exception)
		{
			NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance(session);
			mostSpecificInstance.SendExceptionTag("000004B", Category.Operations, exception);
			StringBuilder stringBuilder = new StringBuilder();
			if (directorySecurity != null)
			{
				foreach (AuthorizationRule accessRule in directorySecurity.GetAccessRules(includeExplicit: true, includeInherited: false, typeof(SecurityIdentifier)))
				{
					if (accessRule == null)
					{
						stringBuilder.Append("Invalid rule");
						continue;
					}
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Identity: {0}", accessRule.IdentityReference.Value);
					if (!(accessRule is FileSystemAccessRule fileSystemAccessRule))
					{
						stringBuilder.Append("Invalid File System Access Rule");
						continue;
					}
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "FileSystemRights: {0}", fileSystemAccessRule.FileSystemRights);
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "AccessControlType: {0}", fileSystemAccessRule.AccessControlType);
				}
			}
			mostSpecificInstance.SendTraceTag("000004C", Category.Operations, Verbosity.Error, PrivacyClassification.EndUserIdentifiableInformation, stringBuilder.ToString());
			throw;
		}
	}
}
