using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NamedDatabaseConnectionQueueProvider(ITreeObject parent) : TreeObject(parent), IDatabaseConnectionQueueProvider, ITreeObject
{
	private readonly ConcurrentDictionary<string, NavSqlDatabaseConnectionQueue> queues = new ConcurrentDictionary<string, NavSqlDatabaseConnectionQueue>();

	public ICollection<NavSqlDatabaseConnectionQueue> GetAllQueues()
	{
		return queues.Values;
	}

	public NavSqlDatabaseConnectionQueue GetOrAddQueue(NavDatabase database)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		return queues.GetOrAdd(database.NormalizedDatabaseName, (string k) => new NavSqlDatabaseConnectionQueue(this));
	}

	public bool TryGetQueue(NavDatabase database, out NavSqlDatabaseConnectionQueue queue)
	{
		return queues.TryGetValue(database.NormalizedDatabaseName, out queue);
	}

	public IList<Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>> GetQueuesByName(string normalizedDatabaseName)
	{
		List<Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>> list = new List<Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>>();
		if (queues.TryGetValue(normalizedDatabaseName, out var value))
		{
			list.Add(new Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>(null, value));
		}
		return list;
	}
}
