using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("ID: {Id} - InUse: {InUse} - HasTransaction: {currentTransaction != null} - Idle: {ConnectionIdleTime} - Exclusive: {isExclusiveConnection}")]
public class NavSqlConnection : TreeObject, IReferenceTarget<NavSqlConnection>
{
	public const long UnknownConnectionId = -1L;

	public const short UnknownSqlServerSessionId = -1;

	private const long UnknownLastActiveTicks = 0L;

	private const string CmdConnectionInitialization = "SET ANSI_NULLS ON\r\nSET ANSI_PADDING ON\r\nSET ANSI_WARNINGS ON\r\nSET ARITHABORT ON\r\nSET CONCAT_NULL_YIELDS_NULL ON\r\nSET QUOTED_IDENTIFIER ON\r\nSET NUMERIC_ROUNDABORT OFF\r\nSET XACT_ABORT OFF";

	private const string CmdConnectionInitializationAndRetrieveSQLSessionID = "SET ANSI_NULLS ON\r\nSET ANSI_PADDING ON\r\nSET ANSI_WARNINGS ON\r\nSET ARITHABORT ON\r\nSET CONCAT_NULL_YIELDS_NULL ON\r\nSET QUOTED_IDENTIFIER ON\r\nSET NUMERIC_ROUNDABORT OFF\r\nSET XACT_ABORT OFF\r\nSELECT @@SPID";

	private const string CmdLockTimeOut = "SET LOCK_TIMEOUT {0}";

	private const string CmdTransactionId = "SELECT TOP 1 transaction_id FROM sys.dm_tran_session_transactions WHERE session_id = @@spid";

	private const string TelemetryDbNameToken = "SQLDATABASE";

	private const string TelemetryCurrentCompanyNameToken = "\"CURRENTCOMPANY$";

	private static long lastConnectionId;

	private const int SqlErrorCodeMissingPrepareStatement = 8179;

	private const int MaxTransientErrorRetryDuration = 60000;

	private static readonly NavMetric4Dimensions sqlOperationMetric = new NavMetric4Dimensions("SqlOperationDurationMs", "DatabaseName", "TenantId", "Task", "Result");

	private SqlConnection connection;

	private SqlTransaction currentTransaction;

	private TransactionType currentTransactionType;

	private uint transactionVersion;

	private readonly NavSqlCommandCache cachedCommands;

	private int inUse;

	private long lastOpenConnectionTicks;

	private ulong lastActiveTimer;

	private bool lockTimeoutEnabled;

	private int savepointNo;

	private bool isExclusiveConnection;

	private int transactionHistoryToken = -1;

	private INavSqlConnectionKeepAlive keepAlive;

	private bool isDead;

	private bool lastSqlCommandSuccess = true;

	private readonly SemaphoreSlim connectionLockRoot = new SemaphoreSlim(1);

	private readonly HashSet<long> knownXLockedTimestamps = new HashSet<long>();

	private IReference<NavDatabase> currentDatabase;

	private readonly Reference<NavSqlConnection> selfReference;

	private NavDatabaseType currentDatabaseType;

	private string currentDatabaseName;

	public const IsolationLevel DefaultReadIsolationLevel = IsolationLevel.ReadUncommitted;

	public const IsolationLevel DefaultWriteIsolationLevel = IsolationLevel.RepeatableRead;

	public static long NextConnectionId => Interlocked.Increment(ref lastConnectionId);

	public long Id { get; private set; } = -1L;

	public int QueueBumperToken { get; set; }

	public short SqlServerSessionId { get; private set; } = -1;

