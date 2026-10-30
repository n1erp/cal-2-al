using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppGroupObjectDependencyResolver
{
	public static OrderedAppCombination SelectAppsPlusDependencies(OrderedAppCombination allGroupApps, OrderedAppCombination toResolveFor)
	{
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>(toResolveFor.OrderedAppMetadata.Count);
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in toResolveFor.OrderedAppMetadata)
		{
			ResolveAndAddDependencies(orderedAppMetadatum, allGroupApps.OrderedAppMetadata, list);
		}
		IEnumerable<NavAppRuntimeMetadata> orderedAppMetadata = NavAppOrderResolver.DetermineProperOrderForApps(list);
		return new OrderedAppCombination(orderedAppMetadata);
	}

	private static void ResolveAndAddDependencies(NavAppRuntimeMetadata appToResolveFor, IReadOnlyList<NavAppRuntimeMetadata> allKnownOrderedApps, List<NavAppRuntimeMetadata> results)
	{
		if (results.Contains(appToResolveFor))
		{
			return;
		}
		foreach (NavAppRuntimeDependencyDefinition dependencyDefinition in appToResolveFor.Dependencies)
		{
			NavAppRuntimeMetadata val = allKnownOrderedApps.FirstOrDefault((NavAppRuntimeMetadata a) => ((object)a.AppId).Equals((object)dependencyDefinition.AppId));
			if (val != null)
			{
				ResolveAndAddDependencies(val, allKnownOrderedApps, results);
			}
		}
		results.Add(appToResolveFor);
	}
}
