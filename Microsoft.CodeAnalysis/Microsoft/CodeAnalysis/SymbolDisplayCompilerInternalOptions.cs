using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum SymbolDisplayCompilerInternalOptions
{
	None = 0,
	UseMetadataMethodNames = 1,
	UseArityForGenericTypes = 2,
	FlagMissingMetadataTypes = 4,
	IncludeScriptType = 8,
	IncludeCustomModifiers = 0x10,
	ReverseArrayRankSpecifiers = 0x20
}
