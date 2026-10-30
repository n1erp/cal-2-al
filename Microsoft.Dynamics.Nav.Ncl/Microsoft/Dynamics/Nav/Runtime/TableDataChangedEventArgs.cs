using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableDataChangedEventArgs : EventArgs
{
	public IEnumerable<TableSet> Tables { get; private set; }

	public TableDataChangedEventArgs(IEnumerable<TableSet> tables)
	{
		Tables = tables;
	}
}
