using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlConnectionPool : ITreeObject, IDisposable
{
	internal delegate int SafeCounterUpdater(ref int counter);

	private TreeHandler tree;

	private const int DefaultReconnectionDelayTime = 10000;

	private const int InactiveConnectionSweepTime = 30000;

	internal const int SleepBeforeGettingNewConnection = 20;

	private const int RemoveInactiveConnectionsInCaseMaxLimitIsReached = 2;

	private const bool SqlUseMars = true;

	private const bool SqlUseIntegratedSecurity = true;

	private static int openConnectionCount;

	private static int tenantConnectionCount;

	private static int appConnectionCount;

	private readonly int inactiveConnectionSweepTime;

	private readonly NonReEntrantTimerWrapper inactiveConnectionRemovalTimer;

	private readonly string serverName;

	private readonly string normalizedServerName;

	private readonly string userName;

	private readonly ProtectedDatabasePassword password;

	private readonly NavSqlConnectionCollection connections;

	private readonly ConcurrentDictionary<string, NavSqlConnection> exclusiveConnections;

	public TreeHandler Tree => tree;

	internal NavSqlTransactionHistory TransactionHistory { get; private set; }

	public static int OpenConnectionCount => openConnectionCount;

	public static int TenantConnectionCount => tenantConnectionCount;

	public static int AppConnectionCount => appConnectionCount;

	public string ServerName => serverName;

	public string NormalizedServerName => normalizedServerName;

	public string UserName => userName;

	public ProtectedDatabasePassword Password => password;

	public int ReconnectDelayTime { get; set; }

	public NavSqlConnectionPool(string serverName, string userName, ProtectedDatabasePassword password)
		: this(serverName, userName, password, enableClosingOfIdleConnections: true)
	{
	}

	internal NavSqlConnectionPool(string serverName, string userName, ProtectedDatabasePassword password, bool enableClosingOfIdleConnections, int reconnectDelayTime = 10000, int inactiveConnectionSweepTime = 30000, Action<NavSqlConnection> onInactiveConnectionClosing = null)
	{
		tree = TreeHandler.CreateTreeRoot(this);
		this.serverName = serverName;
		normalizedServerName = NavDatabaseServer.NormalizeServerName(serverName);
		if (!string.IsNullOrEmpty(userName))
		{
			this.userName = userName;
			this.password = password;
		}
		else
		{
			this.userName = null;
			this.password = null;
		}
		connections = new NavSqlConnectionCollection(this);
		exclusiveConnections = new ConcurrentDictionary<string, NavSqlConnection>();
		this.inactiveConnectionSweepTime = inactiveConnectionSweepTime;
		ReconnectDelayTime = reconnectDelayTime;
		if (enableClosingOfIdleConnections)
		{
			inactiveConnectionRemovalTimer = new NonReEntrantTimerWrapper("inactiveConnectionRemovalTimer", CloseInactiveConnections, onInactiveConnectionClosing, this.inactiveConnectionSweepTime);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OW", Category.SqlConnection, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Created connection pool for {0}", serverName);
	}

	internal void CloseAllAvailableConnections()
	{
		connections.UnregisterAllIdleConnection(0uL);
	}

	internal (SqlConnectionStringBuilder ConnectionStringBuilder, SqlCredential Credential) CreateConnectionInfo(string databaseName)
	{
		return CreateConnectionStringBuilderFromSettings(ServerName, databaseName, userName, password, ServerUserSettings.Instance.SqlConnectionTimeout.TotalSeconds, ServerUserSettings.Instance.NetworkProtocol.Value, ServerUserSettings.Instance.TrustSqlServerCertificate.Value, ServerUserSettings.Instance.EnableSqlConnectionEncryption.Value);
	}

	public static (SqlConnectionStringBuilder ConnectionStringBuilder, SqlCredential Credential) CreateConnectionStringBuilderFromSettings(string serverName, string databaseName, string userName, ProtectedDatabasePassword password, int connectionTimeoutInSeconds, NetProtocolType netProtocolType, bool trustedCertificate, bool enableEncryption)
	{
		SqlCredential item = null;
		SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
		sqlConnectionStringBuilder.DataSource = serverName;
		sqlConnectionStringBuilder.InitialCatalog = databaseName;
		sqlConnectionStringBuilder.MultipleActiveResultSets = true;
		sqlConnectionStringBuilder.IntegratedSecurity = true;
		if (!string.IsNullOrEmpty(userName) && password != null)
		{
			password.SecureStringPassword.MakeReadOnly();
			item = new SqlCredential(userName, password.SecureStringPassword);
			sqlConnectionStringBuilder.IntegratedSecurity = false;
		}
		sqlConnectionStringBuilder.ConnectTimeout = connectionTimeoutInSeconds;
		sqlConnectionStringBuilder.ConnectRetryCount = 0;
		switch (netProtocolType)
		{
		case NetProtocolType.MultiProtocol:
			sqlConnectionStringBuilder.NetworkLibrary = "dbmsrpcn";
			break;
		case NetProtocolType.NamedPipes:
			sqlConnectionStringBuilder.NetworkLibrary = "dbnmpntw";
			break;
		case NetProtocolType.Sockets:
			sqlConnectionStringBuilder.NetworkLibrary = "dbmssocn";
			break;
		}
		sqlConnectionStringBuilder.TrustServerCertificate = trustedCertificate;
		sqlConnectionStringBuilder.Encrypt = enableEncryption;
		sqlConnectionStringBuilder.ApplicationName = ProductNames.ServerServiceDisplayName;
		sqlConnectionStringBuilder.Pooling = false;
		return (ConnectionStringBuilder: sqlConnectionStringBuilder, Credential: item);
	}

	internal static void UpdateOpenConnectionCount(NavDatabaseType kind, SafeCounterUpdater updater)
	{
		switch (kind)
		{
		case NavDatabaseType.Tenant:
			NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithOpenConnectionCountChanged(updater(ref tenantConnectionCount), NavPerformanceCounterEntry.TenantOpenConnectionCounter);
			break;
		case NavDatabaseType.Application:
			NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithOpenConnectionCountChanged(updater(ref appConnectionCount), NavPerformanceCounterEntry.AppOpenConnectionCounter);
			break;
		default:
			throw new InvalidOperationException("Invalid connection counter");
		case NavDatabaseType.External:
			break;
		}
		NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithOpenConnectionCountChanged(updater(ref openConnectionCount), NavPerformanceCounterEntry.OpenConnectionCounter);
	}

	private bool AnyExclusiveConnectionsToDatabase(NavDatabase database)
	{
		return exclusiveConnections.ContainsKey(database.NormalizedDatabaseName);
	}

	private NavSqlConnection GetExistingConnection(NavSqlConnection lastUsedConnection, TransactionType transactionType, NavDatabase database)
	{
		if (AnyExclusiveConnectionsToDatabase(database))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OZ", Category.SqlConnection, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Exclusive connections are active, returning null");
			return null;
		}
		NavSqlConnection navSqlConnection;
		while (true)
		{
			navSqlConnection = connections.DequeueConnection(database, transactionType, lastUsedConnection);
			if (navSqlConnection == null || !navSqlConnection.IsDead)
			{
				break;
			}
			CloseConnection(database, navSqlConnection, null);
		}
		return navSqlConnection;
	}

	public NavSqlConnection GetConnection(NavDatabase database, TransactionType transactionType, NavSqlConnection lastUsedSqlConnection)
	{
		NavSqlConnection navSqlConnection = GetExistingConnection(lastUsedSqlConnection, transactionType, database);
		if (navSqlConnection != null)
		{
			if (lastUsedSqlConnection != null)
			{
				UpdatePerformanceCountersWithPreferredConnectionCacheRequestResult(lastUsedSqlConnection == navSqlConnection);
			}
		}
		else if (!AnyExclusiveConnectionsToDatabase(database))
		{
			navSqlConnection = new NavSqlConnection(this, database);
			connections.Register(navSqlConnection, database);
			navSqlConnection.AddRef();
			if (lastUsedSqlConnection != null)
			{
				UpdatePerformanceCountersWithPreferredConnectionCacheRequestResult(tryGetResult: false);
			}
		}
		if (navSqlConnection == null)
		{
			throw new NavCSideException(22926090, string.Format(CultureInfo.CurrentCulture, Lang.TimeoutGettingAConnectionWhenExclusiveConnectionExists, database.DatabaseName, serverName))
			{
				NavTenantId = database.Tenant?.Id
			};
		}
		return navSqlConnection;
	}

	public void ReleaseConnection(NavSqlConnection connection, bool connectionMayBeReused, bool forceClose = false)
	{
		if (connection.InUse == 0)
		{
			connection.SetKeepAlive(enabled: false);
			if (connection.IsExclusiveConnection)
			{
				ReleaseExclusiveConnection(connection);
			}
			NavDatabase database = connection.Database;
			if (forceClose || connection.IsDead || database == null)
			{
				CloseConnection(database, connection, null);
			}
			else if (connectionMayBeReused)
			{
				connections.EnqueueConnection(connection, database);
			}
			else
			{
				connections.PushConnection(connection, database);
			}
		}
	}

	public void ReleaseExclusiveConnection(NavSqlConnection exclusiveConnection)
	{
		if (!exclusiveConnection.IsExclusiveConnection)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000P4", Category.SqlConnection, new InvalidOperationException("Not releasing an exclusive connection"));
		}
		if (exclusiveConnections.TryRemove(exclusiveConnection.Database.NormalizedDatabaseName, out var _))
		{
			exclusiveConnection.IsExclusiveConnection = false;
		}
	}

	public NavSqlConnection GetExclusiveConnection(NavDatabase database, TransactionType transactionType)
	{
		long ticks = DateTime.UtcNow.Ticks;
		NavSqlConnection navSqlConnection = null;
		int num = ((NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout < 0) ? int.MaxValue : NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout);
		TimeSpan timeSpan;
		do
		{
			if (navSqlConnection == null)
			{
				navSqlConnection = exclusiveConnections.GetOrAdd(database.NormalizedDatabaseName, (string databaseName) => GetConnection(database, transactionType, null));
				navSqlConnection.IsExclusiveConnection = true;
			}
			if (navSqlConnection != null)
			{
				while (true)
				{
					NavSqlConnection navSqlConnection2 = connections.DequeueConnectionFromAnyQueue(database.NormalizedDatabaseName, database);
					if (navSqlConnection2 == null)
					{
						break;
					}
					CloseConnection(navSqlConnection2.Database, navSqlConnection2, null);
				}
				if (connections.IsConnectionExclusive(navSqlConnection, database))
				{
					return navSqlConnection;
				}
			}
			Thread.Sleep(20);
			timeSpan = new TimeSpan(DateTime.UtcNow.Ticks - ticks);
		}
		while (!(timeSpan.TotalSeconds >= (double)num));
		navSqlConnection?.ReleaseRef(connectionMayBeReused: false);
		throw new NavCSideException(22926090, string.Format(CultureInfo.CurrentCulture, Lang.TimeoutCouldNotObtainExclusiveConnection, database.DatabaseName, serverName))
		{
			NavTenantId = database.Tenant?.Id
		};
	}

	public void CloseInactiveConnections(object param)
	{
		Action<NavSqlConnection> action = (Action<NavSqlConnection>)param;
		List<NavSqlConnection> list = connections.UnregisterAllIdleConnection((ulong)ServerUserSettings.Instance.SqlConnectionIdleTimeout.Value.TotalMilliseconds);
		foreach (NavSqlConnection item in list)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001HU", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Closing idle SQL connection ID: {0}. Idle time: {1}", item.Id, item.ConnectionIdleTime);
			action?.Invoke(item);
			item.Dispose();
		}
	}

	public void EnableTransactionHistory()
	{
		if (TransactionHistory == null)
		{
			TransactionHistory = new NavSqlTransactionHistory();
		}
	}

	public void DisableTransactionHistory()
	{
		TransactionHistory = null;
	}

	internal void CloseConnectionsOnDatabase(string normalizedDatabaseName, Action<NavSqlConnection> onClosingAction = null)
	{
		while (true)
		{
			List<NavSqlConnection> list = connections.UnregisterAllIdleConnection(0uL);
			foreach (NavSqlConnection item in list)
			{
				onClosingAction?.Invoke(item);
				item.Dispose();
			}
			if (connections.CountRegisteredConnections(normalizedDatabaseName) != 0)
			{
				Thread.Sleep(20);
				continue;
			}
			break;
		}
	}

	private static void UpdatePerformanceCountersWithPreferredConnectionCacheRequestResult(bool tryGetResult)
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPreferredConnectionCacheRequestResult(tryGetResult);
	}

	public void IncrementBumperToken(NavDatabase database)
	{
		connections.IncrementBumperToken(database);
	}

	public void IncrementBumperToken(string databaseName)
	{
		connections.IncrementBumperToken(databaseName);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000P6", Category.SqlConnection, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Disposing connection pool");
		if (disposing)
		{
			inactiveConnectionRemovalTimer?.Dispose();
			if (tree != null)
			{
				tree.DisposeAllChildren();
				tree.Dispose();
				tree = null;
			}
		}
	}

	internal void RemoveInactiveConnections(NavDatabase database, int numberOfConnectionsToRemove = 2)
	{
		int num = 0;
		for (int i = 0; i < numberOfConnectionsToRemove; i++)
		{
			NavSqlConnection navSqlConnection = connections.DequeueConnection(database);
			if (navSqlConnection == null)
			{
				break;
			}
			num++;
			CloseConnection(database, navSqlConnection, null);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000P7", Category.SqlConnection, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Removed {0} inactive connections from the pool", num);
	}

	private void CloseConnection(NavDatabase database, NavSqlConnection connectionToClose, Action<NavSqlConnection> onClosingAction)
	{
		if (connectionToClose.InUse > 1)
		{
			throw new InvalidOperationException("The connection is being used and therefor cannot be closed.");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000P9", Category.SqlConnection, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Closing connection {0}", connectionToClose.Id);
		onClosingAction?.Invoke(connectionToClose);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000O1", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Closing a SQL connection");
		connections.Unregister(connectionToClose, database);
		connectionToClose.Dispose();
	}

	internal int AllConnectionsCount(NavDatabase database)
	{
		return connections.CountRegisteredConnections(database);
	}
}
