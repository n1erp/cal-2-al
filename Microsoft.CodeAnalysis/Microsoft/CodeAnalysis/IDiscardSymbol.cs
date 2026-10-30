using System;

namespace Microsoft.CodeAnalysis;

public interface IDiscardSymbol : ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol Type { get; }
}
