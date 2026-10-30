using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ViewFiltersExpressionRecommender : PropertyExpressionRecommender<FiltersPropertyValueSyntax, TableFilterExpressionSyntax>
{
	internal ViewFiltersExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		ViewFiltersExpressionRecommender viewFiltersExpressionRecommender = this;
		SeparatedSyntaxList<PropertyExpressionSyntax>? separatedSyntaxList = PropertyExpressionSyntax?.Conditions;
		IEnumerable<PropertyExpressionSyntax> conditions;
		if (!separatedSyntaxList.HasValue)
		{
			conditions = SpecializedCollections.EmptyEnumerable<PropertyExpressionSyntax>();
		}
		else
		{
			IEnumerable<PropertyExpressionSyntax> enumerable = separatedSyntaxList.GetValueOrDefault();
			conditions = enumerable;
		}
		return await viewFiltersExpressionRecommender.RecommendSymbolsAsync(cancellationToken, conditions);
	}

	protected override TableTypeSymbol SetReferencedTable()
	{
		return null;
	}
}
