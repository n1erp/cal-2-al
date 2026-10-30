using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class DatabaseLocksDataProvider : EagerVirtualDataProvider
{
	private const string SelectDatabaseLocksCommand = "SELECT DISTINCT * FROM (SELECT dm_tran_locks.request_owner_id,dm_tran_locks.request_session_id,CASE WHEN resource_type = 'OBJECT' THEN OBJECT_NAME(dm_tran_locks.resource_associated_entity_id) ELSE OBJECT_NAME(partitions.OBJECT_ID) END AS object_name,dm_tran_locks.resource_type,dm_tran_locks.request_mode,dm_tran_locks.request_status FROM sys.dm_tran_locks LEFT JOIN sys.partitions ON partitions.hobt_id = dm_tran_locks.resource_associated_entity_id WHERE resource_associated_entity_id > 0 AND resource_database_id = DB_ID() AND request_mode not like 'I%' AND request_mode not like 'Sch-%') as locks  ORDER BY object_name";

	public static readonly DatabaseLocksDataProvider Instance = new DatabaseLocksDataProvider();

	public override int TableId => 2000000154;

	private DatabaseLocksDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByNo(4).FieldOptionMetadata;
		NCLOptionMetadata fieldOptionMetadata2 = MetaTable.GetFieldByNo(5).FieldOptionMetadata;
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		isOrderedByPrimaryKeyAscending = false;
		List<DatabaseLockDiagnosticEntry> databaseSnapshots = GetDatabaseSnapshots();
		foreach (DatabaseLockDiagnosticEntry item2 in databaseSnapshots)
		{
			ReadOnlyRecordBuffer item = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavBigInteger.Create(item2.SqlTransactionId), GetTruncatedTextValue(2, item2.TableName), GetTruncatedTextValue(3, item2.ResourceType), fieldOptionMetadata.CreateNavOption(ParseRequestModeOptionValue(item2.RequestMode)), fieldOptionMetadata2.CreateNavOption(ParseRequestStatusOptionValue(item2.RequestStatus)), GetTruncatedTextValue(6, item2.UserName), NavInteger.Create((int)item2.RequesterALObjectScope.ALObjectType), NavInteger.Create(item2.RequesterALObjectScope.ALObjectId), GetTruncatedTextValue(9, item2.RequesterALObjectScope.ALMethodScope), NavInteger.Create(item2.RequesterSqlServerSessionId));
			list.Add(item);
		}
		return list;
	}

	internal static List<DatabaseLockDiagnosticEntry> GetDatabaseSnapshots()
	{
		List<DatabaseLockDiagnosticEntry> databaseLockEntries = new List<DatabaseLockDiagnosticEntry>();
		NavSession currentSession = NavCurrentThread.Session;
		if (currentSession == null)
		{
			return databaseLockEntries;
		}
		HashSet<string> globalTableSqlNames = GetAllGlobalSqlTableNames();
		Dictionary<int, NavSession> transactionSessionMap = GetTransactionSessionMapForTenant(currentSession.Tenant);
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(currentSession.Database, TransactionType.Browse))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("SelectDatabaseLocksCommand", currentSession.Database), () => "SELECT DISTINCT * FROM (SELECT dm_tran_locks.request_owner_id,dm_tran_locks.request_session_id,CASE WHEN resource_type = 'OBJECT' THEN OBJECT_NAME(dm_tran_locks.resource_associated_entity_id) ELSE OBJECT_NAME(partitions.OBJECT_ID) END AS object_name,dm_tran_locks.resource_type,dm_tran_locks.request_mode,dm_tran_locks.request_status FROM sys.dm_tran_locks LEFT JOIN sys.partitions ON partitions.hobt_id = dm_tran_locks.resource_associated_entity_id WHERE resource_associated_entity_id > 0 AND resource_database_id = DB_ID() AND request_mode not like 'I%' AND request_mode not like 'Sch-%') as locks  ORDER BY object_name", (NavSqlCommand databaseLocksCommand) =>
			{
				using (databaseLocksCommand.ExecuteReader())
				{
					while (databaseLocksCommand.ReadNextRow())
					{
						try
						{
							AddDatabaseLockToRecordList(databaseLockEntries, databaseLocksCommand.Reader, currentSession, transactionSessionMap, globalTableSqlNames);
						}
						catch (Exception exception)
						{
							databaseLocksCommand.Connection.Database.Tenant.Diagnostics.SendExceptionTag("00000DU", Category.Runtime, exception, Verbosity.Warning);
						}
					}
					databaseLocksCommand.CloseReader(forceClose: false);
				}
				return 0;
			});
		}
		return databaseLockEntries;
	}

	private static void AddDatabaseLockToRecordList(List<DatabaseLockDiagnosticEntry> databaseLockEntries, SqlDataReader reader, NavSession currentSession, Dictionary<int, NavSession> sessionsMap, HashSet<string> globalTableSqlNames)
	{
		long @int = reader.GetInt64(0);
		int int2 = reader.GetInt32(1);
		string text = reader.GetString(2);
		bool flag = globalTableSqlNames.Contains(text);
		bool perCompanySchema = currentSession.Database.TenantDatabaseProperties.PerCompanySchema;
		string text2 = NavSqlStatementHelper.AppendCompanyNameAndSeparator(new StringBuilder(), currentSession.Database, currentSession.Company.CompanyNameToken).ToString();
		string tableName = ((perCompanySchema && !flag && text2.Length > 0) ? text.Replace(text2, string.Empty) : text);
		int requesterUserSessionId = 0;
		string userName = string.Empty;
		ALObjectScopeDiagnosticEntry requesterALObjectScope = new ALObjectScopeDiagnosticEntry();
		if (sessionsMap.TryGetValue(int2, out var value) && value != null && !value.IsDisposed)
		{
			if (value.Company.CompanyNameToken != currentSession.Company.CompanyNameToken && !flag)
			{
				return;
			}
			userName = value.UserDisplayName;
			requesterUserSessionId = value.Id;
			requesterALObjectScope = SessionContextHelper.GetALScope(value);
		}
		DatabaseLockDiagnosticEntry item = new DatabaseLockDiagnosticEntry
		{
			SqlTransactionId = @int,
			RequesterSqlServerSessionId = int2,
			TableName = tableName,
			ResourceType = reader.GetString(3),
			RequestMode = reader.GetString(4),
			RequestStatus = reader.GetString(5),
			UserName = userName,
			RequesterUserSessionId = requesterUserSessionId,
			RequesterALObjectScope = requesterALObjectScope
		};
		databaseLockEntries.Add(item);
	}

	private static Dictionary<int, NavSession> GetTransactionSessionMapForTenant(NavTenant tenant)
	{
		Dictionary<int, NavSession> dictionary = new Dictionary<int, NavSession>();
		try
		{
			IEnumerable<NavSession> activeSessions = tenant.ActiveSessions;
			foreach (NavSession item in activeSessions)
			{
				try
				{
					int? num = item.CurrentSqlConnectionScope?.Connection?.SqlServerSessionId;
					if (num > 0)
					{
						dictionary.Add(num.Value, item);
					}
				}
				catch (Exception exception)
				{
					tenant.Diagnostics.SendExceptionTag("00000DV", Category.Runtime, exception, Verbosity.Warning);
				}
			}
		}
		catch (Exception exception2)
		{
			tenant.Diagnostics.SendExceptionTag("00000DW", Category.Runtime, exception2, Verbosity.Warning);
		}
		return dictionary;
	}

	private static int ParseRequestModeOptionValue(string requestMode)
	{
		if (string.Equals(requestMode, "S", StringComparison.Ordinal))
		{
			return 0;
		}
		if (string.Equals(requestMode, "U", StringComparison.Ordinal))
		{
			return 1;
		}
		if (string.Equals(requestMode, "X", StringComparison.Ordinal))
		{
			return 2;
		}
		if (string.Equals(requestMode, "BU", StringComparison.Ordinal))
		{
			return 3;
		}
		return 4;
	}

	private static int ParseRequestStatusOptionValue(string requestStatus)
	{
		if (string.Equals(requestStatus, "GRANT", StringComparison.Ordinal))
		{
			return 0;
		}
		if (string.Equals(requestStatus, "CONVERT", StringComparison.Ordinal) || string.Equals(requestStatus, "LOW_PRIORITY_CONVERT", StringComparison.Ordinal))
		{
			return 1;
		}
		if (string.Equals(requestStatus, "WAIT", StringComparison.Ordinal) || string.Equals(requestStatus, "LOW_PRIORITY_WAIT", StringComparison.Ordinal))
		{
			return 2;
		}
		return 3;
	}

	private static HashSet<string> GetAllGlobalSqlTableNames()
	{
		return new HashSet<string>(from t in NavGlobal.NCLMetadata.GetSnapshotOfAllNonVirtualMetaTablesExceptApplicationTables()
			where !t.DataPerCompany
			select t.SqlTableName);
	}
}
