using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IPropertySymbol : ISymbol, IEquatable<ISymbol>
{
	bool IsIndexer { get; }

	bool IsReadOnly { get; }

	bool IsWriteOnly { get; }

	bool IsWithEvents { get; }

	bool ReturnsByRef { get; }

	bool ReturnsByRefReadonly { get; }

	RefKind RefKind { get; }

	ITypeSymbol Type { get; }

	ImmutableArray<IParameterSymbol> Parameters { get; }

	IMethodSymbol GetMethod { get; }

	IMethodSymbol SetMethod { get; }

	new IPropertySymbol OriginalDefinition { get; }

	IPropertySymbol OverriddenProperty { get; }

	ImmutableArray<IPropertySymbol> ExplicitInterfaceImplementations { get; }

	ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	ImmutableArray<CustomModifier> TypeCustomModifiers { get; }
}
