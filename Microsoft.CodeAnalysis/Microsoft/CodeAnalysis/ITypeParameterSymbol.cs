using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface ITypeParameterSymbol : ITypeSymbol, INamespaceOrTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	int Ordinal { get; }

	VarianceKind Variance { get; }

	TypeParameterKind TypeParameterKind { get; }

	IMethodSymbol DeclaringMethod { get; }

	INamedTypeSymbol DeclaringType { get; }

	bool HasReferenceTypeConstraint { get; }

	bool HasValueTypeConstraint { get; }

	bool HasUnmanagedTypeConstraint { get; }

	bool HasConstructorConstraint { get; }

	ImmutableArray<ITypeSymbol> ConstraintTypes { get; }

	new ITypeParameterSymbol OriginalDefinition { get; }

	ITypeParameterSymbol ReducedFrom { get; }
}
