using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IQueryTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
}
