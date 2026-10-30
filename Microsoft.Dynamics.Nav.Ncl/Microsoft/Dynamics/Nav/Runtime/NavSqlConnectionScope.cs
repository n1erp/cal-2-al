using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlConnectionScope : IDisposable
{
	private readonly NavSession parentNavSession;

	private readonly int ownerThreadId;

	private NavSqlConnectionScope childScope;

	private NavSqlConnection scopedNavSqlConnection;

	private readonly bool tryReuseConnection;

	private bool isDisposed;

	private NavSqlConnectionScope parentScope;

	private int disposeLock;

	internal static NavSqlConnectionScope Current
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			if (session != null)
			{
				return session.CurrentSqlConnectionScope;
			}
			return NavThreadLocalStorage.Current.CurrentSqlConnectionScope;
		}
	}

	protected NavSqlConnectionScope ParentScope => parentScope;

	internal virtual bool IsTestScope => false;

	internal virtual bool IsUpgradeScope => false;

	protected bool IsDisposed => isDisposed;

	public NavSqlConnection Connection
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("NavSqlConnectionScope");
			}
			return scopedNavSqlConnection;
		}
	}

	private void SetCurrent(NavSqlConnectionScope newCurrent)
	{
		if (parentNavSession != null)
		{
			parentNavSession.CurrentSqlConnectionScope = newCurrent;
		}
		else
		{
			NavThreadLocalStorage.Current.CurrentSqlConnectionScope = newCurrent;
		}
	}

	protected NavSqlConnectionScope(NavDatabase database, bool requireNewConnection, TransactionType transactionType, bool tryReuseConnection, NavSqlConnection lastUsedSqlConnection = null, bool requireExclusiveConnection = false)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		this.tryReuseConnection = tryReuseConnection;
		ownerThreadId = Thread.CurrentThread.ManagedThreadId;
		parentScope = Current;
		if (parentScope != null)
		{
			parentScope.childScope = this;
			parentNavSession = parentScope.parentNavSession;
		}
		else
		{
			NavSession session = NavCurrentThread.Session;
			parentNavSession = session;
		}
		try
		{
			NavSqlConnectionScope navSqlConnectionScope;
			if ((requireNewConnection | requireExclusiveConnection) || (navSqlConnectionScope = FindAncestorWithDatabase(database)) == null)
			{
				AcquireSqlConnectionFromPool(database, requireExclusiveConnection, transactionType, lastUsedSqlConnection);
				return;
			}
			scopedNavSqlConnection = navSqlConnectionScope.Connection;
			SetCurrent(this);
			scopedNavSqlConnection.AddRef();
			Connection.CheckBeginTransaction(transactionType);
		}
		catch (Exception)
		{
			if (parentScope != null)
			{
				parentScope.childScope = null;
			}
			SetCurrent(parentScope);
			if (scopedNavSqlConnection != null)
			{
				scopedNavSqlConnection.ReleaseRef(connectionMayBeReused: false);
			}
			throw;
		}
	}

	[Conditional("DEBUG_SQLCONNECTIONTRACE")]
	private void SendCreateSqlConnectionTraceDebug()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007P", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Dispose SqlConnectionScope, IsDisposed={0}, sessionId={1}, database={2}, threadname={3}, ownerThread={4}, stacktrace={5}", IsDisposed, parentNavSession?.Id, scopedNavSqlConnection?.Database?.DatabaseName, Thread.CurrentThread.Name ?? string.Empty, ownerThreadId, new StackTrace(fNeedFileInfo: true).ToString());
	}

	protected NavSqlConnectionScope FindAncestorWithDatabase(NavDatabase database)
	{
		if (ParentScope == null)
		{
			return null;
		}
		if (ParentScope.Connection == null)
		{
			throw new InvalidOperationException("A scope must always have a connection.");
		}
		if (ParentScope.Connection.Database == database)
		{
			return ParentScope;
		}
		return ParentScope.FindAncestorWithDatabase(database);
	}

	protected NavSqlConnectionScope FindAncestorWithConnection(NavSqlConnection connection)
	{
		if (ParentScope == null)
		{
			return null;
		}
		if (ParentScope.Connection == connection)
		{
			return ParentScope;
		}
		return ParentScope.FindAncestorWithConnection(connection);
	}

	public void EnsureConnectionIsLockedInScopeChain(NavSqlConnection connection)
	{
		bool flag = false;
		for (NavSqlConnectionScope navSqlConnectionScope = this; navSqlConnectionScope != null; navSqlConnectionScope = navSqlConnectionScope.parentScope)
		{
			if (navSqlConnectionScope.Connection == connection)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			throw new InvalidOperationException("connection must be locked in current scope chain");
		}
	}

	private void AcquireSqlConnectionFromPool(NavDatabase database, bool requireExclusiveConnection, TransactionType transactionType, NavSqlConnection lastUsedSqlConnection)
	{
		do
		{
			scopedNavSqlConnection = (requireExclusiveConnection ? database.DatabaseServer.SqlConnectionPool.GetExclusiveConnection(database, transactionType) : database.DatabaseServer.SqlConnectionPool.GetConnection(database, transactionType, lastUsedSqlConnection));
			SetCurrent(this);
			scopedNavSqlConnection.OpenConnectionIfNeeded();
			try
			{
				if (scopedNavSqlConnection.CurrentTransaction != null && scopedNavSqlConnection.CurrentTransaction.Connection == null)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007AG", Category.SqlConnection, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SQL Transaction died while sitting in the pool. Connection ID: {0}", scopedNavSqlConnection.Id);
					scopedNavSqlConnection.EndTransaction(NavSqlEndTransactionType.EndedByServer);
				}
				if (!scopedNavSqlConnection.CheckBeginTransaction(transactionType) && scopedNavSqlConnection.ConnectionIdleTime >= (ulong)database.DatabaseServer.SqlConnectionPool.ReconnectDelayTime)
				{
					scopedNavSqlConnection.EndTransaction(NavSqlEndTransactionType.Commit);
					scopedNavSqlConnection.CheckBeginTransaction(transactionType);
				}
				if (database.DatabaseType == NavDatabaseType.Application && !FeatureSwitches.Instance.DisableKeepAliveOnAppDB)
				{
					scopedNavSqlConnection.SetKeepAlive(enabled: true);
				}
			}
			catch (DeadSqlConnectionException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007BY", Category.SqlConnection, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata("SQL Connection died while sitting in the pool. Connection ID: " + scopedNavSqlConnection.Id));
				scopedNavSqlConnection.ReleaseRef(connectionMayBeReused: false, forceClose: true);
				scopedNavSqlConnection = null;
			}
			catch (NavCSideException ex)
			{
				if (ex.ErrorCode == 22924076)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007AH", Category.SqlConnection, ex, Verbosity.Warning, DiagnosticParameter.SystemMetadata("SQL Connection died while sitting in the pool. Connection ID: " + scopedNavSqlConnection.Id));
					scopedNavSqlConnection.ReleaseRef(connectionMayBeReused: false, forceClose: true);
					scopedNavSqlConnection = null;
					continue;
				}
				throw;
			}
			catch (NavSqlException ex2) when (ex2.TransactionRolledBack)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007AI", Category.SqlConnection, ex2, Verbosity.Warning, DiagnosticParameter.SystemMetadata("SQL Transaction died while sitting in the pool. Connection ID: " + scopedNavSqlConnection.Id));
				scopedNavSqlConnection.EndTransaction(NavSqlEndTransactionType.EndedByServer);
				scopedNavSqlConnection.ReleaseRef(connectionMayBeReused: false);
				scopedNavSqlConnection = null;
			}
			catch (NavSqlException ex3)
			{
				if (ex3.InnerException == null)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000P2", Category.SqlConnection, ex3, Verbosity.Error, DiagnosticParameter.SystemMetadata("Connection ID: " + scopedNavSqlConnection.Id));
					throw;
				}
				if (!(ex3.InnerException is SqlException ex4) || !NavSqlReturnCode.IsSqlAzureConnectionLimitReached(ex4.Number))
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000P3", Category.SqlConnection, ex3, Verbosity.Error, DiagnosticParameter.SystemMetadata("Connection ID: " + scopedNavSqlConnection.Id));
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000NZ", Category.Sql, ex4, Verbosity.Error, DiagnosticParameter.SystemMetadata("Connection ID: " + scopedNavSqlConnection.Id));
				database.DatabaseServer.SqlConnectionPool.RemoveInactiveConnections(database);
				Thread.Sleep(20);
			}
		}
		while (scopedNavSqlConnection == null);
	}

	public static NavSqlConnectionScope Create(NavDatabase database)
	{
		return Create(database, TransactionType.Browse);
	}

	public static NavSqlConnectionScope Create(NavDatabase database, ref IReference<NavSqlConnection> lastUsedSqlConnection)
	{
		return Create(database, TransactionType.Browse, ref lastUsedSqlConnection);
	}

	public static NavSqlConnectionScope CreateForceNewConnection(NavDatabase database)
	{
		return CreateForceNewConnection(database, TransactionType.Browse);
	}

	public static NavSqlConnectionScope CreateForceNewConnection(NavDatabase database, ref IReference<NavSqlConnection> lastUsedSqlConnection)
	{
		return CreateForceNewConnection(database, TransactionType.Browse, ref lastUsedSqlConnection);
	}

	public static NavSqlConnectionScope CreateForceNewConnection(NavDatabase database, TransactionType transactionType)
	{
		return new NavSqlConnectionScope(database, requireNewConnection: true, transactionType, tryReuseConnection: false);
	}

	public static NavSqlConnectionScope CreateForceNewConnection(NavDatabase database, bool updateOrlockingActivity)
	{
		TransactionType transactionType = ((!updateOrlockingActivity) ? TransactionType.Browse : TransactionType.UpdateNoLocks);
		return CreateForceNewConnection(database, transactionType);
	}

	public static NavSqlConnectionScope CreateForceNewConnection(NavDatabase database, TransactionType transactionType, ref IReference<NavSqlConnection> lastUsedSqlConnection)
	{
		NavSqlConnectionScope navSqlConnectionScope = new NavSqlConnectionScope(database, requireNewConnection: true, transactionType, tryReuseConnection: true, lastUsedSqlConnection?.Target);
		lastUsedSqlConnection = navSqlConnectionScope.Connection.GetReference();
		return navSqlConnectionScope;
	}

	public static NavSqlConnectionScope CreateExclusiveConnection(NavDatabase database, TransactionType transactionType)
	{
		return new NavSqlConnectionScope(database, requireNewConnection: true, transactionType, tryReuseConnection: false, null, requireExclusiveConnection: true);
	}

	public static NavSqlConnectionScope Create(NavDatabase database, TransactionType transactionType, ref IReference<NavSqlConnection> lastUsedSqlConnection)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		NavSession session = NavCurrentThread.Session;
		if (session != null && session.CurrentSqlConnectionScope != null)
		{
			if (session.CurrentSqlConnectionScope.IsTestScope)
			{
				return CreateTestScope(database, transactionType);
			}
			if (session.CurrentSqlConnectionScope.IsUpgradeScope)
			{
				return CreateUpgradeScope(database, transactionType);
			}
		}
		NavSqlConnectionScope navSqlConnectionScope = new NavSqlConnectionScope(database, requireNewConnection: false, transactionType, tryReuseConnection: true, lastUsedSqlConnection?.Target);
		lastUsedSqlConnection = navSqlConnectionScope.Connection.GetReference();
		return navSqlConnectionScope;
	}

	public static NavSqlConnectionScope Create(NavDatabase database, TransactionType transactionType)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		NavSession session = NavCurrentThread.Session;
		if (session != null && session.CurrentSqlConnectionScope != null)
		{
			if (session.CurrentSqlConnectionScope.IsTestScope)
			{
				return CreateTestScope(database, transactionType);
			}
			if (session.CurrentSqlConnectionScope.IsUpgradeScope)
			{
				return CreateUpgradeScope(database, transactionType);
			}
		}
		return new NavSqlConnectionScope(database, requireNewConnection: false, transactionType, tryReuseConnection: false);
	}

	public static NavSqlConnectionScope Create(NavDatabase database, bool updateOrlockingActivity)
	{
		TransactionType transactionType = ((!updateOrlockingActivity) ? TransactionType.Browse : TransactionType.UpdateNoLocks);
		return Create(database, transactionType);
	}

	public static NavSqlConnectionScope Create(NavDatabase database, bool updateOrlockingActivity, ref IReference<NavSqlConnection> lastUsedSqlConnection)
	{
		TransactionType transactionType = ((!updateOrlockingActivity) ? TransactionType.Browse : TransactionType.UpdateNoLocks);
		return Create(database, transactionType, ref lastUsedSqlConnection);
	}

	internal static NavSqlConnectionTestScope CreateTestScope(NavDatabase database, TransactionType transactionType)
	{
		return new NavSqlConnectionTestScope(database, requireNewConnection: false, transactionType);
	}

	internal static NavSqlConnectionUpgradeScope CreateUpgradeScope(NavDatabase database, TransactionType transactionType)
	{
		return new NavSqlConnectionUpgradeScope(database, requireNewConnection: false, transactionType);
	}

	internal static NavSqlConnectionRootTestScope CreateRootTestScope(NavDatabase database)
	{
		return new NavSqlConnectionRootTestScope(database, requireNewConnection: false, TransactionType.UpdateNoLocks);
	}

	internal static NavSqlConnectionRootUpgradeScope CreateRootUpgradeScope(NavDatabase database)
	{
		return new NavSqlConnectionRootUpgradeScope(database);
	}

	public void TransactionEnd(NavSqlEndTransactionType endType)
	{
		TryEndTransactionAndDispose(endType);
	}

	protected virtual void TryEndTransactionAndDispose(NavSqlEndTransactionType endType)
	{
		try
		{
			Connection.CheckEndTransaction(endType);
		}
		finally
		{
			Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!(!IsDisposed & disposing))
		{
			return;
		}
		if (Interlocked.Increment(ref disposeLock) > 1)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009J", Category.Runtime, Verbosity.Warning, "Dispose SqlConnectionScope REPEATED DISPOSE, IsDisposed:{0}, sessionId:{1}, database:{2}, threadname:{3}, ownerThread:{4}, stacktrace:{5}", DiagnosticParameter.SystemMetadata(IsDisposed), DiagnosticParameter.SystemMetadata(parentNavSession?.Id), DiagnosticParameter.Oii(scopedNavSqlConnection?.DatabaseName), DiagnosticParameter.SystemMetadata(Thread.CurrentThread.Name), DiagnosticParameter.SystemMetadata(ownerThreadId), DiagnosticParameter.SystemMetadata(Environment.StackTrace));
			return;
		}
		try
		{
			OnDisposing();
		}
		finally
		{
			if (childScope != null)
			{
				childScope.parentScope = parentScope;
			}
			if (ParentScope != null)
			{
				ParentScope.childScope = childScope;
			}
			scopedNavSqlConnection.ReleaseRef(tryReuseConnection);
			NavSqlConnectionScope navSqlConnectionScope = childScope ?? parentScope;
			if (navSqlConnectionScope != null && navSqlConnectionScope.IsDisposed)
			{
				SetCurrent(null);
			}
			else
			{
				SetCurrent(navSqlConnectionScope);
			}
			if (scopedNavSqlConnection.InUse == 0)
			{
				while (navSqlConnectionScope != null)
				{
					navSqlConnectionScope = navSqlConnectionScope.parentScope;
				}
			}
			scopedNavSqlConnection = null;
			isDisposed = true;
		}
	}

	[Conditional("DEBUG_SQLCONNECTIONTRACE")]
	private void SendDisposingSqlConnectionTraceDebug()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007Q", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Dispose SqlConnectionScope, IsDisposed={0}, sessionId={1}, database={2}, threadname={3}, ownerThread={4}, stacktrace={5}", IsDisposed, parentNavSession?.Id, scopedNavSqlConnection?.Database?.DatabaseName, Thread.CurrentThread.Name ?? string.Empty, ownerThreadId, new StackTrace(fNeedFileInfo: true).ToString());
	}

	protected virtual void OnDisposing()
	{
		if (scopedNavSqlConnection.InUse == 1)
		{
			scopedNavSqlConnection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}
	}
}
