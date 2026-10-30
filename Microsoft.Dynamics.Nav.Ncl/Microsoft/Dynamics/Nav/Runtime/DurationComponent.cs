using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
internal enum DurationComponent
{
	Day = 0,
	Hour = 2,
	Minute = 4,
	Second = Hour | Minute,
	Millisecond = 8,
	Plural = 1
}
