using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SpellCheck;

[CodeFixProvider("SpellCheck")]
public class SpellCheckCodeFixProvider : ICodeFixProvider
{
	private class SpellCheckCodeAction : CodeAction.DocumentChangeAction
	{
		public SpellCheckCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
			: base(title, createChangedDocument, equivalenceKey)
		{
		}
	}

	private class MyCodeAction : CodeAction.CodeActionWithNestedActions
	{
		public MyCodeAction(string title, ImmutableArray<CodeAction> nestedActions)
			: base(title, nestedActions)
		{
		}
	}

	private const string AL0104 = "AL0104";

	private const string AL0105 = "AL0105";

	private const string AL0106 = "AL0106";

	private const string AL0107 = "AL0107";

	private const string AL0118 = "AL0118";

	private const string AL0169 = "AL0169";

	private const string AL0185 = "AL0185";

	private const int MinTokenLength = 3;

	public ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create<string>("AL0104", "AL0105", "AL0106", "AL0107", "AL0118", "AL0169", "AL0185");

	private SyntaxToken CreateIdentifier(SyntaxToken nameToken, string newName)
	{
		return SyntaxFactory.IdentifierNoQuotes(newName).WithTriviaFrom(nameToken);
	}

	public async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		Document document = context.Document;
		TextSpan span = context.Span;
		CancellationToken cancellationToken = context.CancellationToken;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		SyntaxNode syntaxNode2 = syntaxNode.FindNode(span);
		if (syntaxNode2 != null && syntaxNode2.Span == span)
		{
			await CheckNodeAsync(context, document, syntaxNode2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return;
		}
		SyntaxToken token = syntaxNode.FindToken(span.Start);
		if (token.Span == span)
		{
			await CheckTokenAsync(context, document, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task CheckNodeAsync(CodeFixContext context, Document document, SyntaxNode node, CancellationToken cancellationToken)
	{
		SemanticModel semanticModel = null;
		foreach (SimpleNameSyntax name in node.DescendantNodesAndSelf(DescendIntoChildren).OfType<SimpleNameSyntax>())
		{
			SyntaxToken token = name.GetFirstToken();
			string valueText = token.ValueText;
			if (valueText != null && valueText.Length >= 3)
			{
				SemanticModel semanticModel2 = semanticModel;
				if (semanticModel2 == null)
				{
					semanticModel2 = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				semanticModel = semanticModel2;
				if (semanticModel.GetSymbolInfo(name, cancellationToken).Symbol == null)
				{
					await CreateSpellCheckCodeIssueAsync(context, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
		}
	}

	private async Task CheckTokenAsync(CodeFixContext context, Document document, SyntaxToken token, CancellationToken cancellationToken)
	{
		if (document.GetLanguageService<ISyntaxFactsService>().IsWord(token))
		{
			string valueText = token.ValueText;
			if (valueText != null && valueText.Length >= 3)
			{
				await CreateSpellCheckCodeIssueAsync(context, token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private bool DescendIntoChildren(SyntaxNode arg)
	{
		return true;
	}

	private async Task CreateSpellCheckCodeIssueAsync(CodeFixContext context, SyntaxToken nameToken, CancellationToken cancellationToken)
	{
		Document document = context.Document;
		CompletionService service = CompletionService.GetService(document);
		OptionSet optionSet = document.Project.Solution.Workspace.Options;
		if (optionSet != null)
		{
			optionSet = optionSet.WithChangedOption(CompletionOptions.SerializeCompletionResult, value: true);
		}
		CompletionList completionList = await service.GetCompletionsAsync(document, nameToken.SpanStart, default, null, optionSet, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (completionList == null)
		{
			return;
		}
		string valueText = nameToken.ValueText;
		WordSimilarityChecker similarityChecker = WordSimilarityChecker.Allocate(valueText, substringsAreSimilar: true);
		try
		{
			await CheckItemsAsync(context, nameToken, completionList, similarityChecker).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			similarityChecker.Free();
		}
	}

	private async Task CheckItemsAsync(CodeFixContext context, SyntaxToken nameToken, CompletionList completionList, WordSimilarityChecker similarityChecker)
	{
		Document document = context.Document;
		CancellationToken cancellationToken = context.CancellationToken;
		MultiDictionary<double, string> results = new MultiDictionary<double, string>();
		foreach (CompletionItem item in completionList.Items)
		{
			string filterText = item.FilterText;
			if (similarityChecker.AreSimilar(filterText, out var matchCost))
			{
				string v = await GetInsertionTextAsync(document, item, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				results.Add(matchCost, v);
			}
		}
		string nameText = nameToken.ValueText;
		ImmutableArray<CodeAction> immutableArray = ((IEnumerable<CodeAction>)(from n in (from t in results.OrderBy((KeyValuePair<double, MultiDictionary<double, string>.ValueSet> kvp) => kvp.Key).SelectMany((KeyValuePair<double, MultiDictionary<double, string>.ValueSet> kvp) => kvp.Value.Order())
				where t != nameText
				select t).Take(3)
			select CreateCodeAction(nameToken, nameText, n, document))).ToImmutableArrayOrEmpty();
		if (immutableArray.Length > 1)
		{
			context.RegisterCodeFix(new MyCodeAction(string.Format(WorkspacesResources.FixTypo, nameText), immutableArray), context.Diagnostics);
		}
		else
		{
			context.RegisterFixes(immutableArray, context.Diagnostics);
		}
	}

	private async Task<string> GetInsertionTextAsync(Document document, CompletionItem item, CancellationToken cancellationToken)
	{
		return (await CompletionService.GetService(document).GetChangeAsync(document, item, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).TextChange.NewText;
	}

	private SpellCheckCodeAction CreateCodeAction(SyntaxToken nameToken, string oldName, string newName, Document document)
	{
		return new SpellCheckCodeAction(string.Format(CultureInfo.CurrentCulture, WorkspacesResources.ChangeTo, oldName, newName), (CancellationToken c) => Update(document, nameToken, newName, c), newName);
	}

	private async Task<Document> Update(Document document, SyntaxToken nameToken, string newName, CancellationToken cancellationToken)
	{
		SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).ReplaceToken(nameToken, CreateIdentifier(nameToken, newName));
		return document.WithSyntaxRoot(root);
	}
}
