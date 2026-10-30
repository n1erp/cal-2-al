using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum DeclarationModifiers
{
	None = 0,
	Abstract = 1,
	Sealed = 2,
	Static = 4,
	New = 8,
	Public = 0x10,
	Protected = 0x20,
	Internal = 0x40,
	ProtectedInternal = 0x80,
	Private = 0x100,
	PrivateProtected = 0x200,
	ReadOnly = 0x400,
	Const = 0x800,
	Volatile = 0x1000,
	Extern = 0x2000,
	Partial = 0x4000,
	Unsafe = 0x8000,
	Fixed = 0x10000,
	Virtual = 0x20000,
	Override = 0x40000,
	Indexer = 0x80000,
	Async = 0x100000,
	Ref = 0x200000,
	All = Abstract | Sealed | Static | New | Public | Protected | Internal | ProtectedInternal | Private | PrivateProtected | ReadOnly | Const | Volatile | Extern | Partial | Unsafe | Fixed | Virtual | Override | Indexer | Async | Ref,
	Unset = 0x400000,
	AccessibilityMask = 0x3F0
}
