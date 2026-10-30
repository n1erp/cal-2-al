using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class BooleanParameterRecommender : TextRecommender
{
	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.IsArgumentExpression())
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		InvocationExpressionSyntax node = syntaxContext.LeftToken.Parent.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (node == null || node.Expression == null)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		foreach (MethodSymbol item in await GetInvocationCandidates(syntaxContext, node.Expression.GetLastToken(), cancellationToken))
		{
			ParameterSymbol parameterTypeAtLeftTokenPosition = GetParameterTypeAtLeftTokenPosition(node, item, syntaxContext, cancellationToken);
			if (!(parameterTypeAtLeftTokenPosition == null) && !parameterTypeAtLeftTokenPosition.IsMemberReference && (parameterTypeAtLeftTokenPosition.Type.NavTypeKind == NavTypeKind.Boolean || parameterTypeAtLeftTokenPosition.Type.NavTypeKind == NavTypeKind.Joker))
			{
				return TextRecommender.CreateTrueAndFalseCompletionItems();
			}
		}
		return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	private static ParameterSymbol GetParameterTypeAtLeftTokenPosition(InvocationExpressionSyntax node, MethodSymbol invokedMethod, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		ImmutableArray<ParameterSymbol> parameters = invokedMethod.OriginalDefinition.GetParameters();
		int num = GetParameterIndex(node, syntaxContext.TargetToken);
		if (parameters.Length > 0 && num >= parameters.Length && parameters[parameters.Length - 1].IsParams)
		{
			num = parameters.Length - 1;
		}
		if (num >= parameters.Length)
		{
			return null;
		}
		return parameters[num];
	}

	private static int GetParameterIndex(InvocationExpressionSyntax node, SyntaxToken leftToken)
	{
		SyntaxToken firstToken = node.GetFirstToken();
		int num = 0;
		foreach (CodeExpressionSyntax argument in node.ArgumentList.Arguments)
		{
			if (LookupPosition.IsBetweenTokens(leftToken.SpanStart, firstToken, argument.GetFirstToken()))
			{
				return num;
			}
			num++;
		}
		return num;
	}

	private static async Task<IEnumerable<MethodSymbol>> GetInvocationCandidates(MemberSyntaxContext syntaxContext, SyntaxToken node, CancellationToken cancellationToken)
	{
		Tuple<ITypeSymbol, ImmutableArray<ISymbol>> tuple = await syntaxContext.GetSymbolInfoAtPositionAsync(node.SpanStart, cancellationToken);
		if (tuple == null)
		{
			return ImmutableArray<MethodSymbol>.Empty;
		}
		return tuple.Item2.OfType<MethodSymbol>();
	}
}
