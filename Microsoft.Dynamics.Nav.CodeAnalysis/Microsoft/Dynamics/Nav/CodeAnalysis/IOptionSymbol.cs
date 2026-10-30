using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IOptionSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }

	int Ordinal { get; }
}
