using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ControlAddInContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ControlAddInContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.Page.HasFlag(PageContexts.ControlAddIn))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return Context.SemanticModel.Compilation.GetObjectSymbolsByKindAcrossModules(SymbolKind.ControlAddIn);
	}
}
