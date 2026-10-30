using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class Heartbeat : IDisposable, ITenantConfigurationSynchronizer
{
	private const int MissedHeartbeatPeriodsBeforeCrashed = 6;

	private const int MissedHeartbeatPeriodsBeforeRemoved = 9;

	private static readonly NavMetric0Dimensions HeartBeatMetric = new NavMetric0Dimensions("ServerHeartBeat");

	private readonly Func<bool> heartbeatAction;

	private int serviceInstanceId = -1;

	private readonly string serverComputerName;

	private readonly string serviceName;

	private readonly string serverInstanceName;

	private readonly int serverPort;

	private readonly int managementPort;

	private readonly PeriodicScheduler scheduler;

	private readonly int period;

	private volatile SortedSet<int> currentAliveServiceInstances = new SortedSet<int>();

	private volatile SortedSet<int> currentServiceInstances = new SortedSet<int>();

	private DateTime lastOtherServerCrashedTime = DateTime.MinValue;

	private object syncRoot = new object();

	private long configurationVersion;

	private int refreshMinimumKnownConfigurationChangeSchedulerCallCount;

	private PeriodicScheduler refreshMinimumKnownConfigurationChangeScheduler;

	private List<ServerInstanceRuntimeInfo> serverInstancesInformation;

	public static readonly int Period = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.HeartbeatPeriod.Value.TotalMilliseconds);

	private const string UpdateCommandName = "HeartbeatUpdateCommand";

	private const string InsertServiceInstanceRecordCmd = "UPDATE [{0}].{1}.[Server Instance] SET [Status] = 2 WHERE [Server Computer Name] = @1 AND [Server Instance Name] = @2 AND [Status] <> 1;INSERT INTO [{0}].{1}.[Server Instance] ([Service Name],[Server Computer Name],[Last Active],[Server Instance Name],[Server Port],[Management Port],[Status], [Last Tenant Config Version])VALUES (@0,@1,GETUTCDATE(),@2,@3,@4,0,0) SELECT CAST(SCOPE_IDENTITY() AS INT)";

	private const string ReinsertServiceInstanceRecordCmd = "SET IDENTITY_INSERT [{0}].{1}.[Server Instance] ON;INSERT INTO [{0}].{1}.[Server Instance] ([Service Name],[Server Computer Name],[Last Active],[Server Instance Name],[Server Port],[Management Port],[Status],[Server Instance ID], [Last Tenant Config Version])VALUES (@0,@1,GETUTCDATE(),@2,@3,@4,0,@5,0) SELECT CAST(SCOPE_IDENTITY() AS INT);SET IDENTITY_INSERT [{0}].{1}.[Server Instance] OFF";

	private const string UpdateLastActiveServiceInstanceCmd = "DECLARE @LastActiveDate AS DATETIME = GETUTCDATE();UPDATE [{0}].{1}.[Server Instance] SET [Last Active] = @LastActiveDate, [Last Tenant Config Version] = @1 WHERE [Server Instance ID] = @0;SELECT IIF(@@ROWCOUNT = 0, NULL, @LastActiveDate)";

	private const string StopServiceInstanceCmd = "UPDATE [{0}].{1}.[Server Instance] SET [Status] = 1 WHERE [Server Instance ID] = @0";

	private const string DeleteOldServerInstances = "DELETE FROM [{0}].{1}.[Server Instance] WHERE [Last Active] < DATEADD(ms, -@toOldDiffTime, GETUTCDATE());";

	private const string SelectOtherServerInstances = "SELECT [Server Instance ID], [Last Active], (CASE WHEN [Status] <> 2 AND DATEADD(ms, -@stillAliveDiffTime, GETUTCDATE()) < [Last Active] THEN 0 ELSE 1 END) AS IsCrashed, [Last Tenant Config Version], [Server Instance Name], [Server Computer Name] FROM [{0}].{1}.[Server Instance] (NOLOCK) WHERE (NOT ([Server Computer Name] = @serverComputerName AND [Server Instance Name] = @serverInstanceName) AND [Status] <> 1) OR ([Status] = 2)";

	private const string SelectAllServerInstances = "SELECT [Server Instance ID], [Last Active], [Last Tenant Config Version], [Server Instance Name], [Server Computer Name] FROM [{0}].{1}.[Server Instance] (NOLOCK) WHERE [Status] = 0";

	public static Heartbeat Instance { get; private set; }

	internal string ServerComputerName => serverComputerName;

	internal string ServiceName => serviceName;

	internal string ServerInstanceName => serverInstanceName;

	public int ServiceInstanceId
	{
		get
		{
			scheduler.EnsureFirstSchedule();
			return serviceInstanceId;
		}
	}

	internal SortedSet<int> AliveServerInstances
	{
		get
		{
			scheduler.EnsureFirstSchedule();
			return currentAliveServiceInstances;
		}
	}

	internal SortedSet<int> KnownServerInstances
	{
		get
		{
			scheduler.EnsureFirstSchedule();
			return currentServiceInstances;
		}
	}

	internal bool IsOnlyServer
	{
		get
		{
			SortedSet<int> aliveServerInstances = AliveServerInstances;
			if (aliveServerInstances != null)
			{
				return aliveServerInstances.Count == 1;
			}
			return true;
		}
	}

	public DateTime LastActiveUpdated { get; private set; } = DateTime.MinValue;

	public event EventHandler<MinimumKnownConfigurationChangedEventArgs> MinimumKnownConfigurationChanged;

	public static void Initialize()
	{
		Instance?.Stop();
		Instance = new Heartbeat(Period, DnsHelper.MachineName, NavEnvironment.ServiceInstanceName, NavEnvironment.ServerInstanceName, NavEnvironment.NavClientServicesPort, NavEnvironment.ManagementServicesPort);
		Instance.scheduler.EnsureFirstSchedule();
	}

	public static void Destroy()
	{
		Instance?.Stop();
		Instance = null;
	}

	public Heartbeat(int period, string serverComputerName, string serviceName, string serverInstanceName, int serverPort, int managementPort, Func<bool> heartbeatAction = null, PeriodicScheduler scheduler = null, PeriodicScheduler refreshMinimumKnownConfigurationChangeScheduler = null)
	{
		this.serviceName = serviceName ?? string.Empty;
		this.serverComputerName = serverComputerName ?? string.Empty;
		this.serverInstanceName = serverInstanceName ?? string.Empty;
		this.serverPort = serverPort;
		this.managementPort = managementPort;
		this.heartbeatAction = heartbeatAction ?? new Func<bool>(WriteHeartbeatToServiceInstanceTable);
		this.period = period;
		this.scheduler = scheduler ?? new ThreadPeriodicScheduler("Heartbeat", this.period, ThreadPriority.Highest);
		this.scheduler.ScheduledAction = this.heartbeatAction;
		this.scheduler.Start();
		this.refreshMinimumKnownConfigurationChangeScheduler = RecreateRefreshMinimumKnownConfigurationChangeScheduler(refreshMinimumKnownConfigurationChangeScheduler);
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
			scheduler.Stop();
		}
	}

	private int ExecuteWriteHeartbeatToServiceInstanceTable(NavSqlCommand updateCommand, NavSqlConnectionScope conScope)
	{
		object obj = updateCommand.ExecuteScalar();
		if (obj != DBNull.Value)
		{
			LastActiveUpdated = DateTime.SpecifyKind((DateTime)obj, DateTimeKind.Utc);
		}
		else if (conScope != null)
		{
			RecreateServiceInstanceRecord(NavGlobal.AppDatabase, serviceInstanceId, conScope);
		}
		return 0;
	}

	public void AcknowledgeConfigurationChange(long newConfigurationVersion)
	{
		configurationVersion = newConfigurationVersion;
		WriteHeartbeatToServiceInstanceTable();
	}

	public void WaitForConfigurationSynchronization(long requiredConfigurationVersion, int timeout)
	{
		object configurationSyncRoot = new object();
		bool configurationAcknowledgedByAllServers = false;
		EventHandler<MinimumKnownConfigurationChangedEventArgs> value = (object sender, MinimumKnownConfigurationChangedEventArgs e) =>
		{
			if (e.ConfigurationVersion >= requiredConfigurationVersion)
			{
				lock (configurationSyncRoot)
				{
					configurationAcknowledgedByAllServers = true;
					Monitor.Pulse(configurationSyncRoot);
				}
			}
		};
		MinimumKnownConfigurationChanged += value;
		try
		{
			StartAdditionalRefreshOfMinimumKnownConfigurationChange();
			try
			{
				lock (configurationSyncRoot)
				{
					if (!configurationAcknowledgedByAllServers)
					{
						Monitor.Wait(configurationSyncRoot, timeout);
					}
				}
				if (!configurationAcknowledgedByAllServers)
				{
					throw new NavTenantStateSynchronizationException(string.Format(CultureInfo.InvariantCulture, "Operation timeout while waiting for state synchronization due to not responding server instances. Server instances that needed to reach required state: {0}", GetServerInstancesInformationText(requiredConfigurationVersion)));
				}
			}
			finally
			{
				StopAdditionalRefreshOfMinimumKnownConfigurationChange();
			}
		}
		finally
		{
			MinimumKnownConfigurationChanged -= value;
		}
	}

	private string GetServerInstancesInformationText(long requiredConfigurationVersion)
	{
		List<ServerInstanceRuntimeInfo> list = serverInstancesInformation;
		if (list == null || list.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		foreach (ServerInstanceRuntimeInfo item in list)
		{
			stringBuilder.AppendLine(FormattableString.Invariant($"Server Instance Id: {item.Id}, "));
			stringBuilder.AppendLine(FormattableString.Invariant($"Server Instance Name: {item.ServerInstanceName}, "));
			stringBuilder.AppendLine(FormattableString.Invariant($"Server Computer Name: {item.ServerComputerName}, "));
			stringBuilder.AppendLine(FormattableString.Invariant($"Has Reached Required Configuration State: {item.LastTenantConfig >= requiredConfigurationVersion} (Actual: {item.LastTenantConfig}, Required: {requiredConfigurationVersion}), "));
			stringBuilder.AppendLine(FormattableString.Invariant($"Last Active Time: {item.LastActive}, "));
			stringBuilder.AppendLine(FormattableString.Invariant($"Has Crashed: {item.HasCrashed}, "));
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	private void StartAdditionalRefreshOfMinimumKnownConfigurationChange()
	{
		lock (syncRoot)
		{
			if (refreshMinimumKnownConfigurationChangeSchedulerCallCount == 0)
			{
				refreshMinimumKnownConfigurationChangeScheduler.Start();
			}
			refreshMinimumKnownConfigurationChangeSchedulerCallCount++;
		}
	}

	private void StopAdditionalRefreshOfMinimumKnownConfigurationChange()
	{
		lock (syncRoot)
		{
			if (refreshMinimumKnownConfigurationChangeSchedulerCallCount <= 0)
			{
				return;
			}
			try
			{
				if (refreshMinimumKnownConfigurationChangeSchedulerCallCount == 1)
				{
					try
					{
						refreshMinimumKnownConfigurationChangeScheduler.Stop();
						return;
					}
					finally
					{
						refreshMinimumKnownConfigurationChangeScheduler = RecreateRefreshMinimumKnownConfigurationChangeScheduler(refreshMinimumKnownConfigurationChangeScheduler);
					}
				}
			}
			finally
			{
				refreshMinimumKnownConfigurationChangeSchedulerCallCount--;
			}
		}
	}

	internal bool WriteHeartbeatToServiceInstanceTable()
	{
		if (LastActiveUpdated != DateTime.MinValue && DateTime.UtcNow.Subtract(LastActiveUpdated).TotalMilliseconds > (double)(period * 2))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000295", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Last heartbeat on instance {0} was delayed more than a heartbeat period. Last update was {1} and period is {2} seconds", serviceInstanceId, LastActiveUpdated.ToString("o", CultureInfo.InvariantCulture), period / 1000);
		}
		bool succeeded = false;
		try
		{
			if (NavEnvironment.Instance?.Tenants?.SystemTenant != null && NavGlobal.SystemTenant.IsDatabaseInitialized)
			{
				NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithHeartbeatDuration(() =>
				{
					lock (syncRoot)
					{
						if (serviceInstanceId == -1)
						{
							CreateServiceInstanceRecord(NavGlobal.AppDatabase);
						}
						else
						{
							NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
							try
							{
								conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("HeartbeatUpdateCommand", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "DECLARE @LastActiveDate AS DATETIME = GETUTCDATE();UPDATE [{0}].{1}.[Server Instance] SET [Last Active] = @LastActiveDate, [Last Tenant Config Version] = @1 WHERE [Server Instance ID] = @0;SELECT IIF(@@ROWCOUNT = 0, NULL, @LastActiveDate)", NavGlobal.AppDatabase.DatabaseName, "dbo"), (NavSqlCommand updateCommand) =>
								{
									updateCommand.Command.Parameters.Add(new SqlParameter
									{
										ParameterName = NavSqlStatementHelper.ParameterName(0),
										SqlDbType = SqlDbType.Int
									});
									updateCommand.Command.Parameters.Add(new SqlParameter
									{
										ParameterName = NavSqlStatementHelper.ParameterName(1),
										SqlDbType = SqlDbType.BigInt
									});
								}, (NavSqlCommand updateCommand) =>
								{
									updateCommand.Command.Parameters[0].Value = serviceInstanceId;
									updateCommand.Command.Parameters[1].Value = configurationVersion;
								}, (NavSqlCommand updateCommand) => ExecuteWriteHeartbeatToServiceInstanceTable(updateCommand, conScope));
							}
							finally
							{
								if (conScope != null)
								{
									((IDisposable)conScope).Dispose();
								}
							}
						}
						UpdateAliveServiceInstances();
						succeeded = true;
					}
				});
			}
			else
			{
				succeeded = true;
			}
		}
		catch (Exception ex) when (ex is NavCSideException || ex is NavSqlException || ex is NavServerStateException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00006E3", Category.Operations, ex, Verbosity.Warning);
		}
		catch (Exception exception)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001DI", Category.Operations, exception);
		}
		finally
		{
			HeartBeatMetric.LogValue(succeeded ? 1 : 0);
		}
		return succeeded;
	}

	internal static List<ServerInstanceRuntimeInfo> GetAllServerInstances()
	{
		List<ServerInstanceRuntimeInfo> serverInstances = new List<ServerInstanceRuntimeInfo>();
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("HealthMonitorGetAllServerInstances", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Server Instance ID], [Last Active], [Last Tenant Config Version], [Server Instance Name], [Server Computer Name] FROM [{0}].{1}.[Server Instance] (NOLOCK) WHERE [Status] = 0", NavGlobal.AppDatabase.DatabaseName, "dbo"), (NavSqlCommand cmd) =>
			{
				using (cmd.ExecuteReader())
				{
					while (cmd.ReadNextRow())
					{
						serverInstances.Add(new ServerInstanceRuntimeInfo(cmd.Reader.GetInt32(0), cmd.Reader.GetDateTime(1), hasCrashed: false, cmd.Reader.GetInt64(2), cmd.Reader.GetString(3), cmd.Reader.GetString(4)));
					}
					cmd.CloseReader(forceClose: false);
				}
				return 0;
			});
		}
		return serverInstances;
	}

	private void AddParametersToSqlCommand(NavSqlCommand sqlCommand, int numberOfParameters, int optionalServiceInstanceId = -1)
	{
		for (int i = 0; i < numberOfParameters; i++)
		{
			SqlParameter sqlParameter = new SqlParameter
			{
				ParameterName = NavSqlStatementHelper.ParameterName(i)
			};
			switch (i)
			{
			case 0:
				sqlParameter.Value = serviceName;
				sqlParameter.SqlDbType = SqlDbType.NVarChar;
				sqlParameter.Size = 250;
				break;
			case 1:
				sqlParameter.Value = serverComputerName;
				sqlParameter.SqlDbType = SqlDbType.NVarChar;
				sqlParameter.Size = 250;
				break;
			case 2:
				sqlParameter.Value = serverInstanceName;
				sqlParameter.SqlDbType = SqlDbType.NVarChar;
				sqlParameter.Size = 250;
				break;
			case 3:
				sqlParameter.Value = serverPort;
				sqlParameter.SqlDbType = SqlDbType.Int;
				break;
			case 4:
				sqlParameter.Value = managementPort;
				sqlParameter.SqlDbType = SqlDbType.Int;
				break;
			case 5:
				sqlParameter.Value = optionalServiceInstanceId;
				sqlParameter.SqlDbType = SqlDbType.Int;
				break;
			}
			sqlCommand.Command.Parameters.Add(sqlParameter);
		}
	}

	private void CreateServiceInstanceRecord(NavDatabase database)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].{1}.[Server Instance] SET [Status] = 2 WHERE [Server Computer Name] = @1 AND [Server Instance Name] = @2 AND [Status] <> 1;INSERT INTO [{0}].{1}.[Server Instance] ([Service Name],[Server Computer Name],[Last Active],[Server Instance Name],[Server Port],[Management Port],[Status], [Last Tenant Config Version])VALUES (@0,@1,GETUTCDATE(),@2,@3,@4,0,0) SELECT CAST(SCOPE_IDENTITY() AS INT)", database.DatabaseName, "dbo"));
		AddParametersToSqlCommand(navSqlCommand, 5);
		object obj = navSqlCommand.ExecuteScalar();
		serviceInstanceId = (int)obj;
	}

	private void RecreateServiceInstanceRecord(NavDatabase database, int newServiceInstanceId, NavSqlConnectionScope scope)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}].{1}.[Server Instance] ON;INSERT INTO [{0}].{1}.[Server Instance] ([Service Name],[Server Computer Name],[Last Active],[Server Instance Name],[Server Port],[Management Port],[Status],[Server Instance ID], [Last Tenant Config Version])VALUES (@0,@1,GETUTCDATE(),@2,@3,@4,0,@5,0) SELECT CAST(SCOPE_IDENTITY() AS INT);SET IDENTITY_INSERT [{0}].{1}.[Server Instance] OFF", database.DatabaseName, "dbo"));
		AddParametersToSqlCommand(navSqlCommand, 6, newServiceInstanceId);
		navSqlCommand.ExecuteScalar();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000296", Category.Operations, Verbosity.Error, PrivacyClassification.SystemMetadata, "The Server Instance record for this service (id: {0}) was unexpectedly removed and had to be recreated", newServiceInstanceId);
		serviceInstanceId = newServiceInstanceId;
	}

	private void OnAliveServerInstancesChanged(AliveServerInstancesChangedEventArgs e)
	{
		NavEnvironment.Instance.OnAliveServerInstancesChanged(this, e);
	}

	private void OnMinimumKnownConfigurationChanged(MinimumKnownConfigurationChangedEventArgs e)
	{
		MinimumKnownConfigurationChanged?.Invoke(this, e);
	}

	private void UpdateAliveServiceInstances()
	{
		SortedSet<int> sortedSet = null;
		SortedSet<int> sortedSet2 = null;
		if (scheduler.State == SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully)
		{
			sortedSet = currentAliveServiceInstances;
			sortedSet2 = currentServiceInstances;
		}
		SortedSet<int> sortedSet3 = new SortedSet<int> { serviceInstanceId };
		SortedSet<int> sortedSet4 = new SortedSet<int> { serviceInstanceId };
		long val = 2147483647L;
		bool flag = false;
		List<ServerInstanceRuntimeInfo> list = new List<ServerInstanceRuntimeInfo>();
		foreach (ServerInstanceRuntimeInfo otherAliveOrCrashedServerInstance in GetOtherAliveOrCrashedServerInstances())
		{
			sortedSet3.Add(otherAliveOrCrashedServerInstance.Id);
			list.Add(otherAliveOrCrashedServerInstance);
			if (otherAliveOrCrashedServerInstance.HasCrashed)
			{
				if (otherAliveOrCrashedServerInstance.LastActive > lastOtherServerCrashedTime && NavEnvironment.Instance.HasAliveServerInstancesChangedSubscribers)
				{
					lastOtherServerCrashedTime = otherAliveOrCrashedServerInstance.LastActive;
					flag = true;
				}
			}
			else
			{
				sortedSet4.Add(otherAliveOrCrashedServerInstance.Id);
			}
			val = Math.Min(val, otherAliveOrCrashedServerInstance.LastTenantConfig);
		}
		currentAliveServiceInstances = sortedSet4;
		currentServiceInstances = sortedSet3;
		if (flag || sortedSet == null || !sortedSet4.SetEquals(sortedSet))
		{
			Verbosity verbosity = (flag ? Verbosity.Warning : Verbosity.Normal);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UI", Category.HealthCheck, verbosity, "Server Instances Changed:  HasServerCrashed={0}, Alive Servers='{1}', Previous Alive Servers='{2}'", DiagnosticParameter.SystemMetadata(flag), DiagnosticParameter.SystemMetadata(string.Join(";", sortedSet4)), DiagnosticParameter.SystemMetadata(string.Join(";", sortedSet ?? new SortedSet<int>())));
			OnAliveServerInstancesChanged(new AliveServerInstancesChangedEventArgs(flag, anotherServerRemoved: false));
		}
		else if (sortedSet2 != null && !sortedSet2.SetEquals(currentServiceInstances))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UJ", Category.HealthCheck, Verbosity.Normal, "Server Instances Changed:  Servers='{0}', PreviousServers='{1}'", DiagnosticParameter.SystemMetadata(string.Join(";", currentServiceInstances)), DiagnosticParameter.SystemMetadata(string.Join(";", sortedSet2)));
			OnAliveServerInstancesChanged(new AliveServerInstancesChangedEventArgs(anotherServerHasCrashed: false, anotherServerRemoved: true));
		}
		serverInstancesInformation = list;
		OnMinimumKnownConfigurationChanged(new MinimumKnownConfigurationChangedEventArgs(val));
	}

	private IEnumerable<ServerInstanceRuntimeInfo> GetOtherAliveOrCrashedServerInstances()
	{
		DeleteOldCrashedServerInstances();
		List<ServerInstanceRuntimeInfo> serverInstances = new List<ServerInstanceRuntimeInfo>();
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("HeartbeatSelectOtherServerInstances", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Server Instance ID], [Last Active], (CASE WHEN [Status] <> 2 AND DATEADD(ms, -@stillAliveDiffTime, GETUTCDATE()) < [Last Active] THEN 0 ELSE 1 END) AS IsCrashed, [Last Tenant Config Version], [Server Instance Name], [Server Computer Name] FROM [{0}].{1}.[Server Instance] (NOLOCK) WHERE (NOT ([Server Computer Name] = @serverComputerName AND [Server Instance Name] = @serverInstanceName) AND [Status] <> 1) OR ([Status] = 2)", NavGlobal.AppDatabase.DatabaseName, "dbo"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@serverComputerName", SqlDbType.NVarChar, 250);
				cmd.Command.Parameters.Add("@serverInstanceName", SqlDbType.NVarChar, 250);
				cmd.Command.Parameters.Add("@stillAliveDiffTime", SqlDbType.Int);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = serverComputerName;
				cmd.Command.Parameters[1].Value = serverInstanceName;
				cmd.Command.Parameters[2].Value = period * 6;
			}, (NavSqlCommand cmd) =>
			{
				using (cmd.ExecuteReader())
				{
					while (cmd.ReadNextRow())
					{
						serverInstances.Add(new ServerInstanceRuntimeInfo(cmd.Reader.GetInt32(0), cmd.Reader.GetDateTime(1), cmd.Reader.GetInt32(2) == 1, cmd.Reader.GetInt64(3), cmd.Reader.GetString(4), cmd.Reader.GetString(5)));
					}
					cmd.CloseReader(forceClose: false);
				}
				return 0;
			});
		}
		return serverInstances;
	}

	private void DeleteOldCrashedServerInstances()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("HeartbeatDeleteOldServerInstances", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].{1}.[Server Instance] WHERE [Last Active] < DATEADD(ms, -@toOldDiffTime, GETUTCDATE());", NavGlobal.AppDatabase.DatabaseName, "dbo"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add("@toOldDiffTime", SqlDbType.Int);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = period * 9;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public void Stop()
	{
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].{1}.[Server Instance] SET [Status] = 1 WHERE [Server Instance ID] = @0", NavGlobal.AppDatabase.DatabaseName, "dbo"));
			navSqlCommand.Command.Parameters.Add(new SqlParameter
			{
				ParameterName = NavSqlStatementHelper.ParameterName(0),
				SqlDbType = SqlDbType.Int
			});
			navSqlCommand.PrepareStatement();
			navSqlCommand.Command.Parameters[0].Value = serviceInstanceId;
			navSqlCommand.ExecuteNonQuery();
		}
		catch
		{
		}
		finally
		{
			Dispose();
		}
	}

	private PeriodicScheduler RecreateRefreshMinimumKnownConfigurationChangeScheduler(PeriodicScheduler currentRefreshScheduler)
	{
		PeriodicScheduler periodicScheduler = ((!(currentRefreshScheduler is ThreadPeriodicScheduler) && currentRefreshScheduler != null) ? currentRefreshScheduler : new ThreadPeriodicScheduler("HeartBeatRefreshMinimumKnownConfigurationChange", ServerUserSettings.Instance.StateSynchronizationAdditionalConfigurationVersionRefreshPeriod.TotalSeconds * 1000));
		if (periodicScheduler.ScheduledAction == null)
		{
			periodicScheduler.ScheduledAction = () =>
			{
				try
				{
					UpdateAliveServiceInstances();
				}
				catch (NavBaseException exception)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001DJ", Category.Operations, exception);
				}
				return true;
			};
		}
		return periodicScheduler;
	}
}
