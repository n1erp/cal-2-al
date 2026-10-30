using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal interface ISymbolWithType : ISymbol, IEquatable<ISymbol>
{
	TypeSymbol Type { get; }
}
