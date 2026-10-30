using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class TableFieldIdRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		SyntaxToken leftToken = syntaxContext.LeftToken;
		SyntaxKind syntaxKind = syntaxContext?.DeclaringObject?.Kind ?? SyntaxKind.None;
		if (syntaxKind != SyntaxKind.TableObject && syntaxKind != SyntaxKind.TableExtensionObject)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (leftToken.GetNextToken().IsKind(SyntaxKind.Int32LiteralToken))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (leftToken.Kind != SyntaxKind.OpenParenToken || !leftToken.GetPreviousToken().IsKind(SyntaxKind.FieldKeyword))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IdSpaces idSpaces = syntaxContext.SemanticModel.Compilation.Options.IdSpaces;
		bool isTableExtension = syntaxKind == SyntaxKind.TableExtensionObject;
		int nextFreeFieldId = GetNextFreeFieldId(syntaxContext, idSpaces, isTableExtension);
		SyntaxToken nextToken = leftToken.GetNextToken();
		TextSpan span = new TextSpan(context.Position, 0);
		string text = nextFreeFieldId.ToString(CultureInfo.InvariantCulture);
		if (nextToken.IsKind(SyntaxKind.IdentifierToken))
		{
			text += ";";
			if (nextToken.SpanStart == context.Position)
			{
				text += " ";
			}
		}
		return new CompletionItem[1] { CreateItem(text, null, span) };
	}

	private static int GetNextFreeFieldId(MemberSyntaxContext syntaxContext, IdSpaces idSpaces, bool isTableExtension)
	{
		int i = 1;
		HashSet<int> setOfUsedIdsInBaseTableFromSymbols = GetSetOfUsedIdsInBaseTableFromSymbols(syntaxContext);
		if (isTableExtension)
		{
			i = idSpaces.ApplicationObjectIdRanges.FirstOrDefault()?.ApplicationObjectIdRangeStart ?? 0;
		}
		for (; setOfUsedIdsInBaseTableFromSymbols.Contains(i); i++)
		{
		}
		return i;
	}

	private static HashSet<int> GetSetOfUsedIdsInBaseTableFromSymbols(MemberSyntaxContext syntaxContext)
	{
		return new HashSet<int>(from FieldSymbol x in from x in syntaxContext.SemanticModel.LookupSymbols(syntaxContext.Position, LookupOptions.Module)
				where x.Kind == SymbolKind.Field
				select x
			select x.Id);
	}

	private static CompletionItem CreateItem(string displayText, int? tokenValue = null, TextSpan span = default(TextSpan))
	{
		string filterText = tokenValue?.ToString(CultureInfo.InvariantCulture) ?? displayText;
		Glyph? glyph = Glyph.None;
		return CommonCompletionItem.Create(displayText, span, glyph, null, null, null, filterText, displayText);
	}
}
