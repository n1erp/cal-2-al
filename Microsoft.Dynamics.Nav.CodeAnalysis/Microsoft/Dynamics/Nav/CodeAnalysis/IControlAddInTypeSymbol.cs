using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface IControlAddInTypeSymbol : IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol
{
}
