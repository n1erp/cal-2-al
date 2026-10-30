using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IParameterSymbol : ISymbol, IEquatable<ISymbol>
{
	bool IsVar { get; }

	bool TypeFromFirstArgument { get; }

	bool IsSameVar { get; }

	int Ordinal { get; }

	bool IsOptional { get; }

	bool IsMember { get; }

	bool IsMemberReference { get; }

	bool IsArray { get; }

	bool AddExpectedType { get; }

	ITypeSymbol ParameterType { get; }
}
