using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IModuleSymbol : ISymbol, IEquatable<ISymbol>
{
	string PublisherName { get; }

	Version Version { get; }

	Guid AppId { get; }

	ImmutableArray<ISymbol> GetObjectSymbols(SymbolKind kind);

	ISymbolWithId GetObjectSymbolById(SymbolKind kind, int id);

	ISymbol GetObjectSymbolByName(SymbolKind kind, string name);
}
