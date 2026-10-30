using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncAppGroupResolver
{
	private readonly IEnumerable<NavAppRuntimeMetadata> allPublishedApps;

	private readonly IEnumerable<AppSyncTrackingRecord> allTrackingRecords;

	private readonly INavAppGroupResolver appGroupResolver;

	public SyncAppGroupResolver(IEnumerable<NavAppRuntimeMetadata> allPublishedApps, IEnumerable<AppSyncTrackingRecord> allTrackingRecords, INavAppGroupResolver appGroupResolver)
	{
		this.allPublishedApps = allPublishedApps;
		this.allTrackingRecords = allTrackingRecords;
		this.appGroupResolver = appGroupResolver;
	}

	public NavAppGroup ResolveGroup(NavAppRuntimeMetadata targetApp, NavAppSyncMode syncMode)
	{
		if (syncMode == NavAppSyncMode.Clean)
		{
			return NavAppGroup.BaseGroup;
		}
		IEnumerable<NavAppRuntimeMetadata> source = SyncDependencySelector.SelectBestDependenciesInOrder(allPublishedApps, allTrackingRecords, targetApp);
		List<PackageId> list = source.Select((NavAppRuntimeMetadata d) => d.PackageId).ToList();
		list.Add(targetApp.PackageId);
		return appGroupResolver.GetOrCreateGroupForAppCombination(list);
	}
}
