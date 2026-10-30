namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavPerformanceCounterNames
{
	public const string CategoryName = "Microsoft Dynamics NAV";

	public const string PrimaryKeyCacheHitCounterName = "% Primary key cache hit rate";

	public const string PrimaryKeyCacheTotalRequestsRawBaseCounterName = "PrimaryKeyCacheTotalRequestsRawBaseCounter";

	public const string PrimaryKeyCacheTotalRequestsCounterName = "# Primary key cache total requests";

	public const string CommandCacheHitCounterName = "% Command cache hit rate";

	public const string CommandCacheTotalRequestsRawBaseCounterName = "CommandCacheTotalRequestsRawBaseCounter";

	public const string CommandCacheHitTotalRequestsCounterName = "# Command cache total requests";

	public const string PreferredConnectionCacheHitCounterName = "% Preferred connection cache hit rate";

	public const string PreferredConnectionCacheTotalRequestsRawBaseCounterName = "PreferredConnectionCacheTotalRequestsRawBaseCounter";

	public const string PreferredConnectionCacheTotalRequestsCounterName = "# Preferred connection total requests";

	public const string PreferredConnectionCacheCommandReuseHitCounterName = "% Preferred connection cache command reuse hit rate";

	public const string PreferredConnectionCacheCommandReuseTotalRequestsRawBaseCounterName = "PreferredConnectionCacheCommandReuseTotalRequestsRawBaseCounter";

	public const string PreferredConnectionCacheCommandReuseTotalRequestsCounterName = "# Preferred connection command reuse total requests";

	public const string OpenConnectionCounterName = "# Open connections";

	public const string TenantConnectionCounterName = "# Open tenant connections";

	public const string AppConnectionCounterName = "# Open application connections";

	public const string QueryRepositioningCounterName = "% Query repositioning rate";

	public const string QueryRepositioningTotalRequestsRawBaseCounterName = "QueryRepositioningTotalRequestsRawBaseCounter";

	public const string ResultSetCacheHitCounterName = "% Result set cache hit rate";

	public const string ResultSetCacheTotalRequestsRawBaseCounterName = "ResultSetCacheTotalRequestsRawBaseCounter";

	public const string ResultSetCacheTotalRequestsCounterName = "# Result set cache total requests";

	public const string CalculatedFieldsCacheHitCounterName = "% Calculated fields cache hit rate";

	public const string CalculatedFieldsTotalRequestsRawBaseCounterName = "CalculatedFieldsTotalRequestsRawBaseCounter";

	public const string CalculatedFieldsTotalRequestsCounterName = "# Calculated fields cache total requests";

	public const string SqlServerHeartbeatTimeName = "Heartbeat time (ms)";

	public const string TemporaryTablesRowsCounterName = "# Rows in all temporary tables";

	public const string SoftThrottledConnectionsCounterName = "Soft throttled connections";

	public const string HardThrottledConnectionsCounterName = "Hard throttled connections";

	public const string TransientErrorsCounterName = "Transient errors";

	public const string NumberOfTenantsCounterName = "# Mounted tenants";

	public const string AverageServerOperationTimeCounterName = "Average server operation time (ms)";

	public const string AverageServerOperationTimeBaseCounterName = "AverageServerOperationTimeBaseCounter";

	public const string ServerOperationsPerSecondCounterName = "Server operations/sec";

	public const string NumberOfActiveSessionsName = "# Active sessions";

	public const string TotalNumberOfPendingTasksName = "Total # Pending tasks";

	public const string TotalNumberOfRunningTasksName = "Total # Running tasks";

	public const string NumberOfRunningTasksName = "# Running tasks";

	public const string NumberOfAvailableTasksName = "# Available tasks";

	public const string MaximumNumberOfTasks = "Maximum # of tasks";

	public const string NumberOfTaskErrorsPerSecondName = "# of task errors/sec";

	public const string AverageTaskExecutionTimeName = "Average task execution time";

	public const string AverageTaskExecutionTimeBaseName = "AverageTaskExecutionTimeBase";

	public const string TimeWithoutTaskScheduleCapacityName = "Time (ms) since the list of running tasks last had capacity for new tasks";
}
