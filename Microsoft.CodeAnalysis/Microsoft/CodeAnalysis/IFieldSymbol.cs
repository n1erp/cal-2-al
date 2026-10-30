using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IFieldSymbol : ISymbol, IEquatable<ISymbol>
{
	ISymbol AssociatedSymbol { get; }

	bool IsConst { get; }

	bool IsReadOnly { get; }

	bool IsVolatile { get; }

	ITypeSymbol Type { get; }

	bool HasConstantValue { get; }

	object ConstantValue { get; }

	ImmutableArray<CustomModifier> CustomModifiers { get; }

	new IFieldSymbol OriginalDefinition { get; }

	IFieldSymbol CorrespondingTupleField { get; }
}
