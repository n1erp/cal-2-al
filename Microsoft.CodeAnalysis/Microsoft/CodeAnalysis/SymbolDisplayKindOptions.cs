using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayKindOptions
{
	None = 0,
	IncludeNamespaceKeyword = 1,
	IncludeTypeKeyword = 2,
	IncludeMemberKeyword = 4
}
