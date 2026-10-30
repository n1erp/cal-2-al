using System;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface IMethodSymbolInternal : IMethodSymbol, ISymbol, IEquatable<ISymbol>
{
	bool IsIterator { get; }

	int CalculateLocalSyntaxOffset(int declaratorPosition, SyntaxTree declaratorTree);
}
