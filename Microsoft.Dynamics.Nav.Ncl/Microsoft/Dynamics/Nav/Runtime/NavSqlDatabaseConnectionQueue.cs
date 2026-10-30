using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlDatabaseConnectionQueue : TreeObject
{
	public class ConnectionQueueLock : IDisposable
	{
		private readonly NavSqlDatabaseConnectionQueue queue;

		public ConnectionQueueLock(NavSqlDatabaseConnectionQueue queue)
		{
			this.queue = queue;
		}

		public void AcquireLock()
		{
			if (queue.semaphore == null)
			{
				throw new InvalidOperationException("Cannot acquire a disposed lock.");
			}
			queue.semaphore?.Wait();
		}

		public void Dispose()
		{
			queue.semaphore?.Release();
		}
	}

	private readonly LinkedList<NavSqlConnection> queue;

	private readonly HashSet<NavSqlConnection> registeredConnections;

	private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

	private readonly ConnectionQueueLock connectionQueueLock;

	private int connectionPoolBumperToken;

	internal int BumperToken => connectionPoolBumperToken;

	public IEnumerable<LinkedListNode<NavSqlConnection>> Queued
	{
		get
		{
			for (LinkedListNode<NavSqlConnection> node = queue.First; node != null; node = node.Next)
			{
				yield return node;
			}
		}
	}

	public int CountQueuedConnections => queue.Count;

	public int CountRegisteredConnection => registeredConnections.Count;

	public NavSqlDatabaseConnectionQueue(ITreeObject parent)
		: base(parent)
	{
		queue = new LinkedList<NavSqlConnection>();
		registeredConnections = new HashSet<NavSqlConnection>();
		connectionQueueLock = new ConnectionQueueLock(this);
	}

	public IDisposable Lock()
	{
		connectionQueueLock.AcquireLock();
		return connectionQueueLock;
	}

	public bool IsRegistered(NavSqlConnection connection)
	{
		return registeredConnections.Contains(connection);
	}

	public void Register(NavSqlConnection connection)
	{
		if (!registeredConnections.Add(connection))
		{
			throw new InvalidOperationException("Connection is already registered.");
		}
		connection.QueueBumperToken = BumperToken;
	}

	public void Unregister(NavSqlConnection connection, bool connectionMayBeQueued)
	{
		if (!registeredConnections.Remove(connection))
		{
			throw new InvalidOperationException("Connection is not registered.");
		}
		if (connectionMayBeQueued)
		{
			LinkedListNode<NavSqlConnection> linkedListNode = queue.Find(connection);
			if (linkedListNode != null)
			{
				queue.Remove(linkedListNode);
			}
		}
	}

	public void Enqueue(NavSqlConnection connection)
	{
		queue.AddLast(connection);
	}

	public void Push(NavSqlConnection connection)
	{
		queue.AddFirst(connection);
	}

	public NavSqlConnection Dequeue()
	{
		LinkedListNode<NavSqlConnection> first = queue.First;
		if (first != null)
		{
			queue.RemoveFirst();
			return first.Value;
		}
		return null;
	}

	public void RemoveFromQueue(LinkedListNode<NavSqlConnection> connectionNode)
	{
		queue.Remove(connectionNode);
	}

	public void IncrementBumperToken()
	{
		Interlocked.Increment(ref connectionPoolBumperToken);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				SemaphoreSlim semaphoreSlim = semaphore;
				semaphore = null;
				semaphoreSlim.Dispose();
				connectionQueueLock.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
