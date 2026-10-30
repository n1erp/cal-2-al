using System;

namespace Microsoft.Dynamics.Nav.Types;

[Flags]
public enum PermissionMask
{
	None = 0,
	Read = 1,
	Insert = 2,
	Modify = 4,
	Delete = 8,
	Execute = 0x10,
	MaxDirect = Read | Insert | Modify | Delete | Execute,
	IndirectRead = 0x20,
	IndirectInsert = 0x40,
	IndirectModify = 0x80,
	IndirectDelete = 0x100,
	IndirectExecute = 0x200,
	ReadDeleteInsertModify = Read | Insert | Modify | Delete | IndirectRead | IndirectInsert | IndirectModify | IndirectDelete,
	Max = ReadDeleteInsertModify | Execute | IndirectExecute,
	HasExpDate = 0x10000,
	IncludedLimitedUsage = 0x20000,
	ExcludedLimitedUsage = 0x40000
}
