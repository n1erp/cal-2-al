using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class RecommendationService : AbstractRecommendationService
{
	protected override async Task<IEnumerable<ISymbol>> GetRecommendedSymbolsAtPositionWorkerAsync(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		if (!(context is MemberSyntaxContext memberSyntaxContext))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (memberSyntaxContext.IsInNonUserCode || memberSyntaxContext.General.HasFlag(GeneralContexts.PropertyDeclaration) || (memberSyntaxContext.General.HasFlag(GeneralContexts.PropertyValue) && !memberSyntaxContext.General.HasFlag(GeneralContexts.AnyExpression) && !memberSyntaxContext.General.HasFlag(GeneralContexts.AnyComplexPropertyExpression) && memberSyntaxContext.PropertyExpressionContexts == PropertyExpressionContexts.None))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ArrayBuilder<ISymbol> result = ArrayBuilder<ISymbol>.GetInstance();
		foreach (ContextAwareSymbolRecommender recommender in CreateRecommenders(memberSyntaxContext))
		{
			IEnumerable<ISymbol> enumerable = await recommender.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (enumerable.Any())
			{
				if (recommender.IsExclusive)
				{
					return enumerable;
				}
				result.AddRange(enumerable);
			}
		}
		return result.ToImmutableAndFree();
	}

	private static ImmutableArray<ContextAwareSymbolRecommender> CreateRecommenders(MemberSyntaxContext context)
	{
		return ImmutableArray.Create(new ContextAwareSymbolRecommender[26]
		{
			new ReferenceMemberListContextSymbolRecommender(context),
			new PageChangeAnchorSymbolRecommmender(context),
			new TableSymbolRecommender(context),
			new FieldModifySymbolRecommmender(context),
			new FieldParameterSymbolContextRecommender(context),
			new ExpressionAndStatementSymbolRecommender(context),
			new TypeContextSymbolRecommender(context),
			new ExtendsContextSymbolRecommender(context),
			new PagePartContextSymbolRecommender(context),
			new ControlAddInContextSymbolRecommender(context),
			new ContainerContextSymbolRecommender(context),
			new TableRelationConditionalExpressionRecommender(context),
			new CalculationFormulaExpressionRecommender(context),
			new PropertyExpressionDestinationContextRecommender(context),
			new SourceTableViewExpressionRecommender(context),
			new ViewOrderByExpressionRecommender(context),
			new ViewFiltersExpressionRecommender(context),
			new TableFilterExpressionRecommender(context),
			new OrderByExpressionRecommender(context),
			new QueryElementSourceExpressionRecommender(context),
			new QueryDataItemLinkExpressionRecommender(context),
			new QueryColumnFilterSymbolRecommender(context),
			new SyntheticOptionContainerMembersRecommender(context),
			new ReportDataItemLinkExpressionRecommender(context),
			new XmlPortFieldSourceRecommender(context),
			new AttributeArgumentSymbolRecommender(context)
		});
	}
}
