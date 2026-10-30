using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public delegate IDictionary<int, NavTableSyncSetup> GetTableSyncSetup(IEnumerable<int> tableIds);
