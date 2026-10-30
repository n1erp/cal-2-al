using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class TableRelationConditionalExpressionRecommender : PropertyExpressionRecommender<TableRelationPropertyValueSyntax, TableFilterExpressionSyntax>
{
	internal TableRelationConditionalExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!HasContext)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		bool shouldUseLhsFieldsFromSource = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<WhereExpressionSyntax>(Context.TargetToken) == null;
		TableRelationConditionalExpressionRecommender tableRelationConditionalExpressionRecommender = this;
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
		return await tableRelationConditionalExpressionRecommender.RecommendSymbolsAsync(cancellationToken, conditions, shouldUseLhsFieldsFromSource);
	}

	protected override TableTypeSymbol SetReferencedTable()
	{
		if (!Context.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
		{
			return null;
		}
		MemberAccessOrIdentifierPropertyValueSyntax relatedTableField = DeclaringSyntax.RelatedTableField;
		return GetReferencedTable(relatedTableField);
	}
}
