using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum SymbolDisplayReturnValueOptions
{
	None = 0,
	IncludeType = 2,
	IncludeName = 4,
	IncludeSymbolKind = 8
}
