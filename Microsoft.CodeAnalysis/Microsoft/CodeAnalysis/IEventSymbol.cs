using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IEventSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }

	bool IsWindowsRuntimeEvent { get; }

	IMethodSymbol AddMethod { get; }

	IMethodSymbol RemoveMethod { get; }

	IMethodSymbol RaiseMethod { get; }

	new IEventSymbol OriginalDefinition { get; }

	IEventSymbol OverriddenEvent { get; }

	ImmutableArray<IEventSymbol> ExplicitInterfaceImplementations { get; }
}
