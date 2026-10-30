using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum IndexChangeTypes
{
	None = 0,
	Missing = 1,
	Excessive = 2,
	ClusterIndex = 4,
	IndexFields = 8,
	SiftExcessive = 0x10,
	SiftMissing = 0x20,
	SumFields = 0x40,
	PrimaryKey = 0x80,
	Unique = 0x100
}
