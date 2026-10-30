using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum LocalSlotConstraints : byte
{
	None = 0,
	ByRef = 1,
	Pinned = 2
}
