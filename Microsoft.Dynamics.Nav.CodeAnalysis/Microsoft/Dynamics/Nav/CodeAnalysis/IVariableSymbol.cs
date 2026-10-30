using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IVariableSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }

	VariableKind VariableKind { get; }
}
