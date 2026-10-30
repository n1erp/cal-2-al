using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CacheSynchronizationController : ICacheSynchronizationController, IDisposable
{
	private readonly CacheSynchronizationStorage cacheSynchronizationStorage;

	private readonly int period;

	private readonly Queue<long> readTimestamps = new Queue<long>();

	private readonly object timerLockObject = new object();

	private bool isDisposed;

	private bool cacheIsStale;

	private long lastReadTimestamp;

	private NonReEntrantTimerWrapper synchronizationTimer;

	public bool EmitTableDataChangesEvents => false;

	internal CacheSynchronizationController(int serverInstanceId = -1)
	{
		period = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.CacheSynchronizationPeriod.Value.TotalMilliseconds);
		cacheSynchronizationStorage = new CacheSynchronizationStorage(() => (serverInstanceId == -1) ? NavEnvironment.Instance.GetServiceInstanceId() : serverInstanceId);
	}

	public void StartCacheSynchronization()
	{
		if (synchronizationTimer != null)
		{
			return;
		}
		lock (timerLockObject)
		{
			if (synchronizationTimer == null)
			{
				if (NavEnvironment.Topology.EnableServiceBusCacheSync)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000087H", Category.CacheSynchronization, Verbosity.Warning, PrivacyClassification.SystemMetadata, "CacheSynchronization is configured to use Azure Service Bus.");
				}
				Initialize();
				synchronizationTimer = new NonReEntrantTimerWrapper("CacheSynchronizationController", TimerCallback, null, period);
			}
		}
	}

	public void StopCacheSynchronization()
	{
		WriteAllModifiedTables();
	}

	internal void Initialize()
	{
		lastReadTimestamp = CacheSynchronizationStorage.LastWriteToCacheSyncTimeStamp;
		NavEnvironment.Instance.AliveServerInstancesChanged += OnAliveServerInstancesChanged;
	}

	internal void Synchronize()
	{
		try
		{
			if (isDisposed)
			{
				return;
			}
			long lastWriteToCacheSyncTimeStamp = CacheSynchronizationStorage.LastWriteToCacheSyncTimeStamp;
			WriteAllModifiedTables();
			if (cacheIsStale)
			{
				ClearCacheForAllTenants();
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001U9", Category.CacheSynchronization, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Cache cleared as it was stale");
				lastReadTimestamp = lastWriteToCacheSyncTimeStamp;
				cacheIsStale = false;
			}
			if (lastWriteToCacheSyncTimeStamp != lastReadTimestamp)
			{
				lastReadTimestamp = ReadAndHandleModifiedTables(lastReadTimestamp);
				readTimestamps.Enqueue(lastReadTimestamp);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001U8", Category.CacheSynchronization, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Timestamp updated {0}", lastReadTimestamp);
				if (readTimestamps.Count > 2)
				{
					cacheSynchronizationStorage.DeleteOlderThan(readTimestamps.Dequeue());
				}
			}
		}
		catch (NavCSideException ex)
		{
			cacheIsStale = true;
			bool flag = ex.ErrorCode == 22924076 || ex.ErrorCode == 22926090;
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001UE", Category.CacheSynchronization, ex, flag ? Verbosity.Normal : Verbosity.Error, DiagnosticParameter.SystemMetadata("ErrorCode " + ex.ErrorCode));
			if (!flag)
			{
				LogWriterHelper.LogExceptionEvent(ex, EventLogEntryType.Error);
			}
		}
		catch (NavSqlException ex2)
		{
			cacheIsStale = true;
			bool flag2 = NavSqlReturnCode.IsTransientError(ex2.ErrorNumber);
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001U6", Category.CacheSynchronization, ex2, flag2 ? Verbosity.Normal : Verbosity.Error, DiagnosticParameter.SystemMetadata("ErrorNumber " + ex2.ErrorNumber));
			if (!flag2)
			{
				LogWriterHelper.LogExceptionEvent(ex2, EventLogEntryType.Error);
			}
		}
		catch (Exception exception)
		{
			cacheIsStale = true;
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001U7", Category.CacheSynchronization, exception);
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
		}
	}

	internal void WriteAllModifiedTables()
	{
		IList<NavTenant> allTenantsWithModifiedTables = GetAllTenantsWithModifiedTables();
		if (allTenantsWithModifiedTables.Count > 0)
		{
			cacheSynchronizationStorage.Write(allTenantsWithModifiedTables);
		}
	}

	public void OnTenantDataChanged(NavTenant tenant)
	{
		NavDatabase database = tenant.Database;
		if (database != null && database.CacheSynchronization.HasChanges)
		{
			cacheSynchronizationStorage.Write(new NavTenant[1] { tenant });
		}
	}

	private void TimerCallback(object state)
	{
		if (Monitor.TryEnter(timerLockObject))
		{
			try
			{
				Synchronize();
				return;
			}
			finally
			{
				Monitor.Exit(timerLockObject);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UH", Category.CacheSynchronization, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Cache synchronization took longer than the specified CacheSynchronizationPeriod.");
		LogWriterHelper.LogEvent(EventLogEntryType.Warning, "Cache synchronization took longer than the specified CacheSynchronizationPeriod.");
	}

	private void OnAliveServerInstancesChanged(object sender, AliveServerInstancesChangedEventArgs aliveServerInstancesChangedEventArgs)
	{
		if (aliveServerInstancesChangedEventArgs.AnotherServerHasCrashed)
		{
			ClearCacheForAllTenants();
		}
	}

	private static void ClearCacheForAllTenants()
	{
		foreach (NavTenant item in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true))
		{
			item.Database.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted();
		}
	}

	private long ReadAndHandleModifiedTables(long lastRead)
	{
		long num = 0L;
		bool flag = false;
		foreach (CacheSynchronizationChangeSet item in cacheSynchronizationStorage.Read(lastRead))
		{
			if (item.Timestamp == lastRead)
			{
				flag = true;
				continue;
			}
			num = Math.Max(num, item.Timestamp);
			foreach (TenantTableSet tenantTableSet in item.TenantTableSets)
			{
				try
				{
					HashSet<int> hashSet = new HashSet<int>();
					foreach (TableSet companyTableSet in tenantTableSet.CompanyTableSets)
					{
						int companyNameToken = tenantTableSet.Tenant.Database.GetCompanyNameToken(companyTableSet.CompanyName);
						foreach (int tableId in companyTableSet.TableIds)
						{
							hashSet.Add(tableId);
							if (tableId == -1)
							{
								tenantTableSet.Tenant.Database.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted();
							}
							else
							{
								tenantTableSet.Tenant.Database.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted(companyNameToken, tableId);
							}
						}
					}
					tenantTableSet.Tenant.Diagnostics.SendTraceTag("00001UA", Category.CacheSynchronization, Verbosity.Verbose, "Read Modified Tables Synchronization {0}", DiagnosticParameter.SystemMetadata(string.Join(",", hashSet)));
					tenantTableSet.Tenant.Database.CacheSynchronization.OnTableDataChanged(new TableDataChangedEventArgs(tenantTableSet.CompanyTableSets));
				}
				catch (ObjectDisposedException ex) when (ex.ObjectName.Equals("NavTenant", StringComparison.Ordinal) || ex.ObjectName.Equals("NavDatabase", StringComparison.Ordinal))
				{
				}
			}
		}
		if (!flag)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001U4", Category.CacheSynchronization, Verbosity.Warning, "Last read record is missing, we need to clear the cache newestReadTimestamp {0} lastreadTimestamp {1}", DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(lastRead));
			ClearCacheForAllTenants();
		}
		return num;
	}

	private static IList<NavTenant> GetAllTenantsWithModifiedTables()
	{
		return (from tenant in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true)
			where tenant.Database.CacheSynchronization.HasChanges
			select tenant).ToList();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		isDisposed = true;
		if (disposing)
		{
			StopCacheSynchronization();
			synchronizationTimer?.Dispose();
		}
	}
}
