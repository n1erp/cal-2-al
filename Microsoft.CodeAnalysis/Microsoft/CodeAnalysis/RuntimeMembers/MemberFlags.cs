using System;

namespace Microsoft.CodeAnalysis.RuntimeMembers;

[Flags]
internal enum MemberFlags : byte
{
	Method = 1,
	Field = 2,
	Constructor = 4,
	PropertyGet = 8,
	Property = 0x10,
	KindMask = Method | Field | Constructor | PropertyGet | Property,
	Static = 0x20,
	Virtual = 0x40
}
