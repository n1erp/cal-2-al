using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IParameterSymbol : ISymbol, IEquatable<ISymbol>
{
	RefKind RefKind { get; }

	bool IsParams { get; }

	bool IsOptional { get; }

	bool IsThis { get; }

	ITypeSymbol Type { get; }

	ImmutableArray<CustomModifier> CustomModifiers { get; }

	ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	int Ordinal { get; }

	bool HasExplicitDefaultValue { get; }

	object ExplicitDefaultValue { get; }

	new IParameterSymbol OriginalDefinition { get; }
}
