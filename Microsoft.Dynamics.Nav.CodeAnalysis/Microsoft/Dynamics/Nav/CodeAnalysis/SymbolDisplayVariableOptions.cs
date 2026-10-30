using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum SymbolDisplayVariableOptions
{
	None = 0,
	IncludeType = 2,
	IncludeName = 4,
	IncludeSymbolKind = 8
}
