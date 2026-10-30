using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPageTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IPageBaseTypeSymbol, IObjectTypeSymbol, ISymbolWithTriggers
{
}
