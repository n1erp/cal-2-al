using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppPackagePoolSelector
{
	protected delegate IEnumerable<NavAppRuntimeMetadata> NavAppReferenceGenerator();

	protected delegate IEnumerable<INavAppReference> TenantNavAppReferenceGenerator(NavSqlConnectionScope tenantDatabaseScope);

	private readonly NavAppReferenceGenerator publishedApps;

	private readonly TenantNavAppReferenceGenerator syncedApps;

	protected NavAppPackagePoolSelector(NavAppReferenceGenerator publishedGenerator, TenantNavAppReferenceGenerator syncedGenerator)
	{
		publishedApps = publishedGenerator;
		syncedApps = syncedGenerator;
	}

	public static NavAppPackagePoolSelector CreateForUpgradeContextFactory(INavAppMetadataRetriever metadataRetriever, Func<NavSqlConnectionScope, IAppSyncTrackingRepository> syncRepositoryCreator)
	{
		return new NavAppPackagePoolSelector(metadataRetriever.GetAllMetadata, (NavSqlConnectionScope scope) => (IEnumerable<INavAppReference>)syncRepositoryCreator(scope).GetAll());
	}

	public IEnumerable<NavAppRuntimeMetadata> GetAllVersionsOfPackages(NavSqlConnectionScope tenantDatabaseScope, bool requireSynced = true)
	{
		return GetPackages(tenantDatabaseScope, requireSynced, (IGrouping<AppId, NavAppRuntimeMetadata> g) => g, null).SelectMany((KeyValuePair<AppId, IGrouping<AppId, NavAppRuntimeMetadata>> p) => p.Value);
	}

	public IDictionary<AppId, NavAppRuntimeMetadata> GetLatestVersionOfPackages(NavSqlConnectionScope tenantDatabaseScope, bool requireSynced = true, IEnumerable<INavAppReference> preferred = null)
	{
		return GetPackages(tenantDatabaseScope, requireSynced, (IGrouping<AppId, NavAppRuntimeMetadata> g) => g.OrderByDescending((NavAppRuntimeMetadata m) => m.Version).FirstOrDefault(), preferred);
	}

	private IDictionary<AppId, T> GetPackages<T>(NavSqlConnectionScope tenantDatabaseScope, bool requireSynced, Func<IGrouping<AppId, NavAppRuntimeMetadata>, T> selector, IEnumerable<INavAppReference> preferred)
	{
		IEnumerable<INavAppReference> enumerable = preferred?.ToList();
		preferred = enumerable ?? Enumerable.Empty<INavAppReference>();
		List<NavAppRuntimeMetadata> source = DetermineExtensionPool(tenantDatabaseScope, requireSynced, preferred);
		return (from m in source
			group m by m.AppId).ToDictionary((IGrouping<AppId, NavAppRuntimeMetadata> g) => g.Key, selector);
	}

	private List<NavAppRuntimeMetadata> DetermineExtensionPool(NavSqlConnectionScope tenantDatabaseScope, bool requireSynced, IEnumerable<INavAppReference> preferred)
	{
		IEnumerable<NavAppRuntimeMetadata> pool = publishedApps();
		if (requireSynced && tenantDatabaseScope != null)
		{
			pool = ConstrictSyncedAndPreferred(pool, syncedApps?.Invoke(tenantDatabaseScope), preferred);
		}
		pool = WeedOutByPreferred(pool, preferred);
		return pool.ToList();
	}

	private static IEnumerable<NavAppRuntimeMetadata> ConstrictSyncedAndPreferred(IEnumerable<NavAppRuntimeMetadata> pool, IEnumerable<INavAppReference> synced, IEnumerable<INavAppReference> preferred)
	{
		synced = synced?.ToList() ?? new List<INavAppReference>();
		foreach (NavAppRuntimeMetadata app in pool)
		{
			if (synced.Any((INavAppReference s) => (WrappedComparable<Guid>)(object)s.AppId == (WrappedComparable<Guid>)(object)app.AppId && s.Version >= app.Version) || preferred.Any((INavAppReference p) => (WrappedComparable<Guid>)(object)p.AppId == (WrappedComparable<Guid>)(object)app.AppId && p.Version == app.Version))
			{
				yield return app;
			}
		}
	}

	private static List<NavAppRuntimeMetadata> WeedOutByPreferred(IEnumerable<NavAppRuntimeMetadata> pool, IEnumerable<INavAppReference> preferred)
	{
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>();
		Dictionary<AppId, IGrouping<AppId, INavAppReference>> lookup = preferred?.GroupBy((INavAppReference p) => p.AppId).ToDictionary((IGrouping<AppId, INavAppReference> g) => g.Key) ?? new Dictionary<AppId, IGrouping<AppId, INavAppReference>>();
		foreach (IGrouping<AppId, NavAppRuntimeMetadata> appFamily in from p in pool
			group p by p.AppId)
		{
			if (lookup.ContainsKey(appFamily.Key))
			{
				List<NavAppRuntimeMetadata> list2 = appFamily.Where((NavAppRuntimeMetadata a) => lookup[appFamily.Key].Any((INavAppReference m) => m.Version == a.Version)).ToList();
				if (list2.Any())
				{
					foreach (NavAppRuntimeMetadata item in list2)
					{
						list.Add(item);
					}
					continue;
				}
			}
			foreach (NavAppRuntimeMetadata item2 in appFamily)
			{
				list.Add(item2);
			}
		}
		return list;
	}
}
