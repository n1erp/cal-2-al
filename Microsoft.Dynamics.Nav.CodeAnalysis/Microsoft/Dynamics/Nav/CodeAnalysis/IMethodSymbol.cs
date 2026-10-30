using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IMethodSymbol : ISymbol, IEquatable<ISymbol>
{
	MethodKind MethodKind { get; }

	ImmutableArray<IParameterSymbol> Parameters { get; }

	ImmutableArray<IVariableSymbol> LocalVariables { get; }

	ImmutableArray<IAttributeSymbol> Attributes { get; }

	IReturnValueSymbol ReturnValueSymbol { get; }

	bool IsLocal { get; }
}
