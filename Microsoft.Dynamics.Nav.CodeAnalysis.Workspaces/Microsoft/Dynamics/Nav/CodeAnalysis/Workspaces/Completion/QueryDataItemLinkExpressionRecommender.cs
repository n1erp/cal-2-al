using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class QueryDataItemLinkExpressionRecommender : ContextAwareSymbolRecommender
{
	private readonly PropertyValueSyntax propertyValue;

	private readonly QueryDataItemSyntax queryDataItemSyntax;

	internal QueryDataItemLinkExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.TargetToken);
		queryDataItemSyntax = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<QueryDataItemSyntax>(Context.TargetToken);
		if (ancestor != null)
		{
			propertyValue = ancestor.Value;
		}
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (propertyValue == null || queryDataItemSyntax == null)
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		if (propertyValue.Kind != SyntaxKind.QueryDataItemLinkPropertyValue)
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		return Task.FromResult(GetRecommendation(cancellationToken));
	}

	private IEnumerable<ISymbol> GetRecommendation(CancellationToken cancellationToken)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.TargetToken);
		switch (Context.LeftToken.Kind)
		{
		case SyntaxKind.EqualsToken:
			if (ancestor.EqualsToken == Context.LeftToken)
			{
				return GetSourceFieldRecommendation(cancellationToken);
			}
			return GetDataItemRecommendation(cancellationToken);
		case SyntaxKind.CommaToken:
			return GetSourceFieldRecommendation(cancellationToken);
		case SyntaxKind.DotToken:
			return GetAncestorDataItemFieldRecommendation(cancellationToken);
		default:
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
	}

	private IEnumerable<ISymbol> GetAncestorDataItemFieldRecommendation(CancellationToken cancellationToken)
	{
		if (Context.TargetToken.Parent.IsKind(SyntaxKind.MemberAccessExpression))
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)Context.TargetToken.Parent;
			if (memberAccessExpressionSyntax.Expression.IsKind(SyntaxKind.IdentifierName))
			{
				string name = ((IdentifierNameSyntax)memberAccessExpressionSyntax.Expression).Unquoted();
				SemanticModel semanticModel = Context.SemanticModel;
				int spanStart = Context.LeftToken.SpanStart;
				CancellationToken token = cancellationToken;
				QueryDataItemSymbol queryDataItemSymbol = semanticModel.LookupSymbols(spanStart, LookupOptions.Default, null, name, token).FirstOrDefault((ISymbol s) => s.Kind == SymbolKind.QueryDataItem) as QueryDataItemSymbol;
				if (queryDataItemSymbol != null)
				{
					SemanticModel semanticModel2 = Context.SemanticModel;
					int spanStart2 = Context.LeftToken.SpanStart;
					token = cancellationToken;
					return from s in semanticModel2.LookupSymbols(spanStart2, LookupOptions.Default, queryDataItemSymbol.RelatedTable, null, token)
						where s.Kind == SymbolKind.Field
						select s;
				}
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}

	private IEnumerable<ISymbol> GetSourceFieldRecommendation(CancellationToken cancellationToken)
	{
		return from s in Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken)
			where s.Kind == SymbolKind.Field
			select s;
	}

	private IEnumerable<ISymbol> GetDataItemRecommendation(CancellationToken cancellationToken)
	{
		IEnumerable<ISymbol> enumerable = from s in Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken)
			where s.Kind == SymbolKind.QueryDataItem
			select s;
		string currentDataItemName = queryDataItemSyntax.Name.GetIdentifierOrLiteralValue();
		if (string.IsNullOrEmpty(currentDataItemName))
		{
			return enumerable;
		}
		ISymbol currentDataItem = enumerable.FirstOrDefault((ISymbol s) => s.Name.Equals(currentDataItemName, StringComparison.OrdinalIgnoreCase));
		if (currentDataItem == null)
		{
			return enumerable;
		}
		return enumerable.Where((ISymbol s) => currentDataItem.IsContainedBy(s));
	}
}
