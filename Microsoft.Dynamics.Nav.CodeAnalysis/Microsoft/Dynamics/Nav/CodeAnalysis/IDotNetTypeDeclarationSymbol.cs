using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IDotNetTypeDeclarationSymbol : ISymbol, IEquatable<ISymbol>
{
	IDotNetTypeSymbol DotNetType { get; }
}
