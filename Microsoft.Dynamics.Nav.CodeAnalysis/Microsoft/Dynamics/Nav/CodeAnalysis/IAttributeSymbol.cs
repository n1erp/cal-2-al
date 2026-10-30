using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IAttributeSymbol : ISymbol, IEquatable<ISymbol>
{
	AttributeCategory AttributeCategory { get; }

	AttributeKind AttributeKind { get; }

	ImmutableArray<IAttributeArgumentSymbol> SignatureArguments { get; }

	ImmutableArray<IAttributeArgumentSymbol> Arguments { get; }
}
