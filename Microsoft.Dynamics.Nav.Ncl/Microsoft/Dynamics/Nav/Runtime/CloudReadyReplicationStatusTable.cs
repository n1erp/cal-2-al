using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class CloudReadyReplicationStatusTable
{
	private NavTenant navTenant;

	private MemoryCache memoryCache;

	private const string CacheKey = "CloudReadyReplicationStatusTableKey";

	private string tenantSpecificCacheKey;

	private ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>> cloudReadyReplicationStatusTableCache
	{
		get
		{
			ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>> concurrentDictionary = memoryCache.Get(tenantSpecificCacheKey) as ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>>;
			if (concurrentDictionary == null)
			{
				concurrentDictionary = GetTableDataAsDictionary();
				memoryCache.Add(tenantSpecificCacheKey, concurrentDictionary, DateTime.UtcNow.Add(ServerUserSettings.Instance.CloudReadyReplicationTableCacheTimeout.Value));
			}
			return concurrentDictionary;
		}
	}

	public CloudReadyReplicationStatusTable(NavTenant tenant)
	{
		navTenant = tenant;
		memoryCache = MemoryCache.Default;
		tenantSpecificCacheKey = "CloudReadyReplicationStatusTableKey" + navTenant.Id;
	}

	public void FlushCache()
	{
		if (memoryCache.Get(tenantSpecificCacheKey) is ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>>)
		{
			memoryCache.Remove(tenantSpecificCacheKey);
		}
	}

	private ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>> GetTableDataAsDictionary()
	{
		ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>> concurrentDictionary = new ConcurrentDictionary<CloudReadyReplicationStatusKey, Lazy<CloudReadyReplicationStatus>>();
		if (navTenant.Database.IsSharedTenantSchema)
		{
			return concurrentDictionary;
		}
		foreach (CloudReadyReplicationStatus cloudReadyReplicationStatusEntry in GetCloudReadyReplicationStatusCollectionFromDatabase(navTenant.Database))
		{
			CloudReadyReplicationStatusKey key = new CloudReadyReplicationStatusKey(cloudReadyReplicationStatusEntry.TableName, cloudReadyReplicationStatusEntry.CompanyName);
			try
			{
				concurrentDictionary.AddOrUpdate(key, new Lazy<CloudReadyReplicationStatus>(() => cloudReadyReplicationStatusEntry), (CloudReadyReplicationStatusKey tN, Lazy<CloudReadyReplicationStatus> lazyVal) => new Lazy<CloudReadyReplicationStatus>(() => cloudReadyReplicationStatusEntry));
			}
			catch
			{
				concurrentDictionary.TryRemove(key, out var _);
				throw;
			}
		}
		return concurrentDictionary;
	}

	private static IEnumerable<CloudReadyReplicationStatus> GetCloudReadyReplicationStatusCollectionFromDatabase(NavDatabase database)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		return NavSqlCloudReadyReplicationStatus.GetCloudReadyReplicationStatusCollection(conScope);
	}

	public bool IsTableStatusBlocked(string tableName, string companyName)
	{
		CloudReadyReplicationStatusKey key = new CloudReadyReplicationStatusKey(tableName, companyName);
		Lazy<CloudReadyReplicationStatus> value = new Lazy<CloudReadyReplicationStatus>();
		if (cloudReadyReplicationStatusTableCache.TryGetValue(key, out value))
		{
			return value.Value.Blocked;
		}
		return false;
	}
}
