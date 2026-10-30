using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class LabelPropertyNameRecommender : TextRecommender
{
	private static readonly IEnumerable<string> labelKeywords = new string[3] { "Comment", "MaxLength", "Locked" };

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		SyntaxToken targetToken = syntaxContext.TargetToken;
		if (targetToken.Kind != SyntaxKind.CommaToken)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		LabelSyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<LabelSyntax>(targetToken);
		if (ancestor == null)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<string> enumerable = labelKeywords;
		CommaSeparatedIdentifierEqualsLiteralListSyntax properties = ancestor.Properties;
		IEnumerable<string> properties2 = ((properties != null) ? properties.Values.Select((IdentifierEqualsLiteralSyntax x) => x.Identifier.Text) : null);
		if (properties2 != null)
		{
			enumerable = enumerable.Where((string x) => !properties2.Any((string y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)));
		}
		return TextRecommender.ToCompletionItemArray(enumerable.ToImmutableArray(), Glyph.Keyword);
	}
}
