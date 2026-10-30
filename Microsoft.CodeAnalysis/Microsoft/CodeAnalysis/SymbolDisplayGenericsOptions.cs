using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayGenericsOptions
{
	None = 0,
	IncludeTypeParameters = 1,
	IncludeTypeConstraints = 2,
	IncludeVariance = 4
}
