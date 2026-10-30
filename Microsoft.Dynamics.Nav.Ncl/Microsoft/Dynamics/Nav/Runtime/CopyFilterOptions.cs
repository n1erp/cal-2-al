using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum CopyFilterOptions
{
	CopyFilter = 1,
	CopySorting = 2,
	CopyMarkedList = 4
}
