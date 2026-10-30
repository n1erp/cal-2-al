using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

public enum NavPerformanceCounterEntry
{
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Primary key cache hit rate", PerformanceCounterType.RawFraction)]
	PrimaryKeyCacheHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "PrimaryKeyCacheTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	PrimaryKeyCacheTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Primary key cache total requests", PerformanceCounterType.NumberOfItems64)]
	PrimaryKeyCacheTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Command cache hit rate", PerformanceCounterType.RawFraction)]
	CommandCacheHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "CommandCacheTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	CommandCacheTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Command cache total requests", PerformanceCounterType.NumberOfItems64)]
	CommandCacheTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Preferred connection cache hit rate", PerformanceCounterType.RawFraction)]
	PreferredConnectionCacheHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "PreferredConnectionCacheTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	PreferredConnectionCacheTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Preferred connection total requests", PerformanceCounterType.NumberOfItems64)]
	PreferredConnectionCacheTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Preferred connection cache command reuse hit rate", PerformanceCounterType.RawFraction)]
	PreferredConnectionCacheCommandReuseHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "PreferredConnectionCacheCommandReuseTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	PreferredConnectionCacheCommandReuseTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Preferred connection command reuse total requests", PerformanceCounterType.NumberOfItems64)]
	PreferredConnectionCacheCommandReuseTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Open connections", PerformanceCounterType.NumberOfItems64)]
	OpenConnectionCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Open tenant connections", PerformanceCounterType.NumberOfItems64)]
	TenantOpenConnectionCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Open application connections", PerformanceCounterType.NumberOfItems64)]
	AppOpenConnectionCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Query repositioning rate", PerformanceCounterType.RawFraction)]
	QueryRepositioningCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "QueryRepositioningTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	QueryRepositioningTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Result set cache hit rate", PerformanceCounterType.RawFraction)]
	ResultSetCacheHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "ResultSetCacheTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	ResultSetCacheTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Result set cache total requests", PerformanceCounterType.NumberOfItems64)]
	ResultSetCacheTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "% Calculated fields cache hit rate", PerformanceCounterType.RawFraction)]
	CalculatedFieldsCacheHitCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "CalculatedFieldsTotalRequestsRawBaseCounter", PerformanceCounterType.RawBase)]
	CalculatedFieldsCacheTotalRequestsRawBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Calculated fields cache total requests", PerformanceCounterType.NumberOfItems64)]
	CalculatedFieldsCacheTotalRequestsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Heartbeat time (ms)", PerformanceCounterType.NumberOfItems64)]
	SqlServerHeartbeatTimeCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Rows in all temporary tables", PerformanceCounterType.NumberOfItems64)]
	TemporaryTablesRowsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Soft throttled connections", PerformanceCounterType.RateOfCountsPerSecond32)]
	SoftThrottledConnectionsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Hard throttled connections", PerformanceCounterType.RateOfCountsPerSecond32)]
	HardThrottledConnectionsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Transient errors", PerformanceCounterType.RateOfCountsPerSecond32)]
	TransientErrorsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Mounted tenants", PerformanceCounterType.NumberOfItems64)]
	NumberOfTenantsCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Server operations/sec", PerformanceCounterType.RateOfCountsPerSecond64)]
	ServerOperationsPerSecondCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Active sessions", PerformanceCounterType.NumberOfItems64)]
	NumberOfActiveSessions,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Average server operation time (ms)", PerformanceCounterType.AverageTimer32)]
	AverageServerOperationTimeCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "AverageServerOperationTimeBaseCounter", PerformanceCounterType.AverageBase)]
	AverageServerOperationTimeBaseCounter,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Total # Pending tasks", PerformanceCounterType.NumberOfItems32)]
	TotalNumberOfPendingTasks,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Total # Running tasks", PerformanceCounterType.NumberOfItems32)]
	TotalNumberOfRunningTasks,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Running tasks", PerformanceCounterType.NumberOfItems32)]
	NumberOfRunningTasks,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# Available tasks", PerformanceCounterType.NumberOfItems32)]
	NumberOfAvailableTasks,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Maximum # of tasks", PerformanceCounterType.NumberOfItems32)]
	MaximumNumberOfTasks,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "# of task errors/sec", PerformanceCounterType.RateOfCountsPerSecond32)]
	NumberOfTaskErrorsPerSecond,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Average task execution time", PerformanceCounterType.AverageTimer32)]
	AverageTaskExecutionTime,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "AverageTaskExecutionTimeBase", PerformanceCounterType.AverageBase)]
	AverageTaskExecutionTimeBase,
	[NavPerformanceCounterMapping("Microsoft Dynamics NAV", "Time (ms) since the list of running tasks last had capacity for new tasks", PerformanceCounterType.ElapsedTime)]
	TimeWithoutTaskScheduleCapacity
}
