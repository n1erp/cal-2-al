using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ICodeunitTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	CodeunitSubtypeKind Subtype { get; }
}
