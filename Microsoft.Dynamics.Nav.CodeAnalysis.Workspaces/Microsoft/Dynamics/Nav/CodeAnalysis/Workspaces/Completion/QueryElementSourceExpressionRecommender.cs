using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class QueryElementSourceExpressionRecommender : ContextAwareSymbolRecommender
{
	public QueryElementSourceExpressionRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.Query.HasFlag(QueryContexts.ColumnOrFilterSource))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken).Where(IsValidForExpression);
	}

	private bool IsValidForExpression(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Field)
		{
			return true;
		}
		return false;
	}
}
