using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IEnumValueSymbol : ISymbol, IEquatable<ISymbol>
{
	int Ordinal { get; }
}
