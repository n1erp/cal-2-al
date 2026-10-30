using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ContainerContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ContainerContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.IsRightOfNameSeparator || Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SyntaxNode parent = Context.TargetToken.Parent;
		ExpressionSyntax expressionSyntax = null;
		if (parent.Kind == SyntaxKind.MemberAccessExpression)
		{
			expressionSyntax = ((MemberAccessExpressionSyntax)parent).Expression;
		}
		if (parent.Kind == SyntaxKind.IdentifierName && parent.GetFirstToken().HasDotDotAsFirstTrivia())
		{
			expressionSyntax = (IdentifierNameSyntax)parent;
		}
		if (parent.Kind == SyntaxKind.OptionAccessExpression)
		{
			expressionSyntax = ((OptionAccessExpressionSyntax)parent).Expression;
		}
		if (expressionSyntax != null)
		{
			return GetSymbolsOffOfExpression(expressionSyntax, cancellationToken);
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private IEnumerable<ISymbol> GetSymbolsOffOfExpression(ExpressionSyntax originalExpression, CancellationToken cancellationToken)
	{
		ExpressionSyntax expressionSyntax = originalExpression.WalkDownParentheses();
		SymbolInfo symbolInfo = Context.SemanticModel.GetSymbolInfo(expressionSyntax, cancellationToken);
		TypeInfo typeInfo = Context.SemanticModel.GetTypeInfo(expressionSyntax, cancellationToken);
		IContainerSymbol container = ((typeInfo != TypeInfo.None) ? typeInfo.Type : null);
		return GetSymbolsOffOfBoundExpression(originalExpression, symbolInfo, container, cancellationToken);
	}

	private IEnumerable<ISymbol> GetSymbolsOffOfBoundExpression(ExpressionSyntax originalExpression, SymbolInfo leftHandBinding, IContainerSymbol container, CancellationToken cancellationToken)
	{
		if (leftHandBinding.CandidateReason == CandidateReason.NotReferencable)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ISymbol symbol = leftHandBinding.GetBestOrAllSymbols().FirstOrDefault();
		if (symbol != null && originalExpression.IsKind(SyntaxKind.ParenthesizedExpression) && symbol.IsKind(SymbolKind.NamedType))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (container == null)
		{
			DebugAssertHelper.Assert(symbol == null, "Unexpected null container in " + originalExpression.ToReportingString());
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		LookupOptions lookupOptions = container.GetLookupOptions();
		int spanStart = originalExpression.SpanStart;
		if (Context.IsRightOfOptionAccess)
		{
			lookupOptions |= LookupOptions.MustBeOption;
			if (container.Kind == SymbolKind.Class && container.TryGetSymbolsFromOptionType(Context.SemanticModel.Compilation, spanStart, cancellationToken, out var symbols))
			{
				return symbols;
			}
		}
		return Context.SemanticModel.LookupSymbols(spanStart, lookupOptions, container, null, cancellationToken);
	}
}
