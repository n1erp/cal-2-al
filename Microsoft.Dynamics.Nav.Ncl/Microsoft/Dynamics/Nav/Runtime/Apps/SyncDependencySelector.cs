using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncDependencySelector
{
	public static IEnumerable<NavAppRuntimeMetadata> SelectBestDependenciesInOrder(IEnumerable<NavAppRuntimeMetadata> allPublished, IEnumerable<AppSyncTrackingRecord> allSynced, NavAppRuntimeMetadata targetApp)
	{
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>();
		ResolveBestDependencies(allPublished.ToList(), allSynced.ToList(), targetApp, list);
		return NavAppOrderResolver.DetermineProperOrderForApps(list);
	}

	private static void ResolveBestDependencies(List<NavAppRuntimeMetadata> allPublished, List<AppSyncTrackingRecord> allSynced, NavAppRuntimeMetadata targetApp, List<NavAppRuntimeMetadata> results)
	{
		foreach (NavAppRuntimeDependencyDefinition dep in targetApp.Dependencies)
		{
			NavAppRuntimeMetadata val = results.FirstOrDefault((NavAppRuntimeMetadata r) => (WrappedComparable<Guid>)(object)r.AppId == (WrappedComparable<Guid>)(object)dep.AppId);
			if (val != null)
			{
				if (val.Version < dep.Version)
				{
					ThrowForUnsatisfiedDependency(dep);
				}
				continue;
			}
			NavAppRuntimeMetadata val2 = (from p in allPublished
				where (WrappedComparable<Guid>)(object)p.AppId == (WrappedComparable<Guid>)(object)dep.AppId && p.Version >= dep.Version && allSynced.Any((AppSyncTrackingRecord s) => (WrappedComparable<Guid>)(object)s.AppId == (WrappedComparable<Guid>)(object)p.AppId && s.Version == p.Version)
				orderby p.Version descending
				select p).FirstOrDefault();
			if (val2 == null)
			{
				ThrowForUnsatisfiedDependency(dep);
			}
			results.Add(val2);
			ResolveBestDependencies(allPublished, allSynced, val2, results);
		}
	}

	private static void ThrowForUnsatisfiedDependency(NavAppRuntimeDependencyDefinition dep)
	{
		string message = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_DependencyNotResolved, NavAppReferenceExtensions.TechnicalDisplayName((INavAppReference)(object)dep));
		throw new NavAppSyncException(message);
	}
}
