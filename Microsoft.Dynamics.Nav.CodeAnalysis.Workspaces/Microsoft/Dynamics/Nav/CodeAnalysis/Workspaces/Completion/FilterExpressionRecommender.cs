using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class FilterExpressionRecommender : TextRecommender
{
	private static ImmutableArray<CompletionItem> unaryPrefixOperators = new SyntaxKind[7]
	{
		SyntaxKind.NotEqualsToken,
		SyntaxKind.EqualsToken,
		SyntaxKind.GreaterThanToken,
		SyntaxKind.GreaterThanEqualsToken,
		SyntaxKind.LessThanToken,
		SyntaxKind.LessThanEqualsToken,
		SyntaxKind.DotDotToken
	}.Select((SyntaxKind t) => CreateItem(t.GetText())).ToImmutableArrayOrEmpty();

	private static ImmutableArray<CompletionItem> binaryOperators = new SyntaxKind[3]
	{
		SyntaxKind.AndFilterKeyword,
		SyntaxKind.OrFilterKeyword,
		SyntaxKind.DotDotToken
	}.Select((SyntaxKind t) => CreateItem(t.GetText())).ToImmutableArrayOrEmpty();

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.PropertyValue))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxToken leftToken = syntaxContext.LeftToken;
		if (Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<FilterExpressionSyntax>(leftToken) != null)
		{
			if (leftToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.AndFilterKeyword, SyntaxKind.OrFilterKeyword))
			{
				return unaryPrefixOperators;
			}
			if (leftToken.IsKind(SyntaxKind.IdentifierToken) || leftToken.Kind.IsLiteral())
			{
				return binaryOperators;
			}
		}
		return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static CompletionItem CreateItem(string displayText, int? tokenValue = null, TextSpan span = default(TextSpan))
	{
		string filterText = tokenValue?.ToString(CultureInfo.InvariantCulture) ?? displayText;
		Glyph? glyph = Glyph.None;
		return CommonCompletionItem.Create(displayText, span, glyph, null, null, null, filterText, displayText);
	}
}
