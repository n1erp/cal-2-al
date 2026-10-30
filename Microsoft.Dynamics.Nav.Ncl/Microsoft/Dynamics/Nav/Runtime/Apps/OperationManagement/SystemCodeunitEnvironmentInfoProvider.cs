using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class SystemCodeunitEnvironmentInfoProvider : IEnvironmentInfoProvider
{
	public EnvironmentInfo GetInfoForTenant(string tenantId)
	{
		string value = NavCurrentThread.Session.SystemCodeunitFactory.VersionTriggers.InvokeReleaseVersion().Value;
		if (Version.TryParse(value, out var result))
		{
			return new EnvironmentInfo
			{
				ReleaseVersion = result
			};
		}
		throw new NavAppExternalDeployException(Lang.NavAppExternalDeployReleaseVersionParseError.FormatWithResolvedCulture(value));
	}
}
