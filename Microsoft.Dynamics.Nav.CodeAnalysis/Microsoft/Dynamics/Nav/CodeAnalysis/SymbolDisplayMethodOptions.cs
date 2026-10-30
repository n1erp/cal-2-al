using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum SymbolDisplayMethodOptions
{
	None = 0,
	IncludeType = 1,
	IncludeAccessibility = 2,
	IncludeParameters = 4,
	IncludeKeyword = 8,
	IncludeModifiers = 0x10
}
