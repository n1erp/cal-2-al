using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppOrderResolver
{
	public static IEnumerable<NavAppRuntimeMetadata> DetermineProperOrderForApps(IEnumerable<NavAppRuntimeMetadata> apps)
	{
		List<NavAppRuntimeMetadata> list = (from a in apps
			where a.Scope == NavAppScope.Global
			orderby a.AppId, a.Version
			select a).Concat(from a in apps
			where a.Scope == NavAppScope.Tenant
			orderby a.AppId, a.Version
			select a).ToList();
		List<NavAppRuntimeMetadata> list2 = new List<NavAppRuntimeMetadata>(list.Count);
		foreach (NavAppRuntimeMetadata item in list)
		{
			ProcessApp(item, list, list2);
		}
		return list2;
	}

	private static void ProcessApp(NavAppRuntimeMetadata currentApp, List<NavAppRuntimeMetadata> orderedByAppId, List<NavAppRuntimeMetadata> results)
	{
		if (results.Contains(currentApp))
		{
			return;
		}
		foreach (NavAppRuntimeDependencyDefinition dependencyDefinition in currentApp.Dependencies)
		{
			NavAppRuntimeMetadata val = orderedByAppId.FirstOrDefault((NavAppRuntimeMetadata a) => ((object)a.AppId).Equals((object)dependencyDefinition.AppId));
			if (val != null)
			{
				ProcessApp(val, orderedByAppId, results);
			}
		}
		results.Add(currentApp);
	}
}
