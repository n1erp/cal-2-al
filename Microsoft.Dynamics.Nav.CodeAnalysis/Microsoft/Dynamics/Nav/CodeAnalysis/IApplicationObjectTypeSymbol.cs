using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IApplicationObjectTypeSymbol : ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
}
