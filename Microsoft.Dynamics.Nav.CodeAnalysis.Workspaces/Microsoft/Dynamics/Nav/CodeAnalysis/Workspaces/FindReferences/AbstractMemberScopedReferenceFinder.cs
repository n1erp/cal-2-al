using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

internal abstract class AbstractMemberScopedReferenceFinder<TSymbol, TSymbolScope> : AbstractReferenceFinder<TSymbol> where TSymbol : ISymbol where TSymbolScope : class, ISymbol
{
	protected override bool CanFind(TSymbol symbol)
	{
		return true;
	}

	protected override Task<ImmutableArray<Document>> DetermineDocumentsToSearchAsync(TSymbol symbol, Project project, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		Location location = symbol.Locations.FirstOrDefault();
		if (location == null || !location.IsInSource)
		{
			return SpecializedTasks.EmptyImmutableArray<Document>();
		}
		Document document = project.GetDocument(location.SourceTree);
		if (document == null)
		{
			return SpecializedTasks.EmptyImmutableArray<Document>();
		}
		if (documents != null && !documents.Contains(document))
		{
			return SpecializedTasks.EmptyImmutableArray<Document>();
		}
		return Task.FromResult(ImmutableArray.Create(document));
	}

	protected override async Task<ImmutableArray<ReferenceLocation>> FindReferencesInDocumentAsync(TSymbol symbol, Document document, CancellationToken cancellationToken)
	{
		ISymbol container = GetContainer(symbol);
		if (container != null)
		{
			return await FindReferencesInContainerAsync(symbol, container, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return ImmutableArray<ReferenceLocation>.Empty;
	}

	protected virtual Func<SyntaxToken, bool> GetTokensMatchFunction(ISyntaxFactsService syntaxFacts, string name)
	{
		return (SyntaxToken t) => AbstractReferenceFinder.IdentifiersMatch(syntaxFacts, name, t);
	}

	private static ISymbol GetContainer(ISymbol symbol)
	{
		for (ISymbol symbol2 = symbol; symbol2 != null; symbol2 = symbol2.ContainingSymbol)
		{
			if (symbol2 is TSymbolScope result)
			{
				return result;
			}
		}
		return null;
	}

	private Task<ImmutableArray<ReferenceLocation>> FindReferencesInContainerAsync(TSymbol symbol, ISymbol container, Document document, CancellationToken cancellationToken)
	{
		return FindReferencesInContainerAsync(symbol, container, document, null, cancellationToken);
	}

	private Task<ImmutableArray<ReferenceLocation>> FindReferencesInContainerAsync(TSymbol symbol, ISymbol container, Document document, Func<SyntaxToken, SyntaxNode> findParentNode, CancellationToken cancellationToken)
	{
		IEnumerable<SyntaxToken> tokens = document.GetLanguageService<ISymbolDeclarationService>().GetDeclarations(container).SelectMany((SyntaxReference r) => r.GetSyntax(cancellationToken).DescendantTokens());
		string name = symbol.Name;
		ISyntaxFactsService languageService = document.GetLanguageService<ISyntaxFactsService>();
		Func<SyntaxToken, SemanticModel, Tuple<bool, CandidateReason>> standardSymbolsMatchFunction = AbstractReferenceFinder.GetStandardSymbolsMatchFunction(symbol, findParentNode, cancellationToken);
		Func<SyntaxToken, bool> tokensMatchFunction = GetTokensMatchFunction(languageService, name);
		return AbstractReferenceFinder.FindReferencesInTokensAsync(document, tokens, tokensMatchFunction, standardSymbolsMatchFunction, cancellationToken);
	}
}
