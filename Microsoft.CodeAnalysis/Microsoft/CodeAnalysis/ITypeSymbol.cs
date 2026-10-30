using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface ITypeSymbol : INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	TypeKind TypeKind { get; }

	INamedTypeSymbol BaseType { get; }

	ImmutableArray<INamedTypeSymbol> Interfaces { get; }

	ImmutableArray<INamedTypeSymbol> AllInterfaces { get; }

	bool IsReferenceType { get; }

	bool IsValueType { get; }

	bool IsAnonymousType { get; }

	bool IsTupleType { get; }

	new ITypeSymbol OriginalDefinition { get; }

	SpecialType SpecialType { get; }

	ISymbol FindImplementationForInterfaceMember(ISymbol interfaceMember);
}
