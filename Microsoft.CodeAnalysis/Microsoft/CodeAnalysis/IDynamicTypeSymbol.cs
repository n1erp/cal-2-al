using System;

namespace Microsoft.CodeAnalysis;

public interface IDynamicTypeSymbol : ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
}
