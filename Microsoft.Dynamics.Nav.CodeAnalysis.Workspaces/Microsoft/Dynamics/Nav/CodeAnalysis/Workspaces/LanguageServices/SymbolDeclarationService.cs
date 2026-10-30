using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SymbolDeclarationService : ISymbolDeclarationService, ILanguageService
{
	public IEnumerable<SyntaxReference> GetDeclarations(ISymbol symbol)
	{
		if (symbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxReference>();
		}
		return symbol.DeclaringSyntaxReferences.AsEnumerable();
	}
}
