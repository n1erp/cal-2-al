using System;

namespace Microsoft.Dynamics.Nav.Types;

[Flags]
public enum PermissionModifiedMask
{
	None = 0,
	Read = 1,
	Insert = 2,
	Modify = 4,
	Delete = 8,
	Execute = 0x10,
	SecurityFilter = 0x20
}
