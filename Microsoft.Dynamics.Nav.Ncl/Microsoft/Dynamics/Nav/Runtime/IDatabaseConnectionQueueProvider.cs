using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IDatabaseConnectionQueueProvider : ITreeObject
{
	ICollection<NavSqlDatabaseConnectionQueue> GetAllQueues();

	NavSqlDatabaseConnectionQueue GetOrAddQueue(NavDatabase database);

	bool TryGetQueue(NavDatabase database, out NavSqlDatabaseConnectionQueue queue);

	IList<Tuple<NavDatabase, NavSqlDatabaseConnectionQueue>> GetQueuesByName(string normalizedDatabaseName);
}