	public SqlConnection Connection
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("NavSqlConnection");
			}
			return connection;
		}
	}

	internal NavDatabase Database
	{
		get
		{
			if (currentDatabase == null)
			{
				throw new InvalidOperationException("The sql connection doesn't have a database instance attached. This must not happen if the connection comes from the pool.");
			}
			return currentDatabase.Target;
		}
		set
		{
			if (value == null)
			{
				currentDatabase = null;
				return;
			}
			if (value.IsDisposed)
			{
				throw new InvalidOperationException("Cannot assign a disposed database to a connection.");
			}
			currentDatabase = value.GetReference();
			currentDatabaseType = value.DatabaseType;
			currentDatabaseName = value.DatabaseName;
		}
	}

	public string DatabaseName => currentDatabaseName;

	public NavSqlConnectionInfo ConnectionInfo => Database.ConnectionInfo;

	public NavSqlConnectionPool ConnectionPool => Tree.Parent as NavSqlConnectionPool;

	public SqlTransaction CurrentTransaction => currentTransaction;

	public long CurrentTransactionId { get; private set; }

	public TransactionType CurrentTransactionType => currentTransactionType;

	public bool CurrentTransactionIsWriteTransaction
	{
		get
		{
			if (currentTransaction == null)
			{
				return false;
			}
			return TransactionIsWriteTransaction(currentTransactionType);
		}
	}

	public uint TransactionVersion => transactionVersion;

	public int InUse
	{
		get
		{
			return inUse;
		}
		private set
		{
			if (SqlProfilerCommentProvider.IsEnabledForAnySession)
			{
				NavCurrentThread.Session?.SqlProfilerCommentProvider.WriteGetOrReturnConnectionToThePoolToSqlProfiler(inUse, value, Database);
			}
			inUse = value;
		}
	}

	public bool IsExclusiveConnection
	{
		get
		{
			return isExclusiveConnection;
		}
		set
		{
			isExclusiveConnection = value;
		}
	}

	public bool IsDead
	{
		get
		{
			if (!IsDisposed)
			{
				return isDead;
			}
			return true;
		}
	}

	public bool LockTimeoutEnabled => lockTimeoutEnabled;

	public NavSqlCommandCache CachedCommands => cachedCommands;

	internal string TenantTraceId
	{
		get
		{
			if (Database != null && Database.Tenant != null)
			{
				return Database.Tenant.Id;
			}
			return string.Empty;
		}
	}

	public ulong ConnectionIdleTime
	{
		get
		{
			if (lastActiveTimer == 0L)
			{
				return 0uL;
			}
			return NativeMethods.GetTickCount64() - lastActiveTimer;
		}
	}

	public static event Action<NavSqlConnection, SqlException> OnSqlException;

	public NavSqlConnection(NavSqlConnectionPool parentConnectionPool, NavDatabase database)
		: base(parentConnectionPool)
	{
		selfReference = new Reference<NavSqlConnection>(this);
		cachedCommands = NavSqlCommandCache.Create(this, ServerUserSettings.Instance.MaxNumberOfCachedSqlCommands.Value, ServerUserSettings.Instance.MaxNumberOfCachedSqlCommandsSharedSchema.Value);
		currentDatabase = database.GetReference();
		currentDatabaseType = database.DatabaseType;
		currentDatabaseName = database.DatabaseName;
	}

	public IReference<NavSqlConnection> GetReference()
	{
		return selfReference;
	}

	public void AddRef()
	{
		InUse++;
	}

	public void ReleaseRef(bool connectionMayBeReused, bool forceClose = false)
	{
		if (--InUse == 0)
		{
			ConnectionPool.ReleaseConnection(this, connectionMayBeReused, forceClose);
		}
	}

	private void ForceCloseConnection()
	{
		if (keepAlive != null)
		{
			keepAlive.Dispose();
			keepAlive = null;
		}
		PerformActionIgnoreExceptions(() =>
		{
			Tree.DisposeAllChildren();
		});
		if (currentTransaction != null)
		{
			PerformActionIgnoreExceptions(() =>
			{
				currentTransaction.Dispose();
			});
			currentTransaction = null;
		}
		PerformActionIgnoreExceptions(ReleaseTransactionHistory);
		if (connection != null)
		{
			PerformActionIgnoreExceptions(() =>
			{
				connection.Close();
			});
			connection = null;
			NavSqlConnectionPool.UpdateOpenConnectionCount(currentDatabaseType, Interlocked.Decrement);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009U2", Category.SqlConnection, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "A connection has been closed\r\nConnection ID: {0}\r\nDatabase server: {1}\r\nDatabase name: {2}.", Id, ConnectionPool.ServerName, currentDatabaseName, SqlServerSessionId);
		}
		cachedCommands.Initialize();
	}

	private static void PerformActionIgnoreExceptions(Action action)
	{
		try
		{
			action();
		}
		catch (Exception exception)
		{
			LogWriterHelper.LogExceptionEventWithTelemetry("00000H8", Category.Runtime, exception, Verbosity.Warning, EventLogEntryType.Warning);
		}
	}

	private void SetConnectionProperties()
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(this);
		if (FeatureSwitches.Instance.DisableRetrieveOfSqlServerSessionId)
		{
			navSqlCommand.ExecuteNonQuery("SET ANSI_NULLS ON\r\nSET ANSI_PADDING ON\r\nSET ANSI_WARNINGS ON\r\nSET ARITHABORT ON\r\nSET CONCAT_NULL_YIELDS_NULL ON\r\nSET QUOTED_IDENTIFIER ON\r\nSET NUMERIC_ROUNDABORT OFF\r\nSET XACT_ABORT OFF");
		}
		else
		{
			SqlServerSessionId = navSqlCommand.ExecuteScalar<short>("SET ANSI_NULLS ON\r\nSET ANSI_PADDING ON\r\nSET ANSI_WARNINGS ON\r\nSET ARITHABORT ON\r\nSET CONCAT_NULL_YIELDS_NULL ON\r\nSET QUOTED_IDENTIFIER ON\r\nSET NUMERIC_ROUNDABORT OFF\r\nSET XACT_ABORT OFF\r\nSELECT @@SPID");
		}
	}

	private static int DecodeReasonCode(SqlExceptionAdapter sqlExceptionAdapter)
	{
		if (NavSqlReturnCode.SqlAzureNeedDecoding.Any((int errorNumber) => errorNumber == sqlExceptionAdapter.Number))
		{
			int num = sqlExceptionAdapter.Message.IndexOf("Code: ", StringComparison.Ordinal) + 6;
			int num2 = sqlExceptionAdapter.Message.IndexOf("\r", num, StringComparison.Ordinal);
			if (num == -1 || num2 == -1 || !int.TryParse(sqlExceptionAdapter.Message.Substring(num, num2 - num), out var result))
			{
				result = 0;
			}
			if (result > 0)
			{
				int num3 = result / 256;
				for (int num4 = 0; num4 < 9; num4++)
				{
					int num5 = 1 << num4 * 2;
					int num6 = 2 << num4 * 2;
					if ((num3 & num5) == num5)
					{
						return 1;
					}
					if ((num3 & num6) == num6)
					{
						return 2;
					}
				}
			}
		}
		return 0;
	}

	private static void CheckSqlAzureErrors(SqlExceptionAdapter sqlExceptionAdapter, bool connectionIsDead, string navTenantId)
	{
		int num = DecodeReasonCode(sqlExceptionAdapter);
		string text = string.Empty;
		if (NavSqlReturnCode.SqlAzureKnownIssue.Any((int errorNumber) => errorNumber == sqlExceptionAdapter.Number))
		{
			text = Lang.SqlConnectionLostKnownIssue;
		}
		else if (NavSqlReturnCode.SqlAzureOperationCanBeRetriedAfterReopen.Any((int errorNumber) => errorNumber == sqlExceptionAdapter.Number))
		{
			text = Lang.SqlConnectionLostOnAlterDatabaseCanBeRetried;
		}
		else
		{
			if (num == 1 || NavSqlReturnCode.SqlAzureOperationCanBeRetried.Any((int errorNumber) => errorNumber == sqlExceptionAdapter.Number))
			{
				text = Lang.SqlConnectionLostOperationCanBeRetried;
				num = 1;
			}
			if (num == 2 || NavSqlReturnCode.SqlAzureOperationMustBeShortened.Any((int errorNumber) => errorNumber == sqlExceptionAdapter.Number))
			{
				text = Lang.SqlConnectionLostOperationMustBeShortened;
				num = 2;
			}
		}
		if (NavSqlReturnCode.IsTransientError(sqlExceptionAdapter.Number))
		{
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateTransientErrorsPerformanceCounter();
			throw new NavSqlException(text, text, null, sqlExceptionAdapter.Number);
		}
		switch (num)
		{
		case 1:
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateSoftThrottledConnectionPerformanceCounter();
			break;
		case 2:
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateHardThrottledConnectionPerformanceCounter();
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (connectionIsDead)
			{
				throw new NavCSideException(22924076, text)
				{
					NavTenantId = navTenantId
				};
			}
			LogWriterHelper.LogExceptionEventWithTelemetry("000018J", Category.Sql, sqlExceptionAdapter.Exception, Verbosity.Warning, EventLogEntryType.Warning);
			throw new NavSqlException(text);
		}
	}

	private static void CheckAndThrowTrapableErrors(int errorNumber, Exception originalException, bool transactionNoLongerValid, int? timeout, string commandText)
	{
		switch (errorNumber)
		{
		case -2:
			if (timeout.HasValue && timeout.Value != ServerUserSettings.Instance.SqlCommandTimeout.TotalSeconds)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000027W", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SQL command timed out due to explicitly lowered timeout of {0}", timeout.Value);
				throw new NavCSideSqlTimeoutException(string.Format(CultureInfo.InvariantCulture, "Error code: {0}", -2), originalException, remappable: true);
			}
			break;
		case 1222:
			throw new NavCSideException(22926090, Lang.LockTimeOutNoTableError);
		case 1205:
			throw new NavCSideException(22926089, Lang.DeadLockErrorNoTable);
		case 2601:
		case 2627:
			if (transactionNoLongerValid)
			{
				throw NavSqlTransactionNoLongerValidException(errorNumber, commandText);
			}
			throw new NavCSideDuplicateKeyException(string.Format(CultureInfo.InvariantCulture, "Error code: {0}", 85132273), originalException, originalException.Message, errorNumber == 2601, remappable: true);
		case 8102:
		{
			NavCSideException ex = new NavCSideException(18023796);
			if (transactionNoLongerValid)
			{
				LogWriterHelper.LogExceptionEventWithTelemetry("000018K", Category.Sql, ex, Verbosity.Warning, EventLogEntryType.Warning);
				throw NavSqlTransactionNoLongerValidException(errorNumber, commandText);
			}
			throw ex;
		}
		}
	}

	private static NavSqlException NavSqlTransactionNoLongerValidException(int errorNumber, string commandText)
	{
		return new NavSqlException(Lang.TransactionNoLongerValidForClient, errorNumber, transactionRolledBack: true);
	}

	private static void ThrowNavSqlException(SqlExceptionAdapter exceptionAdapter, bool transactionNoLongerValid, string commandText, bool isLastExceptionARollbackCause, bool logExceptions)
	{
		string text = string.Empty;
		NavSqlException exception;
		if (!isLastExceptionARollbackCause)
		{
			exception = new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnexpectedSqlError, exceptionAdapter.Message, commandText, Environment.StackTrace), exceptionAdapter.Exception, exceptionAdapter.Number, transactionNoLongerValid);
		}
		else
		{
			Exception ex = ((NavCurrentThread.Session == null) ? null : NavCurrentThread.Session.LastException);
			text = ex?.Message ?? string.Empty;
			if (logExceptions)
			{
				LogWriterHelper.LogExceptionEventWithTelemetry("000018L", Category.Sql, ex, Verbosity.Warning, EventLogEntryType.Error);
			}
			exception = new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnexpectedSqlError, exceptionAdapter.Message + Lang.RollbackExceptionString + text, commandText, Environment.StackTrace), exceptionAdapter.Exception, exceptionAdapter.Number, transactionNoLongerValid);
		}
		if (logExceptions)
		{
			LogWriterHelper.LogExceptionEventWithTelemetry("000018M", Category.Sql, exception, Verbosity.Warning, EventLogEntryType.Error);
		}
		NavSqlException ex2 = (isLastExceptionARollbackCause ? new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnexpectedSqlErrorForClient, exceptionAdapter.Message + Lang.RollbackExceptionString + text), exceptionAdapter.Exception, exceptionAdapter.Number, transactionNoLongerValid) : new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnexpectedSqlErrorForClient, exceptionAdapter.Message), exceptionAdapter.Exception, exceptionAdapter.Number, transactionNoLongerValid));
		throw ex2;
	}

	private void CloseDeadConnection(DbException exception, bool logException = true)
	{
		bool isTransientError = false;
		if ((exception != null) & logException)
		{
			SqlException ex = exception as SqlException;
			if (ex != null && NavSqlReturnCode.IsTransientError(ex.Number))
			{
				LogWriterHelper.LogExceptionEventWithTelemetry("000018N", Category.Sql, ex, Verbosity.Warning);
				isTransientError = true;
			}
			else
			{
				LogWriterHelper.LogExceptionEventWithTelemetry("000018O", Category.Sql, exception, Verbosity.Error);
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001N6", Category.Sql, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, FormattableString.Invariant($"Closing dead SQL connection.\r\nConnection idle time: {(double)ConnectionIdleTime / 1000.0:F1} second(s),\r\nIs write transaction: {CurrentTransactionIsWriteTransaction},\r\nHas transaction: {currentTransaction != null},\r\nIs exclusive connection : {isExclusiveConnection},\r\nKeep alive enabled: {keepAlive != null},\r\nSQL connection state: {connection?.State}\r\nSQL error number: {ex?.Number}\r\nSQL error state: {ex?.State}\r\nSQL database server: {ConnectionPool.ServerName}\r\nNAV database name: {currentDatabaseName}\r\nNAV database type: {currentDatabaseType}\r\nSQL session id: {SqlServerSessionId}"));
		}
		Database?.Health.ConnectionProblem("00001NL", Category.Sql, lastOpenConnectionTicks > 0, isTransientError);
		ForceCloseConnection();
		isDead = true;
	}

	private void HandleDeadConnection(SqlExceptionAdapter exceptionAdapter, bool isLastExceptionARollbackCause)
	{
		if (connection != null && (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed))
		{
			if (!isLastExceptionARollbackCause)
			{
				HandleDeadConnection(exceptionAdapter);
				return;
			}
			Exception lastException = ((NavCurrentThread.Session == null) ? null : NavCurrentThread.Session.LastException);
			HandleDeadConnection(exceptionAdapter, lastException);
		}
	}

	private void HandleDeadConnection(SqlExceptionAdapter exceptionAdapter, Exception lastException = null)
	{
		CloseDeadConnection(exceptionAdapter.DatabaseException);
		if (exceptionAdapter.DatabaseException != null)
		{
			CheckSqlAzureErrors(exceptionAdapter, ConnectionIdleTime >= 60000, TenantTraceId);
		}
		if (lastException == null)
		{
			throw new NavCSideException(22924076, Lang.SqlConnectionLost, exceptionAdapter.DatabaseException)
			{
				NavTenantId = TenantTraceId
			};
		}
		throw new NavCSideException(22924076, Lang.SqlConnectionLost + Lang.RollbackExceptionString + lastException.Message, exceptionAdapter.DatabaseException)
		{
			NavTenantId = TenantTraceId
		};
	}

	private void HandleSqlTimeoutException(SqlExceptionAdapter exceptionAdapter, string commandText)
	{
		CloseDeadConnection(exceptionAdapter.DatabaseException, logException: false);
		ThrowNavSqlException(exceptionAdapter, transactionNoLongerValid: false, commandText, isLastExceptionARollbackCause: true, logExceptions: true);
	}

	private static bool IsLastExceptionRollbackCause(Exception exception, bool isRollbackAction)
	{
		if (isRollbackAction && NavCurrentThread.Session != null && NavCurrentThread.Session.LastException != null)
		{
			return NavCurrentThread.Session.LastException != exception;
		}
		return false;
	}

	private bool IsSqlTimeoutExceptionPresentDuringRollback(SqlExceptionAdapter exceptionAdapter, string commandText, bool isLastExceptionARollbackCause)
	{
		if (isLastExceptionARollbackCause && exceptionAdapter.Number == -2)
		{
			HandleSqlTimeoutException(exceptionAdapter, commandText);
			return true;
		}
		return false;
	}

	private void HandleInnerException(SqlExceptionAdapter exceptionAdapter, bool transactionNoLongerValid, bool isLastExceptionARollbackCause, int? timeout, string commandText)
	{
		SqlExceptionAdapter sqlExceptionAdapter = exceptionAdapter;
		while (sqlExceptionAdapter != null && sqlExceptionAdapter.DatabaseException != null)
		{
			for (int i = 0; i < sqlExceptionAdapter.Errors.Count; i++)
			{
				int number = sqlExceptionAdapter.Errors[i].Number;
				if (sqlExceptionAdapter.Class >= 20 || number == 4060 || number == 6005 || number == 40626)
				{
					if (!isLastExceptionARollbackCause)
					{
						HandleDeadConnection(sqlExceptionAdapter);
					}
					else
					{
						Exception lastException = ((NavCurrentThread.Session == null) ? null : NavCurrentThread.Session.LastException);
						HandleDeadConnection(sqlExceptionAdapter, lastException);
					}
				}
				if (sqlExceptionAdapter.Class == 13 && currentTransaction != null)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000HN", Category.Sql, exceptionAdapter.Exception, Verbosity.Warning);
					EndTransaction(NavSqlEndTransactionType.EndedByServer);
				}
				CheckSqlAzureErrors(sqlExceptionAdapter, connectionIsDead: false, TenantTraceId);
				CheckAndThrowTrapableErrors(number, sqlExceptionAdapter.Exception, transactionNoLongerValid, timeout, commandText);
			}
			sqlExceptionAdapter = sqlExceptionAdapter.InnerException;
		}
	}

	private bool IsTransactionNoLongerValid(string commandText)
	{
		if (currentTransaction != null && currentTransaction.Connection == null)
		{
			EndTransaction(NavSqlEndTransactionType.EndedByServer);
			NavSqlException exception = new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.TransactionNoLongerValid, commandText), transactionRolledBack: true);
			LogWriterHelper.LogExceptionEventWithTelemetry("000018P", Category.Sql, exception, Verbosity.Warning, EventLogEntryType.Error);
			return true;
		}
		return false;
	}

	private void MapException(SqlExceptionAdapter exceptionAdapter, string commandText, bool isRollbackAction = false, int? timeout = null)
	{
		bool isLastExceptionARollbackCause = IsLastExceptionRollbackCause(exceptionAdapter.DatabaseException, isRollbackAction);
		bool flag = IsSqlTimeoutExceptionPresentDuringRollback(exceptionAdapter, commandText, isLastExceptionARollbackCause);
		if (!flag)
		{
			HandleDeadConnection(exceptionAdapter, isLastExceptionARollbackCause);
		}
		bool transactionNoLongerValid = IsTransactionNoLongerValid(commandText);
		HandleInnerException(exceptionAdapter, transactionNoLongerValid, isLastExceptionARollbackCause, timeout, commandText);
		ThrowNavSqlException(exceptionAdapter, transactionNoLongerValid, commandText, isLastExceptionARollbackCause, !flag);
	}

	private void ResetKeepAlive()
	{
		keepAlive?.Reset();
	}

	private void TickConnectionAsActive()
	{
		lastActiveTimer = NativeMethods.GetTickCount64();
	}

	public void OpenConnectionIfNeeded()
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException("NavSqlConnection");
		}
		if (currentDatabase == null)
		{
			throw new InvalidOperationException("The sql connection doesn't have a database instance attached. This must not happen if the connection comes from the pool.");
		}
		if (IsDead)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000B6E", Category.SqlConnection, new DeadSqlConnectionException("The SQL Server connection is no longer available."), Verbosity.Error, DiagnosticParameter.Oii(FormattableString.Invariant($"Connection id: {Id},\r\n                        Database server: {ConnectionPool.ServerName}\r\n                        Database name: {currentDatabaseName}\r\n                        Database type: {currentDatabaseType}")));
		}
		if (connection == null)
		{
			lockTimeoutEnabled = false;
			connection = ConnectionInfo.CreateConnection();
			string connectionString = connection.ConnectionString;
			NavSqlConnectionPool.UpdateOpenConnectionCount(currentDatabaseType, Interlocked.Increment);
			try
			{
				ExecuteAdoActionWithTrace((EventTask)8, TenantTraceId, NavCurrentThread.SessionId, NavCurrentThread.UserName, TryConnect, "OpenConnection");
			}
			catch (NavCSideException ex) when (ex.ErrorCode == 22924076)
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00009IU", Category.SqlConnection, new NavCSideException(22924076, Lang.SqlConnectionCannotBeEstablished)
				{
					NavTenantId = TenantTraceId
				}, Verbosity.Warning, DiagnosticParameter.Oii(connectionString));
			}
			lastOpenConnectionTicks = DateTime.UtcNow.Ticks;
			TickConnectionAsActive();
			Database.Health.ConnectionEstablished();
			if (Id == -1)
			{
				Id = NextConnectionId;
			}
			SetConnectionProperties();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009U3", Category.SqlConnection, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "A connection has been opened\r\nConnection ID: {0}\r\nDatabase server: {1}\r\nDatabase name: {2}.", Id, ConnectionPool.ServerName, currentDatabaseName, SqlServerSessionId);
			isDead = false;
		}
	}

	private long GetTransactionId()
	{
		try
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(this);
			return navSqlCommand.ExecuteScalar<long>("SELECT TOP 1 transaction_id FROM sys.dm_tran_session_transactions WHERE session_id = @@spid");
		}
		catch (Exception exception)
		{
			Database.Tenant.Diagnostics.SendExceptionTag("00000IQ", Category.Sql, exception, Verbosity.Warning);
			return 0L;
		}
	}

	public T ExecuteAdoFunction<T>(Func<T> function, string commandText, int? timeout = null)
	{
		return ExecuteFunction(function, commandText, timeout, isAdoCommand: true);
	}

	public T ExecuteFunction<T>(Func<T> function, string commandText, int? timeout = null)
	{
		return ExecuteFunction(function, commandText, timeout, isAdoCommand: false);
	}

	private T ExecuteFunction<T>(Func<T> function, string commandText, int? timeout, bool isAdoCommand)
	{
		return NavThread.RunExternalAction(() =>
		{
			try
			{
				ResetKeepAlive();
				Lock();
				try
				{
					T result = function();
					TickConnectionAsActive();
					if (!isAdoCommand)
					{
						lastSqlCommandSuccess = true;
					}
					return result;
				}
				finally
				{
					Unlock();
				}
			}
			catch (Exception ex)
			{
				if (!isAdoCommand)
				{
					lastSqlCommandSuccess = false;
				}
				if (ex is SqlException arg)
				{
					OnSqlException?.Invoke(this, arg);
				}
				SqlExceptionAdapterImplementation exceptionAdapter = new SqlExceptionAdapterImplementation(ex);
				MapException(exceptionAdapter, commandText, isRollbackAction: false, timeout);
				throw;
			}
		});
	}

	public T ExecuteFunctionWithTrace<T>(EventTask task, Func<T> function, NavSqlCommand command)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ExecuteFunctionWithTrace(task, function, command.Command.CommandText, command.Command.CommandTimeout, isAdoCommand: false);
	}

	public T ExecuteAdoFunctionWithTrace<T>(EventTask task, Func<T> function, string commandText, int timeout = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ExecuteFunctionWithTrace(task, function, commandText, timeout, isAdoCommand: true);
	}

	private T ExecuteFunctionWithTrace<T>(EventTask task, Func<T> function, string commandText, int timeout, bool isAdoCommand)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Invalid comparison between Unknown and I4
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		Stopwatch stopwatch = Stopwatch.StartNew();
		bool flag = false;
		try
		{
			T result;
			if (!NavServerTraceHelper.IsSqlTracingEnabled())
			{
				result = ExecuteFunction(function, commandText, timeout, isAdoCommand);
			}
			else
			{
				using (NavServerTraceHelper.EnterSpan(task, NavEnvironment.ServerInstanceName, TenantTraceId, NavCurrentThread.SessionId, NavCurrentThread.UserName, commandText))
				{
					result = ExecuteFunction(function, commandText, timeout, isAdoCommand);
				}
			}
			flag = true;
			return result;
		}
		catch (Exception exception) when (DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnThresholdExceptionFilter(exception, "000007L", Category.Sql, ServerUserSettings.Instance.SqlLongRunningThreshold.Value, stopwatch.ElapsedMilliseconds, PrivacyClassification.SystemMetadata, "Long running SQL statement \r\n  Task ID: {0}\r\n  Connection ID: {1}\r\n  Database Name: {3}\r\n  Current Company Name: {4}\r\n  Statement: {2}", SessionContextHelper.GetALCallStack, CreateTelemetryParameters(task, commandText)))
		{
			throw;
		}
		finally
		{
			long elapsedTicks = stopwatch.ElapsedTicks;
			long num = elapsedTicks / 10000;
			if (flag && num >= ServerUserSettings.Instance.SqlLongRunningThreshold.Value)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagThresholdMessage("000007L", Category.Sql, Verbosity.Warning, ServerUserSettings.Instance.SqlLongRunningThreshold.Value, num, PrivacyClassification.SystemMetadata, "Long running SQL statement \r\n  Task ID: {0}\r\n  Connection ID: {1}\r\n  Database Name: {3}\r\n  Current Company Name: {4}\r\n  Statement: {2}", SessionContextHelper.GetALCallStack, CreateTelemetryParameters(task, commandText));
			}
			NavCurrentThread.Session?.SqlDebuggingStatistics.SqlFunctionExecuted(task, commandText, elapsedTicks, SessionContextHelper.GetALCallStack);
			if ((int)task != 5)
			{
				sqlOperationMetric.LogValue(num, currentDatabaseName, Database?.Tenant?.Id ?? string.Empty, EventTaskToString(task), flag ? "success" : "failure");
			}
		}
	}

	private Func<object[]> CreateTelemetryParameters(EventTask task, string commandText)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return () =>
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected I4, but got Unknown
			commandText = commandText.Replace(currentDatabaseName, "SQLDATABASE");
			string text = NavCurrentThread.Session?.CompanyName;
			if (!string.IsNullOrEmpty(text))
			{
				string oldValue = "\"" + NavSqlStatementHelper.ConvertToSqlIdentifier(text) + "$";
				commandText = commandText.Replace(oldValue, "\"CURRENTCOMPANY$");
			}
			return new object[5]
			{
				(int)task,
				Id,
				commandText,
				currentDatabaseName,
				text
			};
		};
	}

	public void ExecuteAdoAction(Action action, string commandText)
	{
		ExecuteAction(action, commandText, isRollbackAction: false, isAdoCommand: true);
	}

	public void ExecuteAction(Action action, NavSqlCommand command)
	{
		ExecuteAction(action, command.Command.CommandText, isRollbackAction: false, isAdoCommand: false);
	}

	private void ExecuteAction(Action action, string commandText, bool isRollbackAction, bool isAdoCommand)
	{
		NavThread.RunExternalAction(() =>
		{
			try
			{
				ResetKeepAlive();
				Lock();
				try
				{
					action();
					if (!isAdoCommand)
					{
						lastSqlCommandSuccess = true;
					}
				}
				finally
				{
					Unlock();
				}
			}
			catch (Exception ex)
			{
				if (!isAdoCommand)
				{
					lastSqlCommandSuccess = false;
				}
				if (ex is SqlException arg)
				{
					OnSqlException?.Invoke(this, arg);
				}
				SqlExceptionAdapterImplementation exceptionAdapter = new SqlExceptionAdapterImplementation(ex);
				MapException(exceptionAdapter, commandText, isRollbackAction);
				throw;
			}
			TickConnectionAsActive();
		});
	}

	public void ExecuteActionWithTrace(EventTask task, string tenantId, int sessionId, string userName, Action action, NavSqlCommand command)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ExecuteActionWithTrace(task, tenantId, sessionId, userName, action, command.Command.CommandText, isAdoCommand: false);
	}

	public void ExecuteAdoActionWithTrace(EventTask task, string tenantId, int sessionId, string userName, Action action, string commandText)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ExecuteActionWithTrace(task, tenantId, sessionId, userName, action, commandText, isAdoCommand: true);
	}

	private void ExecuteActionWithTrace(EventTask task, string tenantId, int sessionId, string userName, Action action, string commandText, bool isAdoCommand)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		bool isRollbackAction = (int)task == 10;
		bool result = false;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnThreshold("000007S", Category.Sql, Verbosity.Warning, ServerUserSettings.Instance.SqlLongRunningThreshold.Value, (Stopwatch watch) =>
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (!NavServerTraceHelper.IsSqlTracingEnabled())
				{
					ExecuteAction(action, commandText, isRollbackAction, isAdoCommand);
				}
				else
				{
					using (NavServerTraceHelper.EnterSpan(task, NavEnvironment.ServerInstanceName, tenantId, sessionId, userName, commandText))
					{
						ExecuteAction(action, commandText, isRollbackAction, isAdoCommand);
					}
				}
				result = true;
			}
			finally
			{
				sqlOperationMetric.LogValue(watch.ElapsedMilliseconds, currentDatabaseName, Database?.Tenant?.Id ?? "", EventTaskToString(task), result ? "success" : "failure");
			}
		}, PrivacyClassification.SystemMetadata, SessionContextHelper.GetALCallStack, "Long running SQL statement \r\n  Task ID: {0}\r\n  Connection ID: {1}\r\n  Database Name: {3}\r\n  Current Company Name: {4}\r\n  Statement: {2}", CreateTelemetryParameters(task, commandText));
	}

	private static string EventTaskToString(EventTask task)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)task <= NavServerEventSource.TaskNames.Length)
		{
			return NavServerEventSource.TaskNames[task];
		}
		return ((object)task/*cast due to constrained. prefix*/).ToString();
	}

	internal T ExecuteCommandAndCacheOnSuccess<T>(NavSqlCommandCacheKey commandKey, Func<NavSqlCommand> newCommandFunc, Func<NavSqlCommand, T> executeFunc)
	{
		NavSqlCommand navSqlCommand = cachedCommands.GetCachedCommand(commandKey);
		bool flag = navSqlCommand == null;
		bool flag2 = false;
		try
		{
			if (flag)
			{
				navSqlCommand = newCommandFunc();
			}
			T result;
			try
			{
				result = executeFunc(navSqlCommand);
			}
			catch (Exception e) when (IsMissingPrepareStatementException(e))
			{
				cachedCommands.RemoveCachedCommand(commandKey);
				navSqlCommand = newCommandFunc();
				flag2 = true;
				result = executeFunc(navSqlCommand);
			}
			if (flag | flag2)
			{
				cachedCommands.AddCachedCommand(commandKey, navSqlCommand);
			}
			return result;
		}
		catch
		{
			if (flag | flag2)
			{
				navSqlCommand?.Dispose();
			}
			throw;
		}
	}

	private static bool IsMissingPrepareStatementException(Exception e)
	{
		int num = (e as NavSqlException)?.ErrorNumber ?? (e as SqlException)?.ErrorCode ?? 0;
		return num == 8179;
	}

	public T ExecuteCommandAndCacheOnSuccess<T>(NavSqlCommandCacheKey commandKey, Func<string> createStatementFunc, Action<NavSqlCommand> addOrSetParametersAction, Func<NavSqlCommand, T> executeFunc)
	{
		NavSqlCommand navSqlCommand = cachedCommands.GetCachedCommand(commandKey);
		bool flag = navSqlCommand == null;
		bool flag2 = false;
		try
		{
			if (flag)
			{
				navSqlCommand = NavSqlCommand.Create(this);
				navSqlCommand.SetCommandText(createStatementFunc());
				addOrSetParametersAction(navSqlCommand);
				if (navSqlCommand.CanCacheCommand)
				{
					navSqlCommand.PrepareStatement();
				}
			}
			else
			{
				if (navSqlCommand.State == NavSqlCommandState.ExecutedWithPendingResults)
				{
					navSqlCommand.CloseReader(forceClose: true);
				}
				addOrSetParametersAction(navSqlCommand);
			}
			T result;
			try
			{
				result = executeFunc(navSqlCommand);
			}
			catch (Exception e) when (!flag && IsMissingPrepareStatementException(e))
			{
				cachedCommands.RemoveCachedCommand(commandKey);
				navSqlCommand = NavSqlCommand.Create(this);
				navSqlCommand.SetCommandText(createStatementFunc());
				addOrSetParametersAction(navSqlCommand);
				if (navSqlCommand.CanCacheCommand)
				{
					navSqlCommand.PrepareStatement();
				}
				flag2 = true;
				result = executeFunc(navSqlCommand);
			}
			if (flag | flag2)
			{
				cachedCommands.AddCachedCommand(commandKey, navSqlCommand);
			}
			return result;
		}
		catch
		{
			if (flag | flag2)
			{
				navSqlCommand?.Dispose();
			}
			throw;
		}
	}

	public T ExecuteCommandWithoutParameters<T>(NavSqlCommandCacheKey commandKey, Func<string> commandTextFunc, Func<NavSqlCommand, T> executeFunc)
	{
		return ExecuteCommandAndCacheOnSuccess(commandKey, () =>
		{
			NavSqlCommand navSqlCommand = NavSqlCommand.Create(this);
			navSqlCommand.SetCommandText(commandTextFunc());
			if (navSqlCommand.CanCacheCommand)
			{
				navSqlCommand.PrepareStatement();
			}
			return navSqlCommand;
		}, executeFunc);
	}

	public T ExecuteCommandWithParameters<T>(NavSqlCommandCacheKey commandKey, Func<string> commandTextFunc, Action<NavSqlCommand> addParametersAction, Action<NavSqlCommand> setParametersAction, Func<NavSqlCommand, T> executeFunc)
	{
		return ExecuteCommandAndCacheOnSuccess(commandKey, () =>
		{
			NavSqlCommand navSqlCommand = NavSqlCommand.Create(this);
			navSqlCommand.SetCommandText(commandTextFunc());
			addParametersAction(navSqlCommand);
			if (navSqlCommand.CanCacheCommand)
			{
				navSqlCommand.PrepareStatement();
			}
			return navSqlCommand;
		}, (NavSqlCommand command) =>
		{
			setParametersAction(command);
			return executeFunc(command);
		});
	}

	internal void BeginTransaction(IsolationLevel newIsolationLevel)
	{
		if (IsDead)
		{
			throw new NavCSideException(22924076);
		}
		Func<SqlTransaction> function = () => currentTransaction = Connection.BeginTransaction(newIsolationLevel);
		int num = (int)newIsolationLevel;
		ExecuteAdoFunctionWithTrace((EventTask)6, function, "BeginTransaction IsolationLevel=" + num);
		if (NavCurrentThread.Session != null && ConnectionPool.TransactionHistory != null && Database.HasViewStatePermission)
		{
			CurrentTransactionId = GetTransactionId();
			transactionHistoryToken = ConnectionPool.TransactionHistory.TrackBeginTransaction(CurrentTransactionId);
		}
		transactionVersion++;
	}

	public bool CheckBeginTransaction(TransactionType transactionType)
	{
		if (IsDead)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000B6E", Category.SqlConnection, new DeadSqlConnectionException("The SQL Server connection is no longer available."), Verbosity.Error, DiagnosticParameter.Oii(FormattableString.Invariant($"Connection id: {Id},\r\n                        Database server: {ConnectionPool.ServerName}\r\n                        Database name: {currentDatabaseName}\r\n                        Database type: {currentDatabaseType}\r\n                        Transaction type: {transactionType}")));
		}
		bool flag = false;
		if (currentTransaction == null)
		{
			flag = true;
		}
		else
		{
			switch (transactionType)
			{
			case TransactionType.UpdateNoLocks:
				flag = currentTransactionType == TransactionType.Browse || currentTransactionType == TransactionType.Report;
				break;
			case TransactionType.Update:
			case TransactionType.Snapshot:
				flag = currentTransactionType == TransactionType.Browse || currentTransactionType == TransactionType.Report;
				if (!flag)
				{
					currentTransactionType = transactionType;
				}
				break;
			}
		}
		if (flag)
		{
			if (currentTransaction != null)
			{
				EndTransaction(NavSqlEndTransactionType.Commit);
			}
			switch (transactionType)
			{
			case TransactionType.Browse:
			case TransactionType.Report:
				BeginTransaction(IsolationLevel.ReadUncommitted);
				break;
			case TransactionType.UpdateNoLocks:
			case TransactionType.Update:
			case TransactionType.Snapshot:
			{
				BeginTransaction(IsolationLevel.RepeatableRead);
				bool flag2 = NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeoutEnabled;
				if (lockTimeoutEnabled != flag2)
				{
					SetLockTimeout(flag2);
				}
				break;
			}
			}
			currentTransactionType = transactionType;
		}
		return flag;
	}

	public void EndTransaction(NavSqlEndTransactionType endType)
	{
		if (keepAlive != null)
		{
			INavSqlConnectionKeepAlive navSqlConnectionKeepAlive = keepAlive;
			keepAlive = null;
			navSqlConnectionKeepAlive.Dispose();
		}
		ClearKnownXLockedTimestamps();
		try
		{
			cachedCommands.CloseAllReadersOnTransactionEnd();
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007B1", Category.SqlConnection, exception, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Exception while closing readers on transaction end. Endtype: " + endType));
		}
		ReleaseTransactionHistory();
		SqlTransaction sqlTransaction = currentTransaction;
		if (sqlTransaction == null)
		{
			return;
		}
		bool flag = lastSqlCommandSuccess;
		try
		{
			switch (endType)
			{
			case NavSqlEndTransactionType.Commit:
				ExecuteAdoActionWithTrace((EventTask)9, TenantTraceId, NavCurrentThread.SessionId, NavCurrentThread.UserName, sqlTransaction.Commit, "Commit");
				break;
			case NavSqlEndTransactionType.Rollback:
				ExecuteAdoActionWithTrace((EventTask)10, TenantTraceId, NavCurrentThread.SessionId, NavCurrentThread.UserName, sqlTransaction.Rollback, "Rollback");
				break;
			case NavSqlEndTransactionType.EndedByServer:
				break;
			}
		}
		catch (NavSqlException ex) when (ex.TransactionRolledBack)
		{
			if (flag)
			{
				throw;
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007B0", Category.SqlConnection, ex, Verbosity.Normal, DiagnosticParameter.SystemMetadata("SQL Server rolled back the transaction, but as the last error has been handled by the NST, this exception is ignored."));
		}
		finally
		{
			currentTransaction = null;
			sqlTransaction.Dispose();
		}
	}

	private void ReleaseTransactionHistory()
	{
		if (ConnectionPool.TransactionHistory != null)
		{
			ConnectionPool.TransactionHistory.TrackEndTransaction(transactionHistoryToken, CurrentTransactionId);
			CurrentTransactionId = 0L;
			transactionHistoryToken = -1;
		}
	}

	public string SetTransactionSavepoint()
	{
		try
		{
			cachedCommands.CloseAllReadersOnTransactionEnd();
			string savepoint = savepointNo.ToString(CultureInfo.InvariantCulture);
			ExecuteAdoAction(() =>
			{
				currentTransaction.Save(savepoint);
			}, "Transaction Save " + savepoint);
			savepointNo++;
			return savepoint;
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.TransactionEndError), ex);
		}
	}

	public void RollbackTransactionSavepoint(string savepoint)
	{
		try
		{
			cachedCommands.CloseAllReadersOnTransactionEnd();
			ExecuteAdoAction(() =>
			{
				currentTransaction.Rollback(savepoint);
			}, "Transaction Rollback " + savepoint);
			ExecuteAdoAction(() =>
			{
				currentTransaction.Save(savepoint);
			}, "Rollback Transaction Save " + savepoint);
		}
		catch (Exception ex) when (!(ex is NavBaseException))
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.TransactionEndError), ex);
		}
	}

	public int CheckEndTransaction(NavSqlEndTransactionType endType)
	{
		if (CurrentTransactionIsWriteTransaction)
		{
			EndTransaction(endType);
		}
		return 0;
	}

	public void SetLockTimeout(bool enableLockTimeout)
	{
		if (NavGlobal.AppDatabase == null || !NavGlobal.AppDatabase.SqlDatabasePropertiesRead)
		{
			throw new NotSupportedException("Before setting locktimeout the AppDatabase must be available");
		}
		int num = (enableLockTimeout ? NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout : (-1));
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(this))
		{
			num = ((num <= 0) ? (-1) : (num * 1000));
			string statement = string.Format(CultureInfo.InvariantCulture, "SET LOCK_TIMEOUT {0}", num);
			navSqlCommand.ExecuteNonQuery(statement);
		}
		lockTimeoutEnabled = enableLockTimeout;
	}

	public static bool CheckIfNavSqlExceptionIsTimeoutException(NavSqlException exception)
	{
		return exception.ErrorNumber == -2;
	}

	private void TryConnect()
	{
		if (connection == null)
		{
			return;
		}
		int num = ConnectionPool.ReconnectDelayTime;
		int num2 = 0;
		Exception ex = null;
		while (connection.State != ConnectionState.Open && num2 < 60000)
		{
			try
			{
				connection.Open();
				if (connection.State == ConnectionState.Open)
				{
					return;
				}
			}
			catch (TimeoutException ex2)
			{
				LogWriterHelper.LogExceptionEventWithTelemetry("000018Q", Category.SqlConnection, ex2, Verbosity.Warning, EventLogEntryType.Warning);
				ex = ex2;
			}
			catch (SqlException ex3) when (NavSqlReturnCode.IsTransientError(ex3.Number))
			{
				NavEnvironment.Instance.PerformanceCounterSetter.UpdateTransientErrorsPerformanceCounter();
				LogWriterHelper.LogExceptionEventWithTelemetry("000018R", Category.SqlConnection, ex3, Verbosity.Warning, EventLogEntryType.Warning);
				ex = ex3;
			}
			num2 += num;
			if (num2 >= 60000)
			{
				if (!(ex is SqlException ex4) || !NavSqlReturnCode.IsTransientError(ex4.Number))
				{
					break;
				}
				throw new NavCSideException(22924076, ex.Message, ex)
				{
					NavTenantId = TenantTraceId
				};
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000016H", Category.SqlConnection, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Connection attempt timed out. Trying again in {0} milliseconds.", num);
			Thread.Sleep(num);
			num = Convert.ToInt32((double)num * 1.5);
		}
		if ((connection != null && connection.State == ConnectionState.Open) || ex == null)
		{
			return;
		}
		throw ex;
	}

	public void SetKeepAlive(bool enabled, int keepAliveInterval = 0)
	{
		keepAlive?.Dispose();
		keepAlive = null;
		if (enabled)
		{
			int keepAliveTime = ((keepAliveInterval == 0) ? (ServerUserSettings.Instance.SqlConnectionKeepAliveInterval.TotalSeconds * 1000) : keepAliveInterval);
			keepAlive = new NavSqlConnectionKeepAlive(this, keepAliveTime);
			keepAlive.OnSuccess += (object sender, EventArgs e) =>
			{
				TickConnectionAsActive();
			};
		}
	}

	internal void Lock()
	{
		connectionLockRoot.Wait();
	}

	internal bool TryLock()
	{
		return connectionLockRoot.Wait(0);
	}

	internal void Unlock()
	{
		connectionLockRoot.Release();
	}

	public static bool TransactionIsWriteTransaction(TransactionType transactionType)
	{
		switch (transactionType)
		{
		case TransactionType.Browse:
		case TransactionType.Report:
			return false;
		case TransactionType.UpdateNoLocks:
		case TransactionType.Update:
		case TransactionType.Snapshot:
			return true;
		default:
			throw new NotSupportedException();
		}
	}

	public void ClearKnownXLockedTimestamps()
	{
		knownXLockedTimestamps.Clear();
	}

	public void AddKnownXLockedTimestamp(long timestamp)
	{
		if (knownXLockedTimestamps.Count > 100)
		{
			knownXLockedTimestamps.Clear();
		}
		knownXLockedTimestamps.Add(timestamp);
	}

	public void RemoveKnownXLockedTimestamp(long timestamp)
	{
		knownXLockedTimestamps.Remove(timestamp);
	}

	public bool IsKnownXLockedTimestampsIsKnownLatestTimestamp(long timestamp)
	{
		return knownXLockedTimestamps.Contains(timestamp);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				ForceCloseConnection();
				selfReference.ClearTarget();
				connectionLockRoot.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
