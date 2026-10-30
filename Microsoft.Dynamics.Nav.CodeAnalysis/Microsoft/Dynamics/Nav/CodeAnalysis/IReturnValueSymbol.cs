using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReturnValueSymbol : ISymbol, IEquatable<ISymbol>
{
	bool IsNamed { get; }

	bool IsOptional { get; }

	ITypeSymbol ReturnType { get; }

	bool TypeFromFirstArgument { get; }
}
