using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class CacheSynchronizationChangeSet
{
	public long Timestamp { get; internal set; }

	public IEnumerable<TenantTableSet> TenantTableSets { get; internal set; }
}
