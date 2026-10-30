using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class DocumentExtensions
{
	public static TLanguageService GetLanguageService<TLanguageService>(this Document document) where TLanguageService : class, ILanguageService
	{
		if (document != null && document.Project != null && document.Project.LanguageServices != null)
		{
			return document.Project.LanguageServices.GetService<TLanguageService>();
		}
		return null;
	}

	public static bool IsOpen(this Document document)
	{
		return document.Project.Solution.Workspace?.IsDocumentOpen(document.Id) ?? false;
	}

	public static async Task<SemanticModel> GetSemanticModelForSpanAsync(this Document document, TextSpan span, CancellationToken cancellationToken)
	{
		try
		{
			ISyntaxFactsService service = document.Project.LanguageServices.GetService<ISyntaxFactsService>();
			ISemanticModelService semanticModelService = document.Project.Solution.Workspace.Services.GetService<ISemanticModelService>();
			if (semanticModelService == null || service == null)
			{
				return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			SyntaxToken syntaxToken = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(span.Start);
			if (syntaxToken.Parent == null)
			{
				return await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			SyntaxNode node = syntaxToken.Parent.AncestorsAndSelf().FirstOrDefault((SyntaxNode a) => a.FullSpan.Contains(span));
			return await GetSemanticModelForNodeAsync(semanticModelService, document, node, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public static Task<SemanticModel> GetSemanticModelForNodeAsync(this Document document, SyntaxNode node, CancellationToken cancellationToken)
	{
		ISemanticModelService service = document.Project.Solution.Workspace.Services.GetService<ISemanticModelService>();
		if (service == null || node == null)
		{
			return document.GetSemanticModelAsync(cancellationToken);
		}
		return GetSemanticModelForNodeAsync(service, document, node, cancellationToken);
	}

	private static Task<SemanticModel> GetSemanticModelForNodeAsync(ISemanticModelService semanticModelService, Document document, SyntaxNode node, CancellationToken cancellationToken)
	{
		return semanticModelService.GetSemanticModelForNodeAsync(document, node, cancellationToken);
	}

	public static bool IsFromPrimaryBranch(this Document document)
	{
		return document.Project.Solution.BranchId == document.Project.Solution.Workspace.PrimaryBranchId;
	}

	public static async Task<bool> IsForkedDocumentWithSyntaxChangesAsync(this Document document, CancellationToken cancellationToken)
	{
		try
		{
			if (document.IsFromPrimaryBranch())
			{
				return false;
			}
			Solution currentSolution = document.Project.Solution.Workspace.CurrentSolution;
			Document currentDocument = currentSolution.GetDocument(document.Id);
			if (currentDocument == null)
			{
				return true;
			}
			return !(await document.GetSyntaxVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Equals(await currentDocument.GetSyntaxVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public static async Task<bool> IsPositionNotOnTokenTrivia(this Document document, LinePosition linePosition, CancellationToken cancellationToken)
	{
		int position = await document.GetPosition(linePosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return (await document.GetTokenAtPositionAsync(position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Span.Contains(position);
	}

	public static async Task<ISymbol> GetSymbolAtPositionAsync(this Document document, LinePosition symbolPosition, CancellationToken cancellationToken)
	{
		return await document.GetSymbolAtPositionAsync(await document.GetPosition(symbolPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<ISymbol> GetSymbolAtPositionAsync(this Document document, int position, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxToken syntaxToken = await document.GetTokenAtPositionAsync(position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (syntaxToken.Kind == SyntaxKind.IdentifierToken || syntaxToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			return GetSymbolFromNode(semanticModel, syntaxToken.Parent, cancellationToken);
		}
		syntaxToken = syntaxToken.GetPreviousToken();
		if (syntaxToken.Kind == SyntaxKind.IdentifierToken || syntaxToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			return GetSymbolFromNode(semanticModel, syntaxToken.Parent, cancellationToken);
		}
		return null;
	}

	public static async Task<PropertySymbol> GetPropertySymbolFromValueAtPositionAsync(this Document document, LinePosition symbolPosition, CancellationToken cancellationToken)
	{
		return await document.GetPropertySymbolFromValueAtPositionAsync(await document.GetPosition(symbolPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static async Task<PropertySymbol> GetPropertySymbolFromValueAtPositionAsync(this Document document, int position, CancellationToken cancellationToken)
	{
		if ((await document.GetTokenAtPositionAsync(position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetAncestor<PropertyValueSyntax>()?.Parent is PropertySyntax propertySyntax)
		{
			return (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetDeclaredSymbol(propertySyntax);
		}
		return null;
	}

	public static async Task<SyntaxNode> GetKeywordNodeAtPositionAsync(this Document document, LinePosition symbolPosition, CancellationToken cancellationToken)
	{
		int position = (await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Lines.GetPosition(symbolPosition);
		return await document.GetKeywordNodeAtPositionAsync(position, cancellationToken);
	}

	public static async Task<SyntaxNode> GetKeywordNodeAtPositionAsync(this Document document, int position, CancellationToken cancellationToken)
	{
		await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxToken syntaxToken = (await (await document.GetSyntaxTreeAsync(cancellationToken)).GetRootAsync(cancellationToken)).FindToken(position);
		if (syntaxToken.Kind.IsKeyword())
		{
			return syntaxToken.Parent;
		}
		return null;
	}

	public static async Task<ApplicationObjectTypeSymbol> GetDeclaringApplicationObjectAsync(this Document document, LinePosition linePosition, CancellationToken cancellationToken)
	{
		int position = await document.GetPosition(linePosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode declaringSyntaxNode = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).DescendantNodes().FirstOrDefault((SyntaxNode n) => n != null && n.Kind.IsApplicationObject() && n.Span.Contains(position));
		if (declaringSyntaxNode == null)
		{
			return null;
		}
		return (await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetDeclaredSymbol(declaringSyntaxNode) as ApplicationObjectTypeSymbol;
	}

	public static async Task<ImmutableArray<IObjectTypeSymbol>> GetDeclaringObjectAsync(this Document document, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ArrayBuilder<IObjectTypeSymbol> instance = ArrayBuilder<IObjectTypeSymbol>.GetInstance();
		foreach (SyntaxNode item in syntaxNode.DescendantNodes())
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (item.Kind.IsObject())
			{
				ObjectTypeSymbol objectTypeSymbol = semanticModel.GetDeclaredSymbol(item, cancellationToken) as ObjectTypeSymbol;
				if (objectTypeSymbol != null)
				{
					instance.Add(objectTypeSymbol);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	public static async Task<ImmutableArray<ISymbol>> GetDeclaredObjectSymbolsAsync(this Document document, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		foreach (SyntaxNode item in syntaxNode.DescendantNodes())
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!item.Kind.IsObject())
			{
				continue;
			}
			ObjectTypeSymbol objectTypeSymbol = semanticModel.GetDeclaredSymbol(item, cancellationToken) as ObjectTypeSymbol;
			if (!(objectTypeSymbol != null))
			{
				continue;
			}
			instance.Add(objectTypeSymbol);
			foreach (Symbol codeMember in objectTypeSymbol.GetCodeMembers())
			{
				if (codeMember.IsDeclaredSymbol())
				{
					instance.Add(codeMember);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal static async Task<ImmutableArray<SyntaxToken>> GetIdentifierTokensWithTextAsync(this Document document, string identifier, CancellationToken cancellationToken)
	{
		if (!document.TryGetSemanticModel(out var model))
		{
			return Contract.FailWithReturn<ImmutableArray<SyntaxToken>>("we should never reach here");
		}
		SyntaxTreeIndex info = await SyntaxTreeIndex.GetIndexAsync(document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!info.ProbablyContainsIdentifier(identifier))
		{
			return ImmutableArray<SyntaxToken>.Empty;
		}
		ISyntaxFactsService syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
		if (syntaxFacts == null)
		{
			return ImmutableArray<SyntaxToken>.Empty;
		}
		SyntaxNode root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SourceText sourceText = null;
		if (!info.ProbablyContainsEscapedIdentifier(identifier))
		{
			sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return FindReferenceCache.GetIdentifierTokensWithText(syntaxFacts, model, root, sourceText, identifier, cancellationToken);
	}

	private static ISymbol GetSymbolFromNode(SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
	{
		ISymbol result = null;
		SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
		if (symbolInfo.Symbol != null)
		{
			result = symbolInfo.Symbol;
		}
		else if (!symbolInfo.CandidateSymbols.IsEmpty)
		{
			result = symbolInfo.CandidateSymbols[0];
		}
		return result;
	}

	private static async Task<SyntaxToken> GetTokenAtPositionAsync(this Document document, int position, CancellationToken cancellationToken)
	{
		return (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(position);
	}

	private static async Task<int> GetPosition(this Document document, LinePosition linePosition, CancellationToken cancellationToken)
	{
		return (await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Lines.GetPosition(linePosition);
	}
}
