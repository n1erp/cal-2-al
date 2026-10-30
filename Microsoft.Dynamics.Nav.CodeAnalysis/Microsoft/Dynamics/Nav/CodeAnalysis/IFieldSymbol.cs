using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IFieldSymbol : ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
	FieldClassKind FieldClass { get; }
}
