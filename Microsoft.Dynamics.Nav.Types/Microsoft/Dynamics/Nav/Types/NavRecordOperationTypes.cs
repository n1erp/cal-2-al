using System;

namespace Microsoft.Dynamics.Nav.Types;

[Flags]
public enum NavRecordOperationTypes
{
	Insert = 1,
	Modify = 2,
	Delete = 4,
	Rename = 8,
	Navigation = 0x10,
	Dirty = 0x20,
	InDatabase = 0x40,
	Bulk = 0x80,
	ModifyAll = Modify | Bulk,
	DeleteAll = Delete | Bulk,
	OutsideOfFilters = 0x100
}
