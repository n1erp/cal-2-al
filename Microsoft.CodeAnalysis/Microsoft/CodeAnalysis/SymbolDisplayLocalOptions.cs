using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayLocalOptions
{
	None = 0,
	IncludeType = 1,
	IncludeConstantValue = 2,
	IncludeRef = 4
}
