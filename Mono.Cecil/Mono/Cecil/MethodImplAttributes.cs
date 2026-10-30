using System;

namespace Mono.Cecil;

[Flags]
public enum MethodImplAttributes : ushort
{
	CodeTypeMask = 3,
	IL = 0,
	Native = 1,
	OPTIL = 2,
	Runtime = CodeTypeMask,
	ManagedMask = 4,
	Unmanaged = ManagedMask,
	Managed = 0,
	ForwardRef = 0x10,
	PreserveSig = 0x80,
	InternalCall = 0x1000,
	Synchronized = 0x20,
	NoOptimization = 0x40,
	NoInlining = 8,
	AggressiveInlining = 0x100
}
