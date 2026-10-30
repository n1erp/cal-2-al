using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IApplicationObjectExtensionTypeSymbol : IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
	IApplicationObjectTypeSymbol Target { get; }
}
