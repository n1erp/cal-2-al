using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IPropertySymbol : ISymbol, IEquatable<ISymbol>
{
	PropertyKind PropertyKind { get; }

	string ValueText { get; }
}
