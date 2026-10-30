using System;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SyntaxRemoveOptions
{
	KeepNoTrivia = 0,
	KeepLeadingTrivia = 1,
	KeepTrailingTrivia = 2,
	KeepExteriorTrivia = KeepLeadingTrivia | KeepTrailingTrivia,
	KeepUnbalancedDirectives = 4,
	KeepDirectives = 8,
	KeepEndOfLine = 0x10,
	AddElasticMarker = 0x20
}
