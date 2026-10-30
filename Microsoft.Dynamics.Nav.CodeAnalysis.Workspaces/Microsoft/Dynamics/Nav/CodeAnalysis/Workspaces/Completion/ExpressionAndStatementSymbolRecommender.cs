using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ExpressionAndStatementSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ExpressionAndStatementSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if ((!Context.General.HasFlag(GeneralContexts.AnyExpression) && !Context.General.HasFlag(GeneralContexts.Statement)) || Context.IsRightOfNameSeparator)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		ImmutableArray<ISymbol> immutableArray = Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken);
		IEnumerable<ISymbol> result;
		if (!Context.General.HasFlag(GeneralContexts.AnyExpression))
		{
			IEnumerable<ISymbol> enumerable = immutableArray;
			result = enumerable;
		}
		else
		{
			result = immutableArray.Where(SemanticFacts.IsValidForExpression);
		}
		return result;
	}
}
