using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayMemberOptions
{
	None = 0,
	IncludeType = 1,
	IncludeModifiers = 2,
	IncludeAccessibility = 4,
	IncludeExplicitInterface = 8,
	IncludeParameters = 0x10,
	IncludeContainingType = 0x20,
	IncludeConstantValue = 0x40,
	IncludeRef = 0x80
}
