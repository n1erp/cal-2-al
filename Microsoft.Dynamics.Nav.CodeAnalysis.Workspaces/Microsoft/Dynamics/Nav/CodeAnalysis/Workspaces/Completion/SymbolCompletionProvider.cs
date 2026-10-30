using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class SymbolCompletionProvider : AbstractSymbolCompletionProvider
{
	protected override Task<IEnumerable<ISymbol>> GetSymbolsWorker(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		return Recommender.GetRecommendedSymbolsAtPositionAsync(context, options, cancellationToken);
	}

	protected sealed override string GetDisplayText(ISymbol symbol, AbstractSyntaxContext context, char ch)
	{
		return GetDisplayText(symbol, context);
	}

	protected virtual string GetDisplayText(ISymbol symbol, AbstractSyntaxContext context)
	{
		return symbol.Name.QuoteIdentifierIfNeeded();
	}

	internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
	{
		return CompletionUtilities.IsTriggerCharacter(text, characterPosition, options);
	}

	protected override async Task<bool> IsSemanticTriggerCharacterAsync(Document document, int characterPosition, CancellationToken cancellationToken)
	{
		bool? flag = await IsTriggerOnDotAsync(document, characterPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (flag.HasValue)
		{
			return flag.Value;
		}
		return true;
	}

	private async Task<bool?> IsTriggerOnDotAsync(Document document, int characterPosition, CancellationToken cancellationToken)
	{
		if ((await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))[characterPosition] != '.')
		{
			return null;
		}
		SyntaxToken syntaxToken = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).FindToken(characterPosition);
		if (syntaxToken.Kind == SyntaxKind.DotToken)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		return !syntaxToken.Kind.IsNumericLiteral();
	}

	protected override (string, string, bool) GetDisplayAndInsertionText(ISymbol symbol, AbstractSyntaxContext context)
	{
		string displayText = GetDisplayText(symbol, context);
		string insertionText = GetInsertionText(symbol, displayText, context, out var isSnippet);
		return ValueTuple.Create(displayText, insertionText, isSnippet);
	}

	protected override CompletionItemRules GetCompletionItemRules(IReadOnlyList<ISymbol> symbols, AbstractSyntaxContext context)
	{
		return CompletionItemRules.Default;
	}

	private string GetInsertionText(ISymbol symbol, string displayText, AbstractSyntaxContext context, out bool isSnippet)
	{
		isSnippet = false;
		if (!symbol.IsKind(SymbolKind.Method))
		{
			return displayText;
		}
		MethodSymbol methodSymbol = (MethodSymbol)symbol;
		MethodKind methodKind = methodSymbol.MethodKind;
		int num = (int)methodKind;
		if (num != 2)
		{
			if (num == 4)
			{
				goto IL_0037;
			}
		}
		else if (((BuiltInMethodTypeSymbol)methodSymbol).IsProperty)
		{
			goto IL_0037;
		}
		MemberSyntaxContext obj = context as MemberSyntaxContext;
		if (obj != null && obj.General == GeneralContexts.AttributeArgumentList)
		{
			return displayText;
		}
		bool flag = methodSymbol.ReturnType.NavTypeKind == NavTypeKind.None;
		if (methodSymbol.ParameterCount == 0)
		{
			if (!flag)
			{
				return displayText + "()";
			}
			return displayText + "();";
		}
		isSnippet = true;
		if (!flag)
		{
			return displayText + "($0)";
		}
		return displayText + "($0);";
		IL_0037:
		return displayText;
	}
}
