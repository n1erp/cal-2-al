using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum CopyRecordOptions
{
	None = 0,
	CopyFiltersSortingMarked = 1,
	ShareTableData = 2
}
