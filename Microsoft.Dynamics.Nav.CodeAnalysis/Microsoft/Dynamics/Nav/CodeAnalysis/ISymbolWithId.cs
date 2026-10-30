using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ISymbolWithId : ISymbol, IEquatable<ISymbol>
{
	int Id { get; }
}
