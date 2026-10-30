using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class SessionEventTableHandler
{
	private class SessionEventData
	{
		public Guid UserSID { get; private set; }

		public int SessionId { get; private set; }

		public ClientType ClientType { get; private set; }

		public NavDatabase Database { get; private set; }

		public string ClientComputerName { get; private set; }

		public string UserID { get; private set; }

		public CompanyId? CompanyId { get; private set; }

		public string SessionComment { get; private set; }

		public Guid SessionUniqueId { get; private set; }

		public SessionEventData(Guid userSid, int sessionId, ClientType clientType, NavDatabase database, string clientComputerName, string userId, string sessionComment, Guid sessionUniqueId, CompanyId? companyId)
		{
			UserSID = userSid;
			SessionId = sessionId;
			ClientType = clientType;
			Database = database;
			ClientComputerName = clientComputerName;
			UserID = userId;
			CompanyId = companyId;
			SessionComment = sessionComment;
			SessionUniqueId = sessionUniqueId;
		}
	}

	private static SemaphoreSlim logoutSemaphore = new SemaphoreSlim(10, 10);

	public const int LoginEventType = 0;

	public const int LogoutEventType = 1;

	private const string MaintenanceLockName = "SessionEventTableMaintenance";

	private static readonly string NonInteractiveClientTypes = "(" + Convert.ToInt16(ClientType.Background, CultureInfo.InvariantCulture) + ", " + Convert.ToInt16(ClientType.WebServiceClient, CultureInfo.InvariantCulture) + ")";

	private static readonly string DeleteSQLStatement = "\r\n                declare @result int;\r\n                EXEC @result = sp_getapplock @Resource = '{1}', @LockMode ='Exclusive', @LockTimeout=0;\r\n                IF @result = 0 DELETE FROM [{0}].[dbo].[Session Event]\r\n                  WHERE NOT EXISTS\r\n                  (SELECT 1 FROM\r\n                     [{0}].[dbo].[Active Session] AS acs WITH(NOLOCK) \r\n                      WHERE\r\n                          {3} acs.[User SID] = [{0}].[dbo].[Session Event].[User SID]                \r\n                          AND acs.[Server Instance ID] = [{0}].[dbo].[Session Event].[Server Instance ID]\r\n                          AND acs.[Session ID] = [{0}].[dbo].[Session Event].[Session ID]\r\n                          AND acs.[Session Unique ID] = [{0}].[dbo].[Session Event].[Session Unique ID]\r\n                          AND acs.[Login Datetime] = [{0}].[dbo].[Session Event].[Event Datetime])\r\n                  AND [Event Datetime] < DATEADD(SECOND, -@0, GETUTCDATE()) OR ([Client Type] IN " + NonInteractiveClientTypes + " AND [Event Datetime] < DATEADD(SECOND, -@1, GETUTCDATE()))\r\n                  {2};";

	private static readonly string DeleteSQLStatementWhenActiveSessionInMemory = "\r\n                declare @result int;\r\n                EXEC @result = sp_getapplock @Resource = '{1}', @LockMode ='Exclusive', @LockTimeout=0;\r\n                IF @result = 0 DELETE FROM [{0}].[dbo].[Session Event]\r\n                  WHERE [Event Datetime] < DATEADD(SECOND, -@0, GETUTCDATE()) OR ([Client Type] IN " + NonInteractiveClientTypes + " AND [Event Datetime] < DATEADD(SECOND, -@1, GETUTCDATE()))\r\n                  {2};";

	private const string InsertLogoutForDeadSessions = "declare @result int;\r\n            EXEC @result = sp_getapplock @Resource = '{2}', @LockMode ='Exclusive', @LockTimeout=0;IF @result = 0 INSERT INTO [{0}].dbo.[Session Event] ({3}[User SID],[Server Instance ID],[Session ID],[Event Type],[Event Datetime],[Client Type],[Database Name],[Client Computer Name],[User ID],[Comment],[Session Unique ID]) SELECT {4}se.[User SID],se.[Server Instance ID],se.[Session ID],4 AS [Event Type],GETUTCDATE() AS [Event Datetime],se.[Client Type],se.[Database Name],se.[Client Computer Name],se.[User ID],'Force closed by server' AS [Comment],se.[Session Unique ID] FROM [{0}].dbo.[Active Session] AS se WITH(NOLOCK) WHERE se.[Server Instance ID] NOT IN ({1}){5} AND se.[Login Datetime] < @0;SELECT @@ROWCOUNT";

	private const int MajorTriggerVersionMultiplier = 10000;

	private static readonly int CurrentMajorTriggerVersion = 4;

	private const int CurrentMinorTriggerVersion = 4;

	private static readonly int CreateSessionEventInsertTriggerVersion = CurrentMajorTriggerVersion * 10000 + 4;

	private const string TriggerVersionPrefix = "-- Version: ";

	private const string TriggerVersionSuffix = ";";

	private const string TriggerVersionExtendedPropertyName = "MS_Version";

	private const string GetSessionEventExtendedProperty = "SELECT value FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Session Event', N'U' ) AND name = 'MS_Version'";

	private static readonly string CreateVersionProperty = "exec sp_addextendedproperty @name = 'MS_Version', @value = '" + CreateSessionEventInsertTriggerVersion.ToString(CultureInfo.InvariantCulture) + "',@level0type = N'SCHEMA', @level0name = 'dbo',  @level1type = N'TABLE',  @level1name = 'Session Event';";

	private static readonly string CreateSessionEventInsertTrigger = "CREATE TRIGGER [dbo].[RemoveOnLogoutActiveSession] ON [dbo].[Session Event] " + Environment.NewLine + "AFTER INSERT " + Environment.NewLine + "AS " + Environment.NewLine + "BEGIN " + Environment.NewLine + "-- Version: {2}" + CreateSessionEventInsertTriggerVersion.ToString(CultureInfo.InvariantCulture) + ";" + Environment.NewLine + "  DELETE ase FROM [dbo].[Active Session] AS ase " + Environment.NewLine + "    INNER JOIN INSERTED AS se ON " + Environment.NewLine + "      (({0}        se.[Client Type] <> 2 AND " + Environment.NewLine + "        se.[Event Type] > 0 AND" + Environment.NewLine + "        ase.[Server Instance ID] = se.[Server Instance ID] AND " + Environment.NewLine + "        ase.[Session ID] = se.[Session ID] AND " + Environment.NewLine + "        ase.[Session Unique ID] = se.[Session Unique ID]) OR " + Environment.NewLine + "       ({0}        se.[Client Type] = 2 AND " + Environment.NewLine + "        (se.[Event Type] = 0 OR se.[Event Type] = 4) AND " + Environment.NewLine + "        ase.[User SID] = se.[User SID] AND " + Environment.NewLine + "        ase.[Session ID] < 0) OR " + Environment.NewLine + "       ({0}        se.[Client Type] IN (0,1,7) AND " + Environment.NewLine + "        se.[Event Type] > 0 AND " + Environment.NewLine + "        ase.[User SID] = se.[User SID] AND " + Environment.NewLine + "        ase.[Session ID] < 0)); " + Environment.NewLine + "  UPDATE ase " + Environment.NewLine + "    SET ase.[Session ID] = -ase.[Session ID] FROM [dbo].[Active Session] AS ase " + Environment.NewLine + "      INNER JOIN INSERTED AS se ON " + Environment.NewLine + "        {1}        se.[Client Type] = 2 AND " + Environment.NewLine + "        se.[Event Type] > 0 AND " + Environment.NewLine + "        ase.[Server Instance ID] = se.[Server Instance ID] AND " + Environment.NewLine + "        ase.[Session ID] > 0 AND " + Environment.NewLine + "        ase.[Session ID] = se.[Session ID] AND " + Environment.NewLine + "        ase.[Session Unique ID] = se.[Session Unique ID] " + Environment.NewLine + "END;";

	private static readonly string CreateSessionEventDeleteTrigger = "CREATE TRIGGER [dbo].[DeleteActiveSession] ON [dbo].[Session Event] " + Environment.NewLine + "AFTER DELETE " + Environment.NewLine + "AS " + Environment.NewLine + "BEGIN " + Environment.NewLine + "  DELETE ase FROM [dbo].[Active Session] AS ase " + Environment.NewLine + "  INNER JOIN DELETED AS se " + Environment.NewLine + "  ON " + Environment.NewLine + "  ase.[Server Instance ID] = se.[Server Instance ID] " + Environment.NewLine + "  AND ase.[Session ID] = se.[Session ID] " + Environment.NewLine + "  AND ase.[Session Unique ID] = se.[Session Unique ID] " + Environment.NewLine + "  AND ase.[Login Datetime] = se.[Event Datetime] " + Environment.NewLine + "  {0}END;";

	private const string DropSessionEventTriggers = "IF EXISTS(SELECT null FROM fn_listextendedproperty ('MS_Version', 'SCHEMA', 'dbo', 'TABLE', 'Session Event', NULL, NULL))exec sp_dropextendedproperty @name = 'MS_Version',@level0type = N'SCHEMA', @level0name = 'dbo',@level1type = N'TABLE',  @level1name = 'Session Event';IF OBJECT_ID ('[{0}].[dbo].[RemoveOnLogoutActiveSession]') != 0   DROP TRIGGER [dbo].[RemoveOnLogoutActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[MaintainActiveSession]') != 0   DROP TRIGGER [dbo].[MaintainActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[DeleteActiveSession]') != 0   DROP TRIGGER [dbo].DeleteActiveSession;";

	private const string TestSessionEventTriggers = "IF OBJECT_ID ('[{0}].[dbo].[RemoveOnLogoutActiveSession]') != 0 AND OBJECT_ID ('[{0}].[dbo].[DeleteActiveSession]') != 0 SELECT 1 ELSE SELECT 0";

	private const string InsertIntoSessionEvent = "DECLARE @currentTime DATETIME SELECT @currentTime = GETUTCDATE() INSERT INTO [{0}].[dbo].[Session Event](  {1}  [User SID],  [Server Instance ID],  [Session ID],  [Event Type],  [Event Datetime],  [Client Type],  [Database Name],  [Client Computer Name],  [User ID],  [Comment],  [Session Unique ID]) VALUES({2}@1,@2,@3,@4,@currentTime,@6,@7,@8,@9,@10,@11)SELECT @currentTime";

	private const string InsertIntoActiveSession = "  INSERT INTO [{0}].[dbo].[Active Session]   (  {1}    [User SID],    [Server Instance ID],    [Session ID],    [Server Instance Name],    [Server Computer Name],    [User ID],    [Client Type],    [Client Computer Name],    [Login Datetime],    [Database Name],    [Session Unique ID]  ) VALUES({2}@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11)";

	private const string IsSessionActiveCommand = "SELECT TOP 1 [Session ID] FROM [{0}].[dbo].[Active Session] WITH (NOLOCK) WHERE {1} [Session ID] = @1 AND [Session Unique ID] = @2;";

	private const string RemoveSustainedWebServiceSessionCommandName = "RemoveSustainedWebServiceSessionCommandName";

	private const string RemoveSustainedWebServiceSessionCommand = "DELETE FROM [{0}].dbo.[Active Session] WHERE ({1} [Session ID] = @3)";

	private const string InsertSessionEventCommandName = "InsertSessionEventCommand";

	private const string InsertActiveSessionCommandName = "InsertActiveSessionCommand";

	internal static void EnsureSessionEventTriggersExists(NavDatabase database)
	{
		if (!database.IsCurrentVersion)
		{
			return;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update);
		using NavSqlDatabaseLock navSqlDatabaseLock = NavSqlDatabaseLock.LockResource(navSqlConnectionScope, "SessionEventTableHandler");
		if (navSqlDatabaseLock.IsLockAcquired())
		{
			EnsureSessionEventTriggersExists(navSqlConnectionScope);
		}
	}

	internal static void EnsureSessionEventTriggersExists(NavSqlConnectionScope scope)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID ('[{0}].[dbo].[RemoveOnLogoutActiveSession]') != 0 AND OBJECT_ID ('[{0}].[dbo].[DeleteActiveSession]') != 0 SELECT 1 ELSE SELECT 0", scope.Connection.Database.DatabaseName));
		int num = 3;
		while (num-- > 0)
		{
			try
			{
				if ((int)navSqlCommand.ExecuteScalar() != 0)
				{
					if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
					{
						DropTriggerCore(scope);
						break;
					}
					if (CheckTriggersCore(scope))
					{
						break;
					}
				}
				else if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
				{
					break;
				}
				CreateTriggerCore(scope);
				break;
			}
			catch (NavSqlException ex)
			{
				if (ex.ErrorNumber != 2714 || num == 0)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000XX", Category.Sql, ex);
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XY", Category.Sql, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "A SQL exception occured during trigger check. {0} attempts remaining", num - 1);
			}
		}
	}

	private static bool CheckTriggersCore(NavSqlConnectionScope scope)
	{
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection))
		{
			string text = (string)navSqlCommand.ExecuteScalar("SELECT value FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'Session Event', N'U' ) AND name = 'MS_Version'");
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			bool flag = false;
			if (text.StartsWith("S", StringComparison.Ordinal))
			{
				flag = true;
				text = text.Substring(1);
			}
			int num = int.Parse(text, CultureInfo.InvariantCulture);
			int num2 = num / 10000;
			if (num2 > CurrentMajorTriggerVersion)
			{
				NavSqlException ex = new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DatabaseVersionTooNew, scope.Connection.Database.DatabaseName, scope.Connection.Database.DatabaseServer.ServerName, ProductNames.ProductName));
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000XZ", Category.Sql, ex);
				throw ex;
			}
			if (num >= CreateSessionEventInsertTriggerVersion && flag == scope.Connection.Database.IsSharedTenantSchema)
			{
				return true;
			}
		}
		return false;
	}

	private static void DropTriggerCore(NavSqlConnectionScope scope)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection);
		navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF EXISTS(SELECT null FROM fn_listextendedproperty ('MS_Version', 'SCHEMA', 'dbo', 'TABLE', 'Session Event', NULL, NULL))exec sp_dropextendedproperty @name = 'MS_Version',@level0type = N'SCHEMA', @level0name = 'dbo',@level1type = N'TABLE',  @level1name = 'Session Event';IF OBJECT_ID ('[{0}].[dbo].[RemoveOnLogoutActiveSession]') != 0   DROP TRIGGER [dbo].[RemoveOnLogoutActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[MaintainActiveSession]') != 0   DROP TRIGGER [dbo].[MaintainActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[DeleteActiveSession]') != 0   DROP TRIGGER [dbo].DeleteActiveSession;", scope.Connection.Database.DatabaseName));
		navSqlCommand.ExecuteNonQuery();
	}

	private static void CreateTriggerCore(NavSqlConnectionScope scope)
	{
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection))
		{
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "IF EXISTS(SELECT null FROM fn_listextendedproperty ('MS_Version', 'SCHEMA', 'dbo', 'TABLE', 'Session Event', NULL, NULL))exec sp_dropextendedproperty @name = 'MS_Version',@level0type = N'SCHEMA', @level0name = 'dbo',@level1type = N'TABLE',  @level1name = 'Session Event';IF OBJECT_ID ('[{0}].[dbo].[RemoveOnLogoutActiveSession]') != 0   DROP TRIGGER [dbo].[RemoveOnLogoutActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[MaintainActiveSession]') != 0   DROP TRIGGER [dbo].[MaintainActiveSession]; IF OBJECT_ID ('[{0}].[dbo].[DeleteActiveSession]') != 0   DROP TRIGGER [dbo].DeleteActiveSession;", scope.Connection.Database.DatabaseName));
			navSqlCommand.ExecuteNonQuery();
		}
		using (NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(scope.Connection))
		{
			string commandText = ((!scope.Connection.Database.IsSharedTenantSchema) ? string.Format(CultureInfo.InvariantCulture, CreateSessionEventDeleteTrigger, string.Empty) : string.Format(CultureInfo.InvariantCulture, CreateSessionEventDeleteTrigger, "AND ase.\"$companyId\" = se.\"$companyId\"" + Environment.NewLine));
			navSqlCommand2.SetCommandText(commandText);
			navSqlCommand2.ExecuteNonQuery();
		}
		using (NavSqlCommand navSqlCommand3 = NavSqlCommand.Create(scope.Connection))
		{
			string commandText2 = ((!scope.Connection.Database.IsSharedTenantSchema) ? string.Format(CultureInfo.InvariantCulture, CreateSessionEventInsertTrigger, string.Empty, string.Empty, string.Empty) : string.Format(CultureInfo.InvariantCulture, CreateSessionEventInsertTrigger, "se.\"$companyId\" = ase.\"$companyId\" AND " + Environment.NewLine, "ase.\"$companyId\" = se.\"$companyId\" AND " + Environment.NewLine, "S"));
			navSqlCommand3.SetCommandText(commandText2);
			navSqlCommand3.ExecuteNonQuery();
		}
		using NavSqlCommand navSqlCommand4 = NavSqlCommand.Create(scope.Connection);
		navSqlCommand4.ExecuteNonQuery(CreateVersionProperty);
	}

	internal static bool IsSessionActive(NavSession session)
	{
		if (!session.Tenant.Database.IsCurrentVersion)
		{
			return true;
		}
		if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
		{
			return ActiveSessionDataProvider.Instance.IsSessionActive(session.UniqueSessionId);
		}
		CompanyId? companyId = session.Tenant.Database.GetCompanyIdForSystemTables(2000000110);
		string companyIdWhereText = (companyId.HasValue ? "\"$companyId\" = @3 AND" : string.Empty);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(session.Database, TransactionType.Update);
		return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(session.Database, "SELECT TOP 1 [Session ID] FROM [{0}].[dbo].[Active Session] WITH (NOLOCK) WHERE {1} [Session ID] = @1 AND [Session Unique ID] = @2;", companyId.HasValue.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 [Session ID] FROM [{0}].[dbo].[Active Session] WITH (NOLOCK) WHERE {1} [Session ID] = @1 AND [Session Unique ID] = @2;", session.Database.DatabaseName, companyIdWhereText), (NavSqlCommand command) =>
		{
			command.Command.Parameters.AddRange(new SqlParameter[2]
			{
				new SqlParameter("@1", SqlDbType.Int),
				new SqlParameter("@2", SqlDbType.UniqueIdentifier)
			});
			if (companyId.HasValue)
			{
				command.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
			}
		}, (NavSqlCommand command) =>
		{
			command.Command.Parameters[0].Value = session.Id;
			command.Command.Parameters[1].Value = session.UniqueSessionId;
			if (companyId.HasValue)
			{
				command.Command.Parameters[2].Value = companyId.Value;
			}
		}, (NavSqlCommand command) => command.ExecuteScalar() != null);
	}

	private static void InsertLogoutTaskWithRetryForever(SessionEventData sessionEventData, bool retryEvent = false)
	{
		try
		{
			if (retryEvent)
			{
				logoutSemaphore.Wait();
			}
			InsertSessionEventLogoutRecordNoRetries(sessionEventData);
		}
		catch (Exception ex) when (ex is NavCSideException || ex is NavSqlException)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001GL", Category.Runtime, ex, Verbosity.Warning);
			Task.Delay(30000).ContinueWith((Task task) =>
			{
				InsertLogoutTaskWithRetryForever(sessionEventData, retryEvent: true);
			});
		}
		finally
		{
			if (retryEvent)
			{
				logoutSemaphore.Release();
			}
		}
	}

	internal static void InsertSessionEventRecord(int eventType, NavSession session)
	{
		if (session.Database.IsDisposed || session.Database.DatabaseType != NavDatabaseType.Tenant || !session.Database.IsCurrentVersion)
		{
			return;
		}
		SessionEventData eventDataFromSession = GetEventDataFromSession(session);
		if (eventType == 0)
		{
			if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
			{
				ActiveSessionDataProvider.Instance.CreateSession(eventDataFromSession.UserSID, eventDataFromSession.SessionId, eventDataFromSession.ClientType, eventDataFromSession.Database, eventDataFromSession.ClientComputerName, eventDataFromSession.UserID, eventDataFromSession.SessionUniqueId, eventDataFromSession.CompanyId);
			}
			else
			{
				InsertSessionEventLoginRecordNoRetries(session);
			}
		}
		else if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
		{
			ActiveSessionDataProvider.Instance.RemoveSession(session.UniqueSessionId);
			if (!string.IsNullOrEmpty(session.SessionComment))
			{
				InsertLogoutTaskWithRetryForever(eventDataFromSession);
			}
		}
		else
		{
			InsertLogoutTaskWithRetryForever(eventDataFromSession);
		}
	}

	private static void SetupSessionEventParameters(NavSqlCommand command, CompanyId? companyId)
	{
		command.Command.Parameters.AddRange(new SqlParameter[10]
		{
			new SqlParameter(NavSqlStatementHelper.ParameterName(1), SqlDbType.UniqueIdentifier),
			new SqlParameter(NavSqlStatementHelper.ParameterName(2), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(3), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(4), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(6), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(7), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(8), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(9), SqlDbType.VarChar, 132),
			new SqlParameter(NavSqlStatementHelper.ParameterName(10), SqlDbType.VarChar, 132),
			new SqlParameter(NavSqlStatementHelper.ParameterName(11), SqlDbType.UniqueIdentifier)
		});
		if (companyId.HasValue)
		{
			command.Command.Parameters.Add(new SqlParameter(NavSqlStatementHelper.ParameterName(12), SqlDbType.Int));
		}
	}

	private static void SetupActiveSessionParameters(NavSqlCommand command, CompanyId? companyId)
	{
		command.Command.Parameters.AddRange(new SqlParameter[11]
		{
			new SqlParameter(NavSqlStatementHelper.ParameterName(1), SqlDbType.UniqueIdentifier),
			new SqlParameter(NavSqlStatementHelper.ParameterName(2), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(3), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(4), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(5), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(6), SqlDbType.VarChar, 132),
			new SqlParameter(NavSqlStatementHelper.ParameterName(7), SqlDbType.Int),
			new SqlParameter(NavSqlStatementHelper.ParameterName(8), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(9), SqlDbType.DateTime),
			new SqlParameter(NavSqlStatementHelper.ParameterName(10), SqlDbType.VarChar, 250),
			new SqlParameter(NavSqlStatementHelper.ParameterName(11), SqlDbType.UniqueIdentifier)
		});
		if (companyId.HasValue)
		{
			command.Command.Parameters.Add(new SqlParameter(NavSqlStatementHelper.ParameterName(12), SqlDbType.Int));
		}
	}

	private static void InsertSessionEventLoginRecordNoRetries(NavSession session)
	{
		CompanyId? companyId = session.Tenant.Database.GetCompanyIdForSystemTables(2000000111);
		string companyIdColumnInsertText = (companyId.HasValue ? "\"$companyId\"," : string.Empty);
		string companyIdInsertValue = (companyId.HasValue ? "@12," : string.Empty);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(session.Database, TransactionType.Update);
		DateTime eventTime = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(session.Database, "InsertSessionEventCommand", companyId.HasValue.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, "DECLARE @currentTime DATETIME SELECT @currentTime = GETUTCDATE() INSERT INTO [{0}].[dbo].[Session Event](  {1}  [User SID],  [Server Instance ID],  [Session ID],  [Event Type],  [Event Datetime],  [Client Type],  [Database Name],  [Client Computer Name],  [User ID],  [Comment],  [Session Unique ID]) VALUES({2}@1,@2,@3,@4,@currentTime,@6,@7,@8,@9,@10,@11)SELECT @currentTime", session.Database.DatabaseName, companyIdColumnInsertText, companyIdInsertValue), (NavSqlCommand command) =>
		{
			SetupSessionEventParameters(command, companyId);
		}, (NavSqlCommand command) =>
		{
			SetSessionEventInsertSqlParameters(command.Command.Parameters, session, 0);
		}, (NavSqlCommand command) => (DateTime)command.ExecuteScalar());
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertActiveSessionCommand", session.Database), () => string.Format(CultureInfo.InvariantCulture, "  INSERT INTO [{0}].[dbo].[Active Session]   (  {1}    [User SID],    [Server Instance ID],    [Session ID],    [Server Instance Name],    [Server Computer Name],    [User ID],    [Client Type],    [Client Computer Name],    [Login Datetime],    [Database Name],    [Session Unique ID]  ) VALUES({2}@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11)", session.Database.DatabaseName, companyIdColumnInsertText, companyIdInsertValue), (NavSqlCommand command) =>
		{
			SetupActiveSessionParameters(command, companyId);
		}, (NavSqlCommand command) =>
		{
			SetActiveSessionInsertSqlParameters(command.Command.Parameters, session, eventTime);
		}, (NavSqlCommand command) => command.ExecuteNonQuery());
	}

	private static void InsertSessionEventLogoutRecordNoRetries(SessionEventData sessionData)
	{
		if (sessionData.Database.IsDisposed)
		{
			return;
		}
		CompanyId? companyId = sessionData.CompanyId;
		string companyIdColumnInsertText = (companyId.HasValue ? "\"$companyId\"," : string.Empty);
		string companyIdInsertValue = (companyId.HasValue ? "@12," : string.Empty);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(sessionData.Database, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(sessionData.Database, "InsertSessionEventCommand", companyId.HasValue.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, "DECLARE @currentTime DATETIME SELECT @currentTime = GETUTCDATE() INSERT INTO [{0}].[dbo].[Session Event](  {1}  [User SID],  [Server Instance ID],  [Session ID],  [Event Type],  [Event Datetime],  [Client Type],  [Database Name],  [Client Computer Name],  [User ID],  [Comment],  [Session Unique ID]) VALUES({2}@1,@2,@3,@4,@currentTime,@6,@7,@8,@9,@10,@11)SELECT @currentTime", sessionData.Database.DatabaseName, companyIdColumnInsertText, companyIdInsertValue), (NavSqlCommand command) =>
		{
			SetupSessionEventParameters(command, companyId);
		}, (NavSqlCommand command) =>
		{
			SetSessionEventInsertSqlParametersFromEventData(command.Command.Parameters, sessionData, 1);
		}, (NavSqlCommand command) => command.ExecuteNonQuery());
	}

	private static SessionEventData GetEventDataFromSession(NavSession session)
	{
		int fieldDefinedLength = session.NCLMetadata.GetMetaTableById(2000000111).GetFieldByNo(10).FieldDefinedLength;
		string text = session.SessionComment ?? string.Empty;
		if (text.Length > fieldDefinedLength)
		{
			text = text.Substring(0, fieldDefinedLength);
		}
		CompanyId? companyIdForSystemTables = session.Tenant.Database.GetCompanyIdForSystemTables(2000000111);
		return new SessionEventData(session.User?.Id ?? Guid.Empty, session.Id, NavSession.TranslateToClientType(session.ClientConnectionType), session.Database, session.ClientMachineName ?? string.Empty, session.User?.Name ?? string.Empty, text, session.UniqueSessionId, companyIdForSystemTables);
	}

	private static void SetSessionEventInsertSqlParameters(SqlParameterCollection parameters, NavSession session, int eventType)
	{
		int index = 0;
		parameters[index++].Value = session.User.Id;
		parameters[index++].Value = NavEnvironment.Instance.GetServiceInstanceId();
		parameters[index++].Value = session.Id;
		parameters[index++].Value = eventType;
		parameters[index++].Value = NavSession.TranslateToClientType(session.ClientConnectionType);
		parameters[index++].Value = session.Database.DatabaseName;
		parameters[index++].Value = session.ClientMachineName ?? string.Empty;
		parameters[index++].Value = session.User.Name;
		int fieldDefinedLength = session.NCLMetadata.GetMetaTableById(2000000111).GetFieldByNo(10).FieldDefinedLength;
		string text = session.SessionComment ?? string.Empty;
		if (text.Length > fieldDefinedLength)
		{
			text = text.Substring(0, fieldDefinedLength);
		}
		parameters[index++].Value = text;
		parameters[index++].Value = session.UniqueSessionId;
		CompanyId? companyIdForSystemTables = session.Tenant.Database.GetCompanyIdForSystemTables(2000000111);
		if (companyIdForSystemTables.HasValue)
		{
			parameters[index].Value = companyIdForSystemTables.Value;
		}
	}

	private static void SetSessionEventInsertSqlParametersFromEventData(SqlParameterCollection parameters, SessionEventData eventData, int eventType)
	{
		int index = 0;
		parameters[index++].Value = eventData.UserSID;
		parameters[index++].Value = NavEnvironment.Instance.GetServiceInstanceId();
		parameters[index++].Value = eventData.SessionId;
		parameters[index++].Value = eventType;
		parameters[index++].Value = eventData.ClientType;
		parameters[index++].Value = eventData.Database.DatabaseName;
		parameters[index++].Value = eventData.ClientComputerName;
		parameters[index++].Value = eventData.UserID;
		parameters[index++].Value = eventData.SessionComment;
		parameters[index++].Value = eventData.SessionUniqueId;
		CompanyId? companyId = eventData.CompanyId;
		if (companyId.HasValue)
		{
			parameters[index].Value = companyId.Value;
		}
	}

	private static void SetActiveSessionInsertSqlParameters(SqlParameterCollection parameters, NavSession session, DateTime eventTime)
	{
		int index = 0;
		parameters[index++].Value = session.User.Id;
		parameters[index++].Value = NavEnvironment.Instance.GetServiceInstanceId();
		parameters[index++].Value = session.Id;
		parameters[index++].Value = NavEnvironment.ServerInstanceName;
		parameters[index++].Value = NavEnvironment.ServerComputerName;
		parameters[index++].Value = session.User.Name;
		parameters[index++].Value = NavSession.TranslateToClientType(session.ClientConnectionType);
		parameters[index++].Value = session.ClientMachineName ?? string.Empty;
		parameters[index++].Value = eventTime;
		parameters[index++].Value = session.Database.DatabaseName;
		parameters[index++].Value = session.UniqueSessionId;
		CompanyId? companyIdForSystemTables = session.Tenant.Database.GetCompanyIdForSystemTables(2000000111);
		if (companyIdForSystemTables.HasValue)
		{
			parameters[index].Value = companyIdForSystemTables.Value;
		}
	}

	internal static int ForceCloseDeadSessions(NavDatabase database)
	{
		if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
		{
			return 0;
		}
		int num = 0;
		SortedSet<int> sortedSet = new SortedSet<int>(Heartbeat.Instance?.AliveServerInstances);
		if (sortedSet.Count == 0)
		{
			return num;
		}
		CompanyId? companyIdForSystemTables = database.GetCompanyIdForSystemTables(2000000110);
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		if (companyIdForSystemTables.HasValue)
		{
			text2 = "\"$companyId\",";
			text3 = "se.\"$companyId\",";
			text = " AND " + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(1);
		}
		string commandText = string.Format(CultureInfo.InvariantCulture, "declare @result int;\r\n            EXEC @result = sp_getapplock @Resource = '{2}', @LockMode ='Exclusive', @LockTimeout=0;IF @result = 0 INSERT INTO [{0}].dbo.[Session Event] ({3}[User SID],[Server Instance ID],[Session ID],[Event Type],[Event Datetime],[Client Type],[Database Name],[Client Computer Name],[User ID],[Comment],[Session Unique ID]) SELECT {4}se.[User SID],se.[Server Instance ID],se.[Session ID],4 AS [Event Type],GETUTCDATE() AS [Event Datetime],se.[Client Type],se.[Database Name],se.[Client Computer Name],se.[User ID],'Force closed by server' AS [Comment],se.[Session Unique ID] FROM [{0}].dbo.[Active Session] AS se WITH(NOLOCK) WHERE se.[Server Instance ID] NOT IN ({1}){5} AND se.[Login Datetime] < @0;SELECT @@ROWCOUNT", database.DatabaseName, string.Join(",", sortedSet), "SessionEventTableMaintenance" + companyIdForSystemTables?.ToString(CultureInfo.InvariantCulture), text2, text3, text);
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update))
		{
			try
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand.SetCommandText(commandText);
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.DateTime, (Heartbeat.Instance.LastActiveUpdated == DateTime.MinValue) ? DateTime.UtcNow : Heartbeat.Instance.LastActiveUpdated);
				if (companyIdForSystemTables.HasValue)
				{
					NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 1), companyIdForSystemTables.Value);
				}
				num = navSqlCommand.ExecuteScalar<int>();
				if (num != 0)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000297", Category.Operations, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Force closed {0} connections", num);
				}
			}
			catch (NavCSideDuplicateKeyException)
			{
			}
		}
		return num;
	}

	internal static void PurgeSessionEventTable(NavDatabase database)
	{
		if (database.IsDisposed || !database.IsCurrentVersion || database.DatabaseType != NavDatabaseType.Tenant || database.Tenant.State == NavTenantState.OperationalSyncInProgress || database.Tenant.State == NavTenantState.OperationalDataUpgradeInProgress)
		{
			return;
		}
		try
		{
			((ITenantSessionHandler)database.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession session) =>
			{
				CompanyId? companyIdForSystemTables = database.GetCompanyIdForSystemTables(2000000111);
				string text = string.Empty;
				string text2 = string.Empty;
				if (companyIdForSystemTables.HasValue)
				{
					text = "AND " + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(2);
					text2 = "acs." + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(2) + " AND ";
				}
				string commandText = string.Format(CultureInfo.CurrentCulture, ServerUserSettings.Instance.TrackSessionsInMemory.Value ? DeleteSQLStatementWhenActiveSessionInMemory : DeleteSQLStatement, database.DatabaseName, "SessionEventTableMaintenance" + companyIdForSystemTables?.ToString(CultureInfo.InvariantCulture), text, text2);
				using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update))
				{
					using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
					session.EnsureWriteTransactionStarted();
					navSqlCommand.SetCommandText(commandText);
					navSqlCommand.Command.Parameters.Add(NavSqlStatementHelper.ParameterName(0), SqlDbType.Int).Value = ServerUserSettings.Instance.SessionEventTableRetainPeriodInSeconds;
					navSqlCommand.Command.Parameters.Add(NavSqlStatementHelper.ParameterName(1), SqlDbType.Int).Value = ServerUserSettings.Instance.NonInteractiveSessionsLogRetainPeriodInSeconds;
					if (companyIdForSystemTables.HasValue)
					{
						NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 2), companyIdForSystemTables.Value);
					}
					navSqlCommand.ExecuteNonQuery();
				}
				session.Commit();
			}), false, false, false, (LanguageSetting)null);
		}
		catch (NavCSideException)
		{
		}
		catch (NavSqlException)
		{
		}
	}

	internal static bool RemoveSustainedWebServiceSession(int sessionID)
	{
		if (sessionID >= 0 || !NavCurrentThread.Session.Database.IsCurrentVersion)
		{
			return false;
		}
		if (ServerUserSettings.Instance.TrackSessionsInMemory.Value && ActiveSessionDataProvider.Instance.TryFindSessionUniqueIdById(sessionID, out var uniqueSessionId))
		{
			return ActiveSessionDataProvider.Instance.RemoveSession(uniqueSessionId);
		}
		CompanyId? companyId = NavCurrentThread.Session.Database.GetCompanyIdForSystemTables(2000000110);
		string companyIdWhereText = (companyId.HasValue ? "\"$companyId\" = @1 AND" : string.Empty);
		int num;
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavCurrentThread.Session.Database, TransactionType.Update))
		{
			num = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(NavCurrentThread.Session.Database, "RemoveSustainedWebServiceSessionCommandName", companyId.HasValue.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].dbo.[Active Session] WHERE ({1} [Session ID] = @3)", NavCurrentThread.Session.Database.DatabaseName, companyIdWhereText), (NavSqlCommand command) =>
			{
				command.Command.Parameters.Add(NavSqlStatementHelper.ParameterName(3), SqlDbType.Int);
				if (companyId.HasValue)
				{
					command.Command.Parameters.Add("@1", SqlDbType.Int);
				}
			}, (NavSqlCommand command) =>
			{
				command.Command.Parameters[0].Value = sessionID;
				if (companyId.HasValue)
				{
					command.Command.Parameters[1].Value = NavCurrentThread.Session.Database.GetCompanyIdForSystemTables(2000000110);
				}
			}, (NavSqlCommand command) => command.ExecuteNonQuery());
		}
		return num > 0;
	}
}
