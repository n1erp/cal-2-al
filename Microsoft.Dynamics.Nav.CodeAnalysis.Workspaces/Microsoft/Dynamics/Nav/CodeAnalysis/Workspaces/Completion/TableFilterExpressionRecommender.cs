using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TableFilterExpressionRecommender : PropertyExpressionRecommender<TableFilterPropertyValueSyntax, PropertyExpressionSyntax>
{
	private readonly PropertyValueSyntax propertyValue;

	internal TableFilterExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
		PropertySyntax ancestor = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.TargetToken);
		if (ancestor != null)
		{
			propertyValue = ancestor.Value;
		}
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (propertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (DeclaringSyntax == null && Context.TargetToken.IsKind(SyntaxKind.EqualsToken) && propertyValue.Kind == SyntaxKind.TableFilterPropertyValue)
		{
			return RecommendationHelper.GetFields(Context, ReferencedTable, cancellationToken);
		}
		TableFilterExpressionRecommender tableFilterExpressionRecommender = this;
		SeparatedSyntaxList<PropertyExpressionSyntax>? separatedSyntaxList = DeclaringSyntax?.Conditions;
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
		return await tableFilterExpressionRecommender.RecommendSymbolsAsync(cancellationToken, conditions, shouldUseLhsFieldsFromSource: false);
	}

	protected override TableTypeSymbol GetSourceTable()
	{
		if (!(propertyValue.Parent.Parent.Parent is PagePartSyntax pagePartSyntax))
		{
			return base.GetSourceTable();
		}
		return (from x in DeclaringObject.GetPageElementSymbols(pagePartSyntax.Name)
			where x.IsKind(SymbolKind.Control)
			select x).Cast<ControlSymbol>().FirstOrDefault()?.TryGetSourceTableByProviderId() ?? base.GetSourceTable();
	}

	protected override TableTypeSymbol SetReferencedTable()
	{
		return GetRelatedTableSymbol(propertyValue);
	}
}
