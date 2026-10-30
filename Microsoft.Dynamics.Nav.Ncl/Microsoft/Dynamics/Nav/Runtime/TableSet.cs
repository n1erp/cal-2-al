using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TableSet
{
	public string CompanyName { get; internal set; }

	public SortedSet<int> TableIds { get; internal set; }
}
