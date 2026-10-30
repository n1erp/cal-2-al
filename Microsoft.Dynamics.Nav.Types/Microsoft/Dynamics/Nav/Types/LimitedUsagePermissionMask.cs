using System;

namespace Microsoft.Dynamics.Nav.Types;

[Flags]
public enum LimitedUsagePermissionMask
{
	None = 0,
	Excluded = 0x20,
	Included = 0x40
}
