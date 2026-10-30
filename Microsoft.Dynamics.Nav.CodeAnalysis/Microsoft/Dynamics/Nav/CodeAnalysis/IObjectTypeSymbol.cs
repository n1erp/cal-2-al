using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IObjectTypeSymbol : ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers
{
}
