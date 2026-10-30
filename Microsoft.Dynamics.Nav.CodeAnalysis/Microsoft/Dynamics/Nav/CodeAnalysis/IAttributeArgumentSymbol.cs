using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IAttributeArgumentSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }

	string ValueText { get; }
}
