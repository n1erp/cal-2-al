using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
internal enum PermissionMask
{
	None = 0,
	Read = 1,
	Insert = 2,
	Modify = 4,
	Delete = 8,
	Execute = 0x10,
	MaxDirect = Read | Insert | Modify | Delete | Execute
}
