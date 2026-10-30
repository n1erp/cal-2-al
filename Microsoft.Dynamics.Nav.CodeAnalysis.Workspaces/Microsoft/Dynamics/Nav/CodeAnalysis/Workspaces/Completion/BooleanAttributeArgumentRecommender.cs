using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class BooleanAttributeArgumentRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.AttributeArgumentList) || !syntaxContext.TargetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(syntaxContext.TargetToken);
		AttributeTypeInfo attributeTypeInfo = ((AttributeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent))?.AttributeInfo;
		if (attributeTypeInfo == null)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		int num = attributeArgumentListSyntax.CalculateAttributeArgumentPosition(syntaxContext.Position);
		if (num < attributeTypeInfo.Parameters.Length && attributeTypeInfo.Parameters[num].Type.NavTypeKind == NavTypeKind.Boolean)
		{
			return TextRecommender.CreateTrueAndFalseCompletionItems();
		}
		return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}
}
