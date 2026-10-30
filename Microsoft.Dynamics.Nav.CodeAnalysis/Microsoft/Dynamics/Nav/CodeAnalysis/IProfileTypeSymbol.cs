using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IProfileTypeSymbol : IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol
{
}
