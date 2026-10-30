using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlConnectionCollection : TreeObject
{
	private readonly IDatabaseConnectionQueueProvider connectionQueueProvider;

	public NavSqlConnectionCollection(ITreeObject parent)
		: base(parent)
	{
		if (ServerUserSettings.Instance.DisableMultipleTenantSqlConnection.Value)
		{
			connectionQueueProvider = new InstanceDatabaseConnectionQueueProvider(this);
		}
		else
		{
			connectionQueueProvider = new NamedDatabaseConnectionQueueProvider(this);
		}
	}

	public void Register(NavSqlConnection connection, NavDatabase database)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		NavSqlDatabaseConnectionQueue orAddQueue = connectionQueueProvider.GetOrAddQueue(database);
		using (orAddQueue.Lock())
		{
			orAddQueue.Register(connection);
		}
	}

	public void Unregister(NavSqlConnection connection, NavDatabase database)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (database == null)
		{
			foreach (NavSqlDatabaseConnectionQueue allQueue in connectionQueueProvider.GetAllQueues())
			{
				using (allQueue.Lock())
				{
					if (allQueue.IsRegistered(connection))
					{
						allQueue.Unregister(connection, connectionMayBeQueued: true);
					}
				}
			}
			return;
		}
		if (connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			using (queue.Lock())
			{
				queue.Unregister(connection, connectionMayBeQueued: true);
				return;
			}
		}
		throw new InvalidOperationException("No connection queue where found for the given database.");
	}

	public void EnqueueConnection(NavSqlConnection connection, NavDatabase database)
	{
		EnqueuePushConnection(connection, database, enqueue: true);
	}

	public void PushConnection(NavSqlConnection connection, NavDatabase database)
	{
		EnqueuePushConnection(connection, database, enqueue: false);
	}

	private void EnqueuePushConnection(NavSqlConnection connection, NavDatabase database, bool enqueue)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		if (connection.InUse > 0)
		{
			throw new ArgumentException("The SQL connection is in use and cannot be released to the pool.");
		}
		if (!connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			return;
		}
		using (queue.Lock())
		{
			connection.Database = null;
			if (enqueue)
			{
				queue.Enqueue(connection);
			}
			else
			{
				queue.Push(connection);
			}
		}
	}

	public NavSqlConnection DequeueConnection(NavDatabase database)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		NavSqlConnection navSqlConnection = null;
		if (connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			using (queue.Lock())
			{
				navSqlConnection = queue.Dequeue();
			}
			if (navSqlConnection != null)
			{
				navSqlConnection.Database = database;
				navSqlConnection.AddRef();
			}
		}
		return navSqlConnection;
	}

	public NavSqlConnection DequeueConnectionFromAnyQueue(string normalizedDatabaseName, NavDatabase database)
	{
		if (string.IsNullOrEmpty(normalizedDatabaseName))
		{
			throw new ArgumentNullException("normalizedDatabaseName");
		}
		NavSqlConnection navSqlConnection = null;
		foreach (Tuple<NavDatabase, NavSqlDatabaseConnectionQueue> item in connectionQueueProvider.GetQueuesByName(normalizedDatabaseName))
		{
			using (item.Item2.Lock())
			{
				navSqlConnection = item.Item2.Dequeue();
			}
			if (navSqlConnection != null)
			{
				navSqlConnection.Database = item.Item1 ?? database;
				navSqlConnection.AddRef();
				return navSqlConnection;
			}
		}
		return null;
	}

	public NavSqlConnection DequeueConnection(NavDatabase database, TransactionType transactionType, NavSqlConnection lastUsedConnection)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		bool writeTransaction = NavSqlConnection.TransactionIsWriteTransaction(transactionType);
		LinkedListNode<NavSqlConnection> linkedListNode = null;
		if (connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			using (queue.Lock())
			{
				LinkedListNode<NavSqlConnection> linkedListNode2 = null;
				foreach (LinkedListNode<NavSqlConnection> item in queue.Queued)
				{
					NavSqlConnection value = item.Value;
					if (value.QueueBumperToken != queue.BumperToken)
					{
						continue;
					}
					if (lastUsedConnection == value)
					{
						linkedListNode = item;
						break;
					}
					if (linkedListNode != null)
					{
						continue;
					}
					if (ConnectionHasSameTransactionKind(writeTransaction, value))
					{
						linkedListNode = item;
						if (lastUsedConnection == null)
						{
							break;
						}
					}
					else if (linkedListNode2 == null)
					{
						linkedListNode2 = item;
					}
				}
				linkedListNode = linkedListNode ?? linkedListNode2;
				if (linkedListNode != null)
				{
					queue.RemoveFromQueue(linkedListNode);
				}
			}
			if (linkedListNode != null)
			{
				linkedListNode.Value.Database = database;
				linkedListNode.Value.AddRef();
			}
		}
		return linkedListNode?.Value;
	}

	public List<NavSqlConnection> UnregisterAllIdleConnection(ulong connectionIdleTime)
	{
		List<NavSqlConnection> list = new List<NavSqlConnection>();
		foreach (NavSqlDatabaseConnectionQueue allQueue in connectionQueueProvider.GetAllQueues())
		{
			using (allQueue.Lock())
			{
				foreach (LinkedListNode<NavSqlConnection> item in allQueue.Queued.Where((LinkedListNode<NavSqlConnection> c) => c.Value.ConnectionIdleTime > connectionIdleTime).ToList())
				{
					allQueue.RemoveFromQueue(item);
					allQueue.Unregister(item.Value, connectionMayBeQueued: false);
					list.Add(item.Value);
				}
			}
		}
		return list;
	}

	public bool IsConnectionExclusive(NavSqlConnection exclusiveConnection, NavDatabase database)
	{
		if (exclusiveConnection == null)
		{
			throw new ArgumentNullException("exclusiveConnection");
		}
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		if (!exclusiveConnection.IsExclusiveConnection)
		{
			throw new ArgumentException("The given connection is not exclusive.");
		}
		foreach (Tuple<NavDatabase, NavSqlDatabaseConnectionQueue> item in connectionQueueProvider.GetQueuesByName(database.NormalizedDatabaseName))
		{
			using (item.Item2.Lock())
			{
				if ((item.Item2.CountRegisteredConnection == 1 && item.Item2.IsRegistered(exclusiveConnection)) || item.Item2.CountRegisteredConnection == 0)
				{
					continue;
				}
				return false;
			}
		}
		return true;
	}

	public int CountRegisteredConnections(NavDatabase database)
	{
		int result = 0;
		if (connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			using (queue.Lock())
			{
				result = queue.CountRegisteredConnection;
			}
		}
		return result;
	}

	public int CountRegisteredConnections(string normalizedDatabaseName)
	{
		int num = 0;
		foreach (Tuple<NavDatabase, NavSqlDatabaseConnectionQueue> item in connectionQueueProvider.GetQueuesByName(normalizedDatabaseName))
		{
			using (item.Item2.Lock())
			{
				num += item.Item2.CountRegisteredConnection;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ConnectionHasSameTransactionKind(bool writeTransaction, NavSqlConnection connection)
	{
		if (!writeTransaction || (connection.CurrentTransaction != null && !connection.CurrentTransactionIsWriteTransaction))
		{
			if (!writeTransaction && connection.CurrentTransaction != null)
			{
				return !connection.CurrentTransactionIsWriteTransaction;
			}
			return false;
		}
		return true;
	}

	public void IncrementBumperToken(NavDatabase database)
	{
		if (connectionQueueProvider.TryGetQueue(database, out var queue))
		{
			queue.IncrementBumperToken();
		}
	}

	public void IncrementBumperToken(string databaseName)
	{
		foreach (Tuple<NavDatabase, NavSqlDatabaseConnectionQueue> item in connectionQueueProvider.GetQueuesByName(NavDatabase.NormalizeDatabaseName(databaseName)))
		{
			item.Item2.IncrementBumperToken();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
