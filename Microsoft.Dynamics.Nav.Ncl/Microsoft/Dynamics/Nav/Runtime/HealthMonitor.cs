using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class HealthMonitor : IDisposable
{
	private static readonly object LockObject = new object();

	public static readonly int PeriodMillis = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.HealthMonitorPeriod.Value.TotalMilliseconds);

	public static readonly int MaxStaleTenantConfigPeriodMillis = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.MaxStaleTenantConfigPeriod.Value.TotalMilliseconds);

	public static readonly int HealthMonitorDelayBeforeServerShutdownMillis = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.HealthMonitorDelayBeforeServerShutdown.Value.TotalMilliseconds);

	private readonly int periodMillis;

	private readonly int maxStaleTenantConfigPeriodMillis;

	private readonly int healthMonitorDelayBeforeServerShutdownMillis;

	private readonly string serverComputerName;

	private readonly string serviceInstanceName;

	private readonly string serverInstanceName;

	private readonly Func<bool> healthMonitorAction;

	private readonly Action shutdownAction;

	private readonly PeriodicScheduler scheduler;

	private readonly object shutdownTaskLockObject = new object();

	private Task shutdownTask;

	private long currentServerStaleTenanConfiguration;

	private long currentServerStaleTenanConfigurationTime;

	public static HealthMonitor Instance { get; private set; }

	public bool IsSystemHealthy { get; private set; } = true;

	public HealthMonitor(int periodMillis, int maxStaleTenantConfigPeriodMillis, int healthMonitorDelayBeforeServerShutdownMillis, string serverComputerName, string serviceInstanceName, string serverInstanceName, Func<bool> healthMonitorAction = null, Action shutdownAction = null, PeriodicScheduler scheduler = null)
	{
		this.periodMillis = periodMillis;
		this.maxStaleTenantConfigPeriodMillis = maxStaleTenantConfigPeriodMillis;
		this.healthMonitorDelayBeforeServerShutdownMillis = healthMonitorDelayBeforeServerShutdownMillis;
		this.serverComputerName = serverComputerName;
		this.serviceInstanceName = serviceInstanceName;
		this.serverInstanceName = serverInstanceName;
		this.shutdownAction = shutdownAction ?? new Action(ShutdownService);
		IsSystemHealthy = true;
		if (FeatureSwitches.Instance.ReportUnhealthyStateForTenantConfigMismatch)
		{
			this.healthMonitorAction = healthMonitorAction ?? new Func<bool>(DoHealthDiagnostics);
			this.scheduler = scheduler ?? new ThreadPeriodicScheduler("HealthMonitor", this.periodMillis, ThreadPriority.Highest);
			this.scheduler.ScheduledAction = this.healthMonitorAction;
			this.scheduler.Start();
		}
	}

	public static void Initialize()
	{
		if (Instance != null)
		{
			return;
		}
		lock (LockObject)
		{
			if (Instance == null)
			{
				Instance = new HealthMonitor(PeriodMillis, MaxStaleTenantConfigPeriodMillis, HealthMonitorDelayBeforeServerShutdownMillis, DnsHelper.MachineName, NavEnvironment.ServiceInstanceName, NavEnvironment.ServerInstanceName);
				Instance.scheduler?.EnsureFirstSchedule();
			}
		}
	}

	private bool DoHealthDiagnostics()
	{
		try
		{
			if (!IsSystemHealthy)
			{
				ScheduleServerShutdown();
			}
			List<ServerInstanceRuntimeInfo> allServerInstances = Heartbeat.GetAllServerInstances();
			if (allServerInstances.Count > 0)
			{
				long num = allServerInstances.Max((ServerInstanceRuntimeInfo info) => info.LastTenantConfig);
				int num2 = allServerInstances.FindIndex((ServerInstanceRuntimeInfo info) => info.Id == Heartbeat.Instance.ServiceInstanceId);
				if (num2 < 0)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009H9", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "HealthMonitor cannot find current service instance. Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName);
					return true;
				}
				long lastTenantConfig = allServerInstances[num2].LastTenantConfig;
				if (lastTenantConfig != num)
				{
					if (currentServerStaleTenanConfiguration != lastTenantConfig)
					{
						IsSystemHealthy = true;
						currentServerStaleTenanConfiguration = lastTenantConfig;
						currentServerStaleTenanConfigurationTime = DateTime.UtcNow.Ticks;
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009HA", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Tenant configuration was updated, reset stale version and time. Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}, Current server tenant configuration: {4} Max tenant config version: {5}", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName, currentServerStaleTenanConfiguration, num);
					}
					else
					{
						if (currentServerStaleTenanConfiguration == 0L && currentServerStaleTenanConfigurationTime == 0L)
						{
							currentServerStaleTenanConfiguration = lastTenantConfig;
							currentServerStaleTenanConfigurationTime = DateTime.UtcNow.Ticks;
						}
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009HB", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Tenant configuration is not up to date. Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}, Current server tenant configuration: {4} Max tenant config version: {5}", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName, currentServerStaleTenanConfiguration, num);
						if ((DateTime.UtcNow.Ticks - currentServerStaleTenanConfigurationTime) / 10000 > maxStaleTenantConfigPeriodMillis)
						{
							IsSystemHealthy = false;
							ScheduleServerShutdown();
						}
					}
				}
				else
				{
					IsSystemHealthy = true;
					currentServerStaleTenanConfiguration = 0L;
					currentServerStaleTenanConfigurationTime = 0L;
				}
			}
			else
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009HE", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "HealthMonitor cannot find any service instances. Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName);
			}
		}
		catch (Exception exception)
		{
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
			scheduler.Stop();
		}
		return true;
	}

	private void ScheduleServerShutdown()
	{
		if (!FeatureSwitches.Instance.TerminateForTenantConfigMismatch)
		{
			return;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009HC", Category.Operations, Verbosity.Error, PrivacyClassification.SystemMetadata, "Tenant configuration is not up to date. Scheduling server shutdown.Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}, Current server tenant configuration: {4} ", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName, currentServerStaleTenanConfiguration);
		if (shutdownTask != null)
		{
			return;
		}
		lock (shutdownTaskLockObject)
		{
			if (shutdownTask == null)
			{
				shutdownTask = Task.Delay(healthMonitorDelayBeforeServerShutdownMillis).ContinueWith((Task t) =>
				{
					shutdownAction();
				});
			}
		}
	}

	private void ShutdownService()
	{
		if (IsSystemHealthy)
		{
			lock (shutdownTaskLockObject)
			{
				shutdownTask = null;
				return;
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009HD", Category.Operations, Verbosity.Error, PrivacyClassification.SystemMetadata, "Tenant configuration is not up to date. Terminating server.Service instance id: {0}, Server computer name: {1}, Service instance name: {2}, Server instance name: {3}, Current server tenant configuration: {4} ", Heartbeat.Instance.ServiceInstanceId, serverComputerName, serviceInstanceName, serverInstanceName, currentServerStaleTenanConfiguration);
		NavEnvironment.Terminate(new NavServerStateException("Stale tenant configuration"), withGrace: true);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			scheduler?.Stop();
		}
	}
}
