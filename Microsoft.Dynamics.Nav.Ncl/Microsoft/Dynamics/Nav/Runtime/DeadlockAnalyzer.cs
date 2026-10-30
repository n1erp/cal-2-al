using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DeadlockAnalyzer
{
	private const string AddTargetWithConfiguration = "ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)";

	private const string SetupSessionEventStatment = "IF NOT EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='{0}') BEGIN\r\n\t        CREATE EVENT SESSION [{0}] ON SERVER\r\n\t\t        ADD EVENT sqlserver.{1}({2})\r\n                ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)\r\n            ALTER EVENT SESSION [{0}] ON SERVER STATE = START\r\n        END\r\n        SELECT IIF(OBJECT_ID(N'deadlock_report_event_file_view', N'V') IS NULL, 0, 1)";

	private const string CleanupSessionEventStatment = "IF EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='{0}') BEGIN\r\n\t        DROP EVENT SESSION [{0}] ON SERVER\r\n        END";

	private const string SetupSessionEventStatmentAzure = "IF NOT EXISTS(SELECT * FROM sys.database_event_sessions WHERE name='{0}') BEGIN\r\n\t        CREATE EVENT SESSION [{0}] ON DATABASE\r\n\t\t        ADD EVENT sqlserver.database_xml_deadlock_report()\r\n                ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)\r\n            ALTER EVENT SESSION [{0}] ON DATABASE STATE = START\r\n        END";

	private const string CleanupSessionEventStatmentAzure = "IF EXISTS(SELECT * FROM sys.database_event_sessions WHERE name='{0}') BEGIN\r\n\t        DROP EVENT SESSION [{0}] ON DATABASE\r\n        END";

	private const string GetDeadlockReportXml = "\r\n            SELECT CAST([target_data] AS XML)AS Target_Data\r\n            FROM sys.dm_xe_session_targets AS xt\r\n            INNER JOIN sys.dm_xe_sessions AS xs\r\n            ON xs.address = xt.event_session_address\r\n            WHERE xs.name = N'{0}'\r\n            AND xt.target_name = N'ring_buffer'";

	private const string GetLatestDeadlockReportsStatement = "SELECT xed.value('@timestamp', 'datetime') as Creation_Date, xed.query('.') AS Extend_Event FROM\r\n            (\r\n            {0}\r\n            ) AS XML_Data\r\n\t        CROSS APPLY Target_Data.nodes('//event[@name=\"{1}\"]') AS XEventData(xed)\r\n\t            where CAST(xed.value('@timestamp', 'datetime') as  datetime) >= dateadd(MINUTE, -1, getutcdate())\r\n            ORDER BY Creation_Date DESC";

	private const string GetDeadlockReportXmlAzure = "\r\n            SELECT CAST([target_data] AS XML)AS Target_Data\r\n            FROM sys.dm_xe_database_session_targets AS xt\r\n            INNER JOIN sys.dm_xe_database_sessions AS xs\r\n            ON xs.address = xt.event_session_address\r\n            WHERE xs.name = N'{0}'\r\n            AND xt.target_name = N'ring_buffer'";

	public const string DeadlockReportViewName = "deadlock_report_event_file_view";

	private const string GetDeadlockReportXmlFromView = "\r\n            SELECT CAST([event_raw_data] AS XML) AS Target_Data FROM [dbo].[deadlock_report_event_file_view]";

	private const string ExtendedEventNamePerServer = "xml_deadlock_report";

	private const string ExtendedEventNamePerDatabase = "database_xml_deadlock_report";

	private IDeadlockReportLogTarget LogTarget { get; }

	public DeadlockAnalyzer(IDeadlockReportLogTarget logTarget)
	{
		LogTarget = logTarget;
	}

	public static void CleanupEventSessionForDatabase(NavDatabase database)
	{
		Func<string> commandTextProvider = () => string.Format(CultureInfo.InvariantCulture, (!database.IsAzureDatabase) ? "IF EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='{0}') BEGIN\r\n\t        DROP EVENT SESSION [{0}] ON SERVER\r\n        END" : "IF EXISTS(SELECT * FROM sys.database_event_sessions WHERE name='{0}') BEGIN\r\n\t        DROP EVENT SESSION [{0}] ON DATABASE\r\n        END", GetExtendedEventName(database));
		ExecuteScalar(database, commandTextProvider, out var _);
	}

	public static bool SetupEventSessionForDatabase(NavDatabase database)
	{
		if (!database.HasAlterAnyEventSessionPermission)
		{
			DiagnosticsResolver.GetMostSpecificInstance(database?.Tenant).SendTraceTag("00000F5", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Deadlock analyzer feature is enabled but a SQL user does not have 'ALTER ANY EVENT SESSION PERMISSION'.");
			return false;
		}
		bool flag = ExecuteScalar(commandTextProvider: database.IsAzureDatabase ? ((Func<string>)(() => string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.database_event_sessions WHERE name='{0}') BEGIN\r\n\t        CREATE EVENT SESSION [{0}] ON DATABASE\r\n\t\t        ADD EVENT sqlserver.database_xml_deadlock_report()\r\n                ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)\r\n            ALTER EVENT SESSION [{0}] ON DATABASE STATE = START\r\n        END", GetExtendedEventName(database)))) : ((Func<string>)(() => (!IsSqlServerDeadlockReportSupportedPerDatabase(database)) ? string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='{0}') BEGIN\r\n\t        CREATE EVENT SESSION [{0}] ON SERVER\r\n\t\t        ADD EVENT sqlserver.{1}({2})\r\n                ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)\r\n            ALTER EVENT SESSION [{0}] ON SERVER STATE = START\r\n        END\r\n        SELECT IIF(OBJECT_ID(N'deadlock_report_event_file_view', N'V') IS NULL, 0, 1)", GetExtendedEventName(database), "xml_deadlock_report", string.Empty) : string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.server_event_sessions WHERE name='{0}') BEGIN\r\n\t        CREATE EVENT SESSION [{0}] ON SERVER\r\n\t\t        ADD EVENT sqlserver.{1}({2})\r\n                ADD TARGET package0.ring_buffer(SET max_events_limit=(50),max_memory=(1024))\r\n\t\t        WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)\r\n            ALTER EVENT SESSION [{0}] ON SERVER STATE = START\r\n        END\r\n        SELECT IIF(OBJECT_ID(N'deadlock_report_event_file_view', N'V') IS NULL, 0, 1)", GetExtendedEventName(database), "database_xml_deadlock_report", string.Format(CultureInfo.InvariantCulture, "WHERE ([database_name]=N'{0}')", database.DatabaseName)))), database: database, result: out var result);
		database.ContainsDeadlockMonitorView = flag && result is int && (int)result > 0;
		return flag;
	}

	public void AnalyzeAndLog(NavSqlConnection connection)
	{
		if (connection == null)
		{
			throw new ArgumentException("Connection can't be null.", "connection");
		}
		try
		{
			NavSqlTransactionHistory history = connection.ConnectionPool.TransactionHistory;
			if (history == null || connection.CurrentTransactionId == 0L)
			{
				return;
			}
			ExecuteReaderForAllDeadlockReports(connection, (string xml) =>
			{
				DeadlockXmlReport report = CreateDeadlockReportFromXmlString(xml);
				if (CheckIfReportContainsVictimTrasaction(report, connection.CurrentTransactionId))
				{
					LogTarget.LogReport(connection.Database, report, history);
					return true;
				}
				return false;
			}, () =>
			{
				DeadlockReportNotFoundAction(LogTarget, connection);
			});
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance(connection?.Database?.Tenant).SendExceptionTag("00000DG", Category.Sql, exception);
		}
	}

	public static void OnDeadlockException(NavSqlConnection connection, SqlException exception)
	{
		if (exception.Number == 1205)
		{
			NavEnvironment.Instance.DeadlockAnalyzer.AnalyzeAndLog(connection);
		}
	}

	private static void DeadlockReportNotFoundAction(IDeadlockReportLogTarget target, NavSqlConnection connection)
	{
		NavDatabase database = connection.Database;
		if (!database.ContainsDeadlockMonitorView)
		{
			DiagnosticsResolver.GetMostSpecificInstance(database?.Tenant).SendTraceTag("00000DF", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Could not find a deadlock report.");
			return;
		}
		string path = "deadlock_transactions" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", DateTimeFormatInfo.InvariantInfo) + ".csv";
		string fullPath = Path.GetFullPath(Path.Combine(NavEnvironment.Instance.TemporaryPathHelper.TempPath, path));
		try
		{
			target.WriteTransactionHistoryToFile(fullPath, connection.ConnectionPool.TransactionHistory);
			DiagnosticsResolver.GetMostSpecificInstance(database?.Tenant).SendTraceTag("00008X9", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Could not find deadlock report containing current transaction {0}. List of active transactions were written to {1}.", connection.CurrentTransactionId, fullPath);
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance(database?.Tenant).SendExceptionTag("00008X8", Category.Sql, exception);
		}
	}

	private static void ExecuteReaderForAllDeadlockReports(NavSqlConnection connection, Func<string, bool> action, Action reportNotFoundAction)
	{
		NavDatabase database = connection.Database;
		string deadlockReportSelectStatement = GetDeadlockReportSelectStatement(database);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		using (navSqlCommand.ExecuteReader(deadlockReportSelectStatement))
		{
			while (navSqlCommand.ReadNextRow())
			{
				SqlXml sqlXml = navSqlCommand.Reader.GetSqlXml(1);
				if (action(sqlXml.Value))
				{
					navSqlCommand.CloseReader(forceClose: false);
					return;
				}
			}
			reportNotFoundAction();
		}
	}

	private static string GetDeadlockReportSelectStatement(NavDatabase database)
	{
		return string.Format(arg0: (!database.ContainsDeadlockMonitorView) ? string.Format(CultureInfo.InvariantCulture, database.IsAzureDatabase ? "\r\n            SELECT CAST([target_data] AS XML)AS Target_Data\r\n            FROM sys.dm_xe_database_session_targets AS xt\r\n            INNER JOIN sys.dm_xe_database_sessions AS xs\r\n            ON xs.address = xt.event_session_address\r\n            WHERE xs.name = N'{0}'\r\n            AND xt.target_name = N'ring_buffer'" : "\r\n            SELECT CAST([target_data] AS XML)AS Target_Data\r\n            FROM sys.dm_xe_session_targets AS xt\r\n            INNER JOIN sys.dm_xe_sessions AS xs\r\n            ON xs.address = xt.event_session_address\r\n            WHERE xs.name = N'{0}'\r\n            AND xt.target_name = N'ring_buffer'", GetExtendedEventName(database)) : "\r\n            SELECT CAST([event_raw_data] AS XML) AS Target_Data FROM [dbo].[deadlock_report_event_file_view]", provider: CultureInfo.InvariantCulture, format: "SELECT xed.value('@timestamp', 'datetime') as Creation_Date, xed.query('.') AS Extend_Event FROM\r\n            (\r\n            {0}\r\n            ) AS XML_Data\r\n\t        CROSS APPLY Target_Data.nodes('//event[@name=\"{1}\"]') AS XEventData(xed)\r\n\t            where CAST(xed.value('@timestamp', 'datetime') as  datetime) >= dateadd(MINUTE, -1, getutcdate())\r\n            ORDER BY Creation_Date DESC", arg1: IsSqlServerDeadlockReportSupportedPerDatabase(database) ? "database_xml_deadlock_report" : "xml_deadlock_report");
	}

	private static bool ExecuteScalar(NavDatabase database, Func<string> commandTextProvider, out object result)
	{
		result = null;
		try
		{
			string cmdText = commandTextProvider();
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
			{
				if (navSqlConnectionScope.Connection.CurrentTransaction != null)
				{
					navSqlConnectionScope.Connection.EndTransaction(NavSqlEndTransactionType.Commit);
				}
				SqlCommand sqlCommand = new SqlCommand(cmdText, navSqlConnectionScope.Connection.Connection);
				result = sqlCommand.ExecuteScalar();
			}
			return true;
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance(database?.Tenant).SendExceptionTag("00000DH", Category.Sql, exception);
		}
		return false;
	}

	private static bool CheckIfReportContainsVictimTrasaction(DeadlockXmlReport report, long transactionId)
	{
		return report.ProcessList.Any((DeadlockXmlReport.Process p) => p.OwnerId == transactionId && report.VictimList.Contains(p.Id));
	}

	private static DeadlockXmlReport CreateDeadlockReportFromXmlString(string xml)
	{
		DeadlockXmlReport deadlockXmlReport = new DeadlockXmlReport();
		deadlockXmlReport.Load(xml);
		return deadlockXmlReport;
	}

	private static bool IsSqlServerDeadlockReportSupportedPerDatabase(NavDatabase database)
	{
		return database.SqlServerProperties.MajorProductVersion >= 12;
	}

	private static string GetExtendedEventName(NavDatabase database)
	{
		return ((IsSqlServerDeadlockReportSupportedPerDatabase(database) || database.IsAzureDatabase) ? database.DatabaseName : "DynamicsNav") + "_deadlock_monitor";
	}
}
