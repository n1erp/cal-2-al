using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITypeSymbol : IContainerSymbol, ISymbol, IEquatable<ISymbol>
{
	TypeCategoryKind TypeCategory { get; }

	NavTypeKind NavTypeKind { get; }

	int Length { get; }

	bool HasLength { get; }
}
