using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
internal enum LookupOptions
{
	Default = 0,
	Module = 2,
	MustBeInvocableIfMember = 8,
	MustBeInstance = 0x10,
	MustNotBeInstance = 0x20,
	MustBeOption = 0x40,
	AllMethods = 0x80,
	MemberAccess = 0x100,
	Trigger = 0x200,
	OnlyPropertyDeclarations = 0x400,
	ValidateSymbolCompilationScope = 0x800,
	MustBePageElement = 0x1000,
	MustBeFieldGroup = 0x2000
}
