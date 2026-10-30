using System;

namespace Microsoft.CodeAnalysis;

public interface ILocalSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }

	bool IsConst { get; }

	bool IsRef { get; }

	RefKind RefKind { get; }

	bool HasConstantValue { get; }

	object ConstantValue { get; }

	bool IsFunctionValue { get; }

	bool IsFixed { get; }
}
