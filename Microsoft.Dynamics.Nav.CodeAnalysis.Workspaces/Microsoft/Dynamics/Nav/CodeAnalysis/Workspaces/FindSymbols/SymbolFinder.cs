using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal static class SymbolFinder
{
	public static async Task<ISymbol> FindSymbolAtPositionAsync(Document document, int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return await FindSymbolAtPositionAsync(await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), position, document.Project.Solution.Workspace, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<ISymbol> FindSymbolAtPositionAsync(SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken = default(CancellationToken))
	{
		return (await GetSemanticInfoAtPositionAsync(semanticModel, position, workspace, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAnySymbol(includeType: false);
	}

	internal static async Task<TokenSemanticInfo> GetSemanticInfoAtPositionAsync(SemanticModel semanticModel, int position, Workspace workspace, CancellationToken cancellationToken)
	{
		SyntaxTree syntaxTree = semanticModel.SyntaxTree;
		ISyntaxFactsService service = workspace.Services.GetLanguageServices("AL").GetService<ISyntaxFactsService>();
		SyntaxToken syntaxToken = await Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.GetTouchingTokenAsync(syntaxTree, position, service.IsBindableToken, cancellationToken, findInsideTrivia: true).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxToken != default(SyntaxToken) && syntaxToken.Span.IntersectsWith(position))
		{
			return GetSemanticInfo(semanticModel, syntaxToken, workspace, cancellationToken);
		}
		return TokenSemanticInfo.Empty;
	}

	public static TokenSemanticInfo GetSemanticInfo(SemanticModel semanticModel, SyntaxToken token, Workspace workspace, CancellationToken cancellationToken)
	{
		AbstractHostLanguageServices languageServices = workspace.Services.GetLanguageServices("AL");
		ISyntaxFactsService service = languageServices.GetService<ISyntaxFactsService>();
		if (!service.IsBindableToken(token))
		{
			return TokenSemanticInfo.Empty;
		}
		ISemanticFactsService service2 = languageServices.GetService<ISemanticFactsService>();
		return GetSemanticInfo(semanticModel, service2, service, token, cancellationToken);
	}

	private static TokenSemanticInfo GetSemanticInfo(SemanticModel semanticModel, ISemanticFactsService semanticFacts, ISyntaxFactsService syntaxFacts, SyntaxToken token, CancellationToken cancellationToken)
	{
		SyntaxNode bindableParent = syntaxFacts.GetBindableParent(token);
		ITypeSymbol type = semanticModel.GetTypeInfo(bindableParent, cancellationToken).Type;
		ISymbol declaredSymbol = semanticFacts.GetDeclaredSymbol(semanticModel, token, cancellationToken);
		ImmutableArray<ISymbol> referencedSymbols = (from s in semanticModel.GetSymbolInfo(bindableParent, cancellationToken).GetBestOrAllSymbols()
			where !s.Equals(declaredSymbol)
			select s).ToImmutableArray();
		return new TokenSemanticInfo(declaredSymbol, referencedSymbols, type);
	}

	internal static bool OriginalSymbolsMatch(ISymbol searchSymbol, ISymbol symbolToMatch)
	{
		if (symbolToMatch == null)
		{
			return false;
		}
		if (searchSymbol == symbolToMatch)
		{
			return true;
		}
		if (IsReferenceInUserControl(searchSymbol, symbolToMatch) || IsReferenceInUserControl(symbolToMatch, searchSymbol))
		{
			return true;
		}
		return false;
	}

	private static bool IsReferenceInUserControl(ISymbol declaration, ISymbol method)
	{
		if (!declaration.ContainingSymbol.IsKind(SymbolKind.ControlAddIn) || !method.IsKind(SymbolKind.Method) || !method.ContainingSymbol.IsKind(SymbolKind.Control))
		{
			return false;
		}
		ControlSymbol controlSymbol = (ControlSymbol)method.ContainingSymbol;
		if (controlSymbol.ControlKind != ControlKind.UserControl)
		{
			return false;
		}
		return declaration.ContainingSymbol.Equals(controlSymbol.RelatedControlAddInSymbol);
	}

	public static async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(ISymbol symbol, Solution solution, CancellationToken cancellationToken = default(CancellationToken))
	{
		StreamingProgressCollector progressCollector = new StreamingProgressCollector(StreamingFindReferencesProgress.Instance);
		await FindReferencesAsync(SymbolAndProjectId.Create(symbol, null), solution, progressCollector, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return progressCollector.GetReferencedSymbols();
	}

	public static async Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(ISymbol symbol, Solution solution, IFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken = default(CancellationToken))
	{
		StreamingProgressCollector streamingProgress = new StreamingProgressCollector(new StreamingFindReferencesProgressAdapter(progress));
		await FindReferencesAsync(SymbolAndProjectId.Create(symbol, null), solution, streamingProgress, documents, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return streamingProgress.GetReferencedSymbols();
	}

	internal static async Task FindReferencesAsync(SymbolAndProjectId symbolAndProjectId, Solution solution, IStreamingFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.FindReference, cancellationToken))
		{
			await FindReferencesInCurrentProcessAsync(symbolAndProjectId, solution, progress, documents, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal static Task FindReferencesInCurrentProcessAsync(SymbolAndProjectId symbolAndProjectId, Solution solution, IStreamingFindReferencesProgress progress, IImmutableSet<Document> documents, CancellationToken cancellationToken)
	{
		ImmutableArray<IReferenceFinder> defaultReferenceFinders = ReferenceFinders.DefaultReferenceFinders;
		progress = progress ?? StreamingFindReferencesProgress.Instance;
		return new FindReferencesSearchEngine(solution, documents, defaultReferenceFinders, progress, cancellationToken).FindReferencesAsync(symbolAndProjectId);
	}
}
