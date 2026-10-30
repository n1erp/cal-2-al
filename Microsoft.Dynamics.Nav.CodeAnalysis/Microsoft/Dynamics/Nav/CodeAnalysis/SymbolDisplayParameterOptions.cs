using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum SymbolDisplayParameterOptions
{
	None = 0,
	IncludeIsVar = 1,
	IncludeType = 2,
	IncludeName = 4,
	IncludeOptionalBrackets = 8,
	IncludeParams = 0x10,
	IncludeSymbolKind = 0x20,
	UseDeclarationSeparator = 0x40
}
