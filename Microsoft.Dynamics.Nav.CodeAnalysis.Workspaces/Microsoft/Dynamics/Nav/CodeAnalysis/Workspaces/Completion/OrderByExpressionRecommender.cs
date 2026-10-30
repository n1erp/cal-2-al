using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class OrderByExpressionRecommender : ContextAwareSymbolRecommender
{
	private readonly OrderByExpressionSyntax orderByExpression;

	internal OrderByExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		orderByExpression = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<OrderByExpressionSyntax>(Context.TargetToken);
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (orderByExpression == null)
		{
			return Task.FromResult(SpecializedCollections.EmptyEnumerable<ISymbol>());
		}
		return Task.FromResult(GetAvailableColumnsForSorting(cancellationToken));
	}

	private void GetNamesOfColumnsInExpression(PooledNameComparisonHashSet result)
	{
		if (!(orderByExpression.Parent is OrderByPropertyValueSyntax { Order: var order }))
		{
			return;
		}
		foreach (OrderByExpressionSyntax item in order)
		{
			foreach (IdentifierNameSyntax sortingField in item.SortingFields)
			{
				result.Add(sortingField.GetIdentifierOrLiteralValue());
			}
		}
	}

	private IEnumerable<ISymbol> GetAvailableColumnsForSorting(CancellationToken cancellationToken)
	{
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			GetNamesOfColumnsInExpression(instance2);
			foreach (ISymbol item in (IEnumerable<ISymbol>)Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.MemberAccess, null, null, cancellationToken))
			{
				if (item.Kind == SymbolKind.QueryColumn && !instance2.Contains(item.Name))
				{
					instance.Add(item);
				}
			}
			return instance.ToArray();
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}
}
