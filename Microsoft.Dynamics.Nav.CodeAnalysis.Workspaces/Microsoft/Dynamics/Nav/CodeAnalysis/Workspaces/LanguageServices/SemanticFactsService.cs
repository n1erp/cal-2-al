using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class SemanticFactsService : ISemanticFactsService, ILanguageService
{
	internal static readonly SemanticFactsService Instance = new SemanticFactsService();

	private SemanticFactsService()
	{
	}

	public ISymbol GetDeclaredSymbol(SemanticModel semanticModel, SyntaxToken token, CancellationToken cancellationToken)
	{
		Location location = token.GetLocation();
		return (from node in token.GetAncestors<SyntaxNode>()
			let symbol = semanticModel.GetDeclaredSymbol(node, cancellationToken)
			where symbol != null && symbol.Locations.Contains(location)
			select symbol).FirstOrDefault();
	}
}
