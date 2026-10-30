using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ViewOrderByExpressionRecommender : ContextAwareSymbolRecommender
{
	private readonly OrderByExpressionSyntax orderByExpression;

	internal ViewOrderByExpressionRecommender(MemberSyntaxContext context)
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
		return Task.FromResult(GetFields(cancellationToken));
	}

	private IEnumerable<ISymbol> GetFields(CancellationToken cancellationToken)
	{
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			GetNamesOfFieldInExpression(instance2);
			foreach (ISymbol field in RecommendationHelper.GetFields(Context, GetSourceTable(), cancellationToken))
			{
				if (!instance2.Contains(field.Name))
				{
					instance.Add(field);
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

	private void GetNamesOfFieldInExpression(PooledNameComparisonHashSet result)
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

	private TableTypeSymbol GetSourceTable()
	{
		return DeclaringObject?.GetRelatedTableSymbol();
	}
}
