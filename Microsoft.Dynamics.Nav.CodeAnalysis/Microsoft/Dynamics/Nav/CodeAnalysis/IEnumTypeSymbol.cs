using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IEnumTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
}
