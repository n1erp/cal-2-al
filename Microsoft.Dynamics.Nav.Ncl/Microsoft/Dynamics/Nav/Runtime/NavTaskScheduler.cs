using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTaskScheduler : IDisposable, ITaskScheduler
{
	protected internal static class SqlDml
	{
		private const string CreateTableTaskScheduleStatement = "IF NOT EXISTS(SELECT* FROM sysobjects WHERE id = OBJECT_ID(N'$ndo$taskscheduling') AND OBJECTPROPERTY(id, N'IsTable') = 1) BEGIN CREATE TABLE [{0}].[dbo].[$ndo$taskscheduling](id uniqueidentifier NOT NULL,timestamp timestamp NOT NULL,tenantid nvarchar(128) COLLATE Latin1_General_100_CI_AS NOT NULL,not_before datetime NOT NULL,ended datetime,[Server Instance ID] int NOT NULL,state int,retries int,tasktype int,CONSTRAINT[PK$ndo$taskscheduling] PRIMARY KEY CLUSTERED (id, tenantid)) CREATE NONCLUSTERED INDEX [taskscheduling_state_not_before_index] ON [{0}].[dbo].[$ndo$taskscheduling](  [state] ASC, [not_before] ASC) INCLUDE(  [timestamp], [ended], [Server Instance ID], [retries], [tasktype]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_timestamp_index] ON [{0}].[dbo].[$ndo$taskscheduling](   [timestamp] ASC) INCLUDE(  [ended], [not_before], [state], [Server Instance ID], [retries], [tasktype]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_not_before_index] ON [{0}].[dbo].[$ndo$taskscheduling](  [not_before] ASC) INCLUDE(  [timestamp], [ended], [Server Instance ID], [retries], [tasktype], [state]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_ended_state] ON [{0}].[dbo].[$ndo$taskscheduling](  [state] ASC, [ended] ASC) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) END";

		private static readonly string IsTaskScheduledCmd = "SELECT [id] FROM [{0}].[dbo].[$ndo$taskscheduling] WHERE [id] = @id AND [tenantid] = @tenantId AND (state=" + 0 + " OR state=" + 1 + ")";

		private static readonly string IsTaskScheduledAndTenantMountedCmd = "SELECT (SELECT [id] FROM [{0}].[dbo].[$ndo$taskscheduling] WHERE [id] = @id AND [tenantid] = @tenantId AND (state=" + 0 + " OR state=" + 1 + ")) taskid, (SELECT [tenantid] FROM [{0}].[dbo].[$ndo$tenants] WHERE [tenantid] = @tenantId) tenantid";

		private static readonly string CancelScheduledTaskCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 4 + ", ended=@ended WHERE [id] = @id AND [tenantid] = @tenantId AND (state = " + 0 + " OR state = " + 1 + ");SELECT @@ROWCOUNT";

		private static readonly string RemoveDoneTasksCmd = "DELETE FROM [{0}].[dbo].[$ndo$taskscheduling] WITH (READPAST) WHERE [state] = " + 4 + " AND [ended] < @ended;SELECT @@ROWCOUNT";

		private static readonly string RemoveAllTasksForTenantCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 4 + ", ended=@ended WHERE [tenantid] = @tenantId;SELECT @@ROWCOUNT";

		private static readonly string RemoveAllNonRunningTasksForTenantCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 4 + ", ended=@ended WHERE [tenantid] = @tenantId AND state != " + 3 + ";SELECT @@ROWCOUNT";

		private static readonly string SetNotStartedTaskReadyCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 1 + ", [not_before] = @notBefore WHERE [id] = @id AND [state] = " + 0 + " AND [tenantid] = @tenantId;SELECT @@ROWCOUNT";

		private const string ScheduleTaskCmd = "INSERT INTO [{0}].[dbo].[$ndo$taskscheduling] ([id],[tenantid],[not_before],[state],[Server Instance ID],[retries],[tasktype]) VALUES (@id,@tenantId,@notbefore,@state,@serverInstanceId,@retries,@tasktype)";

		private const string SelectAllReadyTaskScheduleCmd = "SELECT [id], [timestamp], [tenantid], [not_before], ended, [state], [Server Instance ID], [retries], [tasktype] FROM [{0}].[dbo].[$ndo$taskscheduling] WITH (READPAST,READCOMMITTED) WHERE [not_before] < @notBefore ORDER BY [tasktype], [timestamp]";

		private static readonly string SelectLatestTaskScheduleCmd = "SELECT * FROM (SELECT [id], [timestamp], [tenantid], [not_before], ended, [state], [Server Instance ID], [retries], [tasktype] FROM [{0}].[dbo].[$ndo$taskscheduling] WITH (READPAST,READCOMMITTED) WHERE [timestamp] > @timestamp UNION SELECT [id], [timestamp], [tenantid], [not_before], ended, [state], [Server Instance ID], [retries], [tasktype] FROM [{0}].[dbo].[$ndo$taskscheduling] WITH (READPAST,READCOMMITTED) WHERE ([not_before] < @notBefore AND state = " + 1 + ")) as Result ORDER BY Result.[timestamp]";

		private const string SetTaskStateCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = @newState, [Server Instance ID] = @serverInstanceId, ended=@ended WHERE [id] = @id AND [tenantid] = @tenantId AND [state] = @oldState AND ([Server Instance ID] = 0 OR [Server Instance ID] = @serverInstanceId);SELECT @@ROWCOUNT";

		private static readonly string SetTaskReadyCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 1 + ", [Server Instance ID] = 0 WHERE [id] = @id AND [tenantid] = @tenantId;SELECT @@ROWCOUNT";

		private static readonly string SetTaskRetryCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 1 + ", [Server Instance ID] = 0, [retries] = [retries] + 1, [not_before] = @notBefore WHERE [id] = @id AND [tenantid] = @tenantId;SELECT @@ROWCOUNT";

		private static readonly string RecoverTasksForCreashedServerCmd = "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = " + 1 + ", [Server Instance ID] = 0, [retries] = [retries] + 1, [not_before] = @notBefore WHERE [Server Instance ID] = @serverInstanceId AND ([state] = " + 2 + " OR [state] = " + 3 + " );SELECT @@ROWCOUNT";

		private static bool IsGuidEqualToDatabaseGuid(object databaseGuid, Guid other)
		{
			if (databaseGuid == null || DBNull.Value.Equals(databaseGuid))
			{
				return false;
			}
			return other.Equals(databaseGuid);
		}

		internal static void CreateTaskScheduleTable()
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
			try
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT* FROM sysobjects WHERE id = OBJECT_ID(N'$ndo$taskscheduling') AND OBJECTPROPERTY(id, N'IsTable') = 1) BEGIN CREATE TABLE [{0}].[dbo].[$ndo$taskscheduling](id uniqueidentifier NOT NULL,timestamp timestamp NOT NULL,tenantid nvarchar(128) COLLATE Latin1_General_100_CI_AS NOT NULL,not_before datetime NOT NULL,ended datetime,[Server Instance ID] int NOT NULL,state int,retries int,tasktype int,CONSTRAINT[PK$ndo$taskscheduling] PRIMARY KEY CLUSTERED (id, tenantid)) CREATE NONCLUSTERED INDEX [taskscheduling_state_not_before_index] ON [{0}].[dbo].[$ndo$taskscheduling](  [state] ASC, [not_before] ASC) INCLUDE(  [timestamp], [ended], [Server Instance ID], [retries], [tasktype]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_timestamp_index] ON [{0}].[dbo].[$ndo$taskscheduling](   [timestamp] ASC) INCLUDE(  [ended], [not_before], [state], [Server Instance ID], [retries], [tasktype]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_not_before_index] ON [{0}].[dbo].[$ndo$taskscheduling](  [not_before] ASC) INCLUDE(  [timestamp], [ended], [Server Instance ID], [retries], [tasktype], [state]) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) CREATE NONCLUSTERED INDEX [taskscheduling_ended_state] ON [{0}].[dbo].[$ndo$taskscheduling](  [state] ASC, [ended] ASC) WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) END", NavGlobal.AppDatabase.DatabaseName));
			}
			catch (NavSqlException ex)
			{
				if (!(ex.InnerException is SqlException { Number: 2714 }))
				{
					throw;
				}
			}
		}

		internal static bool IsTaskScheduled(Guid id, string tenantId)
		{
			return RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
				return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("IsTaskScheduledCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, IsTaskScheduledCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = id;
					cmd.Command.Parameters[1].Value = tenantId;
				}, (NavSqlCommand cmd) => IsGuidEqualToDatabaseGuid(cmd.ExecuteScalar(), id));
			}, throwOnError: false);
		}

		internal static void TaskAndTenantExists(Guid id, string tenantId, out bool taskExists, out bool tenantExist)
		{
			bool localTaskExists = false;
			bool localTenantExist = false;
			RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
				return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("IsTaskScheduledAndTenantMountedCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, IsTaskScheduledAndTenantMountedCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = id;
					cmd.Command.Parameters[1].Value = tenantId;
				}, (NavSqlCommand cmd) =>
				{
					using (cmd.ExecuteReader())
					{
						if (!cmd.ReadNextRow())
						{
							localTaskExists = false;
							localTenantExist = false;
						}
						else
						{
							localTaskExists = !cmd.Reader.IsDBNull(0) && IsGuidEqualToDatabaseGuid(cmd.Reader.GetGuid(0), id);
							localTenantExist = !cmd.Reader.IsDBNull(1) && string.Equals(cmd.Reader.GetString(1), tenantId, StringComparison.OrdinalIgnoreCase);
						}
						cmd.CloseReader(forceClose: false);
					}
					return true;
				});
			}, throwOnError: true);
			taskExists = localTaskExists;
			tenantExist = localTenantExist;
		}

		internal static bool CancelScheduledTask(NavSqlConnectionScope conScope, Guid id, string tenantId)
		{
			return RetrySql(() => conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("CancelScheduledTaskCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, CancelScheduledTaskCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
				cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
				cmd.Command.Parameters.Add("@ended", SqlDbType.DateTime);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = id;
				cmd.Command.Parameters[1].Value = tenantId;
				cmd.Command.Parameters[2].Value = DateTime.UtcNow;
			}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar() == 1), throwOnError: false);
		}

		internal static bool RemoveDoneTasks(DateTime now)
		{
			return RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RemoveDoneTasksCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, RemoveDoneTasksCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@ended", SqlDbType.DateTime);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = now.AddMinutes(-5.0);
				}, (NavSqlCommand cmd) =>
				{
					int num = (int)cmd.ExecuteScalar();
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000035", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "# of Done tasks removed={0}", num);
					return true;
				});
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError: false);
		}

		internal static void RemoveAllTasksForTenant(string tenantId)
		{
			RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RemoveAllTasksForTenantCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, RemoveAllTasksForTenantCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
					cmd.Command.Parameters.Add("@ended", SqlDbType.DateTime);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = tenantId;
					cmd.Command.Parameters[1].Value = DateTime.UtcNow;
				}, (NavSqlCommand cmd) =>
				{
					int num = (int)cmd.ExecuteScalar();
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000049", Category.TaskScheduling, Verbosity.Verbose, "{0} rows marked as done for tenant: {1}", DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.Oii(tenantId));
					return num > 0;
				});
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError: false);
		}

		internal static void RemoveAllNonRunningTasksForTenant(string tenantId)
		{
			RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RemoveAllNonRunningTasksForTenantCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, RemoveAllNonRunningTasksForTenantCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
					cmd.Command.Parameters.Add("@ended", SqlDbType.DateTime);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = tenantId;
					cmd.Command.Parameters[1].Value = DateTime.UtcNow;
				}, (NavSqlCommand cmd) =>
				{
					int num = (int)cmd.ExecuteScalar();
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000B2", Category.TaskScheduling, Verbosity.Verbose, "{0} non-running task rows marked as done for tenant: {1}", DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.Oii(tenantId));
					return num > 0;
				});
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError: false);
		}

		internal static void InsertTaskSchedule(NavSqlConnectionScope conScope, Guid id, int state, DateTime notBefore, string tenantid, NavTaskSchedulerTaskType taskType)
		{
			RetrySql(() =>
			{
				conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("ScheduleTaskCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[dbo].[$ndo$taskscheduling] ([id],[tenantid],[not_before],[state],[Server Instance ID],[retries],[tasktype]) VALUES (@id,@tenantId,@notbefore,@state,@serverInstanceId,@retries,@tasktype)", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
					cmd.Command.Parameters.Add("@notbefore", SqlDbType.DateTime);
					cmd.Command.Parameters.Add("@state", SqlDbType.Int);
					cmd.Command.Parameters.Add("@serverInstanceId", SqlDbType.Int);
					cmd.Command.Parameters.Add("@retries", SqlDbType.Int);
					cmd.Command.Parameters.Add("@tasktype", SqlDbType.Int);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = id;
					cmd.Command.Parameters[1].Value = tenantid;
					cmd.Command.Parameters[2].Value = notBefore;
					cmd.Command.Parameters[3].Value = state;
					cmd.Command.Parameters[4].Value = 0;
					cmd.Command.Parameters[5].Value = 0;
					cmd.Command.Parameters[6].Value = taskType;
				}, (NavSqlCommand cmd) =>
				{
					cmd.ExecuteNonQuery();
					return 0;
				});
				return true;
			}, throwOnError: true);
		}

		internal static bool SetTaskState(Guid id, string tenantId, int newState, int oldState, int serviceInstanceId, bool throwOnError)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000036", Category.TaskScheduling, Verbosity.Verbose, "SetTaskState: TaskId={0}, TenantId={1}, State={2}, OldState={3}, ServiceInstanceId={4}", DiagnosticParameter.SystemMetadata(id), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(newState), DiagnosticParameter.SystemMetadata(oldState), DiagnosticParameter.SystemMetadata(serviceInstanceId));
			return RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SetTaskStateCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[dbo].[$ndo$taskscheduling] SET [state] = @newState, [Server Instance ID] = @serverInstanceId, ended=@ended WHERE [id] = @id AND [tenantid] = @tenantId AND [state] = @oldState AND ([Server Instance ID] = 0 OR [Server Instance ID] = @serverInstanceId);SELECT @@ROWCOUNT", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
					cmd.Command.Parameters.Add("@newState", SqlDbType.Int);
					cmd.Command.Parameters.Add("@oldState", SqlDbType.Int);
					cmd.Command.Parameters.Add("@serverInstanceId", SqlDbType.Int);
					cmd.Command.Parameters.Add("@ended", SqlDbType.DateTime);
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = id;
					cmd.Command.Parameters[1].Value = newState;
					cmd.Command.Parameters[2].Value = oldState;
					cmd.Command.Parameters[3].Value = serviceInstanceId;
					cmd.Command.Parameters[4].Value = ((newState == 4) ? ((object)DateTime.UtcNow) : DBNull.Value);
					cmd.Command.Parameters[5].Value = tenantId;
				}, (NavSqlCommand cmd) =>
				{
					int num = (int)cmd.ExecuteScalar();
					bool flag = num == 1;
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000037", Category.TaskScheduling, Verbosity.Verbose, "SetTaskState: TaskId={0}, TenantId={1}, Rowsupdated={2}, Success={3}", DiagnosticParameter.SystemMetadata(id), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(flag));
					return flag;
				});
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError);
		}

		internal static bool SetNotStartedTaskReady(NavSqlConnectionScope conScope, Guid id, string tenantId, DateTime notBefore)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000038", Category.TaskScheduling, Verbosity.Verbose, "SetTaskReady: TaskId={0}, TenantId={1}, NotBefore={2}", DiagnosticParameter.SystemMetadata(id), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(notBefore));
			return RetrySql(() => conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SetNotStartedTaskReadyCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, SetNotStartedTaskReadyCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
				cmd.Command.Parameters.Add("@notBefore", SqlDbType.DateTime);
				cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = id;
				cmd.Command.Parameters[1].Value = notBefore;
				cmd.Command.Parameters[2].Value = tenantId;
			}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar() == 1), throwOnError: true);
		}

		internal static bool SetTaskReady(Guid id, string tenantId)
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
			bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SetTaskReadyCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, SetTaskReadyCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
				cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = id;
				cmd.Command.Parameters[1].Value = tenantId;
			}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar() == 1);
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
			return result;
		}

		internal static int RecoverTasksForCrashedServerInstance(int serverInstanceId)
		{
			return RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				int result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RecoverTasksForCreashedServerCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, RecoverTasksForCreashedServerCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@serverInstanceId", SqlDbType.Int);
					cmd.Command.Parameters.Add("@notBefore", SqlDbType.DateTime);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = serverInstanceId;
					cmd.Command.Parameters[1].Value = DateTime.UtcNow;
				}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar());
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError: false);
		}

		internal static bool SetTaskRetry(Guid id, string tenantId, DateTime notBefore, bool throwOnError)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000039", Category.TaskScheduling, Verbosity.Verbose, "SetRetry: TaskId={0}, TenantId={1}, NotBefore={2}", DiagnosticParameter.SystemMetadata(id), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(notBefore));
			return RetrySql(() =>
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
				bool result = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SetTaskRetryCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, SetTaskRetryCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@id", SqlDbType.UniqueIdentifier);
					cmd.Command.Parameters.Add("@notBefore", SqlDbType.DateTime);
					cmd.Command.Parameters.Add("@tenantId", SqlDbType.NVarChar, 128);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = id;
					cmd.Command.Parameters[1].Value = notBefore;
					cmd.Command.Parameters[2].Value = tenantId;
				}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar() == 1);
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				return result;
			}, throwOnError);
		}

		internal static IEnumerable<NavTaskScheduleInfo> Read(byte[] timestamp, int currentServiceInstanceId, bool readFromStart)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000003A", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Read: Timestamp={0}, ServiceInstanceId={1}, FromStart={2}", TimestampToString(timestamp), currentServiceInstanceId, readFromStart);
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
			if ((timestamp == null) | readFromStart)
			{
				return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SelectAllReadyTaskScheduleCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [id], [timestamp], [tenantid], [not_before], ended, [state], [Server Instance ID], [retries], [tasktype] FROM [{0}].[dbo].[$ndo$taskscheduling] WITH (READPAST,READCOMMITTED) WHERE [not_before] < @notBefore ORDER BY [tasktype], [timestamp]", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add("@notBefore", SqlDbType.DateTime);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = DateTime.UtcNow;
				}, (NavSqlCommand cmd) => ReadTaskRows(cmd, currentServiceInstanceId));
			}
			return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SelectLatestTaskScheduleCmd", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, SelectLatestTaskScheduleCmd, NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@timestamp", SqlDbType.Timestamp, 8);
				cmd.Command.Parameters.Add("@notBefore", SqlDbType.DateTime);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = timestamp;
				cmd.Command.Parameters[1].Value = DateTime.UtcNow;
			}, (NavSqlCommand cmd) => ReadTaskRows(cmd, currentServiceInstanceId));
		}

		private static string TimestampToString(byte[] timestamp)
		{
			if (timestamp == null)
			{
				return "null";
			}
			if (timestamp.Length == 0)
			{
				return "empty";
			}
			byte[] array = new byte[timestamp.Length];
			Array.Copy(timestamp, array, timestamp.Length);
			Array.Reverse(array);
			return BitConverter.ToUInt64(array, 0).ToString(NumberFormatInfo.InvariantInfo);
		}

		private static IEnumerable<NavTaskScheduleInfo> ReadTaskRows(NavSqlCommand command, int currentServiceInstanceId)
		{
			List<NavTaskScheduleInfo> list = new List<NavTaskScheduleInfo>();
			using (command.ExecuteReader())
			{
				while (command.ReadNextRow())
				{
					Guid guid = command.Reader.GetGuid(0);
					byte[] timestamp = (byte[])command.Reader.GetValue(1);
					string tenantId = command.Reader.GetString(2);
					DateTime dateTime = command.Reader.GetDateTime(3);
					DateTime? ended = (command.Reader.IsDBNull(4) ? ((DateTime?)null) : new DateTime?(command.Reader.GetDateTime(4)));
					int @int = command.Reader.GetInt32(5);
					int int2 = command.Reader.GetInt32(6);
					int int3 = command.Reader.GetInt32(7);
					NavTaskSchedulerTaskType int4 = (NavTaskSchedulerTaskType)command.Reader.GetInt32(8);
					list.Add(new NavTaskScheduleInfo(guid, timestamp, tenantId, @int, int2, dateTime, ended, int3, currentServiceInstanceId, int4));
				}
			}
			return list.AsReadOnly();
		}

		internal static T RetrySql<T>(Func<T> func, bool throwOnError)
		{
			Exception ex = null;
			for (int num = 3; num > 0; num--)
			{
				try
				{
					return func();
				}
				catch (NavSqlException ex2)
				{
					ex = ex2;
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000003B", Category.TaskScheduling, ex, Verbosity.Warning);
				}
				catch (NavCSideException ex3)
				{
					ex = ex3;
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000003C", Category.TaskScheduling, ex, Verbosity.Warning);
				}
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000003D", Category.TaskScheduling, ex);
			LogWriterHelper.LogExceptionEvent(ex);
			if (throwOnError)
			{
				throw ex;
			}
			return default;
		}
	}

	private class ScheduledTaskInfo
	{
		public bool IsReady { get; set; }

		public NavValue NotBefore { get; set; }

		public Guid Id { get; set; }

		public NavTaskSchedulerTaskType Type { get; set; }
	}

	internal const int MaxRetryCount = 10;

	private readonly PeriodicScheduler scheduler;

	private static NavMetric0Dimensions totalTasksCounterMetric = new NavMetric0Dimensions("TotalTasksCounter");

	internal static readonly int Period = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.TaskSchedulerPeriod.Value.TotalMilliseconds);

	internal static int MaxNumberOfRunningTasks = ServerUserSettings.Instance.TaskSchedulerMaxConcurrentRunningTasks.Value;

	internal const int TaskScheduleState_NotReady = 0;

	internal const int TaskScheduleState_Ready = 1;

	internal const int TaskScheduleState_Starting = 2;

	internal const int TaskScheduleState_Running = 3;

	internal const int TaskScheduleState_Done = 4;

	private byte[] lastTimestamp;

	private readonly SortedDictionary<Guid, NavTaskScheduleInfo> rawList = new SortedDictionary<Guid, NavTaskScheduleInfo>();

	private readonly List<TaskRunner> taskList = new List<TaskRunner>();

	private readonly Action<TaskRunner> runTaskFunc;

	private readonly Func<Heartbeat> getHeartbeat;

	private readonly int serviceInstanceId;

	private bool aliveServerInstancesHasChanged;

	private int highestSeenServerInstanceId;

	private DateTime nextDoneCleanup = AddDoneCleanupDelay(DateTime.UtcNow);

	private TimeSpan systemTaskStartTime = ServerUserSettings.Instance.TaskSchedulerSystemTaskStartTime.Value;

	private TimeSpan systemTaskEndTime = ServerUserSettings.Instance.TaskSchedulerSystemTaskEndTime.Value;

	private ISystemTaskFactory systemTaskFactory;

	private static DateTime AddDoneCleanupDelay(DateTime dateTime)
	{
		return dateTime.AddMinutes(1.0);
	}

	internal NavTaskScheduler(Action<TaskRunner> runTaskFunc = null, Func<Heartbeat> getHeartbeatFunc = null, int serviceInstanceId = 0, ISystemTaskFactory systemTaskFactory = null)
	{
		scheduler = new ThreadPeriodicScheduler("NavTaskScheduler", Period)
		{
			ScheduledAction = RunScheduler
		};
		ServerUserSettings.SettingsChanged += (object sender, SettingsChangedEventArgs args) =>
		{
			if (args.SettingName.Equals("TaskSchedulerMaxConcurrentRunningTasks", StringComparison.OrdinalIgnoreCase))
			{
				MaxNumberOfRunningTasks = ServerUserSettings.Instance.TaskSchedulerMaxConcurrentRunningTasks.Value;
			}
			else if (args.SettingName.Equals("TaskSchedulerSystemTaskStartTime", StringComparison.OrdinalIgnoreCase))
			{
				systemTaskStartTime = ServerUserSettings.Instance.TaskSchedulerSystemTaskStartTime.Value;
			}
			else if (args.SettingName.Equals("TaskSchedulerSystemTaskEndTime", StringComparison.OrdinalIgnoreCase))
			{
				systemTaskEndTime = ServerUserSettings.Instance.TaskSchedulerSystemTaskEndTime.Value;
			}
		};
		this.runTaskFunc = runTaskFunc ?? new Action<TaskRunner>(RunTask);
		getHeartbeat = getHeartbeatFunc ?? ((Func<Heartbeat>)(() => Heartbeat.Instance));
		this.serviceInstanceId = ((serviceInstanceId == 0) ? NavEnvironment.Instance.GetServiceInstanceId() : serviceInstanceId);
		this.systemTaskFactory = systemTaskFactory ?? new SystemTaskFactory();
	}

	void ITaskScheduler.CreateTask(Guid id, string tenantId, DateTime notBefore, bool isReady, NavTaskSchedulerTaskType taskType)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		SqlDml.InsertTaskSchedule(conScope, id, isReady ? 1 : 0, notBefore, tenantId, taskType);
	}

	bool ITaskScheduler.TaskExists(Guid id, string tenantId)
	{
		return TaskExists(id, tenantId);
	}

	bool ITaskScheduler.CancelTask(Guid id, string tenantId)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		return SqlDml.CancelScheduledTask(conScope, id, tenantId);
	}

	bool ITaskScheduler.SetReady(Guid id, string tenantId, DateTime notBefore)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		return SqlDml.SetNotStartedTaskReady(conScope, id, tenantId, notBefore);
	}

	void ITaskScheduler.ForceTaskListSynchronization(string tenantId)
	{
		NavTenant tenant = null;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000WJ", Category.TaskScheduling, () =>
		{
			tenant = NavEnvironment.Instance.Tenants.GetTenantById(tenantId);
		}, rethrow: false);
		if (tenant != null && !tenant.FailedToMount)
		{
			EnsureTenantScheduledTasksIsScheduledInAppDatabase(tenant);
		}
	}

	internal static void RemoveAllTasksScheduledForTenant(string tenantId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000SB", Category.TaskScheduling, () =>
		{
			SqlDml.RemoveAllTasksForTenant(tenantId);
		}, rethrow: false);
	}

	internal static void RemoveAllNonRunningTasksScheduledForTenant(string tenantId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000SC", Category.TaskScheduling, () =>
		{
			SqlDml.RemoveAllNonRunningTasksForTenant(tenantId);
		}, rethrow: false);
	}

	private void Tenants_TenantDismounted(object sender, NavTenantChangedEventArgs args)
	{
		RemoveAllTasksScheduledForTenant(args.TenantId);
	}

	private void Tenants_TenantDismounting(object sender, NavTenantChangedEventArgs args)
	{
		RemoveAllNonRunningTasksScheduledForTenant(args.TenantId);
	}

	internal static bool TaskExists(Guid id, string tenantId)
	{
		return SqlDml.IsTaskScheduled(id, tenantId);
	}

	internal static void TaskAndTenantExists(Guid id, string tenantId, out bool taskExists, out bool tenantExists)
	{
		if (!ServerUserSettings.Instance.Multitenant.Value)
		{
			tenantExists = true;
			taskExists = TaskExists(id, tenantId);
		}
		else
		{
			SqlDml.TaskAndTenantExists(id, tenantId, out taskExists, out tenantExists);
		}
	}

	public void Start()
	{
		if (ServerUserSettings.Instance.EnableTaskScheduler.Value && !NavEnvironment.Instance.StandaloneInstance)
		{
			NavEnvironment.Instance.AliveServerInstancesChanged += OnAliveServerInstancesChanged;
			scheduler.Start();
			NavEnvironment.Instance.Tenants.TenantDismounting += Tenants_TenantDismounting;
			NavEnvironment.Instance.Tenants.TenantDismounted += Tenants_TenantDismounted;
		}
	}

	public void Stop()
	{
		Dispose();
	}

	private void OnAliveServerInstancesChanged(object sender, AliveServerInstancesChangedEventArgs aliveServerInstancesChangedEventArgs)
	{
		aliveServerInstancesHasChanged = true;
	}

	internal bool RunScheduler()
	{
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		using (mostSpecificInstance.CreateTraceScope("000003W", DiagnosticParameter.SystemMetadata("RunTaskScheduler"), MonitorType.None, Category.TaskScheduling, createNewActivity: true, Verbosity.Verbose))
		{
			try
			{
				RefreshTaskScheduleList();
				RemoveCompletedTasks();
				if (TooManyRunningTasks())
				{
					mostSpecificInstance.SendTraceTag("000002M", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Maximum number of running tasks is reached.");
					return true;
				}
				NavEnvironment.Instance.PerformanceCounterSetter.SetTimeWithoutTaskScheduleCapacity(Stopwatch.GetTimestamp());
				RecoverLostTasks();
				RemoveDoneTasks();
				bool allowRunningSystemTasks = AllowRunningSystemTasks();
				List<NavTaskScheduleInfo> readyToRunTasks = GetReadyToRunTasks(allowRunningSystemTasks);
				if (!readyToRunTasks.Any())
				{
					mostSpecificInstance.SendTraceTag("000002N", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Nothing to run.");
					return true;
				}
				List<NavTaskScheduleInfo> list = new List<NavTaskScheduleInfo>();
				using (List<NavTaskScheduleInfo>.Enumerator enumerator = readyToRunTasks.GetEnumerator())
				{
					while (!TooManyRunningTasks() && enumerator.MoveNext())
					{
						NavTaskScheduleInfo current = enumerator.Current;
						TaskRunner taskRunner = CreateTaskRunner(current, out var removeScheduleInfo, systemTaskFactory);
						if (removeScheduleInfo)
						{
							list.Add(current);
						}
						if (taskRunner != null)
						{
							taskList.Add(taskRunner);
							runTaskFunc(taskRunner);
						}
						RemoveCompletedTasks();
					}
				}
				list.ForEach((NavTaskScheduleInfo info) =>
				{
					rawList.Remove(info.Id);
				});
			}
			catch (Exception ex)
			{
				if (ex is NavSqlException ex2 && NavSqlReturnCode.IsTransientError(ex2.ErrorNumber))
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000BZ", Category.TaskScheduling, ex, Verbosity.Warning);
				}
				else
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000002O", Category.TaskScheduling, ex);
					LogWriterHelper.LogExceptionEvent(ex);
				}
			}
			finally
			{
				UpdateTaskListPerformanceCounters();
			}
			return true;
		}
	}

	private static TaskRunner CreateTaskRunner(NavTaskScheduleInfo scheduleInfo, out bool removeScheduleInfo, ISystemTaskFactory systemTaskFactory)
	{
		removeScheduleInfo = false;
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		mostSpecificInstance.SendTraceTag("000002X", Category.TaskScheduling, Verbosity.Normal, "CreateTaskRunInfo: {0}", DiagnosticParameter.Oii(scheduleInfo.GetFullTraceText()));
		string text = string.Format(CultureInfo.InvariantCulture, "TaskId={0}, TenantId={1}", scheduleInfo.Id, scheduleInfo.TenantId);
		if (!NavEnvironment.Instance.Tenants.TryGetTenantById(scheduleInfo.TenantId, out var tenant))
		{
			mostSpecificInstance.SendTraceTag("000002Y", Category.TaskScheduling, Verbosity.Normal, "Tenant not mounted on NST: {0}", DiagnosticParameter.Oii(text));
			TaskAndTenantExists(scheduleInfo.Id, scheduleInfo.TenantId, out var taskExists, out var tenantExists);
			if (!tenantExists)
			{
				mostSpecificInstance.SendTraceTag("00000VV", Category.TaskScheduling, Verbosity.Normal, "Tenant '{0}' is dismounted, task '{1}' is orphaned. All tenant tasks will be marked for deletion in database.", DiagnosticParameter.Oii(scheduleInfo.Id), DiagnosticParameter.SystemMetadata(scheduleInfo.TenantId));
				mostSpecificInstance.SendTraceTagOnFailure("00000V9", Category.TaskScheduling, () =>
				{
					SqlDml.RemoveAllNonRunningTasksForTenant(scheduleInfo.TenantId);
				}, rethrow: false);
				removeScheduleInfo = true;
			}
			else if (!taskExists)
			{
				removeScheduleInfo = true;
				mostSpecificInstance.SendTraceTag("00000VW", Category.TaskScheduling, Verbosity.Normal, "Tenant '{0}' task '{1}' is orphaned, will be removed from task scheduler queue.", DiagnosticParameter.Oii(scheduleInfo.Id), DiagnosticParameter.SystemMetadata(scheduleInfo.TenantId));
			}
			else
			{
				removeScheduleInfo = false;
			}
			return null;
		}
		if (!scheduleInfo.SetStarting())
		{
			mostSpecificInstance.SendTraceTag("000002Z", Category.TaskScheduling, Verbosity.Verbose, "Task picked up on another NST: {0}", DiagnosticParameter.Oii(text));
			return null;
		}
		try
		{
			return scheduleInfo.TaskType switch
			{
				NavTaskSchedulerTaskType.Codeunit => (TaskRunner)TaskRunnerFactory.CreateTaskRunnerCodeUnit(scheduleInfo, tenant, text), 
				NavTaskSchedulerTaskType.System => TaskRunnerFactory.CreateTaskRunnerSystem(scheduleInfo, tenant, text, systemTaskFactory), 
				_ => throw new InvalidOperationException("Trying to create task runner of unknown task type."), 
			};
		}
		catch (Exception exception)
		{
			mostSpecificInstance.SendExceptionTag("0000034", Category.TaskScheduling, exception);
			LogWriterHelper.LogExceptionEvent(exception);
			return null;
		}
	}

	private List<NavTaskScheduleInfo> GetReadyToRunTasks(bool allowRunningSystemTasks)
	{
		HashSet<string> availableTenants = new HashSet<string>(from t in NavEnvironment.Instance.Tenants.AllTenants()
			where t.CanCreateSession(allowNonoperationalStateConnection: true, enableLogging: false, allowedDuringUpgradeState: false)
			select t.TenantSettings.Id);
		return new List<NavTaskScheduleInfo>(from task in rawList.Values
			where task.IsReady && availableTenants.Contains(task.TenantId) && (allowRunningSystemTasks || task.TaskType != NavTaskSchedulerTaskType.System)
			orderby NavSqlCommand.TimestampToInt64(task.Timestamp)
			select task);
	}

	internal void RunTask(TaskRunner taskRunner)
	{
		taskRunner.RunAsync();
	}

	private bool AllowRunningSystemTasks()
	{
		DateTime now = DateTime.Now;
		DateTime dateTime = now.Date.Add(systemTaskStartTime);
		DateTime dateTime2 = now.Date.Add(systemTaskEndTime);
		if (dateTime2 < dateTime)
		{
			dateTime2 = dateTime2.AddDays(1.0);
		}
		if (now >= dateTime)
		{
			return now < dateTime2;
		}
		return false;
	}

	private void RefreshTaskScheduleList()
	{
		totalTasksCounterMetric.LogValue(rawList.Count);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000S8", Category.TaskScheduling, () =>
		{
			byte[] timestamp = lastTimestamp;
			bool flag = aliveServerInstancesHasChanged;
			IEnumerable<NavTaskScheduleInfo> enumerable = SqlDml.Read(lastTimestamp, serviceInstanceId, flag);
			foreach (NavTaskScheduleInfo item in enumerable)
			{
				rawList.Remove(item.Id);
				switch (item.State)
				{
				case 0:
				case 1:
					if (item.NotBefore < DateTime.UtcNow)
					{
						rawList.Add(item.Id, item);
					}
					break;
				case 2:
				case 3:
					rawList.Add(item.Id, item);
					break;
				}
				timestamp = item.Timestamp;
			}
			lastTimestamp = timestamp;
			if (flag)
			{
				aliveServerInstancesHasChanged = false;
			}
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateNumberOfTotalPendingTasksCounter(rawList.Values.Count((NavTaskScheduleInfo s) => s.State == 1));
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateNumberOfTotalRunningTasksCounter(rawList.Values.Count((NavTaskScheduleInfo s) => s.State == 3));
		}, rethrow: false);
	}

	private void RemoveDoneTasks()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!(utcNow < nextDoneCleanup))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000002R", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Remove Done Tasks");
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000S9", Category.TaskScheduling, () =>
			{
				SqlDml.RemoveDoneTasks(utcNow);
			}, rethrow: false);
			nextDoneCleanup = AddDoneCleanupDelay(utcNow);
		}
	}

	private void RemoveCompletedTasks()
	{
		taskList.RemoveAll((TaskRunner t) =>
		{
			if (t.HasFinalStateBeenSet)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000002S", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Removed from TaskList: " + t.GetTraceText());
				return true;
			}
			if (t.IsFaulted)
			{
				return t.SetRetryOrDoneIfRetryLimitExceeded(throwOnError: false);
			}
			if (!t.IsCompleted)
			{
				return false;
			}
			try
			{
				t.SetDone(throwOnError: true);
				return true;
			}
			catch (NavBaseException ex) when (ex is NavSqlException || ex is NavCSideException)
			{
				return false;
			}
		});
		int num = taskList.Count((TaskRunner t) => t.IsCompleted);
		if (num > 0)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00008OC", Category.TaskScheduling, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Number of completed tasks which were not removed from TaskList: " + num);
		}
		UpdateTaskListPerformanceCounters();
	}

	private void RecoverLostTasks()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000002T", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Recovering losts tasks");
		SortedSet<int> activeInstances = getHeartbeat().KnownServerInstances;
		highestSeenServerInstanceId = Math.Max(highestSeenServerInstanceId, activeInstances.Max());
		foreach (int serverInstanceId in from e in rawList.Values
			where e.IsStartingOrRunning && e.ServiceInstanceId <= highestSeenServerInstanceId && !activeInstances.Contains(e.ServiceInstanceId)
			select e.ServiceInstanceId)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000002U", Category.TaskScheduling, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Recovering tasks for ServerInstanceId=" + serverInstanceId);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("00000SA", Category.TaskScheduling, () =>
			{
				SqlDml.RecoverTasksForCrashedServerInstance(serverInstanceId);
			}, rethrow: false);
		}
	}

	internal static void EnsureTenantScheduledTasksIsScheduledInAppDatabase(NavTenant tenant, string tenantId = null)
	{
		if (NavEnvironment.Topology.UseExternalTaskScheduler)
		{
			throw new InvalidOperationException("EnsureTenantScheduledTasksIsScheduledInAppDatabase method must not be called when external scheduler is enabled");
		}
		tenantId = tenantId ?? tenant.Id;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000002V", Category.TaskScheduling, Verbosity.Normal, "UpdateAppWithTenant Tasks: Tenant{0}", DiagnosticParameter.Oii(tenant.Id));
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnFailure("000002W", Category.TaskScheduling, () =>
		{
			List<ScheduledTaskInfo> tasksFromTenantDb = new List<ScheduledTaskInfo>();
			NavTaskSchedulerHelpers.RunCodeAsSystemSessionInTenant(tenant, (NavSession session, NavRecord record) =>
			{
				using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenant.Database))
				{
					if (record.ALFindSet(DataError.TrapError))
					{
						do
						{
							ScheduledTaskInfo item = new ScheduledTaskInfo
							{
								Id = record.GetFieldValueSafe(1, NavType.GUID).ToGuid(),
								NotBefore = record.GetFieldValueSafe(12, NavType.DateTime),
								IsReady = record.GetFieldValueSafe(11, NavType.Boolean).ToBoolean(),
								Type = NavTaskSchedulerTaskType.Codeunit
							};
							tasksFromTenantDb.Add(item);
						}
						while (record.ALNext() != 0);
					}
					IEnumerable<NavSystemScheduledTask> systemScheduledTasksInTenant = NavSqlSystemScheduledTask.GetSystemScheduledTasksInTenant(conScope, tenantId);
					tasksFromTenantDb.AddRange(systemScheduledTasksInTenant.Select((NavSystemScheduledTask task) => new ScheduledTaskInfo
					{
						Id = task.TaskId,
						NotBefore = NavDateTime.Create(task.NotBefore),
						IsReady = true,
						Type = NavTaskSchedulerTaskType.System
					}));
				}
				return true;
			}, allowAppsDisabledMode: true);
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
			foreach (ScheduledTaskInfo item2 in tasksFromTenantDb)
			{
				TaskAndTenantExists(item2.Id, tenantId, out var taskExists, out var _);
				if (!taskExists)
				{
					SqlDml.InsertTaskSchedule(navSqlConnectionScope, item2.Id, item2.IsReady ? 1 : 0, item2.NotBefore.IsZeroOrEmpty ? DateTime.UtcNow : item2.NotBefore.ToDateTime(), tenantId, item2.Type);
				}
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}, rethrow: false);
	}

	private int GetNumberfOfRunningTasks()
	{
		return taskList.Count((TaskRunner info) => !info.IsTaskFinished);
	}

	private bool TooManyRunningTasks()
	{
		return GetNumberfOfRunningTasks() >= MaxNumberOfRunningTasks;
	}

	private void UpdateTaskListPerformanceCounters()
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdateNumberOfRunningTasksCounter(GetNumberfOfRunningTasks());
		NavEnvironment.Instance.PerformanceCounterSetter.UpdateNumberOfAvailableTasksCounter(MaxNumberOfRunningTasks - GetNumberfOfRunningTasks());
		NavEnvironment.Instance.PerformanceCounterSetter.UpdateMaximumNumberOfRunningTasksCounter(MaxNumberOfRunningTasks);
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
			try
			{
				NavEnvironment.Instance.Tenants.TenantDismounting -= Tenants_TenantDismounting;
				NavEnvironment.Instance.Tenants.TenantDismounted -= Tenants_TenantDismounted;
				NavEnvironment.Instance.AliveServerInstancesChanged -= OnAliveServerInstancesChanged;
				scheduler.Stop();
			}
			catch (Exception)
			{
			}
		}
	}
}
