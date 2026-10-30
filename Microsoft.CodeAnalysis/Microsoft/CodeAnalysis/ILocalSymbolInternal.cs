using System;

namespace Microsoft.CodeAnalysis;

internal interface ILocalSymbolInternal : ILocalSymbol, ISymbol, IEquatable<ISymbol>
{
	bool IsImportedFromMetadata { get; }

	SynthesizedLocalKind SynthesizedKind { get; }

	SyntaxNode GetDeclaratorSyntax();
}
