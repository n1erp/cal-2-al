using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayMiscellaneousOptions
{
	None = 0,
	UseSpecialTypes = 1,
	EscapeKeywordIdentifiers = 2,
	UseAsterisksInMultiDimensionalArrays = 4,
	UseErrorTypeSymbolName = 8,
	RemoveAttributeSuffix = 0x10,
	ExpandNullable = 0x20
}
