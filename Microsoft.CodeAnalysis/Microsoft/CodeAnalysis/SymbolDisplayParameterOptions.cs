using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayParameterOptions
{
	None = 0,
	IncludeExtensionThis = 1,
	IncludeParamsRefOut = 2,
	IncludeType = 4,
	IncludeName = 8,
	IncludeDefaultValue = 0x10,
	IncludeOptionalBrackets = 0x20
}
