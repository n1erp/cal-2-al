using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavPerformanceCounterSetter : IDisposable
{
	internal static class NativeMethods
	{
		[DllImport("Kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool QueryPerformanceCounter(ref long ticks);
	}

	private readonly NavPerformanceCounterManager performanceCounterManager;

	private bool performanceCountersEnabled;

	public NavPerformanceCounterSetter(NavPerformanceCounterManager performanceCounterManager)
	{
		this.performanceCounterManager = performanceCounterManager;
	}

	public void Initialize()
	{
		performanceCountersEnabled = performanceCounterManager.Initialize();
	}

	public void UpdatePerformanceCountersWithPrimaryKeyCacheRequestResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.PrimaryKeyCacheHitCounter, NavPerformanceCounterEntry.PrimaryKeyCacheTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.PrimaryKeyCacheTotalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithCommandCacheRequestResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.CommandCacheHitCounter, NavPerformanceCounterEntry.CommandCacheTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.CommandCacheTotalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithPreferredConnectionCacheRequestResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.PreferredConnectionCacheHitCounter, NavPerformanceCounterEntry.PreferredConnectionCacheTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.PreferredConnectionCacheTotalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithPreferredCacheCommandReuseResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.PreferredConnectionCacheCommandReuseHitCounter, NavPerformanceCounterEntry.PreferredConnectionCacheCommandReuseTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.PreferredConnectionCacheCommandReuseTotalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithResultSetCacheRequestResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.ResultSetCacheHitCounter, NavPerformanceCounterEntry.ResultSetCacheTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.ResultSetCacheTotalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithCalculatedFieldsRequestResult(bool tryGetResult)
	{
		UpdatePerformanceCounters(tryGetResult, NavPerformanceCounterEntry.CalculatedFieldsCacheHitCounter, NavPerformanceCounterEntry.CalculatedFieldsCacheTotalRequestsRawBaseCounter, NavPerformanceCounterEntry.CalculatedFieldsCacheTotalRequestsCounter);
	}

	private void UpdatePerformanceCounters(bool tryGetResult, NavPerformanceCounterEntry hitCounter, NavPerformanceCounterEntry totalRequestsRawBaseCounter, NavPerformanceCounterEntry totalRequestsCounter)
	{
		if (tryGetResult)
		{
			performanceCounterManager.IncrementCounter(hitCounter);
		}
		performanceCounterManager.IncrementCounter(totalRequestsRawBaseCounter);
		performanceCounterManager.IncrementCounter(totalRequestsCounter);
	}

	public void UpdatePerformanceCountersWithQueryRepositionResult(bool repositionNeeded)
	{
		if (repositionNeeded)
		{
			performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.QueryRepositioningCounter);
		}
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.QueryRepositioningTotalRequestsRawBaseCounter);
	}

	public void UpdatePerformanceCountersWithOpenConnectionCountChanged(long connectionCount, NavPerformanceCounterEntry entry)
	{
		performanceCounterManager.SetCounter(connectionCount, entry);
	}

	public void UpdatePerformanceCountersWithHeartbeatDuration(Action action)
	{
		UpdatePerformanceCountersWithDuration(action, NavPerformanceCounterEntry.SqlServerHeartbeatTimeCounter);
	}

	public void UpdatePerformanceCountersWithAverageServiceOperationAction(Action action, NavSession session)
	{
		if (!performanceCountersEnabled)
		{
			action();
			return;
		}
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.ServerOperationsPerSecondCounter);
		Stopwatch stopwatch = ((session != null) ? session.ServiceOperationStopwatch : new Stopwatch());
		bool isRunning = stopwatch.IsRunning;
		try
		{
			if (isRunning)
			{
				stopwatch.Stop();
				performanceCounterManager.IncrementCounterBy(NavPerformanceCounterEntry.AverageServerOperationTimeCounter, stopwatch.ElapsedMilliseconds);
				performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.AverageServerOperationTimeBaseCounter);
			}
			UpdatePerformanceCountersWithAverageServiceOperationDuration(stopwatch, action);
		}
		finally
		{
			if (isRunning)
			{
				stopwatch.Restart();
			}
		}
	}

	private void UpdatePerformanceCountersWithDuration(Action action, NavPerformanceCounterEntry performanceCounterEntry)
	{
		if (!performanceCountersEnabled)
		{
			action();
			return;
		}
		Stopwatch stopWatch = new Stopwatch();
		UpdatePerformanceCountersWithDuration(stopWatch, action, performanceCounterEntry);
	}

	private void UpdatePerformanceCountersWithDuration(Stopwatch stopWatch, Action action, NavPerformanceCounterEntry performanceCounterEntry)
	{
		stopWatch.Restart();
		try
		{
			action();
		}
		finally
		{
			stopWatch.Stop();
			performanceCounterManager.SetCounter(stopWatch.ElapsedMilliseconds, performanceCounterEntry);
		}
	}

	private void UpdatePerformanceCountersWithAverageServiceOperationDuration(Stopwatch stopWatch, Action action)
	{
		stopWatch.Restart();
		try
		{
			action();
		}
		finally
		{
			stopWatch.Stop();
			performanceCounterManager.IncrementCounterBy(NavPerformanceCounterEntry.AverageServerOperationTimeCounter, stopWatch.ElapsedMilliseconds);
			performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.AverageServerOperationTimeBaseCounter);
		}
	}

	public void UpdatePerformanceCountersWithTotalNumberOfRowsInTempTables(long hitCount)
	{
		performanceCounterManager.SetCounter(hitCount, NavPerformanceCounterEntry.TemporaryTablesRowsCounter);
	}

	public void UpdateSoftThrottledConnectionPerformanceCounter()
	{
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.SoftThrottledConnectionsCounter);
	}

	public void UpdateHardThrottledConnectionPerformanceCounter()
	{
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.HardThrottledConnectionsCounter);
	}

	public void UpdateTransientErrorsPerformanceCounter()
	{
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.TransientErrorsCounter);
	}

	public void UpdateNumberOfTenantsCounter(int numberOfTenants)
	{
		performanceCounterManager.SetCounter(numberOfTenants, NavPerformanceCounterEntry.NumberOfTenantsCounter);
	}

	public void IncrementNumberOfActiveSessions()
	{
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.NumberOfActiveSessions);
	}

	public void DecrementNumberOfActiveSessions()
	{
		performanceCounterManager.DecrementCounter(NavPerformanceCounterEntry.NumberOfActiveSessions);
	}

	public void UpdateNumberOfTotalPendingTasksCounter(int numberOfTasks)
	{
		performanceCounterManager.SetCounter(numberOfTasks, NavPerformanceCounterEntry.TotalNumberOfPendingTasks);
	}

	public void UpdateNumberOfTotalRunningTasksCounter(int numberOfTasks)
	{
		performanceCounterManager.SetCounter(numberOfTasks, NavPerformanceCounterEntry.TotalNumberOfRunningTasks);
	}

	public void UpdateNumberOfRunningTasksCounter(int numberOfTasks)
	{
		performanceCounterManager.SetCounter(numberOfTasks, NavPerformanceCounterEntry.NumberOfRunningTasks);
	}

	public void UpdateNumberOfAvailableTasksCounter(int numberOfTasks)
	{
		performanceCounterManager.SetCounter(numberOfTasks, NavPerformanceCounterEntry.NumberOfAvailableTasks);
	}

	public void UpdateMaximumNumberOfRunningTasksCounter(int numberOfTasks)
	{
		performanceCounterManager.SetCounter(numberOfTasks, NavPerformanceCounterEntry.MaximumNumberOfTasks);
	}

	public void IncrementTaskErrorsPerformanceCounter()
	{
		performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.NumberOfTaskErrorsPerSecond);
	}

	public void UpdateTaskExecutionTime(long startTick)
	{
		long performanceCounterTicks = GetPerformanceCounterTicks();
		if (performanceCounterTicks - startTick > 0)
		{
			performanceCounterManager.IncrementCounterBy(NavPerformanceCounterEntry.AverageTaskExecutionTime, performanceCounterTicks - startTick);
			performanceCounterManager.IncrementCounter(NavPerformanceCounterEntry.AverageTaskExecutionTimeBase);
		}
	}

	public void SetTimeWithoutTaskScheduleCapacity(long time)
	{
		performanceCounterManager.SetCounter(time, NavPerformanceCounterEntry.TimeWithoutTaskScheduleCapacity);
	}

	public static long GetPerformanceCounterTicks()
	{
		long ticks = 0L;
		NativeMethods.QueryPerformanceCounter(ref ticks);
		return ticks;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && performanceCounterManager != null)
		{
			performanceCounterManager.Dispose();
		}
	}
}
