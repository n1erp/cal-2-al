using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReferenceResolver
{
	ImmutableArray<ISymbolWithId> GetObjectSymbolsByIdAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, int id);

	ImmutableArray<ISymbol> GetObjectSymbolsByNameAcrossModules(IModuleSymbol referencingModule, SymbolKind kind, string name);

	IEnumerable<ISymbol> GetObjectSymbolsByKindAcrossModules(IModuleSymbol referencingModule, SymbolKind kind);
}
