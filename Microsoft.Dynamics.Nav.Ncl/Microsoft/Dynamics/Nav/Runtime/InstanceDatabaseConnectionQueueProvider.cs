using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class InstanceDatabaseConnectionQueueProvider(ITreeObject parent) : TreeObject(parent), IDatabaseConnectionQueueProvider, ITreeObject
{
	private readonly ConcurrentDictionary<NavDatabase, NavSqlDatabaseConnectionQueue> queues = new ConcurrentDictionary<NavDatabase, NavSqlDatabaseConnectionQueue>();

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
		return queues.GetOrAdd(database, (NavDatabase k) => new NavSqlDatabaseConnectionQueue(this));
	}

	public bool TryGetQueue(NavDatabase database, out NavSqlDatabaseConnectionQueue queue)
	{
		return queues.TryGetValue(database, out queue);
	}

	public IList<Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>> GetQueuesByName(string normalizedDatabaseName)
	{
		return (from kv in queues
			where kv.Key.NormalizedDatabaseName.Equals(normalizedDatabaseName, StringComparison.Ordinal)
			select new Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>(kv.Key, kv.Value)).ToList();
	}
}
