using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.TableLocksLogger;

internal class TableLocksLogger
{
	private const string TableLocksInfoStatment = "SELECT\r\n            lock_sess.request_session_id,\r\n            sess.login_time,\r\n            sess.last_request_start_time,\r\n            sess.last_request_end_time,\r\n            sess.host_name,\r\n            sess.program_name,\r\n            sess.login_name,\r\n            sess.status\r\n        FROM\r\n            (SELECT DISTINCT \r\n                locks.request_session_id\r\n            FROM\r\n                (SELECT *\r\n                FROM sys.objects\r\n                WHERE name = '{0}') as obj\r\n            INNER JOIN\r\n                sys.partitions as partns\r\n            ON obj.object_id = partns.object_id\r\n            INNER JOIN\r\n                sys.dm_tran_locks as locks\r\n            ON locks.resource_associated_entity_id = partns.hobt_id \r\n               AND locks.request_status = 'GRANT') as lock_sess\r\n        INNER JOIN\r\n            sys.dm_exec_sessions as sess\r\n        ON sess.session_id = lock_sess.request_session_id";

	public static void AnalyzeAndLog(NavDatabase database, string tableName)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		if (string.IsNullOrEmpty(tableName))
		{
			throw new ArgumentException("The argument is null or empty.", "tableName");
		}
		try
		{
			string statement = string.Format(CultureInfo.InvariantCulture, "SELECT\r\n            lock_sess.request_session_id,\r\n            sess.login_time,\r\n            sess.last_request_start_time,\r\n            sess.last_request_end_time,\r\n            sess.host_name,\r\n            sess.program_name,\r\n            sess.login_name,\r\n            sess.status\r\n        FROM\r\n            (SELECT DISTINCT \r\n                locks.request_session_id\r\n            FROM\r\n                (SELECT *\r\n                FROM sys.objects\r\n                WHERE name = '{0}') as obj\r\n            INNER JOIN\r\n                sys.partitions as partns\r\n            ON obj.object_id = partns.object_id\r\n            INNER JOIN\r\n                sys.dm_tran_locks as locks\r\n            ON locks.resource_associated_entity_id = partns.hobt_id \r\n               AND locks.request_status = 'GRANT') as lock_sess\r\n        INNER JOIN\r\n            sys.dm_exec_sessions as sess\r\n        ON sess.session_id = lock_sess.request_session_id", tableName);
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database))
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				using (navSqlCommand.ExecuteReader(statement))
				{
					while (navSqlCommand.ReadNextRow())
					{
						int @int = navSqlCommand.Reader.GetInt32(0);
						DateTime dateTime = navSqlCommand.Reader.GetDateTime(1);
						DateTime dateTime2 = navSqlCommand.Reader.GetDateTime(2);
						DateTime dateTime3 = navSqlCommand.Reader.GetDateTime(3);
						string content = navSqlCommand.Reader.GetString(4);
						string content2 = navSqlCommand.Reader.GetString(5);
						string content3 = navSqlCommand.Reader.GetString(6);
						string content4 = navSqlCommand.Reader.GetString(7);
						database.Tenant.Diagnostics.SendTraceTag("000016X", Category.Sql, Verbosity.Warning, "Table {0} is locked by: sessionId:{1}, loginTime:{2}, lastRequestStartTime:{3}, lastRequestEndTime:{4}, hostName:{5}, programName:{6}, loginName:{7}, status:{8}", DiagnosticParameter.SystemMetadata(tableName), DiagnosticParameter.SystemMetadata(@int), DiagnosticParameter.SystemMetadata(dateTime), DiagnosticParameter.SystemMetadata(dateTime2), DiagnosticParameter.SystemMetadata(dateTime3), DiagnosticParameter.SystemMetadata(content), DiagnosticParameter.SystemMetadata(content2), DiagnosticParameter.Euii(content3), DiagnosticParameter.SystemMetadata(content4));
					}
				}
			}
			database.Tenant.Diagnostics.SendTraceTag("000016Y", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Finished logging existing locks for {0}", tableName);
		}
		catch (Exception exception)
		{
			database.Tenant.Diagnostics.SendExceptionTag("000016E", Category.Sql, exception);
		}
	}
}
